using System.Text;
using StorageIntelligence.Accounting;

namespace StorageIntelligence.Accounting.Tests.Support;

/// <summary>A directory in a synthetic tree. Index 0 is the root.</summary>
/// <param name="Parent">Parent index, or -1 for the root.</param>
/// <param name="Inaccessible">The scanner cannot list it; nothing beneath it is observed.</param>
/// <param name="Changed">The scanner reports it changed during the scan.</param>
/// <param name="TruncateAt">If set, the listing stops after this fraction of entries and is marked incomplete.</param>
public sealed record DirSpec(int Parent, bool Inaccessible, bool Changed, int EntryErrors, int NotFollowedLinks, double? TruncateAt);

/// <summary>A file entry. Entries sharing a <see cref="Key"/> are hard links to the same file.</summary>
public sealed record FileSpec(int Dir, string Name, int? Key, long Logical, long Allocated, MeasurementCaveats Caveats, uint LinkCount);

/// <summary>A synthetic directory tree with hard links, caveats and coverage problems.</summary>
public sealed record TreeModel(IReadOnlyList<DirSpec> Dirs, IReadOnlyList<FileSpec> Files)
{
    public IEnumerable<int> ChildrenOf(int dir)
    {
        for (int i = 0; i < Dirs.Count; i++)
        {
            if (Dirs[i].Parent == dir)
            {
                yield return i;
            }
        }
    }

    public bool IsInSubtree(int dir, int ancestor)
    {
        for (int d = dir; d >= 0; d = Dirs[d].Parent)
        {
            if (d == ancestor)
            {
                return true;
            }
        }

        return false;
    }

    public int Depth(int dir)
    {
        int depth = 0;
        for (int d = Dirs[dir].Parent; d >= 0; d = Dirs[d].Parent)
        {
            depth++;
        }

        return depth;
    }

    public static FileId128 IdFor(int key) => new((ulong)(key % 3), (ulong)key * 7919UL);

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Dirs.Count; i++)
        {
            sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"D{i}<-{Dirs[i].Parent} {Dirs[i]}\n");
        }

        foreach (FileSpec f in Files)
        {
            sb.Append(f).Append('\n');
        }

        return sb.ToString();
    }
}
