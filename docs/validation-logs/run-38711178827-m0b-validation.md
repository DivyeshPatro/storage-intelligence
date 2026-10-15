### tests win22 [1]
## Test suite: win22 (SI_PBT_ITER=20000)


### Hang / abort diagnostics

Passed StorageIntelligence.Accounting.Tests.AccountingScenarioTests.Unvalidated_attribute_cases_are_uncertain_on_a_validated_filesystem(caveat: ChangedDuringScan) [1 ms]
Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [3 s]
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.

### Slow tests (>= 1 s)

- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Volume_roots_are_recognised_but_refused_without_ci_markers [4 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Production_scan_matches_independent_reference_in_every_mode [4 s]
- Passed StorageIntelligence.Accounting.Tests.LinkRunFormatTests.Round_trips_any_sorted_records [8 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_are_independent_of_order_and_spill_size_on_stable_fixtures [11 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Unmeasured_small_links_are_unknown_but_measured_large_links_are_shared [3 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_cancelled_scans [2 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Structural_invariants_hold [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Junction_loop_terminates_and_link_targets_are_not_counted [3 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Run_writer_groups_all_records_by_file_id [5 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_complete_scans [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Guard_refuses_anything_but_marked_test_roots [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Cancellation_returns_a_partial_result_with_cancelled_directories [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Insufficient_temp_space_is_reported_as_not_reconciled [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Scanning_does_not_modify_anything [3 s]

### tests win25-vs2026 [1]
## Test suite: win25-vs2026 (SI_PBT_ITER=20000)


### Hang / abort diagnostics

Passed StorageIntelligence.Accounting.Tests.AccountingScenarioTests.Unvalidated_attribute_cases_are_uncertain_on_a_validated_filesystem(caveat: ChangedDuringScan) [76 ms]
Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [4 s]
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.

### Slow tests (>= 1 s)

- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Volume_roots_are_recognised_but_refused_without_ci_markers [4 s]
- Passed StorageIntelligence.Accounting.Tests.LinkRunFormatTests.Round_trips_any_sorted_records [4 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Production_scan_matches_independent_reference_in_every_mode [5 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_are_independent_of_order_and_spill_size_on_stable_fixtures [10 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Unmeasured_small_links_are_unknown_but_measured_large_links_are_shared [5 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_cancelled_scans [2 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Structural_invariants_hold [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [4 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Run_writer_groups_all_records_by_file_id [5 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_complete_scans [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Junction_loop_terminates_and_link_targets_are_not_counted [4 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Guard_refuses_anything_but_marked_test_roots [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Cancellation_returns_a_partial_result_with_cancelled_directories [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Insufficient_temp_space_is_reported_as_not_reconciled [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Scanning_does_not_modify_anything [4 s]

### validation exFAT win22 [2]
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 32768 | 32768 | 32768 | 32768 | 100 | False | 0x20 tag 0x00000000 | True / True / True |
| **resident-100B** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| normal | 0 | 1000000 | 1015808 | 1015808 | 1015808 | 1015808 | 1000000 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 1 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 2 | 13000123 | 13008896 | 13008896 | 13008896 | 13008896 | 13000123 | False | 0x20 tag 0x00000000 | True / True / True |
| **normal** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| sparse | - | - | - | - | - | - | - | - | - | not-supported: sparse is not available on exFAT |
| **sparse** | | | | | | | | | | **not-supported** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on exFAT |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on exFAT |
| **wof** | | | | | | | | | | **not-supported** () |

### validation exFAT win22 [1]
validate exit code: 1, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-446f4825.vhdx
fixture: 5057 files, 37 directories, 0 links, 9 skipped/known-gap features
  skipped hard-links: CreateHardLink failed (1).
  skipped junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
  skipped junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
  skipped symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
  skipped symlink-file: target links\y.bin was not created
  skipped sparse: FSCTL_SET_SPARSE failed (0).
  skipped ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
  skipped wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
  skipped alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Scanner validation: exFAT

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| LowImpact-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- hard-links: CreateHardLink failed (1).
- junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
- junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
- symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
- symlink-file: target links\y.bin was not created
- sparse: FSCTL_SET_SPARSE failed (0).
- ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
- wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Allocation semantics: exFAT (cluster 32768, tolerance 65536 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |

### validation FAT32 win22 [2]
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 4096 | 4096 | 4096 | 4096 | 100 | False | 0x20 tag 0x00000000 | True / True / True |
| **resident-100B** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| normal | 0 | 1000000 | 1003520 | 1003520 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 1 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 2 | 13000123 | 13000704 | 13000704 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | True / True / True |
| **normal** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| sparse | - | - | - | - | - | - | - | - | - | not-supported: sparse is not available on FAT32 |
| **sparse** | | | | | | | | | | **not-supported** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on FAT32 |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on FAT32 |
| **wof** | | | | | | | | | | **not-supported** () |

### validation FAT32 win22 [1]
validate exit code: 1, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-a896b7ac.vhdx
fixture: 5057 files, 37 directories, 0 links, 9 skipped/known-gap features
  skipped hard-links: CreateHardLink failed (1).
  skipped junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
  skipped junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
  skipped symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
  skipped symlink-file: target links\y.bin was not created
  skipped sparse: FSCTL_SET_SPARSE failed (0).
  skipped ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
  skipped wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
  skipped alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Scanner validation: FAT32

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| LowImpact-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- hard-links: CreateHardLink failed (1).
- junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
- junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
- symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
- symlink-file: target links\y.bin was not created
- sparse: FSCTL_SET_SPARSE failed (0).
- ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
- wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Allocation semantics: FAT32 (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |

### validation NTFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-7818d49e.vhdx
fltmc attach wof: ATTACH successful... Instance Name: Wof Instance
fixture: 5068 files, 37 directories, 4 links, 1 skipped/known-gap features
  skipped known-gap:alternate-data-streams: 1 MiB alternate data stream on special\with-ads.bin; listings report only the unnamed stream.

## Scanner validation: NTFS

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5067 | 0 | pass |
| Fast-RetainedFiles | 41 | 5067 | 0 | pass |
| LowImpact-All | 41 | 5067 | 0 | pass |
| Fast-All | 41 | 5067 | 0 | pass |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- known-gap:alternate-data-streams: 1 MiB alternate data stream on special\with-ads.bin; listings report only the unnamed stream.

## Allocation semantics: NTFS (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 0 | 0 | 104 | 104 | 100 | False | 0x20 tag 0x00000000 | True / True / True |
| **resident-100B** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| normal | 0 | 1000000 | 1003520 | 1003520 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 1 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 2 | 13000123 | 13000704 | 13000704 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | True / True / True |
| **normal** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| sparse | 0 | 67108864 | 1048576 | 1048576 | 1048576 | 1048576 | 1048576 | False | 0x220 tag 0x00000000 | True / True / True |
| sparse | 1 | 268435456 | 786432 | 786432 | 786432 | 786432 | 786432 | False | 0x220 tag 0x00000000 | True / True / True |
| sparse | 2 | 16777216 | 0 | 0 | 0 | 0 | 0 | False | 0x220 tag 0x00000000 | True / True / True |
| **sparse** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| ntfs-compressed | 0 | 8388608 | 9469952 | 9469952 | 1048576 | 1048576 | 1048576 | False | 0x820 tag 0x00000000 | False / False / False |
| ntfs-compressed | 1 | 4194304 | 8404992 | 8404992 | 4194304 | 4194304 | 4194304 | False | 0x820 tag 0x00000000 | False / False / False |
| ntfs-compressed | 2 | 1048576 | 1183744 | 1183744 | 131072 | 131072 | 131072 | False | 0x820 tag 0x00000000 | False / False / False |
| **ntfs-compressed** | | | | | | | | | | **inconsistent** () |
| wof | 0 | 8388608 | 589824 | 589824 | 0 | 589824 | 589824 | True | 0x20 tag 0x00000000 | False / True / True |
| wof | 1 | 3145728 | 221184 | 221184 | 0 | 221184 | 221184 | True | 0x20 tag 0x00000000 | False / True / True |
| wof | 2 | 1048576 | 73728 | 73728 | 0 | 73728 | 73728 | True | 0x20 tag 0x00000000 | False / True / True |
| **wof** | | | | | | | | | | **inconsistent** (handle, compressed-size) |

### validation FAT32 win25-vs2026 [2]
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 4096 | 4096 | 4096 | 4096 | 100 | False | 0x20 tag 0x00000000 | True / True / True |
| **resident-100B** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| normal | 0 | 1000000 | 1003520 | 1003520 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 1 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 2 | 13000123 | 13000704 | 13000704 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | True / True / True |
| **normal** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| sparse | - | - | - | - | - | - | - | - | - | not-supported: sparse is not available on FAT32 |
| **sparse** | | | | | | | | | | **not-supported** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on FAT32 |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on FAT32 |
| **wof** | | | | | | | | | | **not-supported** () |

### validation FAT32 win25-vs2026 [1]
validate exit code: 1, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-a913c95a.vhdx
fixture: 5057 files, 37 directories, 0 links, 9 skipped/known-gap features
  skipped hard-links: CreateHardLink failed (1).
  skipped junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
  skipped junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
  skipped symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
  skipped symlink-file: target links\y.bin was not created
  skipped sparse: FSCTL_SET_SPARSE failed (0).
  skipped ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
  skipped wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
  skipped alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Scanner validation: FAT32

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| LowImpact-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- hard-links: CreateHardLink failed (1).
- junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
- junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
- symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
- symlink-file: target links\y.bin was not created
- sparse: FSCTL_SET_SPARSE failed (0).
- ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
- wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Allocation semantics: FAT32 (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |

### validation ReFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ ReFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-refs-9a182352.vhdx
fixture: 5065 files, 37 directories, 4 links, 3 skipped/known-gap features
  skipped ntfs-compression: FSCTL_SET_COMPRESSION failed (50).
  skipped wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
  skipped known-gap:alternate-data-streams: 1 MiB alternate data stream on special\with-ads.bin; listings report only the unnamed stream.

## Scanner validation: ReFS

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5064 | 0 | pass |
| Fast-RetainedFiles | 41 | 5064 | 0 | pass |
| LowImpact-All | 41 | 5064 | 0 | pass |
| Fast-All | 41 | 5064 | 0 | pass |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- ntfs-compression: FSCTL_SET_COMPRESSION failed (50).
- wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- known-gap:alternate-data-streams: 1 MiB alternate data stream on special\with-ads.bin; listings report only the unnamed stream.

## Allocation semantics: ReFS (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | -98304 | -131072 | 104 | 104 | 100 | False | 0x20 tag 0x00000000 | False / False / False |
| **resident-100B** | | | | | | | | | | **inconsistent** () |
| normal | 0 | 1000000 | 2105344 | 1101824 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 1 | 8388608 | 14778368 | 8454144 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 2 | 13000123 | 14614528 | 12902400 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | False / False / False |
| **normal** | | | | | | | | | | **inconsistent** () |
| sparse | 0 | 67108864 | 987136 | 954368 | 1052672 | 1052672 | 1052672 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 1 | 268435456 | 720896 | 688128 | 786432 | 786432 | 786432 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 2 | 16777216 | -61440 | -94208 | 4096 | 4096 | 4096 | False | 0x220 tag 0x00000000 | False / False / False |
| **sparse** | | | | | | | | | | **inconsistent** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on ReFS |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on ReFS |
| **wof** | | | | | | | | | | **not-supported** () |

### validation exFAT win25-vs2026 [2]
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 32768 | 32768 | 32768 | 32768 | 100 | False | 0x20 tag 0x00000000 | True / True / True |
| **resident-100B** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| normal | 0 | 1000000 | 1015808 | 1015808 | 1015808 | 1015808 | 1000000 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 1 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 2 | 13000123 | 13008896 | 13008896 | 13008896 | 13008896 | 13000123 | False | 0x20 tag 0x00000000 | True / True / True |
| **normal** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| sparse | - | - | - | - | - | - | - | - | - | not-supported: sparse is not available on exFAT |
| **sparse** | | | | | | | | | | **not-supported** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on exFAT |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on exFAT |
| **wof** | | | | | | | | | | **not-supported** () |

### validation exFAT win25-vs2026 [1]
validate exit code: 1, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-57ad7677.vhdx
fixture: 5057 files, 37 directories, 0 links, 9 skipped/known-gap features
  skipped hard-links: CreateHardLink failed (1).
  skipped junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
  skipped junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
  skipped symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
  skipped symlink-file: target links\y.bin was not created
  skipped sparse: FSCTL_SET_SPARSE failed (0).
  skipped ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
  skipped wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
  skipped alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Scanner validation: exFAT

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-RetainedFiles | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| LowImpact-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |
| Fast-All | 41 | 5057 | 1 | **FAIL** |
|  | manifest.denied-visible | `denied\secret.bin` | file in a denied directory was measured | |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- hard-links: CreateHardLink failed (1).
- junction-loop: cmd.exe /c mklink /J E:\fixture\junctions\loop E:\fixture failed (1): Local NTFS volumes are required to complete the operation.
- junction-outside: cmd.exe /c mklink /J E:\fixture\junctions\to-outside E:\outside failed (1): Local NTFS volumes are required to complete the operation.
- symlink-directory: Incorrect function. : 'E:\fixture\symlinks\dir-link'.
- symlink-file: target links\y.bin was not created
- sparse: FSCTL_SET_SPARSE failed (0).
- ntfs-compression: FSCTL_SET_COMPRESSION failed (1).
- wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Allocation semantics: exFAT (cluster 32768, tolerance 65536 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |

### validation NTFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-cb61c3ae.vhdx
fltmc attach wof: ATTACH successful... Instance Name: Wof Instance
fixture: 5068 files, 37 directories, 4 links, 1 skipped/known-gap features
  skipped known-gap:alternate-data-streams: 1 MiB alternate data stream on special\with-ads.bin; listings report only the unnamed stream.

## Scanner validation: NTFS

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5067 | 0 | pass |
| Fast-RetainedFiles | 41 | 5067 | 0 | pass |
| LowImpact-All | 41 | 5067 | 0 | pass |
| Fast-All | 41 | 5067 | 0 | pass |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- known-gap:alternate-data-streams: 1 MiB alternate data stream on special\with-ads.bin; listings report only the unnamed stream.

## Allocation semantics: NTFS (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 0 | 0 | 104 | 104 | 100 | False | 0x20 tag 0x00000000 | True / True / True |
| **resident-100B** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| normal | 0 | 1000000 | 1003520 | 1003520 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 1 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | True / True / True |
| normal | 2 | 13000123 | 13000704 | 13000704 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | True / True / True |
| **normal** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| sparse | 0 | 67108864 | 1048576 | 1048576 | 1048576 | 1048576 | 1048576 | False | 0x220 tag 0x00000000 | True / True / True |
| sparse | 1 | 268435456 | 786432 | 786432 | 786432 | 786432 | 786432 | False | 0x220 tag 0x00000000 | True / True / True |
| sparse | 2 | 16777216 | 0 | 0 | 0 | 0 | 0 | False | 0x220 tag 0x00000000 | True / True / True |
| **sparse** | | | | | | | | | | **consistent** (listing, handle, compressed-size) |
| ntfs-compressed | 0 | 8388608 | 9469952 | 9469952 | 1048576 | 1048576 | 1048576 | False | 0x820 tag 0x00000000 | False / False / False |
| ntfs-compressed | 1 | 4194304 | 8404992 | 8404992 | 4194304 | 4194304 | 4194304 | False | 0x820 tag 0x00000000 | False / False / False |
| ntfs-compressed | 2 | 1048576 | 1183744 | 1183744 | 131072 | 131072 | 131072 | False | 0x820 tag 0x00000000 | False / False / False |
| **ntfs-compressed** | | | | | | | | | | **inconsistent** () |
| wof | 0 | 8388608 | 589824 | 589824 | 0 | 589824 | 589824 | True | 0x20 tag 0x00000000 | False / True / True |
| wof | 1 | 3145728 | 221184 | 221184 | 0 | 221184 | 221184 | True | 0x20 tag 0x00000000 | False / True / True |
| wof | 2 | 1048576 | 73728 | 73728 | 0 | 73728 | 73728 | True | 0x20 tag 0x00000000 | False / True / True |
| **wof** | | | | | | | | | | **inconsistent** (handle, compressed-size) |

### validation ReFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ ReFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-refs-89153a9d.vhdx
fixture: 5065 files, 37 directories, 4 links, 3 skipped/known-gap features
  skipped ntfs-compression: FSCTL_SET_COMPRESSION failed (50).
  skipped wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
  skipped alternate-data-stream: The requested operation could not be completed due to a file system limitation : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Scanner validation: ReFS

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5064 | 0 | pass |
| Fast-RetainedFiles | 41 | 5064 | 0 | pass |
| LowImpact-All | 41 | 5064 | 0 | pass |
| Fast-All | 41 | 5064 | 0 | pass |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: identical

### Features not tested or known gaps
- ntfs-compression: FSCTL_SET_COMPRESSION failed (50).
- wof-compression: compact.exe /c /exe:xpress4k E:\fixture\special\wof-text.txt failed (1): Compressing files in E:\fixture\special\

wof-text.txt        [ERR]
wof-text.txt: The file system does not support compression.

0 files within 1 directories were compressed.
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- alternate-data-stream: The requested operation could not be completed due to a file system limitation : '\\?\E:\fixture\special\with-ads.bin:extra'.

## Allocation semantics: ReFS (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | -94208 | -192512 | 4096 | 4096 | 100 | False | 0x20 tag 0x00000000 | False / False / False |
| **resident-100B** | | | | | | | | | | **inconsistent** () |
| normal | 0 | 1000000 | 1282048 | 1167360 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 1 | 8388608 | 16023552 | 8568832 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 2 | 13000123 | 16023552 | 13164544 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | False / False / False |
| **normal** | | | | | | | | | | **inconsistent** () |
| sparse | 0 | 67108864 | 1245184 | 1146880 | 1048576 | 1048576 | 1048576 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 1 | 268435456 | 1064960 | 950272 | 786432 | 786432 | 786432 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 2 | 16777216 | 196608 | 98304 | 0 | 0 | 0 | False | 0x220 tag 0x00000000 | False / False / False |
| **sparse** | | | | | | | | | | **inconsistent** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on ReFS |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on ReFS |
| **wof** | | | | | | | | | | **not-supported** () |

