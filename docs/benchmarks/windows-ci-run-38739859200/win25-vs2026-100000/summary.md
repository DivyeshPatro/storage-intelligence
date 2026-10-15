## Benchmarks: 100,000 files — CI: GitHub-hosted win25-vs2026 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.

- OS: Microsoft Windows Server 2025 Datacenter 10.0.26100; CPU: AMD EPYC 7763 64-Core Processor                 (2 cores, 4 logical); RAM: 16.0 GB; Defender real-time: False
- Disks: Msft Virtual Disk [Unspecified, SAS, 150 GB]; Msft Virtual Disk [Unspecified, SAS, 150 GB]
- Fixture: 100,000 files + 1,000 hard links in 2,000 leaf directories, 59 MB of data, created in 11 s

### Startup (console host; no UI)

| | Process start → ready (ms) | External wall incl. exit (ms) | Working set at ready (MB) | Idle working set (MB) | Idle private bytes (MB) |
|---|---|---|---|---|---|
| Cold (volume re-attached) | 377 | 5475 | 26.3 | 26.6 | 7.8 |
| Warm, median of 10 (max) | 97 (104) | 5155 | 26.4 | 26.6 | 8.0 |

### Scans

| Run | Mode | Links | Cache | CPUs | Time (s) | Entries/s | Baseline WS (MB) | Peak WS (MB) | Peak private (MB) | Peak managed heap (MB) | After scan WS / private (MB) | Peak open dirs | Hard links |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| fast-cold-2cpu | Fast | RetainedFiles | cold | 2 (affinity) | 2.1 | 48,208 | 24.3 | 54.4 | 42.1 | 28.5 | 48.4 / 27.4 | 117 | Reconciled None |
| fast-cold-with-probe | Fast | RetainedFiles | cold | all | 1.9 | 53,692 | 24.4 | 54.1 | 41.8 | 27.3 | 48.4 / 27.4 | 221 | Reconciled None |
| fast-cold | Fast | RetainedFiles | cold | all | 1.5 | 65,624 | 24.3 | 53.3 | 41.4 | 28.2 | 47.7 / 27.1 | 221 | Reconciled None |
| fast-warm-1 | Fast | RetainedFiles | warm | all | 0.4 | 241,343 | 24.3 | 50.3 | 39.4 | 24.5 | 44.8 / 25.1 | 221 | Reconciled None |
| fast-warm-2 | Fast | RetainedFiles | warm | all | 0.3 | 307,048 | 24.4 | 50.1 | 39.2 | 24.0 | 44.6 / 24.9 | 221 | Reconciled None |
| low-impact-cold-2cpu | LowImpact | RetainedFiles | cold | 2 (affinity) | 2.3 | 42,985 | 24.4 | 54.5 | 41.9 | 28.0 | 48.5 / 27.1 | 65 | Reconciled None |
| low-impact-cold-with-probe | LowImpact | RetainedFiles | cold | all | 2.6 | 39,315 | 24.4 | 54.5 | 41.8 | 27.7 | 48.5 / 27.0 | 65 | Reconciled None |
| low-impact-cold | LowImpact | RetainedFiles | cold | all | 2.0 | 50,785 | 24.3 | 54.5 | 42.1 | 27.4 | 48.5 / 27.3 | 65 | Reconciled None |
| low-impact-warm-1 | LowImpact | RetainedFiles | warm | all | 0.7 | 151,590 | 24.3 | 50.1 | 38.6 | 29.6 | 44.5 / 24.2 | 65 | Reconciled None |
| low-impact-warm-2 | LowImpact | RetainedFiles | warm | all | 0.7 | 151,886 | 24.3 | 49.4 | 38.0 | 29.9 | 43.7 / 23.6 | 65 | Reconciled None |
| low-impact-warm-links-all | LowImpact | All | warm | all | 9.0 | 11,207 | 24.5 | 52.0 | 42.1 | 28.9 | 46.3 / 25.1 | 65 | Reconciled None |
| low-impact-warm-links-none | LowImpact | None | warm | all | 0.7 | 152,329 | 24.4 | 50.0 | 38.3 | 29.2 | 44.4 / 23.9 | 65 | Reconciled None |

### Cancellation (time from Cancel() to the scan returning; warm cache)

| Mode | Full scan (s) | Trials cancelled | p50 (ms) | p95 (ms) | Max (ms) |
|---|---|---|---|---|---|
| Fast | 0.4 | 1/? | 40.9 | 40.9 | 40.9 |
| LowImpact | 0.9 | 4/? | 59.3 | 220.5 | 220.5 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|
| baseline (no scan) | 20.0 | 105,968 | 0.17 | 0.23 | 0.38 | 25.8 | 12.65 / 14.95 / 15.10 |
| during low-impact scan (cold) | 3.2 | 10,371 | 0.28 | 0.42 | 0.65 | 24.5 | 12.72 / 14.96 / 15.12 |
| during fast scan (cold) | 2.9 | 7,690 | 0.19 | 0.96 | 1.30 | 26.7 | 12.72 / 15.02 / 15.39 |
