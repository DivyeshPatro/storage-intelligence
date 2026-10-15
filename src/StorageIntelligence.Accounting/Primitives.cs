namespace StorageIntelligence.Accounting;

/// <summary>
/// Volume-local file identity. NTFS uses 64-bit IDs (High = 0); ReFS uses 128-bit IDs.
/// Two directory entries with the same ID on the same volume are hard links to one file.
/// </summary>
public readonly record struct FileId128(ulong High, ulong Low) : IComparable<FileId128>
{
    public int CompareTo(FileId128 other)
    {
        int c = High.CompareTo(other.High);
        return c != 0 ? c : Low.CompareTo(other.Low);
    }

    public static bool operator <(FileId128 left, FileId128 right) => left.CompareTo(right) < 0;
    public static bool operator >(FileId128 left, FileId128 right) => left.CompareTo(right) > 0;
    public static bool operator <=(FileId128 left, FileId128 right) => left.CompareTo(right) <= 0;
    public static bool operator >=(FileId128 left, FileId128 right) => left.CompareTo(right) >= 0;

    public override string ToString() => $"{High:x16}{Low:x16}";
}

/// <summary>Filesystems the scanner may encounter.</summary>
public enum FileSystemKind
{
    Unknown,
    Ntfs,
    ReFS,
    Fat32,
    ExFat,
}

/// <summary>
/// Reasons a size measurement may not be exact. A file carries every reason that applies;
/// <see cref="MeasurementPolicy"/> decides which reasons make its allocated size "uncertain".
/// </summary>
[Flags]
public enum MeasurementCaveats : uint
{
    None = 0,

    /// <summary>File is sparse; allocated size may differ from logical size.</summary>
    Sparse = 1 << 0,

    /// <summary>NTFS (LZNT1) compressed file.</summary>
    Compressed = 1 << 1,

    /// <summary>Windows Overlay Filter (WOF / CompactOS) compressed file.</summary>
    WofCompressed = 1 << 2,

    /// <summary>Cloud-files placeholder (e.g. OneDrive). Only local allocation is measured.</summary>
    CloudPlaceholder = 1 << 3,

    /// <summary>The scanner observed this entry or its directory changing during the scan.</summary>
    ChangedDuringScan = 1 << 4,

    /// <summary>Hard links to the same file reported different sizes (the file changed between observations).</summary>
    InconsistentLinkSizes = 1 << 5,

    /// <summary>Allocation semantics for this filesystem have not been validated against fixtures.</summary>
    UnvalidatedFileSystem = 1 << 6,

    /// <summary>The filesystem supports hard links but no file ID was available, so duplicates cannot be detected.</summary>
    IdentityUnavailable = 1 << 7,

    /// <summary>Some other reparse/attribute state the scanner could not interpret.</summary>
    UnrecognizedAttributes = 1 << 8,
}

/// <summary>
/// Logical, allocated and uncertain-allocated byte counts. Uncertain bytes are a subset of allocated bytes:
/// the part of the allocated total whose measurement has not been validated or may be wrong.
/// Values can be negative only when used as a correction delta.
/// </summary>
public readonly record struct SizeVector(long Logical, long Allocated, long UncertainAllocated)
{
    public static readonly SizeVector Zero;

    public static SizeVector operator +(SizeVector a, SizeVector b) =>
        new(a.Logical + b.Logical, a.Allocated + b.Allocated, a.UncertainAllocated + b.UncertainAllocated);

    public static SizeVector operator -(SizeVector a, SizeVector b) =>
        new(a.Logical - b.Logical, a.Allocated - b.Allocated, a.UncertainAllocated - b.UncertainAllocated);

    public static SizeVector Add(SizeVector left, SizeVector right) => left + right;

    public static SizeVector Subtract(SizeVector left, SizeVector right) => left - right;

    public bool IsZero => Logical == 0 && Allocated == 0 && UncertainAllocated == 0;
}

/// <summary>
/// Decides which measurements are trusted. The default is deliberately conservative: nothing is validated
/// until the M0b fixture tests establish allocation semantics for a filesystem and attribute combination.
/// </summary>
public sealed record MeasurementPolicy
{
    /// <summary>Filesystem being scanned.</summary>
    public FileSystemKind FileSystem { get; init; } = FileSystemKind.Unknown;

    /// <summary>True once plain-file allocation on <see cref="FileSystem"/> has been validated.</summary>
    public bool FileSystemValidated { get; init; }

    /// <summary>True for filesystems that support hard links (NTFS, ReFS) or when unknown.</summary>
    public bool FileSystemSupportsHardLinks { get; init; } = true;

    /// <summary>Reasons that make a file's allocated size uncertain. Defaults to every reason.</summary>
    public MeasurementCaveats UncertainReasons { get; init; } = AllReasons;

    public const MeasurementCaveats AllReasons =
        MeasurementCaveats.Sparse | MeasurementCaveats.Compressed | MeasurementCaveats.WofCompressed |
        MeasurementCaveats.CloudPlaceholder | MeasurementCaveats.ChangedDuringScan |
        MeasurementCaveats.InconsistentLinkSizes | MeasurementCaveats.UnvalidatedFileSystem |
        MeasurementCaveats.IdentityUnavailable | MeasurementCaveats.UnrecognizedAttributes;

    /// <summary>Policy used when nothing has been validated.</summary>
    public static MeasurementPolicy Conservative { get; } = new();

    /// <summary>Adds the flags the policy itself implies (unvalidated filesystem, missing identity).</summary>
    public MeasurementCaveats Effective(MeasurementCaveats observed, bool hasFileId)
    {
        MeasurementCaveats flags = observed;
        if (!FileSystemValidated)
        {
            flags |= MeasurementCaveats.UnvalidatedFileSystem;
        }

        if (FileSystemSupportsHardLinks && !hasFileId)
        {
            flags |= MeasurementCaveats.IdentityUnavailable;
        }

        return flags;
    }

    public bool IsUncertain(MeasurementCaveats effectiveFlags) => (effectiveFlags & UncertainReasons) != 0;

    /// <summary>Converts one file's sizes into a vector. Must be the only conversion used, so deltas stay consistent.</summary>
    public SizeVector ToVector(long logical, long allocated, MeasurementCaveats effectiveFlags) =>
        new(logical, allocated, IsUncertain(effectiveFlags) ? allocated : 0);
}
