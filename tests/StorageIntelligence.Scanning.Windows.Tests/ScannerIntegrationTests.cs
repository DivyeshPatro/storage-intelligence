using StorageIntelligence.Accounting;
using StorageIntelligence.Validation;

namespace StorageIntelligence.Scanning.Windows.Tests;

/// <summary>
/// Integration tests against a fixture built in a guarded temp test root (never a real user folder).
/// They run on Windows CI; on other platforms they are reported as skipped.
/// </summary>
public sealed class ScannerIntegrationTests : IDisposable
{
    private readonly string _root;
    private readonly FixtureManifest? _manifest;

    public ScannerIntegrationTests()
    {
        if (!OperatingSystem.IsWindows())
        {
            _root = "";
            return;
        }

        _root = TestRootGuard.CreateTempTestRoot();
        _manifest = FixtureGenerator.Generate(_root, seed: 11, "NTFS");
    }

    private string Fixture => Path.Combine(_root, "fixture");

    public void Dispose()
    {
        if (_manifest is not null)
        {
            FixtureGenerator.ReleaseDenials(_manifest);
            Directory.Delete(@"\\?\" + _root, recursive: true);
        }
    }

    [WindowsFact]
    public void Guard_refuses_anything_but_marked_test_roots()
    {
        Assert.Throws<InvalidOperationException>(() => TestRootGuard.AssertWritable(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
        Assert.Throws<InvalidOperationException>(() => TestRootGuard.AssertWritable(Path.GetTempPath()));
        Assert.Throws<InvalidOperationException>(() => TestRootGuard.AssertWritable(@"C:\"));
        string unmarked = Path.Combine(Path.GetTempPath(), "si-test-unmarked-" + Guid.NewGuid().ToString("N")[..6]);
        Directory.CreateDirectory(unmarked);
        try
        {
            Assert.Throws<InvalidOperationException>(() => TestRootGuard.AssertWritable(unmarked));
        }
        finally
        {
            Directory.Delete(unmarked);
        }

        TestRootGuard.AssertWritable(_root);
    }

    [WindowsFact]
    public void Production_scan_matches_independent_reference_in_every_mode()
    {
        ReferenceResult reference = ReferenceScanner.Scan(Fixture, "NTFS");
        foreach (ScanMode mode in new[] { ScanMode.LowImpact, ScanMode.Fast })
        {
            foreach (LinkCountMeasurement links in new[] { LinkCountMeasurement.RetainedFiles, LinkCountMeasurement.All })
            {
                var files = new List<ProductionFile>();
                ScanResult scan = WindowsScanner.Scan(Fixture, new ScanOptions
                {
                    Mode = mode,
                    LinkCounts = links,
                    FileObserver = (p, o, c) => { lock (files) { files.Add(new ProductionFile(p, o.LogicalSize, o.AllocatedSize, c.ToString(), o.LinkCount)); } },
                });
                ComparisonResult result = Comparison.Compare(scan, files, reference, _manifest);
                Assert.True(result.Passed, $"{mode}/{links}: " + string.Join("\n", result.Mismatches.Take(30).Select(m => $"{m.Category} {m.Path}: {m.Detail}")));
            }
        }
    }

    [WindowsFact]
    public void Scanning_does_not_modify_anything()
    {
        ReferenceResult before = ReferenceScanner.Scan(Fixture, "NTFS");
        Dictionary<string, (long, long, uint)> snapshot = Snapshot(Fixture);
        _ = WindowsScanner.Scan(Fixture, new ScanOptions { Mode = ScanMode.Fast, LinkCounts = LinkCountMeasurement.All });
        ReferenceResult after = ReferenceScanner.Scan(Fixture, "NTFS");

        Assert.Equal(snapshot, Snapshot(Fixture));
        Assert.Equal(
            before.Files.Select(f => (f.Path, f.Logical, f.Allocated, f.Links, f.Attributes)),
            after.Files.Select(f => (f.Path, f.Logical, f.Allocated, f.Links, f.Attributes)));
    }

    [WindowsFact]
    public void Junction_loop_terminates_and_link_targets_are_not_counted()
    {
        ScanResult scan = WindowsScanner.Scan(Fixture, new ScanOptions());
        Assert.DoesNotContain(scan.DirectoryPaths.Values, p => p.StartsWith("junctions\\", StringComparison.Ordinal) && p != "junctions");
        Assert.DoesNotContain(scan.DirectoryPaths.Values, p => p.StartsWith("symlinks\\", StringComparison.Ordinal));
        Assert.True(scan.Accounting.Root.Coverage.NotFollowedLinks >= 2);
    }

    [WindowsFact]
    public void Changes_during_a_listing_are_reported_and_counted_as_uncertain()
    {
        string target = Path.Combine(Fixture, "links", "y.bin");
        ScanResult scan = WindowsScanner.Scan(Fixture, new ScanOptions
        {
            PolicyOverride = MeasurementPolicy.Conservative with { FileSystem = FileSystemKind.Ntfs, FileSystemValidated = true, UncertainReasons = MeasurementCaveats.None },
            AfterDirectoryOpened = rel =>
            {
                if (rel == "links")
                {
                    File.AppendAllText(target, "grown during scan");
                    File.WriteAllText(Path.Combine(Fixture, "links", "new-during-scan.txt"), "new");
                }
            },
        });

        DirectoryTotals links = Totals(scan, "links");
        Assert.False(links.IsStable);
        Assert.Equal(1, links.Coverage.ChangedDirectories);
        Assert.Contains(scan.Issues, i => i.Kind == "changed-during-scan" && i.RelativePath == "links");
        Assert.True(links.Size.UncertainAllocated > 0, "the modified file's bytes must be uncertain even though the policy trusts plain files");
    }

    [WindowsFact]
    public void Cancellation_returns_a_partial_result_with_cancelled_directories()
    {
        using var cts = new CancellationTokenSource();
        ScanResult scan = WindowsScanner.Scan(Fixture, new ScanOptions
        {
            AfterDirectoryOpened = rel =>
            {
                if (rel == "plain")
                {
                    cts.Cancel();
                }
            },
        }, cts.Token);

        Assert.Equal(ScanCompletion.Cancelled, scan.Accounting.Aggregation.Completion);
        Assert.True(scan.Accounting.Root.Coverage.CancelledDirectories > 0);
        Assert.False(scan.Accounting.Root.IsComplete);
    }

    [WindowsFact]
    public void Insufficient_temp_space_is_reported_as_not_reconciled()
    {
        ScanResult scan = WindowsScanner.Scan(Fixture, new ScanOptions
        {
            TempDirectory = _root,
            TempReserveBytes = long.MaxValue / 2,
            LinkRunBufferRecords = 16,
        });

        Assert.Equal(HardLinkStatus.NotReconciled, scan.Accounting.HardLinks);
        Assert.Equal(ReconciliationFailure.InsufficientTempSpace, scan.Accounting.ReconciliationFailure);
        Assert.False(scan.Accounting.Root.IsComplete);
    }

    [WindowsFact]
    public void Unmeasured_small_links_are_unknown_but_measured_large_links_are_shared()
    {
        ScanResult scan = WindowsScanner.Scan(Fixture, new ScanOptions { LinkCounts = LinkCountMeasurement.RetainedFiles });
        DirectoryTotals root = scan.Accounting.Root;
        // links\big.bin (12 MiB, retained) has a measured second link outside the fixture: shared.
        Assert.True(root.Shared.Allocated >= 12L << 20);
        // links\z.bin (300 KB, not retained) also has an outside link but its count was not measured: unknown, never exclusive.
        Assert.True(root.LinkStatusUnknown.Allocated >= 300_000);
    }

    private static DirectoryTotals Totals(ScanResult scan, string path)
    {
        DirectoryId id = scan.DirectoryPaths.Single(kv => kv.Value == path).Key;
        return scan.Store.GetTotals(id, scan.Accounting.Reconciliation, scan.Accounting.HardLinks);
    }

    /// <summary>Size, last-write time and attributes of every entry, walking without entering reparse points.</summary>
    private static Dictionary<string, (long, long, uint)> Snapshot(string root)
    {
        var result = new Dictionary<string, (long, long, uint)>(StringComparer.Ordinal);
        var pending = new Stack<DirectoryInfo>();
        pending.Push(new DirectoryInfo(@"\\?\" + root));
        var options = new EnumerationOptions { AttributesToSkip = 0, IgnoreInaccessible = true };
        while (pending.Count > 0)
        {
            foreach (FileSystemInfo info in pending.Pop().EnumerateFileSystemInfos("*", options))
            {
                bool directory = info.Attributes.HasFlag(FileAttributes.Directory);
                result[info.FullName] = (directory ? 0 : ((FileInfo)info).Length, info.LastWriteTimeUtc.Ticks, (uint)info.Attributes);
                if (directory && !info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    pending.Push((DirectoryInfo)info);
                }
            }
        }

        return result;
    }
}
