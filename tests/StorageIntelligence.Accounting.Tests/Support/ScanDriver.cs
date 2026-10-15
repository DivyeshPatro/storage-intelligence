using StorageIntelligence.Accounting;

namespace StorageIntelligence.Accounting.Tests.Support;

/// <summary>What the driver actually reported to the aggregator. The oracle works from this log.</summary>
public sealed class EmissionLog
{
    public Dictionary<int, DirectoryId> Discovered { get; } = [];
    public HashSet<int> Ended { get; } = [];
    public HashSet<int> Inaccessible { get; } = [];
    public HashSet<int> Changed { get; } = [];
    public HashSet<int> Incomplete { get; } = [];
    public List<int> EmittedFiles { get; } = [];
    public Dictionary<int, int> EntryErrors { get; } = [];
    public Dictionary<int, int> Links { get; } = [];
    public bool Cancelled { get; set; }
}

public sealed record DriverResult(
    TreeModel Model,
    MeasurementPolicy Policy,
    EmissionLog Log,
    InMemoryScanStore Store,
    ScanAccounting Accounting)
{
    public DirectoryTotals TotalsFor(int modelDir) =>
        Store.GetTotals(Log.Discovered[modelDir], Accounting.Reconciliation, Accounting.HardLinks);
}

public enum TraversalOrder
{
    /// <summary>Any interleaving of events across directories (models several producers).</summary>
    Random,

    /// <summary>List one directory completely, then descend into the most recently discovered one.</summary>
    DepthFirst,
}

/// <summary>Feeds a <see cref="TreeModel"/> to a <see cref="ScanAggregator"/> in a chosen order.</summary>
public static class ScanDriver
{
    private abstract record Entry;
    private sealed record SubdirEntry(int Dir) : Entry;
    private sealed record FileEntry(int File) : Entry;
    private sealed record LinkEntry : Entry;
    private sealed record ErrorEntry : Entry;

    public static DriverResult Run(
        TreeModel model,
        MeasurementPolicy policy,
        int orderSeed,
        TraversalOrder order = TraversalOrder.Random,
        int runBufferCapacity = 4,
        int? cancelAfterSteps = null,
        IRunStore? runStore = null,
        bool reconcile = true)
    {
        var random = new Random(orderSeed);
        var store = new InMemoryScanStore();
        using IRunStore runs = runStore ?? new InMemoryRunStore();
        HardLinkRunWriter? writer = reconcile && policy.FileSystemSupportsHardLinks
            ? new HardLinkRunWriter(runs, runBufferCapacity)
            : null;
        var aggregator = new ScanAggregator(
            new AggregatorOptions { Policy = policy, RetainFileThreshold = 1_000_000 }, store, writer);
        var log = new EmissionLog();

        var filesByDir = model.Files.Select((f, i) => (f, i)).ToLookup(x => x.f.Dir, x => x.i);
        var pending = new List<int>();
        var active = new List<(int Dir, Queue<Entry> Entries)>();

        log.Discovered[0] = aggregator.BeginRoot("root");
        pending.Add(0);

        int steps = 0;
        while (pending.Count > 0 || active.Count > 0)
        {
            if (cancelAfterSteps is int limit && steps++ >= limit)
            {
                log.Cancelled = true;
                break;
            }

            bool startNew = order == TraversalOrder.DepthFirst
                ? active.Count == 0
                : pending.Count > 0 && (active.Count == 0 || random.Next(2) == 0);

            if (startNew)
            {
                int index = order == TraversalOrder.DepthFirst ? pending.Count - 1 : random.Next(pending.Count);
                int dir = pending[index];
                pending.RemoveAt(index);
                Start(dir);
                continue;
            }

            int slot = order == TraversalOrder.DepthFirst ? active.Count - 1 : random.Next(active.Count);
            (int activeDir, Queue<Entry> entries) = active[slot];
            DirectoryId id = log.Discovered[activeDir];
            if (entries.Count == 0)
            {
                if (model.Dirs[activeDir].TruncateAt is not null)
                {
                    aggregator.MarkIncomplete(id);
                    log.Incomplete.Add(activeDir);
                }

                aggregator.EndListing(id);
                log.Ended.Add(activeDir);
                active.RemoveAt(slot);
                continue;
            }

            switch (entries.Dequeue())
            {
                case SubdirEntry s:
                    log.Discovered[s.Dir] = aggregator.DiscoverDirectory(id, $"d{s.Dir}");
                    pending.Add(s.Dir);
                    break;
                case FileEntry f:
                    FileSpec spec = model.Files[f.File];
                    aggregator.AddFile(id, new FileObservation(
                        spec.Name,
                        spec.Key is int key ? TreeModel.IdFor(key) : null,
                        spec.Logical,
                        spec.Allocated,
                        spec.Caveats,
                        spec.LinkCount));
                    log.EmittedFiles.Add(f.File);
                    break;
                case LinkEntry:
                    aggregator.AddNotFollowedLink(id, "link", LinkKind.Junction);
                    log.Links[activeDir] = log.Links.GetValueOrDefault(activeDir) + 1;
                    break;
                case ErrorEntry:
                    aggregator.RecordEntryError(id);
                    log.EntryErrors[activeDir] = log.EntryErrors.GetValueOrDefault(activeDir) + 1;
                    break;
            }
        }

        AggregationOutcome outcome = log.Cancelled ? aggregator.Cancel() : aggregator.Complete();
        ScanAccounting accounting = ScanAccounting.Build(outcome, store, writer, policy);
        return new DriverResult(model, policy, log, store, accounting);

        void Start(int dir)
        {
            DirSpec spec = model.Dirs[dir];
            DirectoryId id = log.Discovered[dir];
            if (spec.Changed)
            {
                aggregator.MarkChanged(id);
                log.Changed.Add(dir);
            }

            if (spec.Inaccessible)
            {
                aggregator.MarkInaccessible(id);
                log.Inaccessible.Add(dir);
                aggregator.EndListing(id);
                log.Ended.Add(dir);
                return;
            }

            var entries = new List<Entry>();
            entries.AddRange(model.ChildrenOf(dir).Select(c => (Entry)new SubdirEntry(c)));
            entries.AddRange(filesByDir[dir].Select(i => (Entry)new FileEntry(i)));
            entries.AddRange(Enumerable.Range(0, spec.NotFollowedLinks).Select(_ => (Entry)new LinkEntry()));
            entries.AddRange(Enumerable.Range(0, spec.EntryErrors).Select(_ => (Entry)new ErrorEntry()));
            random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(entries));
            if (spec.TruncateAt is double fraction)
            {
                entries = entries.Take((int)(entries.Count * fraction)).ToList();
            }

            active.Add((dir, new Queue<Entry>(entries)));
        }
    }
}
