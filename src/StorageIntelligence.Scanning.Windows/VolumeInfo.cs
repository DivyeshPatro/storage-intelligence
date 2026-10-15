using StorageIntelligence.Accounting;
using StorageIntelligence.Scanning.Windows.Interop;

namespace StorageIntelligence.Scanning.Windows;

/// <summary>Filesystem facts for the volume that contains a scan root.</summary>
public sealed record VolumeInfo(string VolumeRoot, string FileSystemName, uint SerialNumber, string Label, long ClusterSize)
{
    public FileSystemKind Kind => FileSystemName.ToUpperInvariant() switch
    {
        "NTFS" => FileSystemKind.Ntfs,
        "REFS" => FileSystemKind.ReFS,
        "FAT32" or "FAT" => FileSystemKind.Fat32,
        "EXFAT" => FileSystemKind.ExFat,
        _ => FileSystemKind.Unknown,
    };

    /// <summary>
    /// The measurement policy for this volume. Nothing is marked validated here: validation evidence is applied by
    /// <see cref="MeasurementEvidence"/>, so an unvalidated case can never be reported as exact by default.
    /// </summary>
    public MeasurementPolicy Policy => MeasurementEvidence.PolicyFor(Kind);

    public static unsafe VolumeInfo Read(string path)
    {
        string extended = Native.ToExtendedPath(path);
        char* rootBuffer = stackalloc char[1024];
        string volumeRoot = Native.GetVolumePathName(extended, rootBuffer, 1024)
            ? new string(rootBuffer)
            : throw new IOException($"Cannot determine the volume of {path} (error {System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");

        using var handle = Native.OpenMetadata(extended, Native.FILE_READ_ATTRIBUTES, out int error);
        if (handle.IsInvalid)
        {
            throw new IOException($"Cannot open {path} (error {error}).");
        }

        char* label = stackalloc char[261];
        char* fsName = stackalloc char[261];
        if (!Native.GetVolumeInformationByHandle(handle, label, 261, out uint serial, out _, out _, fsName, 261))
        {
            throw new IOException($"Cannot read volume information for {path} (error {System.Runtime.InteropServices.Marshal.GetLastPInvokeError()}).");
        }

        long cluster = 0;
        string plainRoot = volumeRoot.StartsWith(@"\\?\", StringComparison.Ordinal) ? volumeRoot[4..] : volumeRoot;
        if (Native.GetDiskFreeSpace(plainRoot, out uint sectorsPerCluster, out uint bytesPerSector, out _, out _))
        {
            cluster = (long)sectorsPerCluster * bytesPerSector;
        }

        return new VolumeInfo(plainRoot, new string(fsName), serial, new string(label), cluster);
    }
}

/// <summary>
/// Which measurement cases are validated, per filesystem. A case is validated only when the directory-listing
/// allocation matched the volume's settled free-space change in every trial on both CI images
/// (docs/m0b-validation-results.md, run 38711178827). Everything else stays uncertain.
/// </summary>
public static class MeasurementEvidence
{
    /// <summary>Caveats that stay uncertain on a validated filesystem.</summary>
    private const MeasurementCaveats UnprovenOnNtfs =
        MeasurementCaveats.Compressed |       // inconsistent: free-space change exceeded every reported value
        MeasurementCaveats.WofCompressed |    // listing reports 0; handle value matched, but only compressible data was testable
        MeasurementCaveats.CloudPlaceholder;  // not testable on hosted runners

    public static MeasurementPolicy PolicyFor(FileSystemKind kind) => kind switch
    {
        // Validated: plain, resident (100 B) and sparse files.
        FileSystemKind.Ntfs => Policy(kind, validated: true, hardLinks: true, MeasurementPolicy.AllReasons & ~MeasurementCaveats.Sparse & ~MeasurementCaveats.UnvalidatedFileSystem | UnprovenOnNtfs),

        // Not validated: plain-file allocation did not track free space (ReFS accounts lazily / in larger units).
        FileSystemKind.ReFS => Policy(kind, validated: false, hardLinks: true, MeasurementPolicy.AllReasons),

        // Validated: plain and resident files (the only kinds these filesystems support).
        FileSystemKind.Fat32 or FileSystemKind.ExFat => Policy(kind, validated: true, hardLinks: false, MeasurementPolicy.AllReasons & ~MeasurementCaveats.UnvalidatedFileSystem),

        _ => Policy(kind, validated: false, hardLinks: true, MeasurementPolicy.AllReasons),
    };

    private static MeasurementPolicy Policy(FileSystemKind kind, bool validated, bool hardLinks, MeasurementCaveats uncertain) => new()
    {
        FileSystem = kind,
        FileSystemValidated = validated,
        FileSystemSupportsHardLinks = hardLinks,
        UncertainReasons = uncertain,
    };
}
