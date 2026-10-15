namespace StorageIntelligence.Accounting;

/// <summary>Options for <see cref="ScanAggregator"/>.</summary>
public sealed record AggregatorOptions
{
    public MeasurementPolicy Policy { get; init; } = MeasurementPolicy.Conservative;

    /// <summary>Files with at least this many allocated bytes are passed to <see cref="IScanSink.OnFileRetained"/>.</summary>
    public long RetainFileThreshold { get; init; } = 10L * 1024 * 1024;

    /// <summary>Maximum distinct extensions tracked before new ones are grouped as "(other)".</summary>
    public int MaxExtensions { get; init; } = 4096;
}

/// <summary>How a scan ended.</summary>
public enum ScanCompletion
{
    Completed,
    Cancelled,
}

/// <summary>What the aggregator knows once the scan ends, before hard-link reconciliation.</summary>
public sealed record AggregationOutcome(
    ScanCompletion Completion,
    DirectoryRecord Root,
    ExtensionTable Extensions,
    IReadOnlyDictionary<int, ExtensionStats> ExtensionTotals,
    IReadOnlyDictionary<MeasurementCaveats, long> FileEntriesByFlag,
    long LinkRecordsWritten,
    int PeakOpenDirectories);

/// <summary>
/// Streams scanner events into per-directory totals with memory bounded by the number of directories that
/// are discovered but not yet finished. A directory is finalized (emitted to the sink and forgotten) as soon as
/// its own listing has ended and all its subdirectories are finalized. Event order across directories is free,
/// so any traversal order and any number of producers (serialized onto one thread) give the same result.
/// </summary>
/// <remarks>Not thread-safe. Feed it from a single consumer thread.</remarks>
public sealed class ScanAggregator
{
    private readonly AggregatorOptions _options;
    private readonly IScanSink _sink;
    private readonly HardLinkRunWriter? _links;
    private readonly Dictionary<long, OpenDirectory> _open = [];
    private readonly ExtensionTable _extensions;
    private readonly Dictionary<int, ExtensionStats> _extensionTotals = [];
    private readonly Dictionary<MeasurementCaveats, long> _entriesByFlag = [];
    private long _nextId;
    private DirectoryRecord? _rootRecord;
    private bool _ended;
    private int _peakOpen;

    /// <param name="links">Receives a record for every file with an ID. Null disables reconciliation
    /// (only valid for filesystems without hard links).</param>
    public ScanAggregator(AggregatorOptions options, IScanSink sink, HardLinkRunWriter? links)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(sink);
        _options = options;
        _sink = sink;
        _links = links;
        _extensions = new ExtensionTable(options.MaxExtensions);
    }

    public int OpenDirectoryCount => _open.Count;

    public int PeakOpenDirectories => _peakOpen;

    /// <summary>Starts the scan at its root directory. Must be called exactly once, first.</summary>
    public DirectoryId BeginRoot(string name)
    {
        EnsureRunning();
        if (_nextId != 0)
        {
            throw new InvalidOperationException("The root has already been started.");
        }

        return Open(parent: null, name, depth: 0);
    }

    /// <summary>Records a subdirectory found while listing <paramref name="parent"/>. It must later be listed (or marked inaccessible) and ended.</summary>
    public DirectoryId DiscoverDirectory(DirectoryId parent, string name)
    {
        OpenDirectory p = GetListing(parent);
        p.PendingChildren++;
        return Open(parent, name, p.Depth + 1);
    }

    public void AddFile(DirectoryId directory, in FileObservation file)
    {
        OpenDirectory d = GetListing(directory);
        if (file.LogicalSize < 0 || file.AllocatedSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(file), "Sizes cannot be negative.");
        }

        MeasurementCaveats flags = _options.Policy.Effective(file.Flags, file.FileId.HasValue);
        SizeVector size = _options.Policy.ToVector(file.LogicalSize, file.AllocatedSize, flags);
        d.Own += size;
        d.OwnEntries++;
        d.Subtree += size;
        d.SubtreeEntries++;
        if (_options.Policy.IsVerifiedSingleLink(file.FileId.HasValue, file.LinkCount))
        {
            d.SubtreeVerified += size;
            d.SubtreeVerifiedFiles++;
        }

        int ext = _extensions.IdFor(file.Name);
        _extensionTotals.TryGetValue(ext, out ExtensionStats stats);
        _extensionTotals[ext] = stats + new ExtensionStats(1, 1, size);
        CountFlags(flags);

        if (file.FileId is FileId128 id && _links is not null)
        {
            _links.Add(new LinkRecord(id, directory, file.LogicalSize, file.AllocatedSize, flags, ext, file.LinkCount));
        }

        if (file.AllocatedSize >= _options.RetainFileThreshold)
        {
            _sink.OnFileRetained(directory, file, flags);
        }
    }

    /// <summary>Records a junction, symbolic link or mount point. It is never followed and contributes no bytes.</summary>
    public void AddNotFollowedLink(DirectoryId directory, string name, LinkKind kind)
    {
        _ = name;
        _ = kind;
        GetListing(directory).Coverage.NotFollowedLinks++;
    }

    /// <summary>
    /// Records a directory that is a reparse point but not a name surrogate (e.g. a cloud-files folder). It is not
    /// traversed, because listing it could trigger provider activity, so its contents are a coverage gap.
    /// </summary>
    public void AddSkippedReparseDirectory(DirectoryId directory, string name)
    {
        _ = name;
        GetListing(directory).Coverage.SkippedReparseDirectories++;
    }

    /// <summary>Records an entry that could not be read (e.g. access denied on one file).</summary>
    public void RecordEntryError(DirectoryId directory) => GetListing(directory).Coverage.EntryErrors++;

    /// <summary>The directory could not be listed at all. End it with <see cref="EndListing"/> afterwards.</summary>
    public void MarkInaccessible(DirectoryId directory)
    {
        OpenDirectory d = GetListing(directory);
        if (!d.Inaccessible)
        {
            d.Inaccessible = true;
            d.Coverage.InaccessibleDirectories++;
        }
    }

    /// <summary>The listing stopped part-way (I/O error, disconnection). Totals for this directory are a lower bound.</summary>
    public void MarkIncomplete(DirectoryId directory)
    {
        OpenDirectory d = GetListing(directory);
        if (!d.Incomplete)
        {
            d.Incomplete = true;
            d.Coverage.IncompleteDirectories++;
        }
    }

    /// <summary>The scanner saw this directory change while it was being scanned.</summary>
    public void MarkChanged(DirectoryId directory)
    {
        OpenDirectory d = GetOpen(directory);
        if (!d.Changed)
        {
            d.Changed = true;
            d.Coverage.ChangedDirectories++;
        }
    }

    /// <summary>No more entries will be reported for this directory.</summary>
    public void EndListing(DirectoryId directory)
    {
        OpenDirectory d = GetListing(directory);
        d.ListingEnded = true;
        TryFinalize(d);
    }

    /// <summary>Ends a scan in which every directory was ended. Throws if any directory is still open.</summary>
    public AggregationOutcome Complete()
    {
        EnsureRunning();
        if (_open.Count != 0 || _rootRecord is null)
        {
            throw new InvalidOperationException($"{_open.Count} directories are still open; use Cancel for partial scans.");
        }

        return End(ScanCompletion.Completed);
    }

    /// <summary>
    /// Ends a partial scan. Every open directory is finalized deepest-first and counted as cancelled;
    /// totals then cover exactly the entries reported so far.
    /// </summary>
    public AggregationOutcome Cancel()
    {
        EnsureRunning();
        if (_nextId == 0)
        {
            throw new InvalidOperationException("The scan never started.");
        }

        // Deepest first: by the time a directory is reached, all of its subdirectories are finalized.
        foreach (OpenDirectory d in _open.Values.OrderByDescending(d => d.Depth).ThenByDescending(d => d.Id.Value).ToList())
        {
            if (!_open.ContainsKey(d.Id.Value))
            {
                continue; // Already finalized when its last child finished.
            }

            if (!d.ListingEnded)
            {
                d.Coverage.CancelledDirectories++;
                d.ListingEnded = true;
            }

            TryFinalize(d);
        }

        return End(ScanCompletion.Cancelled);
    }

    private AggregationOutcome End(ScanCompletion completion)
    {
        _links?.Finish();
        _ended = true;
        return new AggregationOutcome(
            completion,
            _rootRecord!,
            _extensions,
            new Dictionary<int, ExtensionStats>(_extensionTotals),
            new Dictionary<MeasurementCaveats, long>(_entriesByFlag),
            _links?.RecordsWritten ?? 0,
            _peakOpen);
    }

    private DirectoryId Open(DirectoryId? parent, string name, int depth)
    {
        ArgumentNullException.ThrowIfNull(name);
        var id = new DirectoryId(_nextId++);
        _open.Add(id.Value, new OpenDirectory(id, parent, name, depth));
        _peakOpen = Math.Max(_peakOpen, _open.Count);
        return id;
    }

    private void TryFinalize(OpenDirectory d)
    {
        // Iterative so very deep trees cannot overflow the stack.
        OpenDirectory? current = d;
        while (current is not null && current.ListingEnded && current.PendingChildren == 0)
        {
            CoverageCounts subtreeCoverage = current.ChildCoverage + current.Coverage.ToCounts();
            var record = new DirectoryRecord(
                current.Id,
                current.Parent,
                current.Name,
                current.Depth,
                current.Own,
                current.OwnEntries,
                current.Subtree,
                current.SubtreeEntries,
                current.SubtreeDirectories,
                current.Coverage.ToCounts(),
                subtreeCoverage,
                current.SubtreeVerified,
                current.SubtreeVerifiedFiles);

            _open.Remove(current.Id.Value);
            _sink.OnDirectoryCompleted(record);

            if (current.Parent is not DirectoryId parentId)
            {
                _rootRecord = record;
                return;
            }

            OpenDirectory parent = _open[parentId.Value];
            parent.Subtree += record.Subtree;
            parent.SubtreeEntries += record.SubtreeFileEntries;
            parent.SubtreeVerified += record.SubtreeVerified;
            parent.SubtreeVerifiedFiles += record.SubtreeVerifiedFiles;
            parent.SubtreeDirectories += record.SubtreeDirectories + 1;
            parent.ChildCoverage += subtreeCoverage;
            parent.PendingChildren--;
            current = parent;
        }
    }

    private void CountFlags(MeasurementCaveats flags)
    {
        for (uint bit = 1; bit != 0 && bit <= (uint)flags; bit <<= 1)
        {
            if (((uint)flags & bit) != 0)
            {
                var flag = (MeasurementCaveats)bit;
                _entriesByFlag[flag] = _entriesByFlag.GetValueOrDefault(flag) + 1;
            }
        }
    }

    private OpenDirectory GetOpen(DirectoryId id)
    {
        EnsureRunning();
        return _open.TryGetValue(id.Value, out OpenDirectory? d)
            ? d
            : throw new InvalidOperationException($"Directory {id} is not open (unknown or already finalized).");
    }

    private OpenDirectory GetListing(DirectoryId id)
    {
        OpenDirectory d = GetOpen(id);
        return d.ListingEnded
            ? throw new InvalidOperationException($"Directory {id} has already ended its listing.")
            : d;
    }

    private void EnsureRunning()
    {
        if (_ended)
        {
            throw new InvalidOperationException("The scan has ended.");
        }
    }

    private sealed class OpenDirectory(DirectoryId id, DirectoryId? parent, string name, int depth)
    {
        public DirectoryId Id { get; } = id;
        public DirectoryId? Parent { get; } = parent;
        public string Name { get; } = name;
        public int Depth { get; } = depth;
        public SizeVector Own;
        public long OwnEntries;
        public SizeVector Subtree;
        public long SubtreeEntries;
        public SizeVector SubtreeVerified;
        public long SubtreeVerifiedFiles;
        public long SubtreeDirectories;
        public int PendingChildren;
        public bool ListingEnded;
        public bool Inaccessible;
        public bool Incomplete;
        public bool Changed;
        public MutableCoverage Coverage;
        public CoverageCounts ChildCoverage;
    }

    private struct MutableCoverage
    {
        public long InaccessibleDirectories;
        public long EntryErrors;
        public long NotFollowedLinks;
        public long ChangedDirectories;
        public long IncompleteDirectories;
        public long CancelledDirectories;
        public long SkippedReparseDirectories;

        public readonly CoverageCounts ToCounts() => new(
            InaccessibleDirectories, EntryErrors, NotFollowedLinks, ChangedDirectories, IncompleteDirectories,
            CancelledDirectories, SkippedReparseDirectories);
    }
}
