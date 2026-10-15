using System.Diagnostics;
using System.Runtime.InteropServices;
using StorageIntelligence.Scanning.Windows;
using StorageIntelligence.Validation.Interop;

namespace StorageIntelligence.Validation;

/// <summary>One measured file creation.</summary>
/// <summary>One measured file creation. Matches are judged against the settled (post-flush) free-space change.</summary>
public sealed record AllocationTrial(
    string Case,
    int Trial,
    long Logical,
    long FreeSpaceDelta,
    long FreeSpaceDeltaAfterFlush,
    long ListingAllocated,
    long HandleAllocated,
    long CompressedSize,
    bool WofExternal,
    string ListingAttributes,
    bool ListingMatchesFreeSpace,
    bool HandleMatchesFreeSpace,
    bool CompressedSizeMatchesFreeSpace);

/// <param name="Verdict">"consistent" when the directory listing matched the settled free-space change in every trial,
/// "inconsistent" otherwise, or "not-supported".</param>
/// <param name="ConsistentSources">Which measurement sources matched in every trial (listing, handle, compressed-size).</param>
public sealed record AllocationCaseResult(string Case, string Verdict, string Detail, List<AllocationTrial> Trials, List<string>? ConsistentSources = null);

public sealed record AllocationReport(string Root, string FileSystem, long ClusterSize, long ToleranceBytes, List<AllocationCaseResult> Cases);

/// <summary>
/// Independently validates allocated-size semantics. For each case it creates files on an otherwise idle disposable
/// volume and measures the volume's free-space change, then compares it with the allocation the production
/// directory listing reports, the allocation FILE_STANDARD_INFO reports and GetCompressedFileSize. A case is
/// "consistent" only if the listing value is within <see cref="AllocationReport.ToleranceBytes"/> of the
/// free-space change in every trial; otherwise it must stay uncertain.
/// </summary>
public static class AllocationHarness
{
    private const int ToleranceClusters = 2;

    private sealed record CaseSpec(string Name, Func<string, long> Create, bool NeedsHardLinkFs = false, bool NtfsOnly = false);

    public static AllocationReport Run(string root, string fileSystem, long clusterSize)
    {
        TestRootGuard.AssertWritable(root);
        string baseDir = Path.Combine(Path.GetFullPath(root), "allocation");
        Directory.CreateDirectory(baseDir);
        var noise = new byte[1 << 20];
        new Random(7).NextBytes(noise);
        bool ntfs = fileSystem.Equals("NTFS", StringComparison.OrdinalIgnoreCase);
        bool sparseCapable = ntfs || fileSystem.Equals("ReFS", StringComparison.OrdinalIgnoreCase);

        var specs = new List<(string Case, CaseSpec[] Trials)>
        {
            ("resident-100B", [new("resident", p => Write(p, 100, noise, false))]),
            ("normal", [
                new("normal", p => Write(p, 1_000_000, noise, false)),
                new("normal", p => Write(p, 8_388_608, noise, false)),
                new("normal", p => Write(p, 13_000_123, noise, false))]),
            ("sparse", [
                new("sparse", p => Sparse(p, 64L << 20, noise, (8L << 20, 1 << 20)), NeedsHardLinkFs: true),
                new("sparse", p => Sparse(p, 256L << 20, noise, (0, 256 << 10), (100L << 20, 256 << 10), (200L << 20, 256 << 10)), NeedsHardLinkFs: true),
                new("sparse", p => Sparse(p, 16L << 20, noise), NeedsHardLinkFs: true)]),
            ("ntfs-compressed", [
                new("ntfs-compressed", p => Compressed(p, 8 << 20, noise, true), NtfsOnly: true),
                new("ntfs-compressed", p => Compressed(p, 4 << 20, noise, false), NtfsOnly: true),
                new("ntfs-compressed", p => Compressed(p, 1 << 20, noise, true), NtfsOnly: true)]),
            // Incompressible data is not tested for WOF: compact.exe declines to WOF-compress it.
            ("wof", [
                new("wof", p => Wof(p, 8 << 20, noise, true), NtfsOnly: true),
                new("wof", p => Wof(p, 3 << 20, noise, true), NtfsOnly: true),
                new("wof", p => Wof(p, 1 << 20, noise, true), NtfsOnly: true)]),
        };

        long tolerance = ToleranceClusters * clusterSize;
        bool volumeRoot = TestRootGuard.IsVolumeRoot(root);
        var results = new List<AllocationCaseResult>();
        foreach ((string caseName, CaseSpec[] trials) in specs)
        {
            if ((trials[0].NtfsOnly && !ntfs) || (trials[0].NeedsHardLinkFs && !sparseCapable))
            {
                results.Add(new AllocationCaseResult(caseName, "not-supported", $"{caseName} is not available on {fileSystem}", []));
                continue;
            }

            Console.Error.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] allocation: {caseName}");
            var measured = new List<AllocationTrial>();
            string? failure = null;
            for (int i = 0; i < trials.Length; i++)
            {
                string dir = Path.Combine(baseDir, $"{caseName}-{i}");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "f.bin");
                try
                {
                    long before = Settled(dir, root, volumeRoot);
                    long logical = trials[i].Create(path);
                    long immediate = before - StableFree(dir);
                    long settled = before - Settled(dir, root, volumeRoot);
                    measured.Add(Measure(caseName, i, path, dir, logical, immediate, settled, tolerance));
                }
                catch (Exception e) when (e is IOException or InvalidOperationException or UnauthorizedAccessException)
                {
                    failure = e.Message;
                    break;
                }
            }

            if (failure is not null)
            {
                results.Add(new AllocationCaseResult(caseName, "not-supported", failure, measured));
                continue;
            }

            var sources = new List<string>();
            if (measured.All(t => t.ListingMatchesFreeSpace))
            {
                sources.Add("listing");
            }

            if (measured.All(t => t.HandleMatchesFreeSpace))
            {
                sources.Add("handle");
            }

            if (measured.All(t => t.CompressedSizeMatchesFreeSpace))
            {
                sources.Add("compressed-size");
            }

            bool listingOk = sources.Contains("listing");
            string detail = (listingOk ? "listing matched" : "listing did not match") +
                $" the settled free-space change in every trial; sources matching in every trial: {(sources.Count == 0 ? "none" : string.Join(", ", sources))}";
            results.Add(new AllocationCaseResult(caseName, listingOk ? "consistent" : "inconsistent", detail, measured, sources));
        }

        return new AllocationReport(root, fileSystem, clusterSize, tolerance, results);
    }

    private static AllocationTrial Measure(string caseName, int trial, string path, string dir, long logical, long immediate, long delta, long tolerance)
    {
        RawEntry entry = default;
        using (DirectoryReader reader = DirectoryReader.Open(ValidationNative.Extended(dir), ListingFormat.IdExtended, out int error)
            ?? throw new IOException($"cannot list {dir} ({error})"))
        {
            var batch = new List<RawEntry>();
            while (reader.ReadBatch(batch))
            {
            }

            entry = batch.Single(e => e.Name == "f.bin");
        }

        long handleAllocated = -1;
        using (var handle = ValidationNative.OpenAttributes(ValidationNative.Extended(path)))
        {
            unsafe
            {
                ValidationNative.FILE_STANDARD_INFO info;
                if (!handle.IsInvalid && ValidationNative.GetFileInformationByHandleEx(handle, ValidationNative.FileStandardInfo, &info, (uint)sizeof(ValidationNative.FILE_STANDARD_INFO)))
                {
                    handleAllocated = info.AllocationSize;
                }
            }
        }

        long compressed = ValidationNative.CompressedSize(ValidationNative.Extended(path));
        (bool wof, _) = ValidationNative.WofState(ValidationNative.Extended(path));
        return new AllocationTrial(
            caseName,
            trial,
            logical,
            immediate,
            delta,
            entry.AllocationSize,
            handleAllocated,
            compressed,
            wof,
            $"0x{entry.Attributes:X} tag 0x{entry.ReparseTag:X8}",
            Math.Abs(entry.AllocationSize - delta) <= tolerance,
            handleAllocated >= 0 && Math.Abs(handleAllocated - delta) <= tolerance,
            compressed >= 0 && Math.Abs(compressed - delta) <= tolerance);
    }

    /// <summary>
    /// Free space after the filesystem has settled: on a disposable volume the volume cache is flushed first, then the
    /// reading must stay unchanged for 3 seconds (ReFS and NTFS compression account lazily).
    /// </summary>
    private static long Settled(string dir, string root, bool volumeRoot)
    {
        if (volumeRoot)
        {
            ValidationNative.FlushVolume(Path.GetPathRoot(Path.GetFullPath(root))!);
        }

        long value = StableFree(dir);
        for (int i = 0; i < 30; i++)
        {
            Thread.Sleep(3000);
            long again = StableFree(dir);
            if (again == value)
            {
                return value;
            }

            value = again;
        }

        return value;
    }

    /// <summary>Free space, read until two consecutive readings agree (some filesystems account lazily).</summary>
    private static long StableFree(string dir)
    {
        long previous = ValidationNative.TotalFreeBytes(dir);
        for (int i = 0; i < 50; i++)
        {
            Thread.Sleep(100);
            long current = ValidationNative.TotalFreeBytes(dir);
            if (current == previous)
            {
                return current;
            }

            previous = current;
        }

        throw new IOException("free space did not settle within 5 seconds");
    }

    private static long Write(string path, long size, byte[] noise, bool compressible)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        Fill(fs, size, noise, compressible);
        fs.Flush(flushToDisk: true);
        return size;
    }

    private static unsafe long Sparse(string path, long logical, byte[] noise, params (long Offset, int Length)[] data)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);
        if (!ValidationNative.DeviceIoControl(fs.SafeFileHandle, ValidationNative.FSCTL_SET_SPARSE, null, 0, null, 0, out _, 0))
        {
            throw new IOException($"FSCTL_SET_SPARSE failed ({Marshal.GetLastPInvokeError()})");
        }

        fs.SetLength(logical);
        foreach ((long offset, int length) in data)
        {
            fs.Position = offset;
            fs.Write(noise, 0, length);
        }

        fs.Flush(flushToDisk: true);
        return logical;
    }

    private static unsafe long Compressed(string path, int size, byte[] noise, bool compressible)
    {
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);
        ushort format = ValidationNative.COMPRESSION_FORMAT_DEFAULT;
        if (!ValidationNative.DeviceIoControl(fs.SafeFileHandle, ValidationNative.FSCTL_SET_COMPRESSION, &format, sizeof(ushort), null, 0, out _, 0))
        {
            throw new IOException($"FSCTL_SET_COMPRESSION failed ({Marshal.GetLastPInvokeError()})");
        }

        Fill(fs, size, noise, compressible);
        fs.Flush(flushToDisk: true);
        return size;
    }

    private static long Wof(string path, int size, byte[] noise, bool compressible)
    {
        Write(path, size, noise, compressible);
        var info = new ProcessStartInfo("compact.exe") { RedirectStandardOutput = true, UseShellExecute = false };
        info.ArgumentList.Add("/c");
        info.ArgumentList.Add("/exe:xpress4k");
        info.ArgumentList.Add(path);
        using Process p = Process.Start(info) ?? throw new IOException("cannot start compact.exe");
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0 || !ValidationNative.WofState(path).External)
        {
            throw new IOException($"WOF compaction unavailable: {output.Trim()}");
        }

        return size;
    }

    private static void Fill(FileStream fs, long size, byte[] noise, bool compressible)
    {
        byte[] text = "storage intelligence allocation harness "u8.ToArray();
        long remaining = size;
        while (remaining > 0)
        {
            int chunk = (int)Math.Min(remaining, compressible ? text.Length : noise.Length);
            fs.Write(compressible ? text : noise, 0, chunk);
            remaining -= chunk;
        }
    }
}
