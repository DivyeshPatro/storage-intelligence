# M0b validation plan (draft, not started)

M0b has **not** started. This plan incorporates the conditions attached to the v0.3 approval and must be
reviewed before M0b is finalized. Target platform for R1: **Windows 11 x64**. Windows 10 is evaluated separately.

## 1. Correctness criteria (replaces the v0.3 "whole-volume" check)

Unattributed space (volume used − measured) is a remainder **by construction**. It cannot prove correctness and
is no longer a pass/fail criterion. Correctness is established instead by:

1. **Known fixtures.** The fixture generator creates files with known logical sizes, link structure and
   attributes, and writes a manifest describing them.
2. **Independent allocation measurement.** A slow reference scanner, separate from the production code path,
   opens every file and records:
   - file ID and link count (`GetFileInformationByHandle(Ex)`)
   - allocation (`FileStandardInfo.AllocationSize`)
   - `GetCompressedFileSize`
   - for sparse files, the allocated ranges (`FSCTL_QUERY_ALLOCATED_RANGES`)
   - the reparse tag

   Where practical, it also cross-checks against `fsutil file layout` output.
3. **Pass criteria.**
   - The production scanner's events, fed to the accounting core, must equal the reference scanner's totals for
     every directory on **stable fixtures**.
   - Both must agree with the fixture manifest wherever the manifest defines the expected value.
4. **Disagreement policy.** If the production listing data and the reference measurement disagree for a case, or
   the reference itself is not self-consistent (e.g. `AllocationSize` ≠ `GetCompressedFileSize` for compressed
   data), that case stays **uncertain** in `MeasurementPolicy`. It is never claimed exact.

## 2. Allocation semantics to establish, per filesystem

| Case | NTFS | ReFS | FAT32 | exFAT |
|---|---|---|---|---|
| Plain file | to validate | to validate | to validate | to validate |
| Tiny file resident in the MFT (allocated 0) | to validate | n/a | n/a | n/a |
| Sparse | to validate | to validate | n/a | n/a |
| NTFS compressed | to validate | n/a | n/a | n/a |
| WOF (CompactOS) | to validate | likely uncertain | n/a | n/a |
| Cloud placeholder | to validate (hydrated and dehydrated) | to validate | n/a | n/a |
| Block-cloned copies (ReFS) | n/a | **uncertain by design** (shared blocks are not visible per file) | n/a | n/a |
| Hard links | to validate | to validate | n/a | n/a |

Each cell becomes **validated** (with evidence linked from the policy table), **uncertain** or
**not testable on hosted CI**. Only validated cells are removed from `MeasurementPolicy.UncertainReasons` for
that filesystem.

## 3. Scan-mode equivalence

Identical results across low-impact mode, fast mode and the reference scanner are required **only on stable,
controlled fixtures**, where nothing writes to the volume during the scan.

Live-change tests have a separate requirement:
- affected directories and entries are reported (`ChangedDuringScan`, `InconsistentLinkSizes`, incomplete
  listings)
- their bytes are counted as uncertain
- the totals for the unaffected parts still match the oracle

## 4. Windows CI volume feasibility (first M0b task)

Nothing here assumes the hosted runner can create every volume type. The first workflow is a **feasibility
probe** that records, per filesystem, whether it can:

1. create a VHDX (`New-VHD` needs Hyper-V tools; `diskpart create vdisk` is the fallback)
2. attach it, partition it and format it (NTFS, FAT32, exFAT, ReFS; ReFS formatting is often unavailable on
   client and hosted images)
3. create the fixture features on it (hard links; junctions; symbolic links, which may need Developer Mode;
   sparse; compressed; WOF via `compact /EXE`)
4. detach it cleanly

The probe publishes a capability matrix as a build artifact. Whatever is unavailable is listed as
**not validated on CI**. It is not silently skipped.

## 5. Performance evidence (no physical low-end PC available)

| Source | What it can show | Labelled as |
|---|---|---|
| Synthetic fixtures (CI) | Memory stays flat as file count grows; event and aggregation throughput | CI, SSD-backed, shared runner |
| Resource-constrained Windows 11 VM (2 vCPU, 4 GB) | Startup, idle and peak working set, cancellation latency | **VM**: not representative of HDD latency |
| Physical HDD | Seek-bound throughput, effect on foreground I/O | **Pending**: no hardware available |

No performance target is reported as met until it has been measured, and every number states its source.
