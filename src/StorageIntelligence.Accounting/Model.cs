namespace StorageIntelligence.Accounting;

/// <summary>Opaque identifier the aggregator assigns to each directory. The root is always 0.</summary>
public readonly record struct DirectoryId(long Value)
{
    public static readonly DirectoryId Root = new(0);

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>One file as reported by the scanner. Metadata only; contents are never read.</summary>
/// <param name="Name">Entry name (used for the extension and for retained large-file records).</param>
/// <param name="FileId">Volume-local file identity, or null where the filesystem has none (FAT/exFAT).</param>
/// <param name="LogicalSize">End-of-file size in bytes.</param>
/// <param name="AllocatedSize">Allocated size in bytes as reported by the filesystem.</param>
/// <param name="Flags">Measurement caveats observed by the scanner.</param>
/// <param name="LinkCount">Hard-link count if measured, 0 if unknown. Ordinary scans do not measure it.</param>
public readonly record struct FileObservation(
    string Name,
    FileId128? FileId,
    long LogicalSize,
    long AllocatedSize,
    MeasurementCaveats Flags = MeasurementCaveats.None,
    uint LinkCount = 0);

/// <summary>Name-surrogate reparse points. They are recorded but never followed or counted as data.</summary>
public enum LinkKind
{
    Junction,
    SymbolicLink,
    VolumeMountPoint,
    Other,
}

/// <summary>Coverage counters for one directory or rolled up for a subtree.</summary>
public readonly record struct CoverageCounts(
    long InaccessibleDirectories,
    long EntryErrors,
    long NotFollowedLinks,
    long ChangedDirectories,
    long IncompleteDirectories,
    long CancelledDirectories)
{
    public static CoverageCounts operator +(CoverageCounts a, CoverageCounts b) => new(
        a.InaccessibleDirectories + b.InaccessibleDirectories,
        a.EntryErrors + b.EntryErrors,
        a.NotFollowedLinks + b.NotFollowedLinks,
        a.ChangedDirectories + b.ChangedDirectories,
        a.IncompleteDirectories + b.IncompleteDirectories,
        a.CancelledDirectories + b.CancelledDirectories);

    public static CoverageCounts Add(CoverageCounts left, CoverageCounts right) => left + right;

    /// <summary>True when nothing in the subtree was skipped, denied, changed or left incomplete.</summary>
    public bool IsComplete =>
        InaccessibleDirectories == 0 && EntryErrors == 0 && IncompleteDirectories == 0 && CancelledDirectories == 0;
}

/// <summary>
/// A finalized directory as emitted by the aggregator, before hard-link reconciliation.
/// <see cref="Subtree"/> and <see cref="SubtreeFileEntries"/> count every directory entry, so a file with
/// several links inside the subtree is counted once per link. Reconciliation corrects this.
/// <see cref="SubtreeVerified"/> holds entries whose link status is already known to be "single link"
/// (measured link count of 1, or a filesystem without hard links).
/// </summary>
public sealed record DirectoryRecord(
    DirectoryId Id,
    DirectoryId? Parent,
    string Name,
    int Depth,
    SizeVector Own,
    long OwnFileEntries,
    SizeVector Subtree,
    long SubtreeFileEntries,
    long SubtreeDirectories,
    CoverageCounts OwnCoverage,
    CoverageCounts SubtreeCoverage,
    SizeVector SubtreeVerified = default,
    long SubtreeVerifiedFiles = 0);

/// <summary>Receives finalized directories (post-order) and retained large files.</summary>
public interface IScanSink
{
    void OnDirectoryCompleted(DirectoryRecord record);

    void OnFileRetained(DirectoryId directory, FileObservation file, MeasurementCaveats effectiveFlags);
}

/// <summary>Parent lookup for already-finalized directories, used by hard-link reconciliation.</summary>
public interface IParentLookup
{
    bool TryGetParent(DirectoryId directory, out DirectoryId parent);
}
