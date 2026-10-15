using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using StorageIntelligence.Accounting;
using StorageIntelligence.Scanning.Windows;
using StorageIntelligence.Validation;

namespace StorageIntelligence.Bench;

public sealed record FixtureResult(BenchEnvironment Environment, string Root, long Files, long Directories, long HardLinks, long BytesWritten, double Seconds);

public sealed record ScanBenchResult(
    BenchEnvironment Environment,
    string Root,
    string Mode,
    string LinkCounts,
    string CacheState,
    double Seconds,
    long Entries,
    long Directories,
    double EntriesPerSecond,
    long BaselineWorkingSet,
    long BaselinePrivateBytes,
    long PeakWorkingSet,
    long PeakPrivateBytes,
    long PeakManagedHeap,
    long AfterWorkingSet,
    long AfterPrivateBytes,
    int PeakOpenDirectories,
    long LinkRecords,
    string HardLinks,
    bool Complete,
    IReadOnlyList<(double Seconds, long WorkingSet, long PrivateBytes)> Timeline);

public sealed record CancelTrial(int DelayMs, double LatencyMs, bool Cancelled, long DirectoriesCancelled);

public sealed record CancelBenchResult(BenchEnvironment Environment, string Mode, double FullScanSeconds, LatencyStats Latency, List<CancelTrial> Trials);

public sealed record ProbeResult(BenchEnvironment Environment, string Label, double Seconds, LatencyStats ReadLatency, LatencyStats TimerLateness, long Reads);

public sealed record StartupResult(BenchEnvironment Environment, double ProcessStartToMainMs, double MainToReadyMs, double ProcessStartToReadyMs, long ReadyWorkingSet, long ReadyPrivateBytes, long IdleWorkingSet, long IdlePrivateBytes);

public static class WindowsBench
{
    /// <summary>
    /// Creates a large synthetic tree under <c>root\bench</c> (guarded by <see cref="TestRootGuard"/>): three directory
    /// levels, 50 files per leaf, 70 % empty files and 30 % of 1–4096 bytes, and a second hard link for 1 % of files.
    /// </summary>
    public static FixtureResult Fixture(string root, long files)
    {
        TestRootGuard.AssertWritable(root);
        string bench = Path.Combine(Path.GetFullPath(root), "bench");
        if (Directory.Exists(bench))
        {
            throw new InvalidOperationException("bench fixture already exists");
        }

        const int filesPerLeaf = 50;
        long leaves = Math.Max(1, files / filesPerLeaf);
        int fanout = Math.Max(2, (int)Math.Ceiling(Math.Cbrt(leaves)));
        var clock = Stopwatch.StartNew();
        long created = 0, links = 0, bytes = 0, directories = 0;
        byte[] data = new byte[4096];
        new Random(5).NextBytes(data);

        Parallel.For(0, fanout, new ParallelOptions { MaxDegreeOfParallelism = 4 }, a =>
        {
            var random = new Random(a);
            for (int b = 0; b < fanout; b++)
            {
                for (int c = 0; c < fanout; c++)
                {
                    long leaf = ((long)a * fanout + b) * fanout + c;
                    if (leaf * filesPerLeaf >= files)
                    {
                        return;
                    }

                    string dir = Path.Combine(bench, $"a{a}", $"b{b}", $"c{c}");
                    Directory.CreateDirectory(dir);
                    Interlocked.Increment(ref directories);
                    for (int f = 0; f < filesPerLeaf && leaf * filesPerLeaf + f < files; f++)
                    {
                        string path = Path.Combine(dir, $"f{f:D2}.dat");
                        using (SafeFileHandle h = File.OpenHandle(path, FileMode.CreateNew, FileAccess.Write))
                        {
                            if (random.Next(10) < 3)
                            {
                                int length = random.Next(1, 4097);
                                RandomAccess.Write(h, data.AsSpan(0, length), 0);
                                Interlocked.Add(ref bytes, length);
                            }
                        }

                        long n = Interlocked.Increment(ref created);
                        if (n % 100 == 0)
                        {
                            if (!BenchNative.CreateHardLink(Path.Combine(dir, $"f{f:D2}-link.dat"), path, 0))
                            {
                                throw new IOException($"CreateHardLink failed ({System.Runtime.InteropServices.Marshal.GetLastPInvokeError()})");
                            }

                            Interlocked.Increment(ref links);
                        }

                        if (n % 250_000 == 0)
                        {
                            Console.Error.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] fixture: {n} files");
                        }
                    }
                }
            }
        });

        return new FixtureResult(BenchEnvironment.Capture(), bench, created, directories, links, bytes, clock.Elapsed.TotalSeconds);
    }

    /// <summary>Creates the foreground probe's own data file (random bytes) under a guarded root.</summary>
    public static void ProbeFile(string root, long megabytes)
    {
        TestRootGuard.AssertWritable(root);
        string path = Path.Combine(Path.GetFullPath(root), "probe.bin");
        byte[] block = new byte[1 << 20];
        new Random(9).NextBytes(block);
        using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        for (long i = 0; i < megabytes; i++)
        {
            fs.Write(block);
        }

        fs.Flush(flushToDisk: true);
    }

    public static ScanBenchResult Scan(string root, ScanMode mode, LinkCountMeasurement links, string cacheState, string temp)
    {
        MemorySnapshot baseline = MemorySnapshot.AfterFullCollection();
        var sampler = new MemorySampler();
        ScanResult? result = WindowsScanner.Scan(root, new ScanOptions { Mode = mode, LinkCounts = links, TempDirectory = temp });
        sampler.Dispose();

        DirectoryTotals totals = result.Accounting.Root;
        long entries = totals.FileEntries;
        long directories = totals.Directories + 1;
        int peakOpen = result.Accounting.Aggregation.PeakOpenDirectories;
        string hardLinks = $"{result.Accounting.HardLinks} {result.Accounting.ReconciliationFailure}";
        bool complete = totals.IsComplete;
        long linkRecords = result.Accounting.Aggregation.LinkRecordsWritten;
        double seconds = result.Duration.TotalSeconds;
        result = null; // release everything the scan produced before measuring the post-scan state
        MemorySnapshot after = MemorySnapshot.AfterFullCollection();
        return new ScanBenchResult(
            BenchEnvironment.Capture(), root, mode.ToString(), links.ToString(), cacheState, seconds, entries, directories,
            entries / seconds, baseline.WorkingSet, baseline.PrivateBytes, sampler.PeakWorkingSet, sampler.PeakPrivateBytes,
            sampler.PeakManagedHeap, after.WorkingSet, after.PrivateBytes, peakOpen, linkRecords, hardLinks, complete, sampler.Timeline);
    }

    /// <summary>Cancels scans at random points and measures the time from Cancel() to the scan returning.</summary>
    public static CancelBenchResult Cancel(string root, ScanMode mode, int trials, string temp, int seed)
    {
        var options = new ScanOptions { Mode = mode, TempDirectory = temp };
        var full = Stopwatch.StartNew();
        _ = WindowsScanner.Scan(root, options);
        double fullSeconds = full.Elapsed.TotalSeconds;

        var random = new Random(seed);
        var results = new List<CancelTrial>();
        for (int i = 0; i < trials; i++)
        {
            int delay = random.Next(50, Math.Max(100, (int)(fullSeconds * 1000 * 0.8)));
            using var cts = new CancellationTokenSource();
            var latency = new Stopwatch();
            Task<ScanResult> scan = Task.Run(() => WindowsScanner.Scan(root, options, cts.Token));
            Thread.Sleep(delay);
            latency.Start();
            cts.Cancel();
            ScanResult r = scan.GetAwaiter().GetResult();
            latency.Stop();
            results.Add(new CancelTrial(
                delay,
                latency.Elapsed.TotalMilliseconds,
                r.Accounting.Aggregation.Completion == ScanCompletion.Cancelled,
                r.Accounting.Root.Coverage.CancelledDirectories));
        }

        return new CancelBenchResult(
            BenchEnvironment.Capture(),
            mode.ToString(),
            fullSeconds,
            LatencyStats.From(results.Where(t => t.Cancelled).Select(t => t.LatencyMs).ToList()),
            results);
    }

    /// <summary>
    /// Foreground responsiveness probe, run as its own process: 4 KiB unbuffered random reads of the probe file at
    /// queue depth 1 (disk latency an application would see) and 1 ms timer lateness (scheduling delay).
    /// Runs for a fixed time, or until another process exits.
    /// </summary>
    public static unsafe ProbeResult Probe(string probeFile, string label, double seconds, int untilPid)
    {
        using SafeFileHandle file = BenchNative.OpenUnbuffered(probeFile);
        long blocks = RandomAccess.GetLength(file) / 4096;
        byte* buffer = (byte*)System.Runtime.InteropServices.NativeMemory.AlignedAlloc(4096, 4096);
        var reads = new List<double>(100_000);
        var lateness = new List<double>(100_000);
        Process? watched = untilPid > 0 ? Process.GetProcessById(untilPid) : null;
        var stop = Stopwatch.StartNew();
        bool Running() => watched is null ? stop.Elapsed.TotalSeconds < seconds : !watched.HasExited;

        var timer = new Thread(() =>
        {
            var sw = new Stopwatch();
            while (Running())
            {
                sw.Restart();
                Thread.Sleep(1);
                lateness.Add(sw.Elapsed.TotalMilliseconds - 1);
            }
        }) { IsBackground = true };
        timer.Start();

        var random = new Random(3);
        var sw2 = new Stopwatch();
        try
        {
            while (Running())
            {
                long offset = random.NextInt64(blocks) * 4096;
                sw2.Restart();
                RandomAccess.Read(file, new Span<byte>(buffer, 4096), offset);
                reads.Add(sw2.Elapsed.TotalMilliseconds);
            }
        }
        finally
        {
            System.Runtime.InteropServices.NativeMemory.AlignedFree(buffer);
        }

        timer.Join();
        return new ProbeResult(BenchEnvironment.Capture(), label, stop.Elapsed.TotalSeconds, LatencyStats.From(reads), LatencyStats.From(lateness), reads.Count);
    }

    /// <summary>
    /// Startup: process start → Main, and Main → ready, where ready means the scanner has completed one scan of the
    /// application's own (small) directory, so every code path is loaded and JIT-compiled. Then 5 s of idling.
    /// This is the console host only; no UI is involved.
    /// </summary>
    public static StartupResult Startup(DateTime mainEntered)
    {
        DateTime processStart = Process.GetCurrentProcess().StartTime;
        _ = WindowsScanner.Scan(AppContext.BaseDirectory, new ScanOptions { LinkCounts = LinkCountMeasurement.None });
        DateTime ready = DateTime.Now;
        MemorySnapshot readyMemory = MemorySnapshot.Now();
        Thread.Sleep(5000);
        MemorySnapshot idle = MemorySnapshot.AfterFullCollection();
        return new StartupResult(
            BenchEnvironment.Capture(),
            (mainEntered - processStart).TotalMilliseconds,
            (ready - mainEntered).TotalMilliseconds,
            (ready - processStart).TotalMilliseconds,
            readyMemory.WorkingSet,
            readyMemory.PrivateBytes,
            idle.WorkingSet,
            idle.PrivateBytes);
    }
}
