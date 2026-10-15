### tests win25-vs2026 [1]
## Test suite: win25-vs2026 (SI_PBT_ITER=20000)


### Hang / abort diagnostics

Passed StorageIntelligence.Scanning.Windows.Tests.MeasurementEvidenceTests.Only_validated_cases_are_exact(kind: Ntfs, caveats: ChangedDuringScan, uncertain: True) [15 ms]
Passed StorageIntelligence.Accounting.Tests.AccountingScenarioTests.Unvalidated_attribute_cases_are_uncertain_on_a_validated_filesystem(caveat: ChangedDuringScan) [3 ms]
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.
Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [2 s]
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.

### Slow tests (>= 1 s)

- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Volume_roots_are_recognised_but_refused_without_ci_markers [7 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_are_independent_of_order_and_spill_size_on_stable_fixtures [7 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_cancelled_scans [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Production_scan_matches_independent_reference_in_every_mode [5 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Run_writer_groups_all_records_by_file_id [3 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_complete_scans [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Unmeasured_small_links_are_unknown_but_measured_large_links_are_shared [4 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Junction_loop_terminates_and_link_targets_are_not_counted [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Guard_refuses_anything_but_marked_test_roots [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Cancellation_returns_a_partial_result_with_cancelled_directories [4 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Insufficient_temp_space_is_reported_as_not_reconciled [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Scanning_does_not_modify_anything [3 s]

### tests win22 [1]
## Test suite: win22 (SI_PBT_ITER=20000)


### Hang / abort diagnostics

Passed StorageIntelligence.Accounting.Tests.AccountingScenarioTests.Unvalidated_attribute_cases_are_uncertain_on_a_validated_filesystem(caveat: ChangedDuringScan) [35 ms]
Passed StorageIntelligence.Scanning.Windows.Tests.MeasurementEvidenceTests.Only_validated_cases_are_exact(kind: Ntfs, caveats: ChangedDuringScan, uncertain: True) [2 ms]
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.
Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [3 s]
Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.

### Slow tests (>= 1 s)

- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Volume_roots_are_recognised_but_refused_without_ci_markers [4 s]
- Passed StorageIntelligence.Accounting.Tests.LinkRunFormatTests.Round_trips_any_sorted_records [1 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_are_independent_of_order_and_spill_size_on_stable_fixtures [7 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Totals_match_oracle_for_cancelled_scans [1 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Production_scan_matches_independent_reference_in_every_mode [4 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Unmeasured_small_links_are_unknown_but_measured_large_links_are_shared [3 s]
- Passed StorageIntelligence.Accounting.Tests.AccountingPropertyTests.Run_writer_groups_all_records_by_file_id [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Changes_during_a_listing_are_reported_and_counted_as_uncertain [3 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Junction_loop_terminates_and_link_targets_are_not_counted [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Guard_refuses_anything_but_marked_test_roots [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Cancellation_returns_a_partial_result_with_cancelled_directories [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Insufficient_temp_space_is_reported_as_not_reconciled [2 s]
- Passed StorageIntelligence.Scanning.Windows.Tests.ScannerIntegrationTests.Scanning_does_not_modify_anything [2 s]

### validation ReFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ ReFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-refs-58a2d88e.vhdx
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
| normal | 0 | 1000000 | 1200128 | 1101824 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 1 | 8388608 | 16023552 | 8552448 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 2 | 13000123 | 15941632 | 13115392 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | False / False / False |
| **normal** | | | | | | | | | | **inconsistent** () |
| sparse | 0 | 67108864 | 1245184 | 1146880 | 1048576 | 1048576 | 1048576 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 1 | 268435456 | 1064960 | 950272 | 786432 | 786432 | 786432 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 2 | 16777216 | 196608 | 98304 | 0 | 0 | 0 | False | 0x220 tag 0x00000000 | False / False / False |
| **sparse** | | | | | | | | | | **inconsistent** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on ReFS |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on ReFS |
| **wof** | | | | | | | | | | **not-supported** () |

### validation ReFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ ReFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-refs-748bb5de.vhdx
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
| resident-100B | 0 | 100 | -3309568 | -3342336 | 104 | 104 | 100 | False | 0x20 tag 0x00000000 | False / False / False |
| **resident-100B** | | | | | | | | | | **inconsistent** () |
| normal | 0 | 1000000 | 2138112 | 1069056 | 1003520 | 1003520 | 1000000 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 1 | 8388608 | 14614528 | 8290304 | 8388608 | 8388608 | 8388608 | False | 0x20 tag 0x00000000 | False / False / False |
| normal | 2 | 13000123 | 14614528 | 12902400 | 13000704 | 13000704 | 13000123 | False | 0x20 tag 0x00000000 | False / False / False |
| **normal** | | | | | | | | | | **inconsistent** () |
| sparse | 0 | 67108864 | 1052672 | 1019904 | 1052672 | 1052672 | 1052672 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 1 | 268435456 | 720896 | 688128 | 786432 | 786432 | 786432 | False | 0x220 tag 0x00000000 | False / False / False |
| sparse | 2 | 16777216 | 4096 | -28672 | 4096 | 4096 | 4096 | False | 0x220 tag 0x00000000 | False / False / False |
| **sparse** | | | | | | | | | | **inconsistent** () |
| ntfs-compressed | - | - | - | - | - | - | - | - | - | not-supported: ntfs-compressed is not available on ReFS |
| **ntfs-compressed** | | | | | | | | | | **not-supported** () |
| wof | - | - | - | - | - | - | - | - | - | not-supported: wof is not available on ReFS |
| **wof** | | | | | | | | | | **not-supported** () |

### validation FAT32 win22 [2]
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
validate exit code: 0, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-4685d19b.vhdx
fixture: 5057 files, 37 directories, 0 links, 10 skipped/known-gap features
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
  skipped inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Scanner validation: FAT32

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 0 | pass |
| Fast-RetainedFiles | 41 | 5057 | 0 | pass |
| LowImpact-All | 41 | 5057 | 0 | pass |
| Fast-All | 41 | 5057 | 0 | pass |

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
- inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Allocation semantics: FAT32 (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 4096 | 4096 | 4096 | 4096 | 100 | False | 0x20 tag 0x00000000 | True / True / True |

### validation exFAT win25-vs2026 [2]
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
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-51a0dea4.vhdx
fixture: 5057 files, 37 directories, 0 links, 10 skipped/known-gap features
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
  skipped inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Scanner validation: exFAT

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 0 | pass |
| Fast-RetainedFiles | 41 | 5057 | 0 | pass |
| LowImpact-All | 41 | 5057 | 0 | pass |
| Fast-All | 41 | 5057 | 0 | pass |

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
- inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Allocation semantics: exFAT (cluster 32768, tolerance 65536 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 32768 | 32768 | 32768 | 32768 | 100 | False | 0x20 tag 0x00000000 | True / True / True |

### validation NTFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-c239b14f.vhdx
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

### validation NTFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-4b54cb3c.vhdx
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

### validation exFAT win22 [2]
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
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-208ba6eb.vhdx
fixture: 5057 files, 37 directories, 0 links, 10 skipped/known-gap features
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
  skipped inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Scanner validation: exFAT

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 0 | pass |
| Fast-RetainedFiles | 41 | 5057 | 0 | pass |
| LowImpact-All | 41 | 5057 | 0 | pass |
| Fast-All | 41 | 5057 | 0 | pass |

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
- inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Allocation semantics: exFAT (cluster 32768, tolerance 65536 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
|---|---|---|---|---|---|---|---|---|---|---|
| resident-100B | 0 | 100 | 32768 | 32768 | 32768 | 32768 | 100 | False | 0x20 tag 0x00000000 | True / True / True |

### validation FAT32 win25-vs2026 [2]
0 total bytes of data are stored in 0 bytes.
The compression ratio is 1.0 to 1.
- alternate-data-stream: The filename, directory name, or volume label syntax is incorrect. : '\\?\E:\fixture\special\with-ads.bin:extra'.
- inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Allocation semantics: FAT32 (cluster 4096, tolerance 8192 bytes)

| Case | Trial | Logical | Free-space change (immediate) | Free-space change (settled) | Listing alloc | Handle alloc | Compressed size | WOF | Listing attrs | Listing / Handle / Compressed OK |
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

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-34ce02fe.vhdx
fixture: 5057 files, 37 directories, 0 links, 10 skipped/known-gap features
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
  skipped inaccessible-directory: deny ACE had no effect on this filesystem (no ACL support); 'denied' remains readable

## Scanner validation: FAT32

| Scan | Directories | Files | Mismatches | Result |
|---|---|---|---|---|
| LowImpact-RetainedFiles | 41 | 5057 | 0 | pass |
| Fast-RetainedFiles | 41 | 5057 | 0 | pass |
| LowImpact-All | 41 | 5057 | 0 | pass |
| Fast-All | 41 | 5057 | 0 | pass |

### Scan-mode equivalence (stable fixture)
- LowImpact-RetainedFiles vs Fast-RetainedFiles: identical
- LowImpact-All vs Fast-All: **13 differences**: : 42295296 vs 42295296; wide: 20480000 vs 20480000; links: 1048576 vs 1048576; special: 8192 vs 8192; names: 94208 vs 94208; denied: 1048576 vs 1048576; names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa: 73728 vs 73728; names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb: 73728 vs 73728; names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\cccccccccccccccccccccccccccccccccccccccccccccccccc: 73728 vs 73728; names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\cccccccccccccccccccccccccccccccccccccccccccccccccc\dddddddddddddddddddddddddddddddddddddddddddddddddd: 73728 vs 73728

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

