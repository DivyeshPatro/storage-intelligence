using System.Diagnostics;
using StorageIntelligence.Validation.Interop;

namespace StorageIntelligence.Validation;

public sealed record ManifestFile(string Path, long Logical, string? LinkGroup, string Kind);

public sealed record ManifestLink(string Path, string Kind, string Target);

public sealed record SkippedFeature(string Feature, string Reason);

/// <summary>
/// What the generator created. Paths are relative to the fixture root. Everything the generator attempted but the
/// filesystem refused is listed in <see cref="Skipped"/> with the reason; it is never silently dropped.
/// </summary>
public sealed record FixtureManifest(
    string FixtureRoot,
    string FileSystem,
    int Seed,
    List<ManifestFile> Files,
    List<string> Directories,
    List<ManifestLink> Links,
    List<string> InaccessibleDirectories,
    List<ManifestFile> OutsideFiles,
    List<SkippedFeature> Skipped,
    Dictionary<string, int> LinkGroupSizes);

/// <summary>
/// Builds a deterministic fixture tree under a guarded root (<see cref="TestRootGuard"/>):
/// <c>root\fixture</c> (scanned) and <c>root\outside</c> (link targets that must not be counted).
/// </summary>
public sealed class FixtureGenerator
{
    private static readonly long[] Sizes = [0, 1, 100, 700, 4095, 4096, 4097, 65_536, 1_000_000, 3_000_123];

    private readonly string _root;
    private readonly string _fixture;
    private readonly string _outside;
    private readonly Random _random;
    private readonly FixtureManifest _manifest;
    private readonly byte[] _noise;

    private FixtureGenerator(string root, int seed, string fileSystem)
    {
        _root = Path.GetFullPath(root);
        _fixture = Path.Combine(_root, "fixture");
        _outside = Path.Combine(_root, "outside");
        _random = new Random(seed);
        _noise = new byte[1 << 20];
        _random.NextBytes(_noise);
        _manifest = new FixtureManifest(_fixture, fileSystem, seed, [], [], [], [], [], [], []);
    }

    public static FixtureManifest Generate(string root, int seed, string fileSystem)
    {
        TestRootGuard.AssertWritable(root);
        var generator = new FixtureGenerator(root, seed, fileSystem);
        if (Directory.Exists(generator._fixture) || Directory.Exists(generator._outside))
        {
            throw new InvalidOperationException("Fixture already exists under this root; use a fresh root.");
        }

        Directory.CreateDirectory(generator._fixture);
        Directory.CreateDirectory(generator._outside);
        generator.Build();
        return generator._manifest;
    }

    /// <summary>
    /// Deletes a test root created by <see cref="TestRootGuard.CreateTempTestRoot"/>. Reparse points (junctions,
    /// symbolic links) are removed as links and never entered, so a junction loop or a link to a real folder can never
    /// cause anything outside the root to be deleted.
    /// </summary>
    public static void DeleteTestRoot(string root)
    {
        TestRootGuard.AssertWritable(root);
        string extended = ValidationNative.Extended(root);
        DeleteTree(extended);

        static void DeleteTree(string directory)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
            {
                FileAttributes attributes = System.IO.File.GetAttributes(entry);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        Directory.Delete(entry, recursive: false); // removes the link itself
                    }
                    else
                    {
                        System.IO.File.Delete(entry);
                    }
                }
                else if ((attributes & FileAttributes.Directory) != 0)
                {
                    DeleteTree(entry);
                }
                else
                {
                    if ((attributes & FileAttributes.ReadOnly) != 0)
                    {
                        System.IO.File.SetAttributes(entry, attributes & ~FileAttributes.ReadOnly);
                    }

                    System.IO.File.Delete(entry);
                }
            }

            Directory.Delete(directory, recursive: false);
        }
    }

    /// <summary>Removes deny ACEs the generator added so the test root can be deleted.</summary>
    public static void ReleaseDenials(FixtureManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        foreach (string dir in manifest.InaccessibleDirectories)
        {
            RunTool("icacls.exe", Path.Combine(manifest.FixtureRoot, dir), "/remove:d", "*S-1-1-0");
        }
    }

    private void Build()
    {
        Attempt("plain-tree", () => PlainTree("plain", depth: 0));
        Attempt("wide-directory", () =>
        {
            Dir("wide");
            for (int i = 0; i < 5000; i++)
            {
                File($"wide\\w{i:D5}.txt", 1, "plain");
            }
        });
        Attempt("empty-directory", () => Dir("empty"));
        Attempt("hard-links", HardLinks);
        Attempt("junction-loop", () =>
        {
            Dir("junctions");
            Junction("junctions\\loop", _fixture);
        });
        Attempt("junction-outside", () =>
        {
            Dir("junctions");
            Junction("junctions\\to-outside", _outside);
        });
        Attempt("symlink-directory", () =>
        {
            Dir("symlinks");
            Directory.CreateSymbolicLink(Full("symlinks\\dir-link"), Full("plain"));
            _manifest.Links.Add(new ManifestLink("symlinks\\dir-link", "symlink-directory", "plain"));
        });
        Attempt("symlink-file", () =>
        {
            Dir("symlinks");
            string target = Full("links\\y.bin");
            if (!System.IO.File.Exists(target))
            {
                throw new InvalidOperationException("target links\\y.bin was not created");
            }

            System.IO.File.CreateSymbolicLink(Full("symlinks\\file-link.bin"), target);
            _manifest.Links.Add(new ManifestLink("symlinks\\file-link.bin", "symlink-file", "links\\y.bin"));
        });
        Attempt("sparse", () =>
        {
            Dir("special");
            Sparse("special\\sparse-64m-1m.bin", logical: 64L << 20, (8L << 20, 1 << 20));
            Sparse("special\\sparse-16m-empty.bin", logical: 16L << 20);
        });
        Attempt("ntfs-compression", () =>
        {
            Dir("special");
            Compressed("special\\compressed-text.txt", 8 << 20, compressible: true);
            Compressed("special\\compressed-noise.bin", 4 << 20, compressible: false);
        });
        Attempt("wof-compression", () =>
        {
            Dir("special");
            Wof("special\\wof-text.txt", 8 << 20);
        });
        Attempt("alternate-data-stream", () =>
        {
            Dir("special");
            File("special\\with-ads.bin", 1024, "ads-host");
            WriteBytes(@"\\?\" + Full("special\\with-ads.bin") + ":extra", 1 << 20, compressible: false);
            _manifest.Skipped.Add(new SkippedFeature("known-gap:alternate-data-streams", "1 MiB alternate data stream on special\\with-ads.bin; listings report only the unnamed stream."));
        });
        Attempt("resident-file", () =>
        {
            Dir("special");
            File("special\\resident-100.txt", 100, "resident");
        });
        Attempt("unusual-names", () =>
        {
            Dir("names");
            File("names\\naïve-文件-😀.txt", 4097, "plain");
            File("names\\trailing space ", 10, "plain", raw: true);
            File("names\\trailing.dot.", 10, "plain", raw: true);
            File("names\\CON", 10, "plain", raw: true);
        });
        Attempt("long-path", () =>
        {
            string rel = "names";
            for (int i = 0; i < 6; i++)
            {
                rel += "\\" + new string((char)('a' + i), 50);
                Dir(rel);
            }

            File(rel + "\\deep-file.bin", 70_000, "plain");
        });
        Attempt("outside-data", () => FileAt(_outside, "data.bin", 2 << 20, _manifest.OutsideFiles, null));
        Attempt("inaccessible-directory", () =>
        {
            Dir("denied");
            File("denied\\secret.bin", 1 << 20, "denied");
            RunTool("icacls.exe", Full("denied"), "/deny", "*S-1-1-0:(OI)(CI)(RD)");
            try
            {
                // icacls can report success on filesystems without ACLs (FAT32, exFAT): verify the denial took effect.
                _ = Directory.GetFileSystemEntries(Full("denied"));
                throw new IOException("deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable");
            }
            catch (UnauthorizedAccessException)
            {
                _manifest.InaccessibleDirectories.Add("denied");
            }
        });
    }

    private void PlainTree(string rel, int depth)
    {
        Dir(rel);
        int files = _random.Next(0, 7);
        for (int i = 0; i < files; i++)
        {
            File($"{rel}\\f{depth}-{i}.dat", Sizes[_random.Next(Sizes.Length)], "plain");
        }

        if (depth < 3)
        {
            int children = _random.Next(0, 4);
            for (int i = 0; i < children; i++)
            {
                PlainTree($"{rel}\\d{depth}-{i}", depth + 1);
            }
        }
    }

    private void HardLinks()
    {
        Dir("links\\a");
        Dir("links\\b");
        Dir("links\\deep\\d1\\d2");
        File("links\\a\\x.bin", 1 << 20, "hardlink", group: "x");
        Link("links\\a\\x-same-dir.bin", "links\\a\\x.bin", "x");
        Link("links\\b\\x-sibling.bin", "links\\a\\x.bin", "x");
        Link("links\\deep\\d1\\d2\\x-far.bin", "links\\a\\x.bin", "x");
        File("links\\y.bin", 2 << 20, "plain");
        File("links\\z.bin", 300_000, "hardlink", group: "z");
        LinkOutside("z-outside.bin", "links\\z.bin", "z");
        File("links\\big.bin", 12 << 20, "hardlink", group: "big");
        LinkOutside("big-outside.bin", "links\\big.bin", "big");
    }

    private void Attempt(string feature, Action action)
    {
        Console.Error.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] fixture: {feature}");
        try
        {
            action();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidOperationException or PlatformNotSupportedException)
        {
            _manifest.Skipped.Add(new SkippedFeature(feature, e.Message));
        }
    }

    private string Full(string rel) => Path.Combine(_fixture, rel);

    private void Dir(string rel)
    {
        Directory.CreateDirectory(@"\\?\" + Full(rel));
        if (!_manifest.Directories.Contains(rel))
        {
            _manifest.Directories.Add(rel);
        }
    }

    private void File(string rel, long size, string kind, string? group = null, bool raw = false)
    {
        string path = raw ? @"\\?\" + Full(rel) : Full(rel);
        WriteBytes(path, size, compressible: false);
        _manifest.Files.Add(new ManifestFile(rel, size, group, kind));
        if (group is not null)
        {
            _manifest.LinkGroupSizes[group] = _manifest.LinkGroupSizes.GetValueOrDefault(group) + 1;
        }
    }

    private void FileAt(string directory, string name, long size, List<ManifestFile> list, string? group)
    {
        WriteBytes(Path.Combine(directory, name), size, compressible: false);
        list.Add(new ManifestFile(name, size, group, "outside"));
    }

    private void Link(string rel, string existingRel, string group)
    {
        if (!ValidationNative.CreateHardLink(@"\\?\" + Full(rel), @"\\?\" + Full(existingRel), 0))
        {
            throw new IOException($"CreateHardLink failed ({System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");
        }

        long size = new FileInfo(Full(existingRel)).Length;
        _manifest.Files.Add(new ManifestFile(rel, size, group, "hardlink"));
        _manifest.LinkGroupSizes[group]++;
    }

    private void LinkOutside(string name, string existingRel, string group)
    {
        string path = Path.Combine(_outside, name);
        if (!ValidationNative.CreateHardLink(@"\\?\" + path, @"\\?\" + Full(existingRel), 0))
        {
            throw new IOException($"CreateHardLink failed ({System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");
        }

        _manifest.OutsideFiles.Add(new ManifestFile(name, new FileInfo(path).Length, group, "hardlink-outside"));
        _manifest.LinkGroupSizes[group]++;
    }

    private void Junction(string rel, string target)
    {
        RunTool("cmd.exe", "/c", "mklink", "/J", Full(rel), target);
        _manifest.Links.Add(new ManifestLink(rel, "junction", Path.GetRelativePath(_fixture, target)));
    }

    private unsafe void Sparse(string rel, long logical, params (long Offset, int Length)[] data)
    {
        using (var fs = new FileStream(Full(rel), FileMode.CreateNew, FileAccess.ReadWrite))
        {
            if (!ValidationNative.DeviceIoControl(fs.SafeFileHandle, ValidationNative.FSCTL_SET_SPARSE, null, 0, null, 0, out _, 0))
            {
                fs.Dispose();
                System.IO.File.Delete(Full(rel));
                throw new IOException($"FSCTL_SET_SPARSE failed ({System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");
            }

            fs.SetLength(logical);
            foreach ((long offset, int length) in data)
            {
                fs.Position = offset;
                fs.Write(_noise, 0, length);
            }

            fs.Flush(flushToDisk: true);
        }

        _manifest.Files.Add(new ManifestFile(rel, logical, null, "sparse"));
    }

    private unsafe void Compressed(string rel, int size, bool compressible)
    {
        try
        {
            CompressedCore(rel, size, compressible);
        }
        catch
        {
            System.IO.File.Delete(Full(rel)); // never leave an unrecorded half-made file behind
            throw;
        }

        _manifest.Files.Add(new ManifestFile(rel, size, null, compressible ? "ntfs-compressed" : "ntfs-compressed-incompressible"));
    }

    private unsafe void CompressedCore(string rel, int size, bool compressible)
    {
        using (var fs = new FileStream(Full(rel), FileMode.CreateNew, FileAccess.ReadWrite))
        {
            ushort format = ValidationNative.COMPRESSION_FORMAT_DEFAULT;
            if (!ValidationNative.DeviceIoControl(fs.SafeFileHandle, ValidationNative.FSCTL_SET_COMPRESSION, &format, sizeof(ushort), null, 0, out _, 0))
            {
                throw new IOException($"FSCTL_SET_COMPRESSION failed ({System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");
            }

            WriteContent(fs, size, compressible);
            fs.Flush(flushToDisk: true);
        }
    }

    private void Wof(string rel, int size)
    {
        string path = Full(rel);
        using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
        {
            WriteContent(fs, size, compressible: true);
            fs.Flush(flushToDisk: true);
        }

        try
        {
            RunTool("compact.exe", "/c", "/exe:xpress4k", path);
        }
        catch
        {
            System.IO.File.Delete(path);
            throw;
        }

        (bool external, uint provider) = ValidationNative.WofState(ValidationNative.Extended(path));
        if (!external)
        {
            System.IO.File.Delete(path);
            throw new IOException("compact /exe did not produce a WOF-backed file (WOF filter not attached to this volume?).");
        }

        _manifest.Files.Add(new ManifestFile(rel, size, null, $"wof(provider={provider})"));
    }

    private void WriteBytes(string path, long size, bool compressible)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        WriteContent(fs, size, compressible);
    }

    private void WriteContent(FileStream fs, long size, bool compressible)
    {
        byte[] text = "storage intelligence fixture data "u8.ToArray();
        long remaining = size;
        while (remaining > 0)
        {
            int chunk = (int)Math.Min(remaining, _noise.Length);
            if (compressible)
            {
                for (int written = 0; written < chunk;)
                {
                    int n = Math.Min(text.Length, chunk - written);
                    fs.Write(text, 0, n);
                    written += n;
                }
            }
            else
            {
                fs.Write(_noise, 0, chunk);
            }

            remaining -= chunk;
        }
    }

    private static void RunTool(string exe, params string[] args)
    {
        var info = new ProcessStartInfo(exe) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (string a in args)
        {
            info.ArgumentList.Add(a);
        }

        using Process p = Process.Start(info) ?? throw new IOException($"Could not start {exe}.");
        string output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            throw new IOException($"{exe} {string.Join(' ', args)} failed ({p.ExitCode}): {output.Trim()}");
        }
    }
}
