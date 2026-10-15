using System.Diagnostics;
using StorageIntelligence.Accounting;

namespace StorageIntelligence.Bench;

/// <summary>
/// Keeps only what reconciliation needs (each directory's parent) and discards directory records, approximating the
/// planned SQLite-backed store. Used to separate the aggregator's intrinsic memory from the in-memory test store's.
/// </summary>
public sealed class ParentsOnlyStore : IScanSink, IParentLookup
{
    private readonly List<long> _parents = [];

    public long Directories { get; private set; }

    public void OnDirectoryCompleted(DirectoryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        while (_parents.Count <= record.Id.Value)
        {
            _parents.Add(-2);
        }

        _parents[(int)record.Id.Value] = record.Parent?.Value ?? -1;
        Directories++;
    }

    public void OnFileRetained(DirectoryId directory, FileObservation file, MeasurementCaveats effectiveFlags)
    {
    }

    public bool TryGetParent(DirectoryId directory, out DirectoryId parent)
    {
        long p = _parents[(int)directory.Value];
        parent = new DirectoryId(p);
        return p >= 0;
    }
}

public sealed record CoreBenchResult(
    BenchEnvironment Environment,
    string Store,
    long Entries,
    long Directories,
    long HardLinkEntries,
    double Seconds,
    double EntriesPerSecond,
    long BaselineWorkingSet,
    long BaselinePrivateBytes,
    long PeakWorkingSet,
    long PeakPrivateBytes,
    long PeakManagedHeap,
    long RunBytes,
    int RunCount,
    int PeakOpenDirectories,
    long AfterWorkingSet,
    long AfterPrivateBytes,
    long RootAllocated,
    long RootDistinctFiles,
    IReadOnlyList<(double Seconds, long WorkingSet, long PrivateBytes)> Timeline);

/// <summary>
/// Feeds a synthetic, deterministic directory tree (depth-first, as the low-impact scanner does) straight into the
/// accounting core: no disk I/O, so it measures the core's own memory and CPU cost and runs on any OS.
/// Tree: three directory levels with fan-out chosen so leaves hold 50 files each; 1 % of files get a second hard
/// link in a sibling directory.
/// </summary>
public static class CoreBench
{
    public static CoreBenchResult Run(long entries, string store, string tempDirectory)
    {
        const int filesPerLeaf = 50;
        long leaves = Math.Max(1, entries / filesPerLeaf);
        int fanout = Math.Max(2, (int)Math.Ceiling(Math.Cbrt(leaves)));

        MemorySnapshot baseline = MemorySnapshot.AfterFullCollection();
        using var sampler = new MemorySampler();
        var clock = Stopwatch.StartNew();

        IScanSink sink;
        IParentLookup parents;
        if (store == "full")
        {
            var memory = new InMemoryScanStore();
            sink = memory;
            parents = memory;
        }
        else
        {
            var lean = new ParentsOnlyStore();
            sink = lean;
            parents = lean;
        }

        var policy = new MeasurementPolicy { FileSystem = FileSystemKind.Ntfs, FileSystemValidated = true };
        using var runs = new TempFileRunStore(tempDirectory);
        var writer = new HardLinkRunWriter(runs);
        var aggregator = new ScanAggregator(new AggregatorOptions { Policy = policy }, sink, writer);
        var random = new Random(1);
        long emitted = 0, linkEntries = 0;
        ulong nextId = 1;

        DirectoryId root = aggregator.BeginRoot("bench");
        for (int a = 0; a < fanout && emitted < entries; a++)
        {
            DirectoryId da = aggregator.DiscoverDirectory(root, $"a{a}");
            for (int b = 0; b < fanout && emitted < entries; b++)
            {
                DirectoryId db = aggregator.DiscoverDirectory(da, $"b{b}");
                for (int c = 0; c < fanout && emitted < entries; c++)
                {
                    DirectoryId dc = aggregator.DiscoverDirectory(db, $"c{c}");
                    for (int f = 0; f < filesPerLeaf && emitted < entries; f++)
                    {
                        ulong id = nextId++;
                        long size = random.Next(0, 3) == 0 ? 0 : random.Next(1, 200_000);
                        var file = new FileObservation($"f{f}.dat", new FileId128(0, id), size, (size + 4095) / 4096 * 4096);
                        aggregator.AddFile(dc, file);
                        emitted++;
                        if (id % 100 == 0 && emitted < entries)
                        {
                            aggregator.AddFile(dc, file with { Name = $"f{f}-link.dat" });
                            emitted++;
                            linkEntries++;
                        }
                    }

                    aggregator.EndListing(dc);
                }

                aggregator.EndListing(db);
            }

            aggregator.EndListing(da);
        }

        aggregator.EndListing(root);
        AggregationOutcome outcome = aggregator.Complete();
        long runBytes = writer.BytesWritten; // before merging: merged runs are deleted
        ReconciliationResult reconciliation = HardLinkReconciler.Reconcile(writer.EnumerateGroups(), parents, policy, outcome.Extensions);
        reconciliation.Adjustments.TryGetValue(outcome.Root.Id, out DirectoryAdjustment rootAdjustment);
        DirectoryTotals rootTotals = DirectoryTotals.From(outcome.Root, rootAdjustment, HardLinkStatus.Reconciled);
        clock.Stop();
        sampler.Dispose();

        long directories = sink is InMemoryScanStore full ? full.Records.Count : ((ParentsOnlyStore)sink).Directories;
        GC.KeepAlive(sink);
        MemorySnapshot after = MemorySnapshot.AfterFullCollection();
        return new CoreBenchResult(
            BenchEnvironment.Capture(),
            store,
            emitted,
            directories,
            linkEntries,
            clock.Elapsed.TotalSeconds,
            emitted / clock.Elapsed.TotalSeconds,
            baseline.WorkingSet,
            baseline.PrivateBytes,
            sampler.PeakWorkingSet,
            sampler.PeakPrivateBytes,
            sampler.PeakManagedHeap,
            runBytes,
            writer.RunCount,
            outcome.PeakOpenDirectories,
            after.WorkingSet,
            after.PrivateBytes,
            rootTotals.Size.Allocated,
            rootTotals.DistinctFiles,
            sampler.Timeline);
    }
}
