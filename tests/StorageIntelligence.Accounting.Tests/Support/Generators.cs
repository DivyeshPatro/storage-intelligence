using CsCheck;
using StorageIntelligence.Accounting;

namespace StorageIntelligence.Accounting.Tests.Support;

public static class Generators
{
    private static readonly string[] NameSuffixes = [".txt", ".TXT", ".dll", "", ".gitignore", ".tar.gz", ".", ".JPG"];

    private const MeasurementCaveats ScannerCaveats =
        MeasurementCaveats.Sparse | MeasurementCaveats.Compressed | MeasurementCaveats.WofCompressed |
        MeasurementCaveats.CloudPlaceholder | MeasurementCaveats.ChangedDuringScan;

    public static readonly Gen<MeasurementPolicy> Policy = Gen.OneOfConst(
        MeasurementPolicy.Conservative,
        new MeasurementPolicy { FileSystem = FileSystemKind.Ntfs, FileSystemValidated = true },
        new MeasurementPolicy
        {
            FileSystem = FileSystemKind.Ntfs,
            FileSystemValidated = true,
            UncertainReasons = MeasurementCaveats.Sparse | MeasurementCaveats.InconsistentLinkSizes,
        },
        new MeasurementPolicy
        {
            FileSystem = FileSystemKind.ExFat,
            FileSystemValidated = true,
            FileSystemSupportsHardLinks = false,
        });

    private static readonly Gen<DirSpec> RootSpec =
        Gen.Select(Gen.Bool, Gen.Int[0, 2], Gen.Int[0, 2], (changed, errors, links) =>
            new DirSpec(-1, false, changed, errors, links, null));

    /// <summary>Directory attributes; the parent is filled in from a raw value modulo the directory's index.</summary>
    private static readonly Gen<DirSpec> ChildSpec =
        Gen.Select(
            Gen.Int[0, int.MaxValue],
            Gen.Frequency((14, Gen.Const(false)), (1, Gen.Const(true))),
            Gen.Frequency((9, Gen.Const(false)), (1, Gen.Const(true))),
            Gen.Frequency((6, Gen.Const(0)), (1, Gen.Int[1, 3])),
            Gen.Frequency((6, Gen.Const(0)), (1, Gen.Int[1, 3])),
            Gen.Frequency((11, Gen.Const((double?)null)), (1, Gen.Double[0.0, 1.0].Select(d => (double?)d))),
            (parentRaw, inaccessible, changed, errors, links, truncate) =>
                new DirSpec(parentRaw, inaccessible, changed, errors, links, truncate));

    private static readonly Gen<MeasurementCaveats> Caveats = Gen.Frequency(
        (6, Gen.Const(MeasurementCaveats.None)),
        (1, Gen.UInt.Select(u => (MeasurementCaveats)u & ScannerCaveats)));

    private static readonly Gen<(long Logical, long Allocated)> Sizes =
        Gen.Select(Gen.Long[0, 5_000_000], Gen.Int[0, 1200], (logical, clusters) => (logical, clusters * 4096L));

    /// <summary>A synthetic tree with up to <paramref name="maxDirs"/> directories and <paramref name="maxFiles"/> files.</summary>
    public static Gen<TreeModel> Tree(int maxDirs = 30, int maxFiles = 80, bool withHardLinks = true) =>
        from dirCount in Gen.Int[1, maxDirs]
        from root in RootSpec
        from others in ChildSpec.Array[dirCount - 1]
        from keyCount in Gen.Int[1, 12]
        from keySizes in Sizes.Array[keyCount]
        from fileCount in Gen.Int[0, maxFiles]
        from files in FileSpec(dirCount, keySizes, withHardLinks).Array[fileCount]
        select new TreeModel(
            [root, .. others.Select((d, i) => d with { Parent = d.Parent % (i + 1) })],
            files.Select((f, i) => f with { Name = $"f{i}{f.Name}" }).ToArray());

    private static Gen<FileSpec> FileSpec(int dirCount, (long Logical, long Allocated)[] keySizes, bool withHardLinks) =>
        from dir in Gen.Int[0, dirCount - 1]
        from suffix in Gen.OneOfConst(NameSuffixes)
        from keyed in withHardLinks ? Gen.Frequency((3, Gen.Const(false)), (2, Gen.Const(true))) : Gen.Const(false)
        from key in Gen.Int[0, keySizes.Length - 1]
        from mutated in Gen.Frequency((12, Gen.Const(false)), (1, Gen.Const(true)))
        from own in Sizes
        from caveats in Caveats
        from linkCount in Gen.Frequency((8, Gen.Const(0u)), (1, Gen.UInt[1, 6]))
        select keyed
            ? new FileSpec(dir, suffix, key, mutated ? own.Logical : keySizes[key].Logical,
                mutated ? own.Allocated : keySizes[key].Allocated, caveats, linkCount)
            : new FileSpec(dir, suffix, null, own.Logical, own.Allocated, caveats, 0);

    /// <summary>A tree paired with a policy; hard links only where the filesystem supports them.</summary>
    public static readonly Gen<(TreeModel Tree, MeasurementPolicy Policy)> Scenario =
        from policy in Policy
        from tree in Tree(withHardLinks: policy.FileSystemSupportsHardLinks)
        select (tree, policy);
}
