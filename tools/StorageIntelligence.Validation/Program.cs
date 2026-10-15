using System.Text.Json;
using System.Text.Json.Serialization;
using StorageIntelligence.Accounting;
using StorageIntelligence.Scanning.Windows;
using StorageIntelligence.Validation;

// si-validate: M0b validation tool. Commands:
//   fixture         --root <dir> --seed <n> --fs <name> --manifest <file>      (guarded write)
//   scan            --root <dir> [--mode low-impact|fast] [--link-counts none|retained|all] --out <file>
//   validate        --root <dir> --fs <name> --manifest <file> --out-dir <dir>
//   allocation      --root <dir> --fs <name> --cluster <bytes> --out <file>   (guarded write)
//   release-denials --manifest <file>
// Exit codes: 0 success, 1 validation mismatch, 2 usage or environment error.

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};

try
{
    return args.Length == 0 ? Usage() : args[0] switch
    {
        "fixture" => Fixture(),
        "scan" => ScanCommand(),
        "validate" => Validate(),
        "allocation" => Allocation(),
        "release-denials" => ReleaseDenials(),
        _ => Usage(),
    };
}
catch (InvalidOperationException e) when (e.Message.StartsWith("Refusing", StringComparison.Ordinal))
{
    Console.Error.WriteLine(e.Message);
    return 2;
}

static void Progress(string message) =>
    Console.Error.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");

int Usage()
{
    Console.Error.WriteLine("usage: si-validate fixture|scan|validate|allocation|release-denials [options]");
    return 2;
}

string Arg(string name, string? fallback = null)
{
    int i = Array.IndexOf(args, "--" + name);
    if (i >= 0 && i + 1 < args.Length)
    {
        return args[i + 1];
    }

    return fallback ?? throw new ArgumentException($"missing --{name}");
}

int Fixture()
{
    FixtureManifest manifest = FixtureGenerator.Generate(Arg("root"), int.Parse(Arg("seed", "1"), System.Globalization.CultureInfo.InvariantCulture), Arg("fs"));
    Write(Arg("manifest"), manifest);
    Console.WriteLine($"fixture: {manifest.Files.Count} files, {manifest.Directories.Count} directories, {manifest.Links.Count} links, {manifest.Skipped.Count} skipped/known-gap features");
    foreach (SkippedFeature s in manifest.Skipped)
    {
        Console.WriteLine($"  skipped {s.Feature}: {s.Reason}");
    }

    return 0;
}

int ScanCommand()
{
    var scanOptions = new ScanOptions
    {
        Mode = ParseMode(Arg("mode", "low-impact")),
        LinkCounts = ParseLinks(Arg("link-counts", "retained")),
        TempDirectory = Path.GetTempPath(),
    };
    ScanResult result = WindowsScanner.Scan(Arg("root"), scanOptions);
    Write(Arg("out"), Report(result));
    Console.WriteLine(Summary(result));
    return 0;
}

int Validate()
{
    string fixtureRoot = Path.Combine(Path.GetFullPath(Arg("root")), "fixture");
    string outDir = Arg("out-dir");
    Directory.CreateDirectory(outDir);
    FixtureManifest manifest = JsonSerializer.Deserialize<FixtureManifest>(File.ReadAllText(Arg("manifest")), options)!;
    string fs = Arg("fs");

    Progress("reference scan");
    ReferenceResult reference = ReferenceScanner.Scan(fixtureRoot, fs);
    Progress($"reference scan done: {reference.Files.Count} files, {reference.Directories.Count} directories");
    Write(Path.Combine(outDir, "reference.json"), reference);

    var lines = new List<string> { $"## Scanner validation: {fs}", "", "| Scan | Directories | Files | Mismatches | Result |", "|---|---|---|---|---|" };
    var results = new Dictionary<string, ScanResult>();
    bool passed = true;
    foreach ((ScanMode mode, LinkCountMeasurement links) in new[]
    {
        (ScanMode.LowImpact, LinkCountMeasurement.RetainedFiles),
        (ScanMode.Fast, LinkCountMeasurement.RetainedFiles),
        (ScanMode.LowImpact, LinkCountMeasurement.All),
        (ScanMode.Fast, LinkCountMeasurement.All),
    })
    {
        var files = new List<ProductionFile>();
        var scanOptions = new ScanOptions
        {
            Mode = mode,
            LinkCounts = links,
            TempDirectory = Path.GetTempPath(),
            FileObserver = (path, o, caveats) =>
            {
                lock (files)
                {
                    files.Add(new ProductionFile(path, o.LogicalSize, o.AllocatedSize, caveats.ToString(), o.LinkCount));
                }
            },
        };
        Progress($"production scan {mode}/{links}");
        ScanResult scan = WindowsScanner.Scan(fixtureRoot, scanOptions);
        string name = $"{mode}-{links}";
        Progress($"production scan {name} done in {scan.Duration.TotalMilliseconds:F0} ms");
        results[name] = scan;
        Write(Path.Combine(outDir, $"scan-{name}.json"), Report(scan));
        Write(Path.Combine(outDir, $"files-{name}.json"), files);

        ComparisonResult comparison = Comparison.Compare(scan, files, reference, manifest);
        Write(Path.Combine(outDir, $"compare-{name}.json"), comparison);
        passed &= comparison.Passed;
        lines.Add($"| {name} | {comparison.DirectoriesCompared} | {comparison.FilesCompared} | {comparison.Mismatches.Count} | {(comparison.Passed ? "pass" : "**FAIL**")} |");
        foreach (Mismatch m in comparison.Mismatches.Take(25))
        {
            lines.Add($"|  | {m.Category} | `{m.Path}` | {m.Detail.Replace("|", "/", StringComparison.Ordinal)} | |");
        }
    }

    lines.Add("");
    lines.Add("### Scan-mode equivalence (stable fixture)");
    foreach ((string a, string b) in new[] { ("LowImpact-RetainedFiles", "Fast-RetainedFiles"), ("LowImpact-All", "Fast-All") })
    {
        List<string> differences = ModeDifferences(results[a], results[b]);
        passed &= differences.Count == 0;
        lines.Add($"- {a} vs {b}: {(differences.Count == 0 ? "identical" : $"**{differences.Count} differences**:")}");
        foreach (string difference in differences.Take(20))
        {
            lines.Add($"  - {difference}");
        }
    }

    lines.Add("");
    lines.Add("### Features not tested or known gaps");
    foreach (SkippedFeature s in manifest.Skipped)
    {
        lines.Add($"- {s.Feature}: {s.Reason}");
    }

    if (manifest.Skipped.Count == 0)
    {
        lines.Add("- none");
    }

    File.WriteAllLines(Path.Combine(outDir, "summary.md"), lines);
    foreach (string line in lines)
    {
        Console.WriteLine(line);
    }

    return passed ? 0 : 1;
}

int Allocation()
{
    AllocationReport report = AllocationHarness.Run(Arg("root"), Arg("fs"), long.Parse(Arg("cluster"), System.Globalization.CultureInfo.InvariantCulture));
    Write(Arg("out"), report);
    Console.WriteLine($"## Allocation semantics: {report.FileSystem} (cluster {report.ClusterSize}, tolerance {report.ToleranceBytes} bytes)");
    Console.WriteLine();
    Console.WriteLine("| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |");
    Console.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|");
    foreach (AllocationCaseResult c in report.Cases)
    {
        if (c.Trials.Count == 0)
        {
            Console.WriteLine($"| {c.Case} | - | - | - | - | - | - | - | - | - | {c.Verdict}: {c.Detail} |");
        }

        foreach (AllocationTrial t in c.Trials)
        {
            Console.WriteLine($"| {t.Case} | {t.Trial} | {t.Logical} | {t.FreeSpaceDelta} | {t.FreeSpaceDeltaAfterFlush} | {t.ListingAllocated} | {t.HandleAllocated} | {t.CompressedSize} | {t.WofExternal} | {t.ListingAttributes} | {t.ListingMatchesFreeSpace} / {t.HandleMatchesFreeSpace} / {t.CompressedSizeMatchesFreeSpace} |");
        }

        Console.WriteLine($"| **{c.Case}** | | | | | | | | | | **{c.Verdict}** ({string.Join(", ", c.ConsistentSources ?? [])}) |");
    }

    return 0;
}

int ReleaseDenials()
{
    FixtureManifest manifest = JsonSerializer.Deserialize<FixtureManifest>(File.ReadAllText(Arg("manifest")), options)!;
    FixtureGenerator.ReleaseDenials(manifest);
    return 0;
}

static List<string> ModeDifferences(ScanResult a, ScanResult b)
{
    var differences = new List<string>();
    var totalsB = b.DirectoryPaths.ToDictionary(kv => kv.Value, kv => Strip(b.Store.GetTotals(kv.Key, b.Accounting.Reconciliation, b.Accounting.HardLinks)), StringComparer.Ordinal);
    foreach ((DirectoryId id, string path) in a.DirectoryPaths)
    {
        DirectoryTotals ta = Strip(a.Store.GetTotals(id, a.Accounting.Reconciliation, a.Accounting.HardLinks));
        if (!totalsB.TryGetValue(path, out DirectoryTotals? tb))
        {
            differences.Add($"missing {path}");
        }
        else if (ta != tb)
        {
            differences.Add($"{(path.Length == 0 ? "(root)" : path)}: {FieldDifferences(ta, tb)}");
        }
    }

    if (a.DirectoryPaths.Count != b.DirectoryPaths.Count)
    {
        differences.Add($"directory count {a.DirectoryPaths.Count} vs {b.DirectoryPaths.Count}");
    }

    return differences;

    static DirectoryTotals Strip(DirectoryTotals t) => t with { Id = default, Parent = null };

    // Names every differing field, so a mode difference is diagnosable from the summary alone.
    static string FieldDifferences(DirectoryTotals a, DirectoryTotals b)
    {
        var parts = new List<string>();
        foreach (System.Reflection.PropertyInfo property in typeof(DirectoryTotals).GetProperties())
        {
            object? va = property.GetValue(a), vb = property.GetValue(b);
            if (!Equals(va, vb))
            {
                parts.Add($"{property.Name} {va} vs {vb}");
            }
        }

        return string.Join("; ", parts);
    }
}

object Report(ScanResult r) => new
{
    r.Root,
    r.Volume,
    Mode = r.Options.Mode,
    LinkCounts = r.Options.LinkCounts,
    r.StartedUtc,
    DurationMs = r.Duration.TotalMilliseconds,
    HardLinks = r.Accounting.HardLinks,
    r.Accounting.ReconciliationFailure,
    r.Accounting.ReconciliationFailureDetail,
    Reconciliation = r.Accounting.Reconciliation is null ? null : new
    {
        r.Accounting.Reconciliation.FilesWithMultipleLinks,
        r.Accounting.Reconciliation.FilesWithLinksOutsideScope,
        r.Accounting.Reconciliation.FilesWithInconsistentLinkSizes,
        r.Accounting.Reconciliation.RecordsProcessed,
    },
    r.Accounting.Aggregation.PeakOpenDirectories,
    r.Accounting.Aggregation.LinkRecordsWritten,
    r.LinkCountFailures,
    r.AllocationCorrections,
    r.IssueCount,
    r.Issues,
    Directories = r.DirectoryPaths.OrderBy(kv => kv.Value, StringComparer.Ordinal).Select(kv =>
    {
        DirectoryTotals t = r.Store.GetTotals(kv.Key, r.Accounting.Reconciliation, r.Accounting.HardLinks);
        return new
        {
            Path = kv.Value,
            t.Size,
            t.Shared,
            t.ExclusiveVerified,
            t.LinkStatusUnknown,
            t.FileEntries,
            t.DistinctFiles,
            t.Directories,
            t.Coverage,
            t.IsComplete,
        };
    }),
    Extensions = r.Accounting.ExtensionTotals.OrderByDescending(kv => kv.Value.Size.Allocated),
};

static string Summary(ScanResult r) =>
    $"{r.Root} [{r.Volume.FileSystemName}] {r.Options.Mode}/{r.Options.LinkCounts}: {r.Accounting.Root.Size.Allocated} bytes allocated " +
    $"({r.Accounting.Root.Size.UncertainAllocated} uncertain), {r.Accounting.Root.DistinctFiles} files, links {r.Accounting.HardLinks}, " +
    $"complete={r.Accounting.Root.IsComplete}, {r.Duration.TotalMilliseconds:F0} ms";

void Write<T>(string path, T value) => File.WriteAllText(path, JsonSerializer.Serialize(value, options));

static ScanMode ParseMode(string s) => s switch
{
    "low-impact" => ScanMode.LowImpact,
    "fast" => ScanMode.Fast,
    _ => throw new ArgumentException($"unknown mode {s}"),
};

static LinkCountMeasurement ParseLinks(string s) => s switch
{
    "none" => LinkCountMeasurement.None,
    "retained" => LinkCountMeasurement.RetainedFiles,
    "all" => LinkCountMeasurement.All,
    _ => throw new ArgumentException($"unknown link-count mode {s}"),
};
