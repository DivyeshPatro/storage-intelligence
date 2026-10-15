namespace StorageIntelligence.Accounting;

/// <summary>Correction for one directory's subtree totals, produced by reconciliation.</summary>
/// <param name="Delta">Added to the streaming subtree totals to get distinct-file totals. Usually negative.</param>
/// <param name="DistinctFilesDelta">Added to the subtree's file-entry count to get its distinct-file count.</param>
/// <param name="Shared">Bytes of distinct files in this subtree known to have a link outside it.</param>
/// <param name="SharedFiles">Number of such files.</param>
/// <param name="VerifiedDelta">Correction to the streaming verified-exclusive totals.</param>
/// <param name="VerifiedFilesDelta">Correction to the streaming verified-exclusive file count.</param>
public readonly record struct DirectoryAdjustment(
    SizeVector Delta,
    long DistinctFilesDelta,
    SizeVector Shared,
    long SharedFiles,
    SizeVector VerifiedDelta = default,
    long VerifiedFilesDelta = 0)
{
    public static DirectoryAdjustment operator +(DirectoryAdjustment a, DirectoryAdjustment b) => new(
        a.Delta + b.Delta,
        a.DistinctFilesDelta + b.DistinctFilesDelta,
        a.Shared + b.Shared,
        a.SharedFiles + b.SharedFiles,
        a.VerifiedDelta + b.VerifiedDelta,
        a.VerifiedFilesDelta + b.VerifiedFilesDelta);

    public static DirectoryAdjustment Add(DirectoryAdjustment left, DirectoryAdjustment right) => left + right;
}

/// <summary>Result of hard-link reconciliation for one scan.</summary>
public sealed record ReconciliationResult(
    IReadOnlyDictionary<DirectoryId, DirectoryAdjustment> Adjustments,
    IReadOnlyDictionary<int, ExtensionStats> ExtensionDeltas,
    long FilesWithMultipleLinks,
    long LinkEntriesInScope,
    long FilesWithLinksOutsideScope,
    long BytesWithLinksOutsideScope,
    long FilesWithInconsistentLinkSizes,
    long RecordsProcessed);

/// <summary>
/// Converts per-entry streaming totals into distinct-file totals.
/// </summary>
/// <remarks>
/// For a file with links at directories d1..dn, and any ancestor A, let c(A) be the number of links inside A's
/// subtree. Streaming aggregation counted the file c(A) times in A; the distinct total counts it once if c(A) ≥ 1.
/// Walking up from every link and counting arrivals gives exactly c(A), so the per-ancestor correction is
/// canonical − (sum of the c(A) link contributions). Cost: O(links × depth), only for files with several links.
/// See docs/accounting-semantics.md.
/// </remarks>
public static class HardLinkReconciler
{
    public static ReconciliationResult Reconcile(
        IEnumerable<IReadOnlyList<LinkRecord>> groups,
        IParentLookup parents,
        MeasurementPolicy policy,
        ExtensionTable extensions)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(parents);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(extensions);

        var adjustments = new Dictionary<DirectoryId, DirectoryAdjustment>();
        var extensionDeltas = new Dictionary<int, ExtensionStats>();
        var ancestors = new Dictionary<DirectoryId, Arrivals>();
        long multi = 0, linkEntries = 0, outside = 0, outsideBytes = 0, inconsistent = 0, processed = 0;

        foreach (IReadOnlyList<LinkRecord> group in groups)
        {
            processed += group.Count;
            uint reportedLinks = 0;
            foreach (LinkRecord r in group)
            {
                reportedLinks = Math.Max(reportedLinks, r.LinkCount);
            }

            long totalLinks = Math.Max(group.Count, reportedLinks);
            if (totalLinks <= 1)
            {
                continue; // The common case: one name, no correction needed.
            }

            (LinkRecord canonical, MeasurementCaveats canonicalFlags) = ChooseCanonical(group, extensions);
            if ((canonicalFlags & MeasurementCaveats.InconsistentLinkSizes) != 0)
            {
                inconsistent++;
            }

            SizeVector canonicalSize = policy.ToVector(canonical.LogicalSize, canonical.AllocatedSize, canonicalFlags);

            if (reportedLinks > group.Count)
            {
                outside++;
                outsideBytes += canonical.AllocatedSize;
            }

            if (group.Count > 1)
            {
                multi++;
                linkEntries += group.Count;
                foreach (LinkRecord r in group)
                {
                    SizeVector occurrence = policy.ToVector(r.LogicalSize, r.AllocatedSize, r.EffectiveFlags);
                    Accumulate(extensionDeltas, r.ExtensionId, new ExtensionStats(0, -1, SizeVector.Zero - occurrence));
                }

                Accumulate(extensionDeltas, canonical.ExtensionId, new ExtensionStats(0, 1, canonicalSize));
            }

            ancestors.Clear();
            foreach (LinkRecord r in group)
            {
                SizeVector occurrence = policy.ToVector(r.LogicalSize, r.AllocatedSize, r.EffectiveFlags);
                bool streamedAsVerified = policy.IsVerifiedSingleLink(hasFileId: true, r.LinkCount);
                DirectoryId current = r.Directory;
                int guard = 0;
                while (true)
                {
                    ancestors.TryGetValue(current, out Arrivals a);
                    ancestors[current] = new Arrivals(
                        a.Count + 1,
                        a.Sum + occurrence,
                        streamedAsVerified ? a.VerifiedCount + 1 : a.VerifiedCount,
                        streamedAsVerified ? a.VerifiedSum + occurrence : a.VerifiedSum);
                    if (!parents.TryGetParent(current, out DirectoryId parent))
                    {
                        break;
                    }

                    current = parent;
                    if (++guard > 100_000)
                    {
                        throw new InvalidOperationException("Parent chain does not terminate (cycle in directory records).");
                    }
                }
            }

            // A file is verified-exclusive to a subtree only when its link count was measured and every link is
            // inside the subtree. With an unmeasured count, links may exist where the scan cannot see them.
            bool linkCountMeasured = reportedLinks > 0;
            foreach ((DirectoryId dir, Arrivals a) in ancestors)
            {
                bool shared = a.Count < totalLinks;
                bool verified = !shared && linkCountMeasured;
                var adjustment = new DirectoryAdjustment(
                    canonicalSize - a.Sum,
                    1 - a.Count,
                    shared ? canonicalSize : SizeVector.Zero,
                    shared ? 1 : 0,
                    (verified ? canonicalSize : SizeVector.Zero) - a.VerifiedSum,
                    (verified ? 1 : 0) - a.VerifiedCount);
                if (adjustment == default)
                {
                    continue;
                }

                adjustments[dir] = adjustments.GetValueOrDefault(dir) + adjustment;
            }
        }

        return new ReconciliationResult(
            adjustments, extensionDeltas, multi, linkEntries, outside, outsideBytes, inconsistent, processed);
    }

    /// <summary>
    /// Picks the link whose sizes represent the file, independent of enumeration order:
    /// largest allocated, then largest logical, then ordinal-smallest extension. If links disagree on size the
    /// file changed during the scan, so it is flagged. The canonical flags are the union over all links.
    /// </summary>
    internal static (LinkRecord Canonical, MeasurementCaveats Flags) ChooseCanonical(
        IReadOnlyList<LinkRecord> group, ExtensionTable extensions)
    {
        LinkRecord best = group[0];
        MeasurementCaveats flags = MeasurementCaveats.None;
        bool sizesDiffer = false;
        foreach (LinkRecord r in group)
        {
            flags |= r.EffectiveFlags;
            if (r.AllocatedSize != group[0].AllocatedSize || r.LogicalSize != group[0].LogicalSize)
            {
                sizesDiffer = true;
            }

            if (IsBetter(r, best, extensions))
            {
                best = r;
            }
        }

        if (sizesDiffer)
        {
            flags |= MeasurementCaveats.InconsistentLinkSizes;
        }

        return (best, flags);
    }

    private static bool IsBetter(LinkRecord candidate, LinkRecord current, ExtensionTable extensions)
    {
        if (candidate.AllocatedSize != current.AllocatedSize)
        {
            return candidate.AllocatedSize > current.AllocatedSize;
        }

        if (candidate.LogicalSize != current.LogicalSize)
        {
            return candidate.LogicalSize > current.LogicalSize;
        }

        return string.CompareOrdinal(extensions.NameOf(candidate.ExtensionId), extensions.NameOf(current.ExtensionId)) < 0;
    }

    private readonly record struct Arrivals(int Count, SizeVector Sum, int VerifiedCount, SizeVector VerifiedSum);

    private static void Accumulate(Dictionary<int, ExtensionStats> map, int key, ExtensionStats delta) =>
        map[key] = map.GetValueOrDefault(key) + delta;
}
