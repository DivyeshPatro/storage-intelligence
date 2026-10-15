using StorageIntelligence.Accounting;

namespace StorageIntelligence.Accounting.Tests.Support;

/// <summary>Expected totals for one directory, computed by brute force from the definitions.</summary>
public sealed record ExpectedTotals(
    SizeVector Size,
    SizeVector Shared,
    long FileEntries,
    long DistinctFiles,
    long SharedFiles,
    long Directories,
    CoverageCounts Coverage);

/// <summary>
/// Brute-force reference for the accounting semantics. It shares no code with the production algorithm:
/// it enumerates the distinct files in each subtree directly from the emission log.
/// Definitions (docs/accounting-semantics.md):
/// <list type="bullet">
/// <item>A file is a set of observed entries with the same file ID; an entry without an ID is its own file.</item>
/// <item>A file's size is taken from its canonical entry: largest allocated, then largest logical, then
/// ordinal-smallest extension. Its caveats are the union over its entries, plus InconsistentLinkSizes if the
/// entries disagree on size.</item>
/// <item>Size(D) = sum of sizes of distinct files with at least one entry in D's subtree.</item>
/// <item>Shared(D) = sum over those files whose total link count (max of observed entries and any measured
/// link count) exceeds the number of entries inside D's subtree.</item>
/// </list>
/// </summary>
public static class Oracle
{
    private sealed record FileFacts(SizeVector Size, int ExtensionOwner, long TotalLinks, IReadOnlyList<int> Entries);

    public static ExpectedTotals For(DriverResult run, int dir)
    {
        TreeModel model = run.Model;
        EmissionLog log = run.Log;
        MeasurementPolicy policy = run.Policy;
        List<FileFacts> files = Files(model, log, policy);

        bool InSubtree(int d) => model.IsInSubtree(d, dir);

        SizeVector size = SizeVector.Zero, shared = SizeVector.Zero;
        long distinct = 0, sharedFiles = 0;
        foreach (FileFacts file in files)
        {
            int inside = file.Entries.Count(e => InSubtree(model.Files[e].Dir));
            if (inside == 0)
            {
                continue;
            }

            size += file.Size;
            distinct++;
            if (inside < file.TotalLinks)
            {
                shared += file.Size;
                sharedFiles++;
            }
        }

        var discoveredInSubtree = log.Discovered.Keys.Where(InSubtree).ToList();
        var coverage = new CoverageCounts(
            discoveredInSubtree.Count(log.Inaccessible.Contains),
            discoveredInSubtree.Sum(d => log.EntryErrors.GetValueOrDefault(d)),
            discoveredInSubtree.Sum(d => log.Links.GetValueOrDefault(d)),
            discoveredInSubtree.Count(log.Changed.Contains),
            discoveredInSubtree.Count(log.Incomplete.Contains),
            log.Cancelled ? discoveredInSubtree.Count(d => !log.Ended.Contains(d)) : 0);

        return new ExpectedTotals(
            size,
            shared,
            log.EmittedFiles.Count(f => InSubtree(model.Files[f].Dir)),
            distinct,
            sharedFiles,
            discoveredInSubtree.Count - 1,
            coverage);
    }

    /// <summary>Expected whole-volume extension totals: each distinct file under its canonical entry's extension.</summary>
    public static Dictionary<string, ExtensionStats> Extensions(DriverResult run)
    {
        var result = new Dictionary<string, ExtensionStats>(StringComparer.Ordinal);
        foreach (int entry in run.Log.EmittedFiles)
        {
            string ext = ExtensionTable.Normalize(run.Model.Files[entry].Name);
            result[ext] = result.GetValueOrDefault(ext) + new ExtensionStats(1, 0, SizeVector.Zero);
        }

        foreach (FileFacts file in Files(run.Model, run.Log, run.Policy))
        {
            string ext = ExtensionTable.Normalize(run.Model.Files[file.ExtensionOwner].Name);
            result[ext] = result.GetValueOrDefault(ext) + new ExtensionStats(0, 1, file.Size);
        }

        return result;
    }

    private static List<FileFacts> Files(TreeModel model, EmissionLog log, MeasurementPolicy policy)
    {
        var result = new List<FileFacts>();
        foreach (IGrouping<int?, int> group in log.EmittedFiles
                     .GroupBy(f => model.Files[f].Key)
                     .ToList())
        {
            if (group.Key is null)
            {
                foreach (int entry in group)
                {
                    FileSpec spec = model.Files[entry];
                    MeasurementCaveats caveats = policy.Effective(spec.Caveats, hasFileId: false);
                    result.Add(new FileFacts(policy.ToVector(spec.Logical, spec.Allocated, caveats), entry, 1, [entry]));
                }

                continue;
            }

            var entries = group.ToList();
            var specs = entries.Select(e => model.Files[e]).ToList();
            MeasurementCaveats union = specs.Aggregate(
                MeasurementCaveats.None, (acc, s) => acc | policy.Effective(s.Caveats, hasFileId: true));
            if (specs.Select(s => (s.Logical, s.Allocated)).Distinct().Count() > 1)
            {
                union |= MeasurementCaveats.InconsistentLinkSizes;
            }

            int canonical = entries
                .OrderByDescending(e => model.Files[e].Allocated)
                .ThenByDescending(e => model.Files[e].Logical)
                .ThenBy(e => ExtensionTable.Normalize(model.Files[e].Name), StringComparer.Ordinal)
                .First();
            FileSpec c = model.Files[canonical];
            long totalLinks = Math.Max(entries.Count, specs.Max(s => (long)s.LinkCount));
            result.Add(new FileFacts(policy.ToVector(c.Logical, c.Allocated, union), canonical, totalLinks, entries));
        }

        return result;
    }
}
