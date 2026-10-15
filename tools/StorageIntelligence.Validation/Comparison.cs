using StorageIntelligence.Accounting;
using StorageIntelligence.Scanning.Windows;

namespace StorageIntelligence.Validation;

/// <summary>A single disagreement, with enough detail to reproduce it.</summary>
public sealed record Mismatch(string Category, string Path, string Detail);

/// <summary>Per-file facts the production scanner reported (captured through ScanOptions.FileObserver).</summary>
public sealed record ProductionFile(string Path, long Logical, long Allocated, string Caveats, uint LinkCount);

public sealed record ComparisonResult(
    string Mode,
    string LinkCounts,
    int DirectoriesCompared,
    int FilesCompared,
    List<Mismatch> Mismatches,
    List<Mismatch> KnownGaps)
{
    public bool Passed => Mismatches.Count == 0;
}

/// <summary>
/// Compares the production scan with the independent reference scan and the fixture manifest.
/// Exact equality is required for totals, counts and coverage. Hard-link classification must match exactly when
/// every link count was measured; otherwise production's Shared and ExclusiveVerified must be subsets of the
/// reference's (production may only say "unknown" where the reference knows more, never claim more).
/// </summary>
public static class Comparison
{
    public static ComparisonResult Compare(
        ScanResult production,
        IReadOnlyList<ProductionFile> productionFiles,
        ReferenceResult reference,
        FixtureManifest? manifest)
    {
        ArgumentNullException.ThrowIfNull(production);
        ArgumentNullException.ThrowIfNull(productionFiles);
        ArgumentNullException.ThrowIfNull(reference);

        var mismatches = new List<Mismatch>();
        var gaps = new List<Mismatch>();
        bool allLinksMeasured = production.Options.LinkCounts == LinkCountMeasurement.All;

        var prodDirs = production.DirectoryPaths.ToDictionary(
            kv => kv.Value,
            kv => production.Store.GetTotals(kv.Key, production.Accounting.Reconciliation, production.Accounting.HardLinks),
            StringComparer.Ordinal);

        if (production.Accounting.HardLinks == HardLinkStatus.NotReconciled)
        {
            mismatches.Add(new Mismatch("reconciliation", "", $"not reconciled: {production.Accounting.ReconciliationFailure} {production.Accounting.ReconciliationFailureDetail}"));
        }

        foreach (ReferenceDirectory r in reference.Directories)
        {
            if (!prodDirs.TryGetValue(r.Path, out DirectoryTotals? p))
            {
                mismatches.Add(new Mismatch("directory-missing", r.Path, "reference traversed it; production did not"));
                continue;
            }

            Check("size.allocated", r.Path, r.Allocated, p.Size.Allocated);
            Check("size.logical", r.Path, r.Logical, p.Size.Logical);
            Check("file-entries", r.Path, r.FileEntries, p.FileEntries);
            Check("distinct-files", r.Path, r.DistinctFiles, p.DistinctFiles);
            Check("directories", r.Path, r.Directories, p.Directories);
            Check("coverage.inaccessible", r.Path, r.Inaccessible, p.Coverage.InaccessibleDirectories);
            Check("coverage.not-followed-links", r.Path, r.NotFollowedLinks, p.Coverage.NotFollowedLinks);
            Check("coverage.skipped-reparse", r.Path, r.SkippedReparse, p.Coverage.SkippedReparseDirectories);
            Check("coverage.entry-errors", r.Path, r.EntryErrors, p.Coverage.EntryErrors);

            if (allLinksMeasured)
            {
                Check("links.shared", r.Path, r.SharedAllocated, p.Shared.Allocated);
                Check("links.exclusive", r.Path, r.ExclusiveAllocated, p.ExclusiveVerified.Allocated);
                Check("links.unknown", r.Path, 0, p.LinkStatusUnknown.Allocated);
            }
            else
            {
                CheckAtMost("links.shared-subset", r.Path, p.Shared.Allocated, r.SharedAllocated);
                CheckAtMost("links.exclusive-subset", r.Path, p.ExclusiveVerified.Allocated, r.ExclusiveAllocated);
            }
        }

        foreach (string path in prodDirs.Keys.Except(reference.Directories.Select(d => d.Path), StringComparer.Ordinal))
        {
            mismatches.Add(new Mismatch("directory-extra", path, "production traversed a directory the reference did not"));
        }

        var refFiles = reference.Files.ToDictionary(f => f.Path, StringComparer.Ordinal);
        foreach (ProductionFile p in productionFiles)
        {
            if (!refFiles.TryGetValue(p.Path, out ReferenceFile? r))
            {
                mismatches.Add(new Mismatch("file-extra", p.Path, "production observed a file the reference did not"));
                continue;
            }

            Check("file.logical", p.Path, r.Logical, p.Logical);
            Check("file.allocated(listing-vs-handle)", p.Path, r.Allocated, p.Allocated);
            CheckCaveat("Sparse", (r.Attributes & 0x200) != 0, p);
            CheckCaveat("Compressed", (r.Attributes & 0x800) != 0, p);
            bool wofByTag = r.ReparseTag == 0x80000017;
            if (r.WofExternal && !p.Caveats.Contains("WofCompressed", StringComparison.Ordinal))
            {
                mismatches.Add(new Mismatch("caveat.WofCompressed", p.Path,
                    $"WOF API reports a WOF-backed file (tag visible to handle query: {wofByTag}) but the directory listing did not flag it"));
            }

            if (allLinksMeasured)
            {
                Check("file.link-count", p.Path, r.Links, p.LinkCount);
            }
        }

        foreach (string path in refFiles.Keys.Except(productionFiles.Select(f => f.Path), StringComparer.Ordinal))
        {
            mismatches.Add(new Mismatch("file-missing", path, "reference measured a file production did not observe"));
        }

        if (manifest is not null)
        {
            CheckManifest(manifest, reference, mismatches, gaps);
        }

        return new ComparisonResult(
            production.Options.Mode.ToString(),
            production.Options.LinkCounts.ToString(),
            reference.Directories.Count,
            productionFiles.Count,
            mismatches,
            gaps);

        void Check(string category, string path, long expected, long actual)
        {
            if (expected != actual)
            {
                mismatches.Add(new Mismatch(category, path, $"reference {expected}, production {actual}"));
            }
        }

        void CheckAtMost(string category, string path, long actual, long bound)
        {
            if (actual > bound)
            {
                mismatches.Add(new Mismatch(category, path, $"production {actual} exceeds reference {bound}"));
            }
        }

        void CheckCaveat(string caveat, bool expected, ProductionFile p)
        {
            bool actual = p.Caveats.Contains(caveat, StringComparison.Ordinal);
            if (expected != actual)
            {
                mismatches.Add(new Mismatch("caveat." + caveat, p.Path, $"handle attributes say {expected}, listing caveat {actual}"));
            }
        }
    }

    /// <summary>Validates the reference itself against what the generator created.</summary>
    private static void CheckManifest(FixtureManifest manifest, ReferenceResult reference, List<Mismatch> mismatches, List<Mismatch> gaps)
    {
        var refFiles = reference.Files.ToDictionary(f => f.Path, StringComparer.Ordinal);
        var inaccessible = manifest.InaccessibleDirectories;
        foreach (ManifestFile f in manifest.Files)
        {
            if (inaccessible.Any(d => f.Path.StartsWith(d + "\\", StringComparison.Ordinal)))
            {
                if (refFiles.ContainsKey(f.Path))
                {
                    mismatches.Add(new Mismatch("manifest.denied-visible", f.Path, "file in a denied directory was measured"));
                }

                continue;
            }

            if (!refFiles.TryGetValue(f.Path, out ReferenceFile? r))
            {
                mismatches.Add(new Mismatch("manifest.file-missing", f.Path, "created by the generator but not found by the reference"));
                continue;
            }

            if (r.Logical != f.Logical)
            {
                mismatches.Add(new Mismatch("manifest.logical", f.Path, $"manifest {f.Logical}, reference {r.Logical}"));
            }

            if (f.LinkGroup is string group && r.Links != manifest.LinkGroupSizes[group])
            {
                mismatches.Add(new Mismatch("manifest.link-count", f.Path, $"manifest group {group} has {manifest.LinkGroupSizes[group]} links, reference {r.Links}"));
            }
        }

        foreach (string path in refFiles.Keys.Except(manifest.Files.Select(f => f.Path), StringComparer.Ordinal))
        {
            mismatches.Add(new Mismatch("manifest.unexpected-file", path, "measured by the reference but not created by the generator (junction/symlink followed?)"));
        }

        foreach (SkippedFeature s in manifest.Skipped)
        {
            gaps.Add(new Mismatch(s.Feature.StartsWith("known-gap:", StringComparison.Ordinal) ? s.Feature : "not-tested:" + s.Feature, "", s.Reason));
        }
    }
}
