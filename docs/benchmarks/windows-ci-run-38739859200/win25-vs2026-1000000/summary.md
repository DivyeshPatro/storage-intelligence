## Benchmarks: 1,000,000 files — CI: GitHub-hosted win25-vs2026 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.

- OS: Microsoft Windows Server 2025 Datacenter 10.0.26100; CPU: AMD EPYC 9V74 80-Core Processor                 (2 cores, 4 logical); RAM: 16.0 GB; Defender real-time: False
- Disks: Msft Virtual Disk [Unspecified, SAS, 150 GB]; Msft Virtual Disk [Unspecified, SAS, 150 GB]
- Fixture: 1,000,000 files + 10,000 hard links in 20,000 leaf directories, 587 MB of data, created in 85 s

### Startup (console host; no UI)

| | Process start → ready (ms) | External wall incl. exit (ms) | Working set at ready (MB) | Idle working set (MB) | Idle private bytes (MB) |
|---|---|---|---|---|---|
| Cold (volume re-attached) | 252 | 5337 | 26.3 | 26.5 | 7.8 |
| Warm, median of 10 (max) | 98 (103) | 5157 | 26.3 | 26.5 | 7.9 |

### Scans

| Run | Mode | Links | Cache | CPUs | Time (s) | Entries/s | Baseline WS (MB) | Peak WS (MB) | Peak private (MB) | Peak managed heap (MB) | After scan WS / private (MB) | Peak open dirs | Hard links |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| fast-cold-2cpu | Fast | RetainedFiles | cold | 2 (affinity) | 9.5 | 106,632 | 24.3 | 78.1 | 57.2 | 41.5 | 51.9 / 30.2 | 251 | Reconciled None |
| fast-cold-with-probe | Fast | RetainedFiles | cold | all | 10.5 | 96,199 | 24.4 | 80.2 | 60.0 | 37.9 | 54.0 / 32.2 | 468 | Reconciled None |
| fast-cold | Fast | RetainedFiles | cold | all | 8.5 | 118,774 | 24.3 | 80.7 | 60.3 | 41.2 | 54.5 / 33.1 | 467 | Reconciled None |
| fast-warm-1 | Fast | RetainedFiles | warm | all | 1.7 | 596,533 | 24.3 | 85.1 | 65.4 | 42.2 | 59.1 / 37.8 | 468 | Reconciled None |
| fast-warm-2 | Fast | RetainedFiles | warm | all | 1.7 | 606,556 | 24.3 | 84.8 | 64.6 | 44.7 | 54.9 / 33.4 | 468 | Reconciled None |
| low-impact-cold-2cpu | LowImpact | RetainedFiles | cold | 2 (affinity) | 15.4 | 65,666 | 24.3 | 77.9 | 57.9 | 41.2 | 51.7 / 29.9 | 139 | Reconciled None |
| low-impact-cold-with-probe | LowImpact | RetainedFiles | cold | all | 17.3 | 58,507 | 24.3 | 78.0 | 58.1 | 40.3 | 51.7 / 30.1 | 139 | Reconciled None |
| low-impact-cold | LowImpact | RetainedFiles | cold | all | 12.5 | 80,505 | 24.4 | 77.8 | 57.6 | 39.9 | 51.4 / 29.6 | 139 | Reconciled None |
| low-impact-warm-1 | LowImpact | RetainedFiles | warm | all | 3.0 | 333,906 | 24.3 | 78.4 | 58.6 | 40.4 | 52.0 / 30.6 | 139 | Reconciled None |
| low-impact-warm-2 | LowImpact | RetainedFiles | warm | all | 3.1 | 325,969 | 24.4 | 78.3 | 58.2 | 39.8 | 51.9 / 30.2 | 139 | Reconciled None |
| low-impact-warm-links-all | LowImpact | All | warm | all | 57.7 | 17,503 | 24.3 | 80.6 | 60.5 | 41.4 | 54.2 / 32.4 | 139 | Reconciled None |
| low-impact-warm-links-none | LowImpact | None | warm | all | 3.1 | 331,127 | 24.3 | 78.3 | 58.5 | 37.7 | 52.0 / 30.5 | 139 | Reconciled None |

### Cancellation (time from Cancel() to the scan returning; warm cache)

| Mode | Full scan (s) | Trials cancelled | p50 (ms) | p95 (ms) | Max (ms) |
|---|---|---|---|---|---|
| Fast | 1.7 | 5/? | 166.4 | 176.2 | 176.2 |
| LowImpact | 3.2 | 8/? | 103.3 | 246.1 | 246.1 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|
| baseline (no scan) | 20.0 | 149,639 | 0.12 | 0.17 | 0.24 | 23.7 | 3.06 / 14.90 / 15.10 |
| during low-impact scan (cold) | 32.3 | 184,788 | 0.14 | 0.26 | 0.32 | 27.8 | 12.61 / 14.91 / 15.07 |
| during fast scan (cold) | 11.8 | 29,400 | 0.51 | 0.71 | 0.88 | 25.6 | 12.70 / 15.01 / 15.14 |
