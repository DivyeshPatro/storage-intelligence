using StorageIntelligence.Accounting;

namespace StorageIntelligence.Accounting.Tests;

/// <summary>Hand-written scenarios that document the semantics in readable form.</summary>
public sealed class AccountingScenarioTests
{
    private static readonly MeasurementPolicy Validated = new() { FileSystem = FileSystemKind.Ntfs, FileSystemValidated = true };
    private const long S = 1_048_576;

    private sealed class Harness
    {
        public InMemoryScanStore Store { get; } = new();
        public InMemoryRunStore Runs { get; } = new();
        public HardLinkRunWriter Writer { get; }
        public ScanAggregator Aggregator { get; }
        public MeasurementPolicy Policy { get; }

        public Harness(MeasurementPolicy? policy = null, bool reconcile = true)
        {
            Policy = policy ?? Validated;
            Writer = new HardLinkRunWriter(Runs, 2);
            Aggregator = new ScanAggregator(new AggregatorOptions { Policy = Policy }, Store, reconcile ? Writer : null);
            ReconcileEnabled = reconcile;
        }

        private bool ReconcileEnabled { get; }

        public void File(DirectoryId dir, string name, ulong? id, long size, MeasurementCaveats caveats = MeasurementCaveats.None, uint links = 0) =>
            Aggregator.AddFile(dir, new FileObservation(name, id is ulong v ? new FileId128(0, v) : null, size, size, caveats, links));

        public ScanAccounting Finish(bool cancel = false)
        {
            AggregationOutcome outcome = cancel ? Aggregator.Cancel() : Aggregator.Complete();
            return ScanAccounting.Build(outcome, Store, ReconcileEnabled ? Writer : null, Policy);
        }

        public DirectoryTotals Totals(ScanAccounting a, DirectoryId id) => Store.GetTotals(id, a.Reconciliation, a.HardLinks);
    }

    [Fact]
    public void Plain_files_are_summed()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "a.txt", 1, S);
        h.File(root, "b.txt", 2, 2 * S);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(3 * S, a.Root.Size.Allocated);
        Assert.Equal(0, a.Root.Size.UncertainAllocated);
        Assert.Equal(2, a.Root.DistinctFiles);
        Assert.True(a.Root.IsComplete);
    }

    [Fact]
    public void Two_links_in_one_directory_count_once()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "a.dll", 7, S);
        h.File(root, "b.dll", 7, S);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(S, a.Root.Size.Allocated);
        Assert.Equal(2, a.Root.FileEntries);
        Assert.Equal(1, a.Root.DistinctFiles);
        Assert.Equal(SizeVector.Zero, a.Root.Shared);
        Assert.Equal(1, a.Reconciliation!.FilesWithMultipleLinks);
    }

    [Fact]
    public void Links_in_sibling_folders_make_sibling_totals_non_additive()
    {
        // Like WinSxS and System32: the same file appears under two sibling folders.
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        DirectoryId sxs = h.Aggregator.DiscoverDirectory(root, "WinSxS");
        DirectoryId sys = h.Aggregator.DiscoverDirectory(root, "System32");
        h.Aggregator.EndListing(root);
        h.File(sxs, "kernel.dll", 42, 10 * S);
        h.File(sxs, "only-sxs.dll", 43, S);
        h.Aggregator.EndListing(sxs);
        h.File(sys, "kernel.dll", 42, 10 * S);
        h.Aggregator.EndListing(sys);
        ScanAccounting a = h.Finish();

        DirectoryTotals tSxs = h.Totals(a, sxs), tSys = h.Totals(a, sys);
        Assert.Equal(11 * S, tSxs.Size.Allocated);
        Assert.Equal(10 * S, tSys.Size.Allocated);
        Assert.Equal(11 * S, a.Root.Size.Allocated); // not 21 MB: siblings are not additive
        Assert.Equal(10 * S, tSxs.Shared.Allocated);
        Assert.Equal(1 * S, tSxs.Exclusive.Allocated);
        Assert.Equal(0, tSys.Exclusive.Allocated);
        Assert.Equal(SizeVector.Zero, a.Root.Shared);
        Assert.Equal(a.Root.Size.Allocated, a.ExtensionTotals[".dll"].Size.Allocated);
        Assert.Equal(3, a.ExtensionTotals[".dll"].FileEntries);
        Assert.Equal(2, a.ExtensionTotals[".dll"].DistinctFiles);
    }

    [Fact]
    public void Links_far_apart_are_reconciled_at_their_common_ancestor()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        DirectoryId x = h.Aggregator.DiscoverDirectory(root, "x");
        DirectoryId p = h.Aggregator.DiscoverDirectory(root, "p");
        h.Aggregator.EndListing(root);
        DirectoryId y = h.Aggregator.DiscoverDirectory(x, "y");
        h.Aggregator.EndListing(x);
        DirectoryId z = h.Aggregator.DiscoverDirectory(y, "z");
        h.Aggregator.EndListing(y);
        h.File(z, "f.bin", 9, 5 * S);
        h.Aggregator.EndListing(z);
        h.File(p, "f-link.bin", 9, 5 * S);
        h.Aggregator.EndListing(p);
        ScanAccounting a = h.Finish();

        Assert.Equal(5 * S, h.Totals(a, x).Size.Allocated);
        Assert.Equal(5 * S, h.Totals(a, x).Shared.Allocated);
        Assert.Equal(5 * S, a.Root.Size.Allocated);
        Assert.Equal(0, a.Root.Shared.Allocated);
    }

    [Fact]
    public void A_measured_link_count_above_observed_links_marks_the_file_shared_even_at_the_root()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("D:\\scoped");
        h.File(root, "pkg.tgz", 5, 3 * S, links: 2);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(3 * S, a.Root.Size.Allocated);
        Assert.Equal(3 * S, a.Root.Shared.Allocated);
        Assert.Equal(0, a.Root.Exclusive.Allocated);
        Assert.Equal(1, a.Reconciliation!.FilesWithLinksOutsideScope);
    }

    [Fact]
    public void Links_that_disagree_on_size_use_the_largest_and_are_marked_uncertain()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "log.txt", 3, S);
        h.File(root, "log-link.txt", 3, 2 * S); // the file grew between the two observations
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(2 * S, a.Root.Size.Allocated);
        Assert.Equal(2 * S, a.Root.Size.UncertainAllocated);
        Assert.Equal(1, a.Reconciliation!.FilesWithInconsistentLinkSizes);
    }

    [Fact]
    public void Conservative_policy_marks_everything_uncertain_until_validated()
    {
        var h = new Harness(MeasurementPolicy.Conservative);
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "a.txt", 1, S);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(S, a.Root.Size.UncertainAllocated);
        Assert.Equal(1, a.Aggregation.FileEntriesByFlag[MeasurementCaveats.UnvalidatedFileSystem]);
    }

    [Theory]
    [InlineData(MeasurementCaveats.Sparse)]
    [InlineData(MeasurementCaveats.Compressed)]
    [InlineData(MeasurementCaveats.WofCompressed)]
    [InlineData(MeasurementCaveats.CloudPlaceholder)]
    [InlineData(MeasurementCaveats.ChangedDuringScan)]
    public void Unvalidated_attribute_cases_are_uncertain_on_a_validated_filesystem(MeasurementCaveats caveat)
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "plain.bin", 1, S);
        h.File(root, "special.bin", 2, 4 * S, caveat);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(5 * S, a.Root.Size.Allocated);
        Assert.Equal(4 * S, a.Root.Size.UncertainAllocated);
    }

    [Fact]
    public void Missing_file_id_on_a_hard_link_filesystem_is_uncertain()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "noid.bin", null, S);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(S, a.Root.Size.UncertainAllocated);
    }

    [Fact]
    public void Exfat_has_no_hard_links_so_reconciliation_is_not_applicable()
    {
        var policy = new MeasurementPolicy { FileSystem = FileSystemKind.ExFat, FileSystemValidated = true, FileSystemSupportsHardLinks = false };
        var store = new InMemoryScanStore();
        var agg = new ScanAggregator(new AggregatorOptions { Policy = policy }, store, links: null);
        DirectoryId root = agg.BeginRoot("E:");
        agg.AddFile(root, new FileObservation("a.jpg", null, S, S));
        agg.EndListing(root);
        ScanAccounting a = ScanAccounting.Build(agg.Complete(), store, null, policy);

        Assert.Equal(HardLinkStatus.NotApplicable, a.HardLinks);
        Assert.Equal(0, a.Root.Size.UncertainAllocated);
        Assert.True(a.Root.IsComplete);
    }

    [Fact]
    public void Without_reconciliation_on_ntfs_totals_are_flagged_as_not_reconciled()
    {
        var h = new Harness(reconcile: false);
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.File(root, "a.dll", 7, S);
        h.File(root, "b.dll", 7, S);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(HardLinkStatus.NotReconciled, a.HardLinks);
        Assert.Equal(2 * S, a.Root.Size.Allocated); // upper bound, explicitly labelled
        Assert.False(a.Root.IsComplete);
    }

    [Fact]
    public void Junctions_are_counted_in_coverage_and_contribute_no_bytes()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        h.Aggregator.AddNotFollowedLink(root, "Documents and Settings", LinkKind.Junction);
        h.Aggregator.EndListing(root);
        ScanAccounting a = h.Finish();

        Assert.Equal(1, a.Root.Coverage.NotFollowedLinks);
        Assert.Equal(0, a.Root.Size.Allocated);
        Assert.True(a.Root.Coverage.IsComplete); // not following a link is policy, not a gap
    }

    [Fact]
    public void Inaccessible_and_incomplete_directories_make_coverage_incomplete()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        DirectoryId svi = h.Aggregator.DiscoverDirectory(root, "System Volume Information");
        DirectoryId data = h.Aggregator.DiscoverDirectory(root, "data");
        h.Aggregator.EndListing(root);
        h.Aggregator.MarkInaccessible(svi);
        h.Aggregator.EndListing(svi);
        h.File(data, "a.bin", 1, S);
        h.Aggregator.MarkIncomplete(data);
        h.Aggregator.EndListing(data);
        ScanAccounting a = h.Finish();

        Assert.Equal(1, a.Root.Coverage.InaccessibleDirectories);
        Assert.Equal(1, a.Root.Coverage.IncompleteDirectories);
        Assert.False(a.Root.IsComplete);
        Assert.Equal(S, a.Root.Size.Allocated); // a lower bound, because coverage is incomplete
    }

    [Fact]
    public void Cancel_finalizes_open_directories_and_counts_them()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        DirectoryId a1 = h.Aggregator.DiscoverDirectory(root, "a");
        _ = h.Aggregator.DiscoverDirectory(root, "never-listed");
        h.File(root, "r.bin", 1, S);
        h.File(a1, "x.bin", 2, S);
        ScanAccounting a = h.Finish(cancel: true);

        Assert.Equal(ScanCompletion.Cancelled, a.Aggregation.Completion);
        Assert.Equal(3, a.Root.Coverage.CancelledDirectories);
        Assert.Equal(2 * S, a.Root.Size.Allocated);
        Assert.False(a.Root.IsComplete);
        Assert.Equal(3, h.Store.Records.Count);
    }

    [Fact]
    public void Directories_are_emitted_in_post_order_and_forgotten()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        DirectoryId a = h.Aggregator.DiscoverDirectory(root, "a");
        DirectoryId b = h.Aggregator.DiscoverDirectory(a, "b");
        h.Aggregator.EndListing(root);
        h.Aggregator.EndListing(a);
        Assert.Equal(3, h.Aggregator.OpenDirectoryCount);
        h.Aggregator.EndListing(b);

        Assert.Equal([b, a, root], h.Store.CompletionOrder);
        Assert.Equal(0, h.Aggregator.OpenDirectoryCount);
    }

    [Fact]
    public void Protocol_violations_throw()
    {
        var h = new Harness();
        DirectoryId root = h.Aggregator.BeginRoot("C:");
        DirectoryId a = h.Aggregator.DiscoverDirectory(root, "a");
        h.Aggregator.EndListing(root);

        Assert.Throws<InvalidOperationException>(() => h.Aggregator.AddFile(root, new FileObservation("late.txt", null, 1, 1)));
        Assert.Throws<InvalidOperationException>(() => h.Aggregator.Complete());
        Assert.Throws<InvalidOperationException>(() => h.Aggregator.BeginRoot("again"));
        Assert.Throws<ArgumentOutOfRangeException>(() => h.Aggregator.AddFile(a, new FileObservation("neg", null, -1, 0)));
        h.Aggregator.EndListing(a);
        Assert.Throws<InvalidOperationException>(() => h.Aggregator.EndListing(a));
        _ = h.Aggregator.Complete();
        Assert.Throws<InvalidOperationException>(() => h.Aggregator.Cancel());
    }

    [Fact]
    public void Very_deep_trees_do_not_overflow_the_stack()
    {
        var h = new Harness();
        DirectoryId current = h.Aggregator.BeginRoot("C:");
        var chain = new List<DirectoryId> { current };
        for (int i = 0; i < 20_000; i++)
        {
            DirectoryId next = h.Aggregator.DiscoverDirectory(current, "d");
            if (i == 1)
            {
                h.File(current, "deep-link.bin", 1, S);
            }

            h.Aggregator.EndListing(current);
            current = next;
            chain.Add(current);
        }

        h.File(current, "deep.bin", 1, S);
        h.Aggregator.EndListing(current);
        ScanAccounting a = h.Finish();

        Assert.Equal(S, a.Root.Size.Allocated);
        Assert.Equal(20_000, a.Root.Directories);
    }

    [Fact]
    public void Depth_first_traversal_keeps_open_directories_bounded()
    {
        // 11,111 directories (fan-out 10, depth 4). Depth-first, at most depth × fan-out + 1 are open.
        var store = new InMemoryScanStore();
        var agg = new ScanAggregator(new AggregatorOptions { Policy = Validated }, store, links: null);
        var stack = new Stack<(DirectoryId Id, int Depth)>();
        stack.Push((agg.BeginRoot("C:"), 0));
        while (stack.TryPop(out (DirectoryId Id, int Depth) d))
        {
            if (d.Depth < 4)
            {
                for (int i = 0; i < 10; i++)
                {
                    stack.Push((agg.DiscoverDirectory(d.Id, $"c{i}"), d.Depth + 1));
                }
            }

            agg.AddFile(d.Id, new FileObservation("f.txt", null, 10, 4096));
            agg.EndListing(d.Id);
        }

        AggregationOutcome outcome = agg.Complete();
        Assert.Equal(11_111, store.Records.Count);
        Assert.InRange(outcome.PeakOpenDirectories, 1, 4 * 10 + 1);
    }

    [Fact]
    public void Large_files_are_retained_for_listing()
    {
        var store = new InMemoryScanStore();
        var agg = new ScanAggregator(new AggregatorOptions { Policy = Validated, RetainFileThreshold = 10 * S }, store, links: null);
        DirectoryId root = agg.BeginRoot("C:");
        agg.AddFile(root, new FileObservation("small.bin", null, S, S));
        agg.AddFile(root, new FileObservation("big.iso", null, 10 * S, 10 * S));
        agg.EndListing(root);
        _ = agg.Complete();

        Assert.Equal("big.iso", Assert.Single(store.RetainedFiles).File.Name);
    }
}
