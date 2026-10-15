using CsCheck;
using StorageIntelligence.Accounting;
using StorageIntelligence.Accounting.Tests.Support;

namespace StorageIntelligence.Accounting.Tests;

/// <summary>
/// Property-based tests of the accounting core against the brute-force <see cref="Oracle"/>.
/// Iteration count defaults to a CI-friendly value; set SI_PBT_ITER to run more (e.g. 1000000).
/// </summary>
public sealed class AccountingPropertyTests
{
    private static readonly long Iterations =
        long.TryParse(Environment.GetEnvironmentVariable("SI_PBT_ITER"), out long n) ? n : 3_000;

    private static string Describe((TreeModel Tree, MeasurementPolicy Policy, int Seed, int Buffer) c) =>
        $"policy={c.Policy}\nseed={c.Seed} buffer={c.Buffer}\n{c.Tree}";

    private static readonly Gen<(TreeModel Tree, MeasurementPolicy Policy, int Seed, int Buffer)> Case =
        from s in Generators.Scenario
        from seed in Gen.Int
        from buffer in Gen.Int[1, 16]
        select (s.Tree, s.Policy, seed, buffer);

    /// <summary>I1 + I2: every directory's distinct, shared, entry and coverage totals equal the oracle's.</summary>
    [Fact]
    public void Totals_match_oracle_for_complete_scans() =>
        Case.Sample(c =>
        {
            DriverResult run = ScanDriver.Run(c.Tree, c.Policy, c.Seed, runBufferCapacity: c.Buffer);
            Assert.False(run.Log.Cancelled);
            AssertMatchesOracle(run);
        }, iter: Iterations, print: Describe);

    /// <summary>I7: a cancelled scan accounts exactly for what was reported and counts what was left unfinished.</summary>
    [Fact]
    public void Totals_match_oracle_for_cancelled_scans() =>
        Case.SelectMany(c => Gen.Int[0, 200].Select(cancel => (c, cancel)))
            .Sample(x =>
            {
                DriverResult run = ScanDriver.Run(
                    x.c.Tree, x.c.Policy, x.c.Seed, runBufferCapacity: x.c.Buffer, cancelAfterSteps: x.cancel);
                AssertMatchesOracle(run);
                if (run.Log.Cancelled)
                {
                    Assert.Equal(run.Log.Discovered.Count - run.Log.Ended.Count, run.Accounting.Root.Coverage.CancelledDirectories);
                }
            }, iter: Iterations, print: x => $"cancelAfter={x.cancel}\n{Describe(x.c)}");

    /// <summary>
    /// I3: on a stable fixture, totals do not depend on event order, traversal strategy or how the sort spills.
    /// Truncated listings are excluded: which entries a cut-short listing observes depends on timing, which is
    /// exactly the unstable case where only the oracle comparison (not cross-run identity) is required.
    /// </summary>
    [Fact]
    public void Totals_are_independent_of_order_and_spill_size_on_stable_fixtures() =>
        Case.Select(c => c with { Tree = Stable(c.Tree) }).Sample(c =>
        {
            DriverResult a = ScanDriver.Run(c.Tree, c.Policy, c.Seed, runBufferCapacity: c.Buffer);
            DriverResult b = ScanDriver.Run(c.Tree, c.Policy, unchecked(c.Seed * 31 + 7), runBufferCapacity: 1);
            DriverResult d = ScanDriver.Run(c.Tree, c.Policy, c.Seed, TraversalOrder.DepthFirst, runBufferCapacity: 1 << 16);
            foreach (int dir in a.Log.Discovered.Keys)
            {
                DirectoryTotals ta = Strip(a.TotalsFor(dir));
                Assert.Equal(ta, Strip(b.TotalsFor(dir)));
                Assert.Equal(ta, Strip(d.TotalsFor(dir)));
            }

            Assert.Equal(Sorted(a.Accounting.ExtensionTotals), Sorted(b.Accounting.ExtensionTotals));
            Assert.Equal(Sorted(a.Accounting.ExtensionTotals), Sorted(d.Accounting.ExtensionTotals));
        }, iter: Iterations / 3, print: Describe);

    /// <summary>
    /// Structural invariants that follow from the semantics, checked independently of the oracle:
    /// shared ⊆ size; uncertain ⊆ allocated; parent ≤ own + Σ children (sub-additive, not additive);
    /// exclusive is super-additive; extension totals sum to the root.
    /// </summary>
    [Fact]
    public void Structural_invariants_hold() =>
        Case.Sample(c =>
        {
            DriverResult run = ScanDriver.Run(c.Tree, c.Policy, c.Seed, runBufferCapacity: c.Buffer);
            var totals = run.Store.GetAllTotals(run.Accounting.Reconciliation, run.Accounting.HardLinks)
                .ToDictionary(t => t.Id);
            var children = totals.Values.Where(t => t.Parent is not null).ToLookup(t => t.Parent!.Value);

            foreach (DirectoryTotals t in totals.Values)
            {
                AssertWithin(SizeVector.Zero, t.Shared, t.Size);
                Assert.InRange(t.Size.UncertainAllocated, 0, t.Size.Allocated);
                Assert.InRange(t.SharedFiles, 0, t.DistinctFiles);
                Assert.InRange(t.DistinctFiles, 0, t.FileEntries);

                DirectoryRecord record = run.Store.Records[t.Id];
                SizeVector childSum = children[t.Id].Aggregate(record.Own, (acc, ch) => acc + ch.Size);
                SizeVector childExclusive = children[t.Id].Aggregate(SizeVector.Zero, (acc, ch) => acc + ch.Exclusive);
                // Own is per-entry; when links disagree on size the canonical size can exceed an entry's size,
                // so sub-additivity is only a theorem for consistent link sizes.
                if (run.Accounting.Reconciliation is null or { FilesWithInconsistentLinkSizes: 0 })
                {
                    Assert.True(t.Size.Allocated <= childSum.Allocated && t.Size.Logical <= childSum.Logical, "sub-additivity");
                }

                Assert.True(t.Exclusive.Allocated >= childExclusive.Allocated, "exclusive super-additivity");
            }

            DirectoryTotals root = run.Accounting.Root;
            ExtensionStats extSum = run.Accounting.ExtensionTotals.Values.Aggregate(default(ExtensionStats), (a, e) => a + e);
            Assert.Equal(root.Size, extSum.Size);
            Assert.Equal(root.DistinctFiles, extSum.DistinctFiles);
            Assert.Equal(root.FileEntries, extSum.FileEntries);
        }, iter: Iterations, print: Describe);

    /// <summary>The external sort yields every record exactly once, grouped by file ID, for any buffer size.</summary>
    [Fact]
    public void Run_writer_groups_all_records_by_file_id() =>
        Gen.Select(Gen.Int[0, 40].Array[0, 300], Gen.Int[1, 50])
            .Sample(x =>
            {
                (int[] keys, int buffer) = x;
                using var store = new InMemoryRunStore();
                var writer = new HardLinkRunWriter(store, buffer);
                for (int i = 0; i < keys.Length; i++)
                {
                    writer.Add(new LinkRecord(TreeModel.IdFor(keys[i]), new DirectoryId(i), i, i, MeasurementCaveats.None, 0, 0));
                }

                writer.Finish();
                var groups = writer.EnumerateGroups().Select(g => g.ToList()).ToList();
                Assert.Equal(keys.Length, groups.Sum(g => g.Count));
                Assert.All(groups, g => Assert.Single(g.Select(r => r.FileId).Distinct()));
                Assert.Equal(groups.Count, groups.Select(g => g[0].FileId).Distinct().Count());
                Assert.Equal(groups.Select(g => g[0].FileId).Order(), groups.Select(g => g[0].FileId));
                Assert.Equal(keys.Distinct().Count(), groups.Count);
                Assert.Equal((keys.Length + buffer - 1) / buffer, writer.RunCount);
            }, iter: Iterations);

    internal static void AssertMatchesOracle(DriverResult run)
    {
        foreach (int dir in run.Log.Discovered.Keys)
        {
            ExpectedTotals expected = Oracle.For(run, dir);
            DirectoryTotals actual = run.TotalsFor(dir);
            Assert.Equal(expected.Size, actual.Size);
            Assert.Equal(expected.Shared, actual.Shared);
            Assert.Equal(expected.FileEntries, actual.FileEntries);
            Assert.Equal(expected.DistinctFiles, actual.DistinctFiles);
            Assert.Equal(expected.SharedFiles, actual.SharedFiles);
            Assert.Equal(expected.Directories, actual.Directories);
            Assert.Equal(expected.Coverage, actual.Coverage);
        }

        Assert.Equal(Sorted(Oracle.Extensions(run)), Sorted(run.Accounting.ExtensionTotals));
        Assert.Equal(
            run.Policy.FileSystemSupportsHardLinks ? HardLinkStatus.Reconciled : HardLinkStatus.NotApplicable,
            run.Accounting.HardLinks);
    }

    private static TreeModel Stable(TreeModel tree) =>
        tree with { Dirs = tree.Dirs.Select(d => d with { TruncateAt = null }).ToList() };

    private static DirectoryTotals Strip(DirectoryTotals t) => t with { Id = default, Parent = null, Name = "" };

    private static List<KeyValuePair<string, ExtensionStats>> Sorted(IReadOnlyDictionary<string, ExtensionStats> d) =>
        d.Where(kv => kv.Value != default).OrderBy(kv => kv.Key, StringComparer.Ordinal).ToList();

    private static List<KeyValuePair<string, ExtensionStats>> Sorted(Dictionary<string, ExtensionStats> d) =>
        Sorted((IReadOnlyDictionary<string, ExtensionStats>)d);

    private static void AssertWithin(SizeVector low, SizeVector value, SizeVector high)
    {
        Assert.InRange(value.Logical, low.Logical, high.Logical);
        Assert.InRange(value.Allocated, low.Allocated, high.Allocated);
        Assert.InRange(value.UncertainAllocated, low.UncertainAllocated, high.UncertainAllocated);
    }
}
