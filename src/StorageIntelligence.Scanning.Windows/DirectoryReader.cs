using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using StorageIntelligence.Accounting;
using StorageIntelligence.Scanning.Windows.Interop;

namespace StorageIntelligence.Scanning.Windows;

/// <summary>One directory entry exactly as the filesystem listed it. Metadata only.</summary>
public readonly record struct RawEntry(
    string Name,
    uint Attributes,
    uint ReparseTag,
    long EndOfFile,
    long AllocationSize,
    long LastWriteTime,
    long ChangeTime,
    FileId128? FileId)
{
    public bool IsDirectory => (Attributes & Native.FILE_ATTRIBUTE_DIRECTORY) != 0;

    public bool IsReparsePoint => (Attributes & Native.FILE_ATTRIBUTE_REPARSE_POINT) != 0;
}

/// <summary>Which directory-information class a volume supports.</summary>
public enum ListingFormat
{
    /// <summary>FILE_ID_EXTD_DIR_INFO: 128-bit file ID and reparse tag per entry.</summary>
    IdExtended,

    /// <summary>FILE_FULL_DIR_INFO: no file ID (FAT/exFAT and other filesystems without stable IDs).</summary>
    Full,
}

/// <summary>
/// Lists one directory through an open handle in batches, without opening any child. The handle is opened with
/// FILE_LIST_DIRECTORY only and FILE_FLAG_OPEN_REPARSE_POINT, so the directory itself is never a followed link.
/// </summary>
public sealed unsafe class DirectoryReader : IDisposable
{
    private const int BufferSize = 64 * 1024;

    private readonly SafeFileHandle _handle;
    private readonly byte* _buffer;
    private ListingFormat _format;
    private bool _done;
    private bool _started;

    private DirectoryReader(SafeFileHandle handle, ListingFormat format)
    {
        _handle = handle;
        _format = format;
        _buffer = (byte*)NativeMemory.AlignedAlloc(BufferSize, 8);
    }

    public ListingFormat Format => _format;

    /// <summary>Win32 error that stopped the listing early, or 0.</summary>
    public int Error { get; private set; }

    /// <summary>Opens a directory for listing. Returns null and the Win32 error if it cannot be opened.</summary>
    public static DirectoryReader? Open(string extendedPath, ListingFormat preferred, out int error)
    {
        SafeFileHandle handle = Native.OpenMetadata(extendedPath, Native.FILE_LIST_DIRECTORY, out error);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            return null;
        }

        return new DirectoryReader(handle, preferred);
    }

    /// <summary>Basic information (timestamps) of the directory itself, for change detection.</summary>
    public bool TryGetDirectoryInfo(out long lastWriteTime, out long changeTime)
    {
        if (Native.TryGetBasicInfo(_handle, out Native.FILE_BASIC_INFO info))
        {
            lastWriteTime = info.LastWriteTime;
            changeTime = info.ChangeTime;
            return true;
        }

        lastWriteTime = changeTime = 0;
        return false;
    }

    /// <summary>
    /// Appends the next batch of entries ("." and ".." excluded). Returns false once the listing has ended,
    /// either normally or with <see cref="Error"/> set.
    /// </summary>
    public bool ReadBatch(List<RawEntry> into)
    {
        ArgumentNullException.ThrowIfNull(into);
        if (_done)
        {
            return false;
        }

        int infoClass = _format == ListingFormat.IdExtended ? Native.FileIdExtdDirectoryInfo : Native.FileFullDirectoryInfo;
        if (!Native.GetFileInformationByHandleEx(_handle, infoClass, _buffer, BufferSize))
        {
            int error = Marshal.GetLastPInvokeError();
            if (error == Native.ERROR_NO_MORE_FILES)
            {
                _done = true;
                return false;
            }

            if (_format == ListingFormat.IdExtended && !_started && IsUnsupported(error))
            {
                // The volume does not support the extended class; fall back once, from the start.
                _format = ListingFormat.Full;
                return ReadBatch(into);
            }

            Error = error;
            _done = true;
            return false;
        }

        _started = true;
        Parse(into);
        return true;
    }

    public void Dispose()
    {
        _handle.Dispose();
        NativeMemory.AlignedFree(_buffer);
    }

    private static bool IsUnsupported(int error) =>
        error is Native.ERROR_INVALID_PARAMETER or Native.ERROR_NOT_SUPPORTED or Native.ERROR_INVALID_FUNCTION;

    private void Parse(List<RawEntry> into)
    {
        byte* p = _buffer;
        while (true)
        {
            uint next = *(uint*)p;
            RawEntry entry = _format == ListingFormat.IdExtended ? ParseExtended(p) : ParseFull(p);
            if (entry.Name is not "." and not "..")
            {
                into.Add(entry);
            }

            if (next == 0)
            {
                break;
            }

            p += next;
        }
    }

    // FILE_ID_EXTD_DIR_INFO: NextEntryOffset 0, FileIndex 4, CreationTime 8, LastAccessTime 16, LastWriteTime 24,
    // ChangeTime 32, EndOfFile 40, AllocationSize 48, FileAttributes 56, FileNameLength 60, EaSize 64,
    // ReparsePointTag 68, FileId 72 (16 bytes), FileName 88.
    private static RawEntry ParseExtended(byte* p)
    {
        uint attributes = *(uint*)(p + 56);
        int nameBytes = (int)*(uint*)(p + 60);
        return new RawEntry(
            new string((char*)(p + 88), 0, nameBytes / 2),
            attributes,
            (attributes & Native.FILE_ATTRIBUTE_REPARSE_POINT) != 0 ? *(uint*)(p + 68) : 0,
            *(long*)(p + 40),
            *(long*)(p + 48),
            *(long*)(p + 24),
            *(long*)(p + 32),
            new FileId128(*(ulong*)(p + 80), *(ulong*)(p + 72)));
    }

    // FILE_FULL_DIR_INFO: as above up to FileNameLength 60, EaSize 64 (holds the reparse tag when the entry is a
    // reparse point), FileName 68.
    private static RawEntry ParseFull(byte* p)
    {
        uint attributes = *(uint*)(p + 56);
        int nameBytes = (int)*(uint*)(p + 60);
        return new RawEntry(
            new string((char*)(p + 68), 0, nameBytes / 2),
            attributes,
            (attributes & Native.FILE_ATTRIBUTE_REPARSE_POINT) != 0 ? *(uint*)(p + 64) : 0,
            *(long*)(p + 40),
            *(long*)(p + 48),
            *(long*)(p + 24),
            *(long*)(p + 32),
            null);
    }
}
