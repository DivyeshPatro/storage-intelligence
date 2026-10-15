## Benchmarks: 1,000,000 files — CI: GitHub-hosted win22 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.

- OS: Microsoft Windows Server 2022 Datacenter 10.0.20348; CPU: AMD EPYC 7763 64-Core Processor                 (2 cores, 4 logical); RAM: 16.0 GB; Defender real-time: False
- Disks: Msft Virtual Disk [Unspecified, SAS, 150 GB]; Msft Virtual Disk [Unspecified, SAS, 256 GB]
- Fixture: 1,000,000 files + 10,000 hard links in 20,000 leaf directories, 587 MB of data, created in 67 s

### Startup (console host; no UI)

| | Process start → ready (ms) | External wall incl. exit (ms) | Working set at ready (MB) | Idle working set (MB) | Idle private bytes (MB) |
|---|---|---|---|---|---|
| Cold (volume re-attached) | 274 | 5370 | 24.4 | 24.5 | 7.7 |
| Warm, median of 10 (max) | 91 (102) | 5153 | 24.5 | 24.7 | 7.9 |

### Scans

| Run | Mode | Links | Cache | CPUs | Time (s) | Entries/s | Baseline WS (MB) | Peak WS (MB) | Peak private (MB) | Peak managed heap (MB) | After scan WS / private (MB) | Peak open dirs | Hard links |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| fast-cold-2cpu | Fast | RetainedFiles | cold | 2 (affinity) | 10.6 | 95,203 | 22.6 | 73.6 | 53.7 | 37.6 | 49.4 / 29.4 | 251 | Reconciled None |
| fast-cold-with-probe | Fast | RetainedFiles | cold | all | 12.0 | 84,310 | 22.7 | 77.0 | 58.3 | 38.1 | 50.9 / 31.1 | 468 | Reconciled None |
| fast-cold | Fast | RetainedFiles | cold | all | 9.7 | 103,758 | 22.6 | 78.7 | 59.7 | 37.7 | 52.9 / 33.0 | 462 | Reconciled None |
| fast-warm-1 | Fast | RetainedFiles | warm | all | 1.8 | 557,047 | 22.6 | 80.4 | 62.0 | 38.3 | 54.2 / 34.8 | 462 | Reconciled None |
| fast-warm-2 | Fast | RetainedFiles | warm | all | 1.8 | 551,484 | 22.7 | 79.2 | 60.9 | 39.4 | 54.1 / 34.6 | 468 | Reconciled None |
| low-impact-cold-2cpu | LowImpact | RetainedFiles | cold | 2 (affinity) | 15.1 | 66,823 | 22.6 | 76.1 | 57.9 | 42.0 | 49.9 / 29.9 | 139 | Reconciled None |
| low-impact-cold-with-probe | LowImpact | RetainedFiles | cold | all | 18.7 | 53,965 | 22.6 | 76.0 | 57.9 | 42.7 | 49.8 / 30.0 | 139 | Reconciled None |
| low-impact-cold | LowImpact | RetainedFiles | cold | all | 14.6 | 68,949 | 22.6 | 78.1 | 59.9 | 41.7 | 51.8 / 32.0 | 139 | Reconciled None |
| low-impact-warm-1 | LowImpact | RetainedFiles | warm | all | 3.2 | 315,759 | 22.6 | 77.3 | 59.3 | 41.8 | 51.0 / 31.3 | 139 | Reconciled None |
| low-impact-warm-2 | LowImpact | RetainedFiles | warm | all | 3.2 | 314,649 | 22.6 | 78.0 | 59.8 | 41.0 | 51.7 / 31.9 | 139 | Reconciled None |
| low-impact-warm-links-all | LowImpact | All | warm | all | 65.7 | 15,367 | 22.6 | 78.7 | 60.7 | 41.3 | 48.4 / 28.7 | 139 | Reconciled None |
| low-impact-warm-links-none | LowImpact | None | warm | all | 3.2 | 318,034 | 22.6 | 77.4 | 59.4 | 40.6 | 51.1 / 31.4 | 139 | Reconciled None |

### Cancellation (time from Cancel() to the scan returning; warm cache)

| Mode | Full scan (s) | Trials cancelled | p50 (ms) | p95 (ms) | Max (ms) |
|---|---|---|---|---|---|
| Fast | 1.8 | 5/? | 159.2 | 204.1 | 204.1 |
| LowImpact | 3.2 | 7/? | 87.2 | 171.0 | 171.0 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|
| baseline (no scan) | 20.0 | 123,971 | 0.15 | 0.21 | 0.29 | 13.7 | 14.62 / 14.67 / 14.86 |
| during low-impact scan (cold) | 21.1 | 79,537 | 0.25 | 0.37 | 0.53 | 15.9 | 14.61 / 14.67 / 15.06 |
| during fast scan (cold) | 14.3 | 32,638 | 0.23 | 0.85 | 1.19 | 12.7 | 14.61 / 14.67 / 15.09 |
