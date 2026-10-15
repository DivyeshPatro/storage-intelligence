using System.Runtime.InteropServices;
using StorageIntelligence.Validation.Interop;

namespace StorageIntelligence.Validation;

/// <summary>One file as measured independently through its own handle.</summary>
public sealed record ReferenceFile(
    string Path,
    string Directory,
    long Logical,
    long Allocated,
    long CompressedSize,
    uint Links,
    string Key,
    uint Attributes,
    uint ReparseTag,
    bool WofExternal);

/// <summary>Brute-force subtree totals. Link counts are measured for every file, so classification is complete.</summary>
public sealed record ReferenceDirectory(
    string Path,
    long Logical,
    long Allocated,
    long FileEntries,
    long DistinctFiles,
    long Directories,
    long SharedAllocated,
    long SharedFiles,
    long ExclusiveAllocated,
    long ExclusiveFiles,
    long Inaccessible,
    long NotFollowedLinks,
    long SkippedReparse,
    long EntryErrors);

public sealed record ReferenceResult(string Root, string FileSystem, List<ReferenceFile> Files, List<ReferenceDirectory> Directories, List<string> Issues);

/// <summary>
/// Independent reference scanner. It shares no code with the production scanner or the accounting core:
/// enumeration uses FindFirstFileExW, each file is opened for attributes only (FILE_READ_ATTRIBUTES, reparse points
/// not followed) to read FILE_STANDARD_INFO, FILE_ID_INFO and FILE_ATTRIBUTE_TAG_INFO, and totals are computed by
/// brute force from set definitions. It is slow by design and is only used on fixtures.
/// </summary>
public static unsafe class ReferenceScanner
{
    private sealed class DirFacts
    {
        public long Inaccessible;
        public long NotFollowed;
        public long SkippedReparse;
        public long EntryErrors;
    }

    public static ReferenceResult Scan(string root, string fileSystem)
    {
        bool hardLinks = fileSystem.ToUpperInvariant() is "NTFS" or "REFS";
        string rootFull = Path.GetFullPath(root).TrimEnd('\\');
        var files = new List<ReferenceFile>();
        var dirs = new Dictionary<string, DirFacts>(StringComparer.Ordinal) { [""] = new() };
        var issues = new List<string>();
        var stack = new Stack<string>();
        stack.Push("");

        while (stack.Count > 0)
        {
            string rel = stack.Pop();
            string dirPath = rel.Length == 0 ? rootFull : rootFull + "\\" + rel;
            DirFacts facts = dirs[rel];
            ValidationNative.WIN32_FIND_DATAW data;
            nint find = ValidationNative.FindFirstFileEx(
                ValidationNative.Extended(dirPath) + "\\*", ValidationNative.FindExInfoBasic, &data,
                ValidationNative.FindExSearchNameMatch, 0, ValidationNative.FIND_FIRST_EX_LARGE_FETCH);
            if (find == -1)
            {
                int error = Marshal.GetLastPInvokeError();
                if (error != ValidationNative.ERROR_FILE_NOT_FOUND)
                {
                    facts.Inaccessible++;
                    issues.Add($"inaccessible {rel} ({error})");
                }

                continue;
            }

            try
            {
                do
                {
                    string name = new((char*)data.cFileName);
                    if (name is "." or "..")
                    {
                        continue;
                    }

                    string childRel = rel.Length == 0 ? name : rel + "\\" + name;
                    bool reparse = (data.dwFileAttributes & ValidationNative.FILE_ATTRIBUTE_REPARSE_POINT) != 0;
                    bool surrogate = reparse && (data.dwReserved0 & 0x20000000) != 0;
                    if ((data.dwFileAttributes & ValidationNative.FILE_ATTRIBUTE_DIRECTORY) != 0)
                    {
                        if (reparse)
                        {
                            if (surrogate)
                            {
                                facts.NotFollowed++;
                            }
                            else
                            {
                                facts.SkippedReparse++;
                            }

                            continue;
                        }

                        dirs[childRel] = new DirFacts();
                        stack.Push(childRel);
                        continue;
                    }

                    if (surrogate)
                    {
                        facts.NotFollowed++;
                        continue;
                    }

                    ReferenceFile? file = Measure(rootFull + "\\" + childRel, childRel, rel, hardLinks);
                    if (file is null)
                    {
                        facts.EntryErrors++;
                        issues.Add($"unmeasurable {childRel} ({Marshal.GetLastPInvokeError()})");
                        continue;
                    }

                    files.Add(file);
                }
                while (ValidationNative.FindNextFile(find, &data));
            }
            finally
            {
                ValidationNative.FindClose(find);
            }
        }

        var directories = dirs.Keys.OrderBy(k => k, StringComparer.Ordinal)
            .Select(d => Totals(d, files, dirs))
            .ToList();
        return new ReferenceResult(rootFull, fileSystem, files, directories, issues);
    }

    private static ReferenceFile? Measure(string fullPath, string rel, string dirRel, bool hardLinks)
    {
        string extended = ValidationNative.Extended(fullPath);
        using var handle = ValidationNative.OpenAttributes(extended);
        if (handle.IsInvalid)
        {
            return null;
        }

        ValidationNative.FILE_STANDARD_INFO standard;
        ValidationNative.FILE_ID_INFO id;
        ValidationNative.FILE_ATTRIBUTE_TAG_INFO tag;
        if (!ValidationNative.GetFileInformationByHandleEx(handle, ValidationNative.FileStandardInfo, &standard, (uint)sizeof(ValidationNative.FILE_STANDARD_INFO)) ||
            !ValidationNative.GetFileInformationByHandleEx(handle, ValidationNative.FileAttributeTagInfo, &tag, (uint)sizeof(ValidationNative.FILE_ATTRIBUTE_TAG_INFO)))
        {
            return null;
        }

        string key = hardLinks && ValidationNative.GetFileInformationByHandleEx(handle, ValidationNative.FileIdInfo, &id, (uint)sizeof(ValidationNative.FILE_ID_INFO))
            ? $"{id.VolumeSerialNumber:x}:{id.FileIdHigh:x16}{id.FileIdLow:x16}"
            : "entry:" + rel;
        // Extended path: a plain path would let Win32 map device names such as 'CON' to the console device.
        (bool wof, _) = ValidationNative.WofState(extended);
        return new ReferenceFile(
            rel,
            dirRel,
            standard.EndOfFile,
            standard.AllocationSize,
            ValidationNative.CompressedSize(extended),
            hardLinks ? standard.NumberOfLinks : 1,
            key,
            tag.FileAttributes,
            tag.ReparseTag,
            wof);
    }

    private static ReferenceDirectory Totals(string dir, List<ReferenceFile> files, Dictionary<string, DirFacts> dirs)
    {
        bool Inside(string path) => dir.Length == 0 || path == dir || path.StartsWith(dir + "\\", StringComparison.Ordinal);

        var entries = files.Where(f => Inside(f.Directory)).ToList();
        long logical = 0, allocated = 0, sharedAllocated = 0, sharedFiles = 0, exclusiveAllocated = 0, exclusiveFiles = 0;
        foreach (IGrouping<string, ReferenceFile> file in entries.GroupBy(f => f.Key, StringComparer.Ordinal))
        {
            ReferenceFile first = file.First();
            logical += first.Logical;
            allocated += first.Allocated;
            if (file.Count() < file.Max(f => f.Links))
            {
                sharedAllocated += first.Allocated;
                sharedFiles++;
            }
            else
            {
                exclusiveAllocated += first.Allocated;
                exclusiveFiles++;
            }
        }

        var subtree = dirs.Where(kv => Inside(kv.Key)).ToList();
        return new ReferenceDirectory(
            dir,
            logical,
            allocated,
            entries.Count,
            entries.Select(f => f.Key).Distinct(StringComparer.Ordinal).Count(),
            subtree.Count - 1,
            sharedAllocated,
            sharedFiles,
            exclusiveAllocated,
            exclusiveFiles,
            subtree.Sum(kv => kv.Value.Inaccessible),
            subtree.Sum(kv => kv.Value.NotFollowed),
            subtree.Sum(kv => kv.Value.SkippedReparse),
            subtree.Sum(kv => kv.Value.EntryErrors));
    }
}
