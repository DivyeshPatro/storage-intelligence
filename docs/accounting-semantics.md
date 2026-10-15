# Accounting semantics

Status: M0b. Implemented in `src/StorageIntelligence.Accounting`. Verified by property-based tests against a
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
- **Shared(D)** = the part of Size(D) belonging to files **known** to have a link outside subtree(D): another
  link was observed elsewhere in the scan, or the file's measured link count is higher than its links inside D.
- **ExclusiveVerified(D)** = the part of Size(D) belonging to files whose **complete** link count is known and whose
  links are all inside subtree(D). A link count is known when it was measured (`FILE_STANDARD_INFO.NumberOfLinks`)
  or when the filesystem cannot have hard links (FAT32, exFAT).
- **LinkStatusUnknown(D)** = Size(D) − Shared(D) − ExclusiveVerified(D): every observed link is inside D but the
  complete link count was not measured, so links may exist where the scan cannot see them (outside the scan root,
  in an unreadable folder). **An unmeasured link count is never treated as proof of exclusivity.**
- **FileEntries(D)** counts names. **DistinctFiles(D)** counts files.

Link counts are measured with one metadata-only open per file. The default `RetainedFiles` mode measures them for
retained (large) files, which are the ones the largest-file listings show; `All` measures every file (validation);
`None` measures none.

### Totals are not additive across siblings

Because a file can have links in two sibling folders, both siblings include it in their Size:

```
C:\           Size 11 MB   (kernel.dll counted once)
├─ WinSxS     Size 11 MB   Shared 10 MB   Exclusive (verified) 0 MB   Link status unknown 1 MB
└─ System32   Size 10 MB   Shared 10 MB   Exclusive (verified) 0 MB
```

What does and does not hold (all are checked by tests):

| Relation | Holds? |
|---|---|
| Size(parent) = Own(parent) + Σ Size(children) | **No.** Only when no file has links in two different children. |
| Size(parent) ≤ Own(parent) + Σ Size(children) (sub-additive) | Yes, when links agree on size (see *Changed files*). |
| ExclusiveVerified(parent) ≥ Σ ExclusiveVerified(children) (super-additive) | Yes |
| Shared + ExclusiveVerified + LinkStatusUnknown = Size, each ≥ 0 | Yes |
| UncertainAllocated ≤ Allocated | Yes |
| Σ over extensions = volume total | Yes |

**UI requirement (R1).** The folder view must not present child percentages that sum to the parent unless no
file is shared between siblings. Each row shows Size; where Shared > 0 it shows "shared with other folders", and it
shows "exclusive (verified)" and "link status unknown" separately: it must never present unknown bytes as
exclusive. The column header explains that sizes overlap. A "% of parent" bar uses Size(child) / Size(parent) and
is labelled as overlapping when the children's sum exceeds the parent's size.

### Which link supplies a file's size

All links to one file normally report identical sizes. When they differ, the file changed between two
observations. A single **canonical** value is then used everywhere in reconciliation (every ancestor, extension
totals, shared and exclusive figures), chosen independently of scan order: largest allocated, then largest logical,
then the ordinal-smallest extension. The file is flagged `InconsistentLinkSizes`, its caveats are the union of all
its links' caveats, and its bytes are **always** uncertain, whatever the measurement policy says (see below).

### Extension totals

Extension totals are volume-wide. **FileEntries** counts names per extension. A distinct file's bytes and
DistinctFiles count go to its canonical link's extension, so extension totals sum exactly to the volume total
even when links to one file have different extensions.

## Measurement caveats

Each entry carries caveats. `MeasurementPolicy.UncertainReasons` decides which of them make the entry's
allocated bytes uncertain. Four caveats are uncertain **regardless of policy**, because they describe an
inconsistent or incomplete observation that no validation evidence can make exact: `ChangedDuringScan`,
`InconsistentLinkSizes`, `IdentityUnavailable` and `UnrecognizedAttributes`. **The default policy treats every caveat as uncertain and treats every filesystem as
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
(junctions, symbolic links, mount points), skipped data-bearing reparse directories (for example cloud-files
folders, which are not entered because listing them could trigger provider activity), changed directories,
incomplete listings and cancelled directories.

- A subtree is **complete** when it has no inaccessible, errored, incomplete, cancelled or skipped-reparse
  directories. Not following a link is policy, not a gap. Totals for an incomplete subtree are a **lower bound**.
- A subtree is **stable** when no directory in it changed while being listed. Unstable totals describe no single
  moment and must be shown as approximate.

## Hard-link reconciliation

During the scan every entry with a file ID is appended to a bounded buffer. Sorted runs are spilled to
delete-on-close temp files in a versioned compact format (`LinkRunFormat` v1: magic, version, record count,
delta/varint records, trailer; 14.1 bytes per record measured on NTFS-like IDs versus 52 for fixed width). A reader
rejects an unknown version, a bad magic or trailer, or a truncated run.

**Temp space.** A preflight (`TempSpaceGuard.Preflight`) estimates the space needed before a scan, and before every
spill the writer checks that writing the run at its worst-case size still leaves the configured reserve free. If not,
reconciliation is abandoned, runs are deleted and the result is `NotReconciled` with reason
`InsufficientTempSpace`. A damaged run yields `NotReconciled` with reason `RunStoreError`. In both cases the
streaming totals remain, explicitly labelled as an upper bound; they are never presented as reconciled. After the scan the runs are merged by file ID. For each file with more than one link,
or with a measured link count above its observed links:

1. Walk up from every link to the root, counting arrivals per ancestor *A*. That count is *c(A)*, the number of
   links inside subtree(A).
2. Streaming aggregation counted the file once per link in *A*. The correction for *A* is
   `canonical − Σ(link contributions inside A)`, and its distinct-file correction is `1 − c(A)`.
3. For *A*, the file is **shared** if `c(A) < total links` (total = max(observed links, measured count)); otherwise
   it is **exclusive (verified)** if its link count was measured, and **link status unknown** if not.

The cost is O(links × depth), and it applies only to files with several links. Until this step finishes, totals
are `HardLinkStatus.NotReconciled`: an explicitly labelled upper bound.

## Known gaps (documented, not hidden)

- **Alternate data streams** are not counted.
- **Directory index allocation and filesystem metadata** are invisible to listings.
- **Links outside the scan scope** are detected only when a link count is measured; ordinary scans do not
  measure it (no per-file opens).
- **Unattributed space** (volume used − measured) is a derived remainder: an accounting identity, not evidence of
  correctness. It is reported but never used as a validation criterion.
