namespace StorageIntelligence.Accounting;

/// <summary>Whether hard links have been reconciled for a set of totals.</summary>
public enum HardLinkStatus
{
    /// <summary>Totals count each directory entry; files with several links in a subtree are over-counted.</summary>
    NotReconciled,

    /// <summary>Totals count each distinct file once per subtree.</summary>
    Reconciled,

    /// <summary>The filesystem has no hard links (or file identity), so entry counts are already distinct.</summary>
    NotApplicable,
}

/// <summary>
/// Final totals for one directory subtree. See docs/accounting-semantics.md.
/// Size is split three ways by what is known about each file's hard links:
/// <see cref="Shared"/> (known to have a link outside the subtree), <see cref="ExclusiveVerified"/> (link count
/// measured and every link inside the subtree, or no hard links possible) and <see cref="LinkStatusUnknown"/>
/// (every observed link is inside, but the complete link count was not measured).
/// <list type="bullet">
/// <item><see cref="Size"/> counts each distinct file in the subtree once. Because a file can have links in
/// several sibling subtrees, sibling totals are not additive: a parent's total can be less than the sum of its
/// children's totals plus its own files.</item>
/// <item><see cref="Shared"/> is the part of <see cref="Size"/> belonging to files that also have a link outside
/// this subtree (within the scanned volume, or outside the scan where link counts were measured).</item>
/// <item><see cref="ExclusiveVerified"/>: bytes proven reachable only through this subtree. Verified-exclusive
/// totals of disjoint sibling subtrees are additive.</item>
/// </list>
/// </summary>
public sealed record DirectoryTotals(
    DirectoryId Id,
    DirectoryId? Parent,
    string Name,
    int Depth,
    SizeVector Size,
    SizeVector Shared,
    SizeVector ExclusiveVerified,
    long FileEntries,
    long DistinctFiles,
    long SharedFiles,
    long ExclusiveVerifiedFiles,
    long Directories,
    CoverageCounts Coverage,
    HardLinkStatus HardLinks)
{
    /// <summary>Bytes whose files have every observed link inside the subtree but no measured link count.</summary>
    public SizeVector LinkStatusUnknown => Size - Shared - ExclusiveVerified;

    public long LinkStatusUnknownFiles => DistinctFiles - SharedFiles - ExclusiveVerifiedFiles;

    /// <summary>True when totals are exact up to the stated uncertain bytes: complete coverage and links reconciled.</summary>
    public bool IsComplete => Coverage.IsComplete && HardLinks != HardLinkStatus.NotReconciled;

    public static DirectoryTotals From(DirectoryRecord record, DirectoryAdjustment adjustment, HardLinkStatus status)
    {
        ArgumentNullException.ThrowIfNull(record);
        return new DirectoryTotals(
            record.Id,
            record.Parent,
            record.Name,
            record.Depth,
            record.Subtree + adjustment.Delta,
            adjustment.Shared,
            record.SubtreeVerified + adjustment.VerifiedDelta,
            record.SubtreeFileEntries,
            record.SubtreeFileEntries + adjustment.DistinctFilesDelta,
            adjustment.SharedFiles,
            record.SubtreeVerifiedFiles + adjustment.VerifiedFilesDelta,
            record.SubtreeDirectories,
            record.SubtreeCoverage,
            status);
    }
}

/// <summary>
/// Keeps every directory record in memory. Suitable for tests, the reference implementation and small scans;
/// the production store will be SQLite-backed with the same contract.
/// </summary>
public sealed class InMemoryScanStore : IScanSink, IParentLookup
{
    private readonly Dictionary<DirectoryId, DirectoryRecord> _records = [];
    private readonly List<(DirectoryId Directory, FileObservation File, MeasurementCaveats Flags)> _retained = [];

    public IReadOnlyDictionary<DirectoryId, DirectoryRecord> Records => _records;

    public IReadOnlyList<(DirectoryId Directory, FileObservation File, MeasurementCaveats Flags)> RetainedFiles => _retained;

    /// <summary>Order in which directories were finalized (post-order).</summary>
    public List<DirectoryId> CompletionOrder { get; } = [];

    public void OnDirectoryCompleted(DirectoryRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!_records.TryAdd(record.Id, record))
        {
            throw new InvalidOperationException($"Directory {record.Id} was completed twice.");
        }

        CompletionOrder.Add(record.Id);
    }

    public void OnFileRetained(DirectoryId directory, FileObservation file, MeasurementCaveats effectiveFlags) =>
        _retained.Add((directory, file, effectiveFlags));

    public bool TryGetParent(DirectoryId directory, out DirectoryId parent)
    {
        if (_records.TryGetValue(directory, out DirectoryRecord? record) && record.Parent is DirectoryId p)
        {
            parent = p;
            return true;
        }

        if (!_records.ContainsKey(directory))
        {
            throw new KeyNotFoundException($"Directory {directory} has no record.");
        }

        parent = default;
        return false;
    }

    public DirectoryTotals GetTotals(DirectoryId id, ReconciliationResult? reconciliation, HardLinkStatus status)
    {
        DirectoryAdjustment adjustment = default;
        reconciliation?.Adjustments.TryGetValue(id, out adjustment);
        return DirectoryTotals.From(_records[id], adjustment, status);
    }

    public IEnumerable<DirectoryTotals> GetAllTotals(ReconciliationResult? reconciliation, HardLinkStatus status) =>
        _records.Keys.Select(id => GetTotals(id, reconciliation, status));
}

/// <summary>Whole-scan result: aggregation, reconciliation and the final per-extension totals.</summary>
public sealed record ScanAccounting(
    AggregationOutcome Aggregation,
    ReconciliationResult? Reconciliation,
    HardLinkStatus HardLinks,
    DirectoryTotals Root,
    IReadOnlyDictionary<string, ExtensionStats> ExtensionTotals)
{
    /// <summary>Runs reconciliation (when a run writer was used) and assembles the final result.</summary>
    public static ScanAccounting Build(
        AggregationOutcome aggregation,
        InMemoryScanStore store,
        HardLinkRunWriter? links,
        MeasurementPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(aggregation);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(policy);

        ReconciliationResult? reconciliation = null;
        HardLinkStatus status;
        if (links is not null)
        {
            reconciliation = HardLinkReconciler.Reconcile(links.EnumerateGroups(), store, policy, aggregation.Extensions);
            status = HardLinkStatus.Reconciled;
        }
        else
        {
            status = policy.FileSystemSupportsHardLinks ? HardLinkStatus.NotReconciled : HardLinkStatus.NotApplicable;
        }

        var extensions = new Dictionary<string, ExtensionStats>(StringComparer.Ordinal);
        foreach ((int id, ExtensionStats stats) in aggregation.ExtensionTotals)
        {
            extensions[aggregation.Extensions.NameOf(id)] = stats;
        }

        if (reconciliation is not null)
        {
            foreach ((int id, ExtensionStats delta) in reconciliation.ExtensionDeltas)
            {
                string name = aggregation.Extensions.NameOf(id);
                extensions[name] = extensions.GetValueOrDefault(name) + delta;
            }
        }

        DirectoryTotals root = store.GetTotals(aggregation.Root.Id, reconciliation, status);
        return new ScanAccounting(aggregation, reconciliation, status, root, extensions);
    }
}
