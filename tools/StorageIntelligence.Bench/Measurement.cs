using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace StorageIntelligence.Bench;

/// <summary>Process memory at one moment. Working set is resident memory; private bytes is committed private memory.</summary>
public readonly record struct MemorySnapshot(long WorkingSet, long PrivateBytes, long ManagedHeap)
{
    public static MemorySnapshot Now()
    {
        using Process p = Process.GetCurrentProcess();
        return new MemorySnapshot(p.WorkingSet64, p.PrivateMemorySize64, GC.GetTotalMemory(forceFullCollection: false));
    }

    /// <summary>Snapshot after a full blocking collection and compaction (the "idle" state).</summary>
    public static MemorySnapshot AfterFullCollection()
    {
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        return Now();
    }
}

/// <summary>Samples process memory on a background thread and keeps the maxima and a coarse timeline.</summary>
public sealed class MemorySampler : IDisposable
{
    private readonly Thread _thread;
    private readonly int _intervalMs;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly List<(double Seconds, long WorkingSet, long PrivateBytes)> _timeline = [];
    private volatile bool _stop;

    public MemorySampler(int intervalMs = 50)
    {
        _intervalMs = intervalMs;
        _thread = new Thread(Run) { IsBackground = true, Name = "si-bench-memory", Priority = ThreadPriority.AboveNormal };
        _thread.Start();
    }

    public long PeakWorkingSet { get; private set; }

    public long PeakPrivateBytes { get; private set; }

    public long PeakManagedHeap { get; private set; }

    public int Samples { get; private set; }

    /// <summary>At most ~200 points, evenly thinned, for plotting memory over a scan.</summary>
    public IReadOnlyList<(double Seconds, long WorkingSet, long PrivateBytes)> Timeline
    {
        get
        {
            lock (_timeline)
            {
                int step = Math.Max(1, _timeline.Count / 200);
                return _timeline.Where((_, i) => i % step == 0).ToList();
            }
        }
    }

    public void Dispose()
    {
        _stop = true;
        _thread.Join();
        Sample();
    }

    private void Run()
    {
        while (!_stop)
        {
            Sample();
            Thread.Sleep(_intervalMs);
        }
    }

    private void Sample()
    {
        MemorySnapshot s = MemorySnapshot.Now();
        PeakWorkingSet = Math.Max(PeakWorkingSet, s.WorkingSet);
        PeakPrivateBytes = Math.Max(PeakPrivateBytes, s.PrivateBytes);
        PeakManagedHeap = Math.Max(PeakManagedHeap, s.ManagedHeap);
        Samples++;
        lock (_timeline)
        {
            _timeline.Add((_clock.Elapsed.TotalSeconds, s.WorkingSet, s.PrivateBytes));
        }
    }
}

/// <summary>Order statistics over a sample of latencies (milliseconds).</summary>
public sealed record LatencyStats(int Count, double Min, double P50, double P95, double P99, double Max, double Mean)
{
    public static LatencyStats From(IReadOnlyCollection<double> values)
    {
        if (values.Count == 0)
        {
            return new LatencyStats(0, 0, 0, 0, 0, 0, 0);
        }

        double[] sorted = values.Order().ToArray();
        return new LatencyStats(
            sorted.Length,
            sorted[0],
            Percentile(sorted, 0.50),
            Percentile(sorted, 0.95),
            Percentile(sorted, 0.99),
            sorted[^1],
            sorted.Average());
    }

    private static double Percentile(double[] sorted, double p) =>
        sorted[Math.Min(sorted.Length - 1, (int)Math.Ceiling(p * sorted.Length) - 1)];
}

/// <summary>Describes where a measurement was taken. Every result carries one.</summary>
public sealed record BenchEnvironment(
    string Machine,
    string Kind,
    string Os,
    int LogicalProcessors,
    string Processor,
    long PhysicalMemoryBytes,
    string Runtime,
    string GcMode,
    long ProcessAffinityMask,
    string Notes)
{
    public static BenchEnvironment Capture()
    {
        string kind = System.Environment.GetEnvironmentVariable("SI_BENCH_KIND")
            ?? (System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ? "CI (GitHub-hosted VM)" : "unspecified");
        using Process p = Process.GetCurrentProcess();
        long affinity = OperatingSystem.IsWindows() ? (long)p.ProcessorAffinity : -1;
        return new BenchEnvironment(
            System.Environment.MachineName,
            kind,
            RuntimeInformation.OSDescription,
            System.Environment.ProcessorCount,
            System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? RuntimeInformation.ProcessArchitecture.ToString(),
            GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            RuntimeInformation.FrameworkDescription,
            $"{(System.Runtime.GCSettings.IsServerGC ? "server" : "workstation")}, latency {System.Runtime.GCSettings.LatencyMode}",
            affinity,
            System.Environment.GetEnvironmentVariable("SI_BENCH_NOTES") ?? "");
    }
}

/// <summary>Benchmark-only native calls. None of these are reachable from the scanner.</summary>
internal static unsafe partial class BenchNative
{
    internal const uint GENERIC_READ = 0x80000000;
    internal const uint FILE_SHARE_READ = 0x1;
    internal const uint OPEN_EXISTING = 3;
    internal const uint FILE_FLAG_NO_BUFFERING = 0x20000000;

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial SafeFileHandle CreateFile(string name, uint access, uint share, nint sa, uint disposition, uint flags, nint template);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateHardLinkW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CreateHardLink(string newLink, string existing, nint sa);

    /// <summary>Opens the benchmark's own probe file for unbuffered reads, so every probe read reaches the disk.</summary>
    internal static SafeFileHandle OpenUnbuffered(string path)
    {
        SafeFileHandle handle = CreateFile(path, GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, FILE_FLAG_NO_BUFFERING, 0);
        return handle.IsInvalid ? throw new IOException($"cannot open probe file {path} ({Marshal.GetLastPInvokeError()})") : handle;
    }
}
