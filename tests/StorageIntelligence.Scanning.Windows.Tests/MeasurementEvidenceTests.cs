using StorageIntelligence.Accounting;

namespace StorageIntelligence.Scanning.Windows.Tests;

/// <summary>The policy must claim exactness only for cases with validation evidence.</summary>
public sealed class MeasurementEvidenceTests
{
    [Theory]
    [InlineData(FileSystemKind.Ntfs, MeasurementCaveats.None, false)]
    [InlineData(FileSystemKind.Ntfs, MeasurementCaveats.Sparse, false)]
    [InlineData(FileSystemKind.Ntfs, MeasurementCaveats.Compressed, true)]
    [InlineData(FileSystemKind.Ntfs, MeasurementCaveats.WofCompressed, true)]
    [InlineData(FileSystemKind.Ntfs, MeasurementCaveats.CloudPlaceholder, true)]
    [InlineData(FileSystemKind.Ntfs, MeasurementCaveats.ChangedDuringScan, true)]
    [InlineData(FileSystemKind.ReFS, MeasurementCaveats.None, true)]
    [InlineData(FileSystemKind.ReFS, MeasurementCaveats.Sparse, true)]
    [InlineData(FileSystemKind.Fat32, MeasurementCaveats.None, false)]
    [InlineData(FileSystemKind.ExFat, MeasurementCaveats.None, false)]
    [InlineData(FileSystemKind.Unknown, MeasurementCaveats.None, true)]
    public void Only_validated_cases_are_exact(FileSystemKind kind, MeasurementCaveats caveats, bool uncertain)
    {
        MeasurementPolicy policy = MeasurementEvidence.PolicyFor(kind);
        MeasurementCaveats effective = policy.Effective(caveats, hasFileId: policy.FileSystemSupportsHardLinks);
        Assert.Equal(uncertain, policy.IsUncertain(effective));
    }
}
