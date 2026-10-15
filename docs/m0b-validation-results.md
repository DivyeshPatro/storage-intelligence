# M0b validation results

Status: **evidence recorded; M0b acceptance pending review.** All measurements below come from GitHub-hosted
Windows Server runners (SSD-backed, shared). They say nothing about performance, and nothing about Windows 11 client
behaviour beyond what the two server builds share with it.

Primary evidence run: `m0b-validation` run **38711178827** (commit `6668317`). Re-read its results with
`tools/ci/run-annotations.sh 38711178827`; artifacts (JSON for every scan, comparison, reference scan, manifest and
allocation trial) are in the GitHub UI for the retention period.

## 1. Production scanner vs independent reference scanner

The fixture (seed 1, about 5,060 files and 37 directories) is generated on a fresh disposable volume per filesystem.
Each row is four production scans, low-impact and fast modes, each with link counts measured for retained files only
and for all files, compared with the reference scanner and with the fixture manifest.

| Filesystem | windows-2025 | windows-2022 | Scan-mode equivalence |
|---|---|---|---|
| NTFS | 4/4 scans: **0 mismatches** | 4/4: **0 mismatches** | identical |
| ReFS | 4/4: **0 mismatches** | 4/4: **0 mismatches** | identical |
| FAT32 | 1 mismatch per scan: fixture defect (see below) | same | identical |
| exFAT | 1 mismatch per scan: fixture defect | same | identical |

The FAT32/exFAT mismatch was the generator recording `denied` as inaccessible after `icacls` reported success on a
filesystem without ACLs; the scanner correctly measured the readable folder. The generator now verifies that a denial
takes effect. After that fix every configuration has 0 mismatches (section 5).

Compared, per directory: allocated and logical size, file entries, distinct files, directory count, inaccessible
directories, not-followed links, skipped reparse directories, entry errors, and hard-link classification (exact when
all link counts are measured; subset relations otherwise). Per file: logical size, allocated size (listing vs
handle), sparse, compressed and WOF detection, and link counts.

## 2. Allocated-size semantics (settled free-space change on an idle disposable volume)

Each trial creates one file in a fresh folder and compares the **settled** change in volume free space (after a volume
flush and a 3-second stable reading) with three sources: the directory listing, `FILE_STANDARD_INFO.AllocationSize` on
a metadata handle, and `GetCompressedFileSize`. Tolerance: 2 clusters. Results were identical on both images.

| Case (trials) | NTFS | ReFS | FAT32 | exFAT |
|---|---|---|---|---|
| Plain file (1 MB, 8 MiB, 13 MB) | **consistent**: all three sources | **inconsistent**: free space fell by up to ~15 MB for an 8 MiB file | **consistent** | **consistent** |
| 100-byte file | **consistent** (resident: 0 clusters) | inconsistent | **consistent** | **consistent** |
| Sparse (64 MiB/1 MiB data, 256 MiB/768 KiB, 16 MiB/none) | **consistent** | inconsistent | not supported | not supported |
| NTFS compression (8 MiB text, 4 MiB random, 1 MiB text) | **inconsistent**: no source matches; free space fell by roughly logical + compressed size | not supported | not supported | not supported |
| WOF xpress4k (8 MiB, 3 MiB, 1 MiB text) | listing **inconsistent** (reports 0); handle and compressed size **consistent** | not supported | not supported | not supported |

### Resulting policy (`MeasurementEvidence.PolicyFor`)

| Filesystem | Treated as exact | Stays uncertain |
|---|---|---|
| NTFS | plain, resident and sparse files | NTFS-compressed, WOF, cloud placeholders, plus the always-uncertain caveats |
| ReFS | nothing | everything |
| FAT32 / exFAT | plain and resident files | the always-uncertain caveats |
| Unknown | nothing | everything |

The always-uncertain caveats are `ChangedDuringScan`, `InconsistentLinkSizes`, `IdentityUnavailable` and
`UnrecognizedAttributes`.

## 3. Defects found and fixed during M0b

| # | Component | Defect | How found |
|---|---|---|---|
| 1 | Production scanner | **WOF files are listed with allocation 0 and no reparse attribute**, so CompactOS files would be undercounted. Now: a non-sparse, non-compressed entry with allocation < size is re-measured on a metadata handle, and WOF is identified with `FSCTL_GET_EXTERNAL_BACKING`. | reference comparison + allocation harness |
| 2 | Production scanner | Directory handles lacked `FILE_READ_ATTRIBUTES`, so change detection silently never ran. | code review during hang investigation |
| 3 | Production scanner | A worker failure while the batch channel was full could drop a final batch and hang the consumer. | code review |
| 4 | Reference scanner | `WIN32_FIND_DATAW` declared FILETIME as 8-byte `long`: names were read 2 characters late, "." became "", and the root was revisited forever (the CI hang). | progress instrumentation |
| 5 | Reference scanner | Per-file paths were normalized: names with trailing spaces/dots and `CON` could not be opened. | reference comparison |
| 6 | Reference scanner | `FileAttributeTagInfo` unsupported on FAT32/exFAT: every file failed. | reference comparison |
| 7 | Fixture generator | Half-created files left behind when compression/WOF failed; deny ACL recorded on filesystems without ACLs. | manifest comparison |
| 8 | Test-root guard | `E:\` was not recognized as a volume root (refused; the safe direction). | CI |
| 9 | Comparison | Caveat substring match (`WofCompressed` contains `Compressed`); link counts compared where hard links cannot exist. | CI |
| 10 | Test cleanup | `Directory.Delete` failed on the junction loop; replaced by a guarded deleter that removes links without following them. | CI |

## 4. Not tested on CI (reported, not passed)

- **Cloud-files placeholders** (OneDrive Files On-Demand): require a sync provider. Directory reparse points that are
  not links are skipped and counted as a coverage gap; placeholder files are always uncertain.
- **ReFS block cloning / Dev Drive**: not probed; ReFS is entirely uncertain.
- **Alternate data streams**: a documented gap; a 1 MiB stream is in the fixture and is not counted.
- **Incompressible WOF data**: `compact.exe` declines to WOF-compress it, so it cannot be created this way.
- **Windows 11 client builds**: hosted runners are Windows Server 2022/2025.
- **Physical HDD behaviour and low-end hardware performance**: pending; no hardware available.

## 5. Confirmation runs after the final fixes

Copies of every run's annotations are in [validation-logs/](validation-logs/).

| Run | Commit | Result |
|---|---|---|
| m0b-validation 38711825938 | `92d0ce4` | Both test jobs pass. All 32 production-vs-reference comparisons (4 filesystems × 2 images × 4 scans): **0 mismatches**. One scan-mode-equivalence failure on FAT32/windows-2025 (section 6). |
| m0b-validation 38712350472 | `bb05fd4` | **All jobs pass**: tests, 0 mismatches everywhere, scan modes identical everywhere. |
| equivalence-repeat 38712914822 | `0dda365` | 18 repetitions (FAT32, exFAT, NTFS × 2 images × 3). 17 identical; **1 FAT32 difference** (section 6). All production-vs-reference comparisons: 0 mismatches. |
| Local, clean build `92d0ce4` | | 47/47 accounting tests at 1,000,000 cases per property (12 min 40 s); 14/14 deliberate bugs caught; final clean rebuild green. |

## 6. Open: intermittent uncertainty difference on FAT32

In 2 of 16 FAT32 validations (0 of 16 exFAT, 0 of 16 NTFS, 0 of 10 ReFS; runs 38709645699 through 38712914822), the low-impact and fast scans with all link
counts disagreed **only in uncertainty**: one scan marked some files `ChangedDuringScan` (their timestamps were at or
after the scan start) and the other did not. Sizes, counts, link classification and coverage were identical.

The scanner behaved as specified: a file whose timestamps are newer than the scan start is reported as changed and
its bytes are uncertain. Something updated the timestamps of fixture files on FAT32 between scans; candidates are the
runner's real-time antivirus or FAT's timestamp/last-access handling. **The cause is not proven.** Until it is, scan-mode
equivalence on FAT32 is established for sizes, counts and coverage but not for the uncertainty classification. A
diagnostic that records the timestamps the scanner compared is the next step.
