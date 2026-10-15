# Accounting semantics

Status: M0a. Implemented in `src/StorageIntelligence.Accounting`. Verified by property-based tests against a
brute-force oracle (`tests/.../Support/Oracle.cs`). Not yet validated against real filesystems; see
[m0b-validation-plan.md](m0b-validation-plan.md).

## Terms

| Term | Meaning |
|---|---|
| **Entry** | One name in a directory listing. |
| **File** | The set of entries that share a file ID on one volume (hard links). An entry with no file ID is its own file. |
| **Logical size** | End-of-file size. |
| **Allocated size** | Allocation reported by the filesystem for the file's unnamed data stream. |
| **Uncertain bytes** | The part of an allocated total whose measurement has not been validated or may be wrong (see *Measurement caveats*). Always a subset of allocated bytes. |

## Directory totals

For a directory **D**, with *subtree(D)* = D and everything beneath it:

- **Size(D)** = the sum of sizes of the **distinct files** that have at least one entry in subtree(D).
  A file with three links inside subtree(D) counts once.
- **Shared(D)** = the part of Size(D) belonging to files that also have a link **outside** subtree(D).
  "Outside" means elsewhere in the scan, or outside the scan if a link count was measured.
- **Exclusive(D)** = Size(D) − Shared(D): bytes reachable only through subtree(D).
- **FileEntries(D)** counts names. **DistinctFiles(D)** counts files.

### Totals are not additive across siblings

Because a file can have links in two sibling folders, both siblings include it in their Size:

```
C:\           Size 11 MB   (kernel.dll counted once)
├─ WinSxS     Size 11 MB   Shared 10 MB   Exclusive 1 MB
└─ System32   Size 10 MB   Shared 10 MB   Exclusive 0 MB
```

What does and does not hold (all are checked by tests):

| Relation | Holds? |
|---|---|
| Size(parent) = Own(parent) + Σ Size(children) | **No.** Only when no file has links in two different children. |
| Size(parent) ≤ Own(parent) + Σ Size(children) (sub-additive) | Yes, when links agree on size (see *Changed files*). |
| Exclusive(parent) ≥ Σ Exclusive(children) (super-additive) | Yes |
| Shared(D) ≤ Size(D), UncertainAllocated ≤ Allocated | Yes |
| Σ over extensions = volume total | Yes |

**UI requirement (R1).** The folder view must not present child percentages that sum to the parent unless no
file is shared between siblings. Each row shows Size, and where Shared > 0 a "shared with other folders" figure
and Exclusive. The column header explains that sizes overlap. A "% of parent" bar uses Size(child) / Size(parent)
and is labelled as overlapping when the children's sum exceeds the parent's size.

### Which link supplies a file's size

All links to one file normally report identical sizes. When they differ, the file changed between two
observations. The file's **canonical** size is then chosen independently of scan order: largest allocated, then
largest logical, then the ordinal-smallest extension. The file is flagged `InconsistentLinkSizes`, and its
caveats are the union of all its links' caveats.

### Extension totals

Extension totals are volume-wide. **FileEntries** counts names per extension. A distinct file's bytes and
DistinctFiles count go to its canonical link's extension, so extension totals sum exactly to the volume total
even when links to one file have different extensions.

## Measurement caveats

Each entry carries caveats. `MeasurementPolicy.UncertainReasons` decides which of them make the entry's
allocated bytes uncertain. **The default policy treats every caveat as uncertain and treats every filesystem as
unvalidated**, so all bytes are uncertain until M0b validation evidence flips a specific case to trusted.

| Caveat | Source |
|---|---|
| `UnvalidatedFileSystem` | Policy: no fixture evidence for this filesystem yet |
| `Sparse`, `Compressed`, `WofCompressed`, `CloudPlaceholder` | File attributes / reparse tag |
| `ChangedDuringScan` | Scanner observed the entry or its directory change |
| `InconsistentLinkSizes` | Reconciliation: links disagreed on size |
| `IdentityUnavailable` | Hard-link-capable filesystem but no file ID, so duplicates cannot be detected |
| `UnrecognizedAttributes` | Anything else the scanner could not interpret |

## Coverage

Per directory and rolled up per subtree: inaccessible directories, entry errors, not-followed links
(junctions, symbolic links, mount points), changed directories, incomplete listings and cancelled directories.
A subtree is **complete** when it has no inaccessible, errored, incomplete or cancelled directories. Not following
a link is policy, not a gap. Totals for an incomplete subtree are a **lower bound** and must be labelled as such.

## Hard-link reconciliation

During the scan every entry with a file ID is appended to a bounded buffer. Sorted runs are spilled to
delete-on-close temp files. After the scan the runs are merged by file ID. For each file with more than one link,
or with a measured link count above its observed links:

1. Walk up from every link to the root, counting arrivals per ancestor *A*. That count is *c(A)*, the number of
   links inside subtree(A).
2. Streaming aggregation counted the file once per link in *A*. The correction for *A* is
   `canonical − Σ(link contributions inside A)`, and its distinct-file correction is `1 − c(A)`.
3. *A* is shared for that file if `c(A) < total links`.

The cost is O(links × depth), and it applies only to files with several links. Until this step finishes, totals
are `HardLinkStatus.NotReconciled`: an explicitly labelled upper bound.

## Known gaps (documented, not hidden)

- **Alternate data streams** are not counted.
- **Directory index allocation and filesystem metadata** are invisible to listings.
- **Links outside the scan scope** are detected only when a link count is measured; ordinary scans do not
  measure it (no per-file opens).
- **Unattributed space** (volume used − measured) is a derived remainder: an accounting identity, not evidence of
  correctness. It is reported but never used as a validation criterion.
