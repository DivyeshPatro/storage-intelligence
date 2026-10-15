## Benchmarks: 3,000,000 files — CI: GitHub-hosted win25-vs2026 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.

- OS: Microsoft Windows Server 2025 Datacenter 10.0.26100; CPU: AMD EPYC 7763 64-Core Processor                 (2 cores, 4 logical); RAM: 16.0 GB; Defender real-time: False
- Disks: Msft Virtual Disk [Unspecified, SAS, 150 GB]; Msft Virtual Disk [Unspecified, SAS, 150 GB]
- Fixture: 3,000,000 files + 30,000 hard links in 60,000 leaf directories, 1759 MB of data, created in 290 s

### Startup (console host; no UI)

| | Process start → ready (ms) | External wall incl. exit (ms) | Working set at ready (MB) | Idle working set (MB) | Idle private bytes (MB) |
|---|---|---|---|---|---|
| Cold (volume re-attached) | 348 | 5482 | 26.3 | 26.7 | 8.1 |
| Warm, median of 10 (max) | 103 (121) | 5162 | 26.3 | 26.7 | 8.0 |

### Scans

| Run | Mode | Links | Cache | CPUs | Time (s) | Entries/s | Baseline WS (MB) | Peak WS (MB) | Peak private (MB) | Peak managed heap (MB) | After scan WS / private (MB) | Peak open dirs | Hard links |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| fast-cold-2cpu | Fast | RetainedFiles | cold | 2 (affinity) | 33.1 | 91,671 | 24.3 | 104.2 | 84.7 | 64.1 | 46.6 / 25.4 | 359 | Reconciled None |
| fast-cold-with-probe | Fast | RetainedFiles | cold | all | 37.5 | 80,847 | 24.3 | 100.1 | 79.4 | 64.2 | 46.9 / 25.1 | 668 | Reconciled None |
| fast-cold | Fast | RetainedFiles | cold | all | 30.9 | 98,127 | 24.3 | 112.0 | 91.4 | 69.0 | 59.0 / 37.3 | 664 | Reconciled None |
| fast-warm-1 | Fast | RetainedFiles | warm | all | 3.9 | 771,323 | 24.3 | 106.6 | 86.1 | 61.6 | 52.9 / 31.3 | 672 | Reconciled None |
| fast-warm-2 | Fast | RetainedFiles | warm | all | 4.0 | 766,465 | 24.3 | 104.6 | 83.8 | 64.4 | 52.7 / 30.9 | 672 | Reconciled None |
| low-impact-cold-2cpu | LowImpact | RetainedFiles | cold | 2 (affinity) | 54.2 | 55,868 | 24.3 | 101.7 | 81.2 | 63.2 | 46.5 / 24.9 | 199 | Reconciled None |
| low-impact-cold-with-probe | LowImpact | RetainedFiles | cold | all | 63.4 | 47,792 | 24.3 | 101.1 | 80.3 | 61.4 | 48.1 / 26.3 | 199 | Reconciled None |
| low-impact-cold | LowImpact | RetainedFiles | cold | all | 48.9 | 61,958 | 24.3 | 101.5 | 81.2 | 64.5 | 46.7 / 25.3 | 199 | Reconciled None |
| low-impact-warm-1 | LowImpact | RetainedFiles | warm | all | 11.7 | 259,785 | 24.3 | 99.6 | 79.3 | 64.5 | 62.6 / 41.3 | 199 | Reconciled None |
| low-impact-warm-2 | LowImpact | RetainedFiles | warm | all | 11.0 | 275,866 | 24.4 | 99.8 | 79.2 | 62.3 | 50.7 / 29.1 | 199 | Reconciled None |
| low-impact-warm-links-all | LowImpact | All | warm | all | 220.7 | 13,726 | 24.4 | 101.1 | 81.3 | 62.6 | 64.5 / 43.6 | 199 | Reconciled None |
| low-impact-warm-links-none | LowImpact | None | warm | all | 9.3 | 327,124 | 24.3 | 102.9 | 82.6 | 63.3 | 50.6 / 29.1 | 199 | Reconciled None |

### Cancellation (time from Cancel() to the scan returning; warm cache)

| Mode | Full scan (s) | Trials cancelled | p50 (ms) | p95 (ms) | Max (ms) |
|---|---|---|---|---|---|
| Fast | 4.0 | 4/? | 197.0 | 635.6 | 635.6 |
| LowImpact | 9.3 | 5/? | 282.2 | 515.9 | 515.9 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|
| baseline (no scan) | 20.0 | 115,915 | 0.16 | 0.22 | 0.33 | 27.9 | 12.65 / 14.94 / 15.10 |
| during low-impact scan (cold) | 65.8 | 215,678 | 0.29 | 0.38 | 0.55 | 29.0 | 12.62 / 14.92 / 15.10 |
| during fast scan (cold) | 39.5 | 64,821 | 0.70 | 0.91 | 1.22 | 26.0 | 12.74 / 15.03 / 15.12 |
