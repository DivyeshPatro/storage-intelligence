### volume-capabilities win25-vs2026 [3]
| exFAT | yes (cluster 32768) | wof_compression | unsupported | compact.exe /c /exe:xpress4k E:\probe\wof.txt failed (1):   Compressing files in E:\probe\  wof.txt             [ERR] wof.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| exFAT | yes (cluster 32768) | filter_instances | unsupported | wof filter not attached: Instances for E: volume:  Filter                  Altitude        Instance Name       Frame   SprtFtrs  VlStatus --------------------  ------------  ----------------------  -----   --------  -------- FsDepends                407000     FsDepends                 0     0000000f   WdFilter                 328010     WdFilter Instance         0     0000000f   bfs              ... |
| exFAT | yes (cluster 32768) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| exFAT | yes (cluster 32768) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| runner volumes | yes (cluster ) | wof_compression (D:\) | unsupported | compact.exe /c /exe:xpress4k D:\a\_temp\si-wof-21fc1ba6.txt failed (1):   Compressing files in D:\a\_temp\  si-wof-21fc1ba6.txt [ERR] si-wof-21fc1ba6.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| runner volumes | yes (cluster ) | wof_compression (C:\) | supported | hr=0x00000000 external=1 provider=2 logical=1150002 compressedSize=81920 |

### volume-capabilities win25-vs2026 [2]
| FAT32 | yes (cluster 4096) | hard_link | unsupported | fsutil hardlink create E:\probe\plain-link.bin E:\probe\plain.bin failed (1): Error 1: Incorrect function. |
| FAT32 | yes (cluster 4096) | junction | unsupported | cmd.exe /c mklink /J E:\probe\junction E:\probe failed (1): Local NTFS volumes are required to complete the operation. |
| FAT32 | yes (cluster 4096) | symlink_file | unsupported | cmd.exe /c mklink E:\probe\symlink.bin E:\probe\plain.bin failed (1): The device does not support symbolic links. |
| FAT32 | yes (cluster 4096) | sparse | unsupported | fsutil sparse setflag E:\probe\sparse.bin failed (1): Error 1: Incorrect function. |
| FAT32 | yes (cluster 4096) | ntfs_compression | unsupported | compact.exe /c E:\probe\compressed.txt failed (1):   Compressing files in E:\probe\  compressed.txt      [ERR] compressed.txt: The file system does not support compression or the cluster size of the volume is larger than 4096 bytes.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| FAT32 | yes (cluster 4096) | wof_compression | unsupported | compact.exe /c /exe:xpress4k E:\probe\wof.txt failed (1):   Compressing files in E:\probe\  wof.txt             [ERR] wof.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| FAT32 | yes (cluster 4096) | filter_instances | unsupported | wof filter not attached: Instances for E: volume:  Filter                  Altitude        Instance Name       Frame   SprtFtrs  VlStatus --------------------  ------------  ----------------------  -----   --------  -------- FsDepends                407000     FsDepends                 0     0000000f   WdFilter                 328010     WdFilter Instance         0     0000000f   bfs              ... |
| FAT32 | yes (cluster 4096) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| FAT32 | yes (cluster 4096) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| exFAT | yes (cluster 32768) | file_id | supported | File ID is 0x0000000000078000 |
| exFAT | yes (cluster 32768) | hard_link | unsupported | fsutil hardlink create E:\probe\plain-link.bin E:\probe\plain.bin failed (1): Error 1: Incorrect function. |
| exFAT | yes (cluster 32768) | junction | unsupported | cmd.exe /c mklink /J E:\probe\junction E:\probe failed (1): Local NTFS volumes are required to complete the operation. |
| exFAT | yes (cluster 32768) | symlink_file | unsupported | cmd.exe /c mklink E:\probe\symlink.bin E:\probe\plain.bin failed (1): The device does not support symbolic links. |
| exFAT | yes (cluster 32768) | sparse | unsupported | fsutil sparse setflag E:\probe\sparse.bin failed (1): Error 1: Incorrect function. |
| exFAT | yes (cluster 32768) | ntfs_compression | unsupported | compact.exe /c E:\probe\compressed.txt failed (1):   Compressing files in E:\probe\  compressed.txt      [ERR] compressed.txt: The file system does not support compression or the cluster size of the volume is larger than 4096 bytes.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |

### volume-capabilities win25-vs2026 [1]
## Test volume capabilities: Microsoft Windows Server 2025 Datacenter 10.0.26100 build 26100

| Filesystem | Created | Feature | Status | Detail |
|---|---|---|---|---|
| NTFS | yes (cluster 4096) | file_id | supported | File ID is 0x00000000000000000001000000000027 |
| NTFS | yes (cluster 4096) | hard_link | supported | Hard link created for E:\probe\plain-link.bin <<===>> E:\probe\plain.bin |
| NTFS | yes (cluster 4096) | junction | supported | Junction created for E:\probe\junction <<===>> E:\probe |
| NTFS | yes (cluster 4096) | symlink_file | supported | symbolic link created for E:\probe\symlink.bin <<===>> E:\probe\plain.bin |
| NTFS | yes (cluster 4096) | sparse | supported | This file is set as sparse |
| NTFS | yes (cluster 4096) | ntfs_compression | supported | Compressed attribute set |
| NTFS | yes (cluster 4096) | wof_filter_attach | supported | ATTACH successful... Instance Name: Wof Instance |
| NTFS | yes (cluster 4096) | wof_compression | supported | hr=0x00000000 external=1 provider=2 logical=1150002 compressedSize=81920 |
| NTFS | yes (cluster 4096) | filter_instances | supported | wof attached |
| NTFS | yes (cluster 4096) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| NTFS | yes (cluster 4096) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| ReFS | yes (cluster 4096) | file_id | supported | File ID is 0x00000000000007020000000000000002 |
| ReFS | yes (cluster 4096) | hard_link | supported | Hard link created for E:\probe\plain-link.bin <<===>> E:\probe\plain.bin |
| ReFS | yes (cluster 4096) | junction | supported | Junction created for E:\probe\junction <<===>> E:\probe |
| ReFS | yes (cluster 4096) | symlink_file | supported | symbolic link created for E:\probe\symlink.bin <<===>> E:\probe\plain.bin |
| ReFS | yes (cluster 4096) | sparse | supported | This file is set as sparse |
| ReFS | yes (cluster 4096) | ntfs_compression | unsupported | compact.exe /c E:\probe\compressed.txt failed (1):   Compressing files in E:\probe\  compressed.txt      [ERR] compressed.txt: The request is not supported.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| ReFS | yes (cluster 4096) | wof_compression | unsupported | compact.exe /c /exe:xpress4k E:\probe\wof.txt failed (1):   Compressing files in E:\probe\  wof.txt             [ERR] wof.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| ReFS | yes (cluster 4096) | filter_instances | unsupported | wof filter not attached: Instances for E: volume:  Filter                  Altitude        Instance Name       Frame   SprtFtrs  VlStatus --------------------  ------------  ----------------------  -----   --------  -------- FsDepends                407000     FsDepends                 0     0000000f   WdFilter                 328010     WdFilter Instance         0     0000000f   bfs              ... |
| ReFS | yes (cluster 4096) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| ReFS | yes (cluster 4096) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| FAT32 | yes (cluster 4096) | file_id | supported | File ID is 0x0000000000403040 |

### volume-capabilities win22 [3]
| exFAT | yes (cluster 32768) | wof_compression | unsupported | compact.exe /c /exe:xpress4k E:\probe\wof.txt failed (1):   Compressing files in E:\probe\  wof.txt             [ERR] wof.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| exFAT | yes (cluster 32768) | filter_instances | unsupported | wof filter not attached: Instances for E: volume:  Filter                  Altitude        Instance Name       Frame   SprtFtrs  VlStatus --------------------  ------------  ----------------------  -----   --------  -------- FsDepends                407000     FsDepends                 0     00000007   WdFilter                 328010     WdFilter Instance         0     0000000f |
| exFAT | yes (cluster 32768) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| exFAT | yes (cluster 32768) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| runner volumes | yes (cluster ) | wof_compression (D:\) | unsupported | compact.exe /c /exe:xpress4k D:\a\_temp\si-wof-0289ed8b.txt failed (1):   Compressing files in D:\a\_temp\  si-wof-0289ed8b.txt [ERR] si-wof-0289ed8b.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| runner volumes | yes (cluster ) | wof_compression (C:\) | supported | hr=0x00000000 external=1 provider=2 logical=1150002 compressedSize=81920 |

### volume-capabilities win22 [2]
| FAT32 | yes (cluster 4096) | hard_link | unsupported | fsutil hardlink create E:\probe\plain-link.bin E:\probe\plain.bin failed (1): Error:  Incorrect function. |
| FAT32 | yes (cluster 4096) | junction | unsupported | cmd.exe /c mklink /J E:\probe\junction E:\probe failed (1): Local NTFS volumes are required to complete the operation. |
| FAT32 | yes (cluster 4096) | symlink_file | unsupported | cmd.exe /c mklink E:\probe\symlink.bin E:\probe\plain.bin failed (1): The device does not support symbolic links. |
| FAT32 | yes (cluster 4096) | sparse | unsupported | fsutil sparse setflag E:\probe\sparse.bin failed (1): Error:  Incorrect function. |
| FAT32 | yes (cluster 4096) | ntfs_compression | unsupported | compact.exe /c E:\probe\compressed.txt failed (1):   Compressing files in E:\probe\  compressed.txt      [ERR] compressed.txt: The file system does not support compression or the cluster size of the volume is larger than 4096 bytes.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| FAT32 | yes (cluster 4096) | wof_compression | unsupported | compact.exe /c /exe:xpress4k E:\probe\wof.txt failed (1):   Compressing files in E:\probe\  wof.txt             [ERR] wof.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| FAT32 | yes (cluster 4096) | filter_instances | unsupported | wof filter not attached: Instances for E: volume:  Filter                  Altitude        Instance Name       Frame   SprtFtrs  VlStatus --------------------  ------------  ----------------------  -----   --------  -------- FsDepends                407000     FsDepends                 0     00000007   WdFilter                 328010     WdFilter Instance         0     0000000f |
| FAT32 | yes (cluster 4096) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| FAT32 | yes (cluster 4096) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| exFAT | yes (cluster 32768) | file_id | supported | File ID is 0x0000000000068000 |
| exFAT | yes (cluster 32768) | hard_link | unsupported | fsutil hardlink create E:\probe\plain-link.bin E:\probe\plain.bin failed (1): Error:  Incorrect function. |
| exFAT | yes (cluster 32768) | junction | unsupported | cmd.exe /c mklink /J E:\probe\junction E:\probe failed (1): Local NTFS volumes are required to complete the operation. |
| exFAT | yes (cluster 32768) | symlink_file | unsupported | cmd.exe /c mklink E:\probe\symlink.bin E:\probe\plain.bin failed (1): The device does not support symbolic links. |
| exFAT | yes (cluster 32768) | sparse | unsupported | fsutil sparse setflag E:\probe\sparse.bin failed (1): Error:  Incorrect function. |
| exFAT | yes (cluster 32768) | ntfs_compression | unsupported | compact.exe /c E:\probe\compressed.txt failed (1):   Compressing files in E:\probe\  compressed.txt      [ERR] compressed.txt: The file system does not support compression or the cluster size of the volume is larger than 4096 bytes.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |

### volume-capabilities win22 [1]
## Test volume capabilities: Microsoft Windows Server 2022 Datacenter 10.0.20348 build 20348

| Filesystem | Created | Feature | Status | Detail |
|---|---|---|---|---|
| NTFS | yes (cluster 4096) | file_id | supported | File ID is 0x00000000000000000001000000000026 |
| NTFS | yes (cluster 4096) | hard_link | supported | Hardlink created for E:\probe\plain-link.bin <<===>> E:\probe\plain.bin |
| NTFS | yes (cluster 4096) | junction | supported | Junction created for E:\probe\junction <<===>> E:\probe |
| NTFS | yes (cluster 4096) | symlink_file | supported | symbolic link created for E:\probe\symlink.bin <<===>> E:\probe\plain.bin |
| NTFS | yes (cluster 4096) | sparse | supported | This file is set as sparse |
| NTFS | yes (cluster 4096) | ntfs_compression | supported | Compressed attribute set |
| NTFS | yes (cluster 4096) | wof_filter_attach | supported | ATTACH successful... Instance Name: Wof Instance |
| NTFS | yes (cluster 4096) | wof_compression | supported | hr=0x00000000 external=1 provider=2 logical=1150002 compressedSize=81920 |
| NTFS | yes (cluster 4096) | filter_instances | supported | wof attached |
| NTFS | yes (cluster 4096) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| NTFS | yes (cluster 4096) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| ReFS | yes (cluster 4096) | file_id | supported | File ID is 0x00000000000007020000000000000002 |
| ReFS | yes (cluster 4096) | hard_link | supported | Hardlink created for E:\probe\plain-link.bin <<===>> E:\probe\plain.bin |
| ReFS | yes (cluster 4096) | junction | supported | Junction created for E:\probe\junction <<===>> E:\probe |
| ReFS | yes (cluster 4096) | symlink_file | supported | symbolic link created for E:\probe\symlink.bin <<===>> E:\probe\plain.bin |
| ReFS | yes (cluster 4096) | sparse | supported | This file is set as sparse |
| ReFS | yes (cluster 4096) | ntfs_compression | unsupported | compact.exe /c E:\probe\compressed.txt failed (1):   Compressing files in E:\probe\  compressed.txt      [ERR] compressed.txt: The request is not supported.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| ReFS | yes (cluster 4096) | wof_compression | unsupported | compact.exe /c /exe:xpress4k E:\probe\wof.txt failed (1):   Compressing files in E:\probe\  wof.txt             [ERR] wof.txt: The file system does not support compression.  0 files within 1 directories were compressed. 0 total bytes of data are stored in 0 bytes. The compression ratio is 1.0 to 1. |
| ReFS | yes (cluster 4096) | filter_instances | unsupported | wof filter not attached: Instances for E: volume:  Filter                  Altitude        Instance Name       Frame   SprtFtrs  VlStatus --------------------  ------------  ----------------------  -----   --------  -------- FsDepends                407000     FsDepends                 0     00000007   WdFilter                 328010     WdFilter Instance         0     0000000f |
| ReFS | yes (cluster 4096) | cloud_placeholder | not_probed | Requires a cloud sync provider; not available on hosted runners. |
| ReFS | yes (cluster 4096) | block_cloning | not_probed | ReFS block cloning is treated as uncertain by design. |
| FAT32 | yes (cluster 4096) | file_id | supported | File ID is 0x0000000000401040 |

