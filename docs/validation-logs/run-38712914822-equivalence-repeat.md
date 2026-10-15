### validation exFAT win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-d1ad1152.vhdx
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

### validation exFAT win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-57aeed0f.vhdx
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

### validation exFAT win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-39717759.vhdx
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

### validation FAT32 win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-d5f2e290.vhdx
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

### validation exFAT win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-751d48fc.vhdx
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

### validation FAT32 win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-49ca6446.vhdx
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

### validation NTFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-20b6e07a.vhdx
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

### validation FAT32 win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-b68a6b44.vhdx
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

### validation NTFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-c9e0542b.vhdx
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

### validation exFAT win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-884b0bd6.vhdx
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

### validation exFAT win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ exFAT, cluster 32768, VHDX D:\a\_temp\si-vhd\si-exfat-97a07f47.vhdx
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

### validation NTFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-c11cf263.vhdx
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

### validation NTFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-726463d3.vhdx
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

### validation NTFS win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-38c6adec.vhdx
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

### validation FAT32 win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-84449177.vhdx
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

### validation FAT32 win22 [4]
  - plain\d0-2: Size SizeVector { Logical = 3135591, Allocated = 3153920, UncertainAllocated = 3153920, IsZero = False } vs SizeVector { Logical = 3135591, Allocated = 3153920, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 3135591, Allocated = 3153920, UncertainAllocated = 3153920, IsZero = False } vs SizeVector { Logical = 3135591, Allocated = 3153920, UncertainAllocated = 0, IsZero = False }
  - plain\d0-2\d1-0: Size SizeVector { Logical = 3000423, Allocated = 3014656, UncertainAllocated = 3014656, IsZero = False } vs SizeVector { Logical = 3000423, Allocated = 3014656, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 3000423, Allocated = 3014656, UncertainAllocated = 3014656, IsZero = False } vs SizeVector { Logical = 3000423, Allocated = 3014656, UncertainAllocated = 0, IsZero = False }
  - plain\d0-1\d1-0: Size SizeVector { Logical = 3143485, Allocated = 3158016, UncertainAllocated = 3158016, IsZero = False } vs SizeVector { Logical = 3143485, Allocated = 3158016, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 3143485, Allocated = 3158016, UncertainAllocated = 3158016, IsZero = False } vs SizeVector { Logical = 3143485, Allocated = 3158016, UncertainAllocated = 0, IsZero = False }
  - plain\d0-1\d1-1: Size SizeVector { Logical = 1500, Allocated = 12288, UncertainAllocated = 12288, IsZero = False } vs SizeVector { Logical = 1500, Allocated = 12288, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 1500, Allocated = 12288, UncertainAllocated = 12288, IsZero = False } vs SizeVector { Logical = 1500, Allocated = 12288, UncertainAllocated = 0, IsZero = False }

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

### validation FAT32 win22 [3]
  - names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\cccccccccccccccccccccccccccccccccccccccccccccccccc\dddddddddddddddddddddddddddddddddddddddddddddddddd: Size SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }
  - links\a: Size SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 1048576, IsZero = False } vs SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 1048576, IsZero = False } vs SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 0, IsZero = False }
  - names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\cccccccccccccccccccccccccccccccccccccccccccccccccc\dddddddddddddddddddddddddddddddddddddddddddddddddd\eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee: Size SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }
  - names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\cccccccccccccccccccccccccccccccccccccccccccccccccc\dddddddddddddddddddddddddddddddddddddddddddddddddd\eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee\ffffffffffffffffffffffffffffffffffffffffffffffffff: Size SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }
  - plain\d0-0: Size SizeVector { Logical = 12220356, Allocated = 12263424, UncertainAllocated = 12263424, IsZero = False } vs SizeVector { Logical = 12220356, Allocated = 12263424, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 12220356, Allocated = 12263424, UncertainAllocated = 12263424, IsZero = False } vs SizeVector { Logical = 12220356, Allocated = 12263424, UncertainAllocated = 0, IsZero = False }
  - plain\d0-1: Size SizeVector { Logical = 4153179, Allocated = 4190208, UncertainAllocated = 4190208, IsZero = False } vs SizeVector { Logical = 4153179, Allocated = 4190208, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 4153179, Allocated = 4190208, UncertainAllocated = 4190208, IsZero = False } vs SizeVector { Logical = 4153179, Allocated = 4190208, UncertainAllocated = 0, IsZero = False }

### validation FAT32 win22 [2]
  - links: Size SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 1048576, IsZero = False } vs SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 1048576, IsZero = False } vs SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 0, IsZero = False }
  - special: Size SizeVector { Logical = 1124, Allocated = 8192, UncertainAllocated = 8192, IsZero = False } vs SizeVector { Logical = 1124, Allocated = 8192, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 1124, Allocated = 8192, UncertainAllocated = 8192, IsZero = False } vs SizeVector { Logical = 1124, Allocated = 8192, UncertainAllocated = 0, IsZero = False }
  - names: Size SizeVector { Logical = 74127, Allocated = 94208, UncertainAllocated = 94208, IsZero = False } vs SizeVector { Logical = 74127, Allocated = 94208, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 74127, Allocated = 94208, UncertainAllocated = 94208, IsZero = False } vs SizeVector { Logical = 74127, Allocated = 94208, UncertainAllocated = 0, IsZero = False }
  - denied: Size SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 1048576, IsZero = False } vs SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 1048576, IsZero = False } vs SizeVector { Logical = 1048576, Allocated = 1048576, UncertainAllocated = 0, IsZero = False }
  - names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa: Size SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }
  - names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb: Size SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }
  - names\aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\cccccccccccccccccccccccccccccccccccccccccccccccccc: Size SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 73728, IsZero = False } vs SizeVector { Logical = 70000, Allocated = 73728, UncertainAllocated = 0, IsZero = False }

### validation FAT32 win22 [1]
validate exit code: 1, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-fa24a09e.vhdx
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
- LowImpact-All vs Fast-All: **29 differences**:
  - (root): Size SizeVector { Logical = 21690626, Allocated = 42295296, UncertainAllocated = 42295296, IsZero = False } vs SizeVector { Logical = 21690626, Allocated = 42295296, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 21690626, Allocated = 42295296, UncertainAllocated = 42295296, IsZero = False } vs SizeVector { Logical = 21690626, Allocated = 42295296, UncertainAllocated = 0, IsZero = False }
  - plain: Size SizeVector { Logical = 19513223, Allocated = 19615744, UncertainAllocated = 19615744, IsZero = False } vs SizeVector { Logical = 19513223, Allocated = 19615744, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 19513223, Allocated = 19615744, UncertainAllocated = 19615744, IsZero = False } vs SizeVector { Logical = 19513223, Allocated = 19615744, UncertainAllocated = 0, IsZero = False }
  - wide: Size SizeVector { Logical = 5000, Allocated = 20480000, UncertainAllocated = 20480000, IsZero = False } vs SizeVector { Logical = 5000, Allocated = 20480000, UncertainAllocated = 0, IsZero = False }; ExclusiveVerified SizeVector { Logical = 5000, Allocated = 20480000, UncertainAllocated = 20480000, IsZero = False } vs SizeVector { Logical = 5000, Allocated = 20480000, UncertainAllocated = 0, IsZero = False }

### validation FAT32 win22 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ FAT32, cluster 4096, VHDX D:\a\_temp\si-vhd\si-fat32-1ec69a2a.vhdx
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

### validation NTFS win25-vs2026 [1]
validate exit code: 0, allocation exit code: 0

Volume E:\ NTFS, cluster 4096, VHDX D:\a\_temp\si-vhd\si-ntfs-9ada41e0.vhdx
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

