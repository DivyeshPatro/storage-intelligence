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
/// Which measurement cases are validated, per filesystem. Every entry must cite the validation run that
/// established it (docs/m0b-validation-results.md). Until then a case stays uncertain.
/// </summary>
public static class MeasurementEvidence
{
    public static MeasurementPolicy PolicyFor(FileSystemKind kind) => new()
    {
        FileSystem = kind,
        FileSystemValidated = false,
        FileSystemSupportsHardLinks = kind is not (FileSystemKind.Fat32 or FileSystemKind.ExFat),
        UncertainReasons = MeasurementPolicy.AllReasons,
    };
}
