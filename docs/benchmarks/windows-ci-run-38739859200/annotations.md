### bench 1000000 win22 [2]
| baseline (no scan) | 20.0 | 123,971 | 0.15 | 0.21 | 0.29 | 13.7 | 14.62 / 14.67 / 14.86 |
| during low-impact scan (cold) | 21.1 | 79,537 | 0.25 | 0.37 | 0.53 | 15.9 | 14.61 / 14.67 / 15.06 |
| during fast scan (cold) | 14.3 | 32,638 | 0.23 | 0.85 | 1.19 | 12.7 | 14.61 / 14.67 / 15.09 |

### bench 1000000 win22 [1]
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
| Fast | 1.8 | 5/8 | 159.2 | 204.1 | 204.1 |
| LowImpact | 3.2 | 7/8 | 87.2 | 171.0 | 171.0 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|

### raw bench 1000000 win22 [4]
startup-warm-9: {"ProcessStartToMainMs":65.7372,"MainToReadyMs":36.2943,"ProcessStartToReadyMs":102.0315,"ReadyWorkingSet":25702400,"ReadyPrivateBytes":22843392,"IdleWorkingSet":25829376,"IdlePrivateBytes":8200192,"ExternalWallMs":5168.434}

### raw bench 1000000 win22 [3]
scan-low-impact-warm-links-all: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"All","CacheState":"warm","Seconds":65.7236835,"Entries":1010000,"Directories":20742,"EntriesPerSecond":15367.367533501068,"BaselineWorkingSet":23715840,"BaselinePrivateBytes":8187904,"PeakWorkingSet":82513920,"PeakPrivateBytes":63668224,"PeakManagedHeap":43299968,"AfterWorkingSet":50704384,"AfterPrivateBytes":30109696,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-links-none: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"None","CacheState":"warm","Seconds":3.1757567,"Entries":1010000,"Directories":20742,"EntriesPerSecond":318034.43884728325,"BaselineWorkingSet":23683072,"BaselinePrivateBytes":7376896,"PeakWorkingSet":81145856,"PeakPrivateBytes":62255104,"PeakManagedHeap":42543808,"AfterWorkingSet":53608448,"AfterPrivateBytes":32960512,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
startup-cold: {"ProcessStartToMainMs":205.0332,"MainToReadyMs":69.1622,"ProcessStartToReadyMs":274.1954,"ReadyWorkingSet":25587712,"ReadyPrivateBytes":22700032,"IdleWorkingSet":25726976,"IdlePrivateBytes":8073216,"ExternalWallMs":5369.722}
startup-warm-1: {"ProcessStartToMainMs":52.5325,"MainToReadyMs":35.0001,"ProcessStartToReadyMs":87.5326,"ReadyWorkingSet":25681920,"ReadyPrivateBytes":22822912,"IdleWorkingSet":25866240,"IdlePrivateBytes":8192000,"ExternalWallMs":5148.0979}
startup-warm-10: {"ProcessStartToMainMs":60.894,"MainToReadyMs":35.323,"ProcessStartToReadyMs":96.217,"ReadyWorkingSet":25657344,"ReadyPrivateBytes":22732800,"IdleWorkingSet":25808896,"IdlePrivateBytes":8138752,"ExternalWallMs":5150.5236}
startup-warm-2: {"ProcessStartToMainMs":60.8347,"MainToReadyMs":38.6621,"ProcessStartToReadyMs":99.4968,"ReadyWorkingSet":25690112,"ReadyPrivateBytes":22859776,"IdleWorkingSet":25845760,"IdlePrivateBytes":8245248,"ExternalWallMs":5158.9578}
startup-warm-3: {"ProcessStartToMainMs":52.9156,"MainToReadyMs":35.7822,"ProcessStartToReadyMs":88.6978,"ReadyWorkingSet":25698304,"ReadyPrivateBytes":22822912,"IdleWorkingSet":25845760,"IdlePrivateBytes":8200192,"ExternalWallMs":5147.2648}
startup-warm-4: {"ProcessStartToMainMs":53.9739,"MainToReadyMs":35.1569,"ProcessStartToReadyMs":89.1308,"ReadyWorkingSet":25796608,"ReadyPrivateBytes":22913024,"IdleWorkingSet":25956352,"IdlePrivateBytes":8314880,"ExternalWallMs":5150.9973}
startup-warm-5: {"ProcessStartToMainMs":52.5871,"MainToReadyMs":35.762,"ProcessStartToReadyMs":88.3491,"ReadyWorkingSet":25718784,"ReadyPrivateBytes":22839296,"IdleWorkingSet":25890816,"IdlePrivateBytes":8249344,"ExternalWallMs":5150.3119}
startup-warm-6: {"ProcessStartToMainMs":55.4171,"MainToReadyMs":35.7118,"ProcessStartToReadyMs":91.1289,"ReadyWorkingSet":25739264,"ReadyPrivateBytes":22884352,"IdleWorkingSet":25890816,"IdlePrivateBytes":8232960,"ExternalWallMs":5154.7114}
startup-warm-7: {"ProcessStartToMainMs":54.5929,"MainToReadyMs":35.8195,"ProcessStartToReadyMs":90.4124,"ReadyWorkingSet":25718784,"ReadyPrivateBytes":22863872,"IdleWorkingSet":25907200,"IdlePrivateBytes":8245248,"ExternalWallMs":5156.4877}
startup-warm-8: {"ProcessStartToMainMs":63.8965,"MainToReadyMs":37.973,"ProcessStartToReadyMs":101.8695,"ReadyWorkingSet":25718784,"ReadyPrivateBytes":22867968,"IdleWorkingSet":25907200,"IdlePrivateBytes":8306688,"ExternalWallMs":5174.3344}

### raw bench 1000000 win22 [2]
scan-fast-warm-1: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":1.8131316,"Entries":1010000,"Directories":20742,"EntriesPerSecond":557047.265625948,"BaselineWorkingSet":23715840,"BaselinePrivateBytes":8167424,"PeakWorkingSet":84287488,"PeakPrivateBytes":65032192,"PeakManagedHeap":40153936,"AfterWorkingSet":56852480,"AfterPrivateBytes":36483072,"PeakOpenDirectories":462,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-warm-2: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":1.8314205,"Entries":1010000,"Directories":20742,"EntriesPerSecond":551484.4897717374,"BaselineWorkingSet":23830528,"BaselinePrivateBytes":8364032,"PeakWorkingSet":83021824,"PeakPrivateBytes":63811584,"PeakManagedHeap":41269120,"AfterWorkingSet":56688640,"AfterPrivateBytes":36249600,"PeakOpenDirectories":468,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-2cpu: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":15.1145465,"Entries":1010000,"Directories":20742,"EntriesPerSecond":66823.04361563214,"BaselineWorkingSet":23732224,"BaselinePrivateBytes":8187904,"PeakWorkingSet":79835136,"PeakPrivateBytes":60735488,"PeakManagedHeap":44079416,"AfterWorkingSet":52301824,"AfterPrivateBytes":31404032,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-with-probe: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":18.7158812,"Entries":1010000,"Directories":20742,"EntriesPerSecond":53964.86487635966,"BaselineWorkingSet":23719936,"BaselinePrivateBytes":8155136,"PeakWorkingSet":79720448,"PeakPrivateBytes":60747776,"PeakManagedHeap":44734744,"AfterWorkingSet":52178944,"AfterPrivateBytes":31408128,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":14.6485014,"Entries":1010000,"Directories":20742,"EntriesPerSecond":68949.03256110553,"BaselineWorkingSet":23728128,"BaselinePrivateBytes":8224768,"PeakWorkingSet":81866752,"PeakPrivateBytes":62836736,"PeakManagedHeap":43701200,"AfterWorkingSet":54325248,"AfterPrivateBytes":33550336,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-1: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":3.198637,"Entries":1010000,"Directories":20742,"EntriesPerSecond":315759.4938093944,"BaselineWorkingSet":23732224,"BaselinePrivateBytes":8204288,"PeakWorkingSet":81018880,"PeakPrivateBytes":62156800,"PeakManagedHeap":43870024,"AfterWorkingSet":53481472,"AfterPrivateBytes":32813056,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-2: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":3.2099303,"Entries":1010000,"Directories":20742,"EntriesPerSecond":314648.5766373183,"BaselineWorkingSet":23724032,"BaselinePrivateBytes":8216576,"PeakWorkingSet":81739776,"PeakPrivateBytes":62738432,"PeakManagedHeap":42990880,"AfterWorkingSet":54210560,"AfterPrivateBytes":33439744,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}

### raw bench 1000000 win22 [1]
cancel-fast: {"Mode":"Fast","FullScanSeconds":1.7956995,"Latency":{"Count":5,"Min":45.0155,"P50":159.164,"P95":204.1264,"P99":204.1264,"Max":204.1264,"Mean":146.45136}}
cancel-low-impact: {"Mode":"LowImpact","FullScanSeconds":3.1521796,"Latency":{"Count":7,"Min":53.9186,"P50":87.2212,"P95":171.0437,"P99":171.0437,"Max":171.0437,"Mean":108.73417142857144}}
environment: {"kind":"CI: GitHub-hosted win22 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.","os":"Microsoft Windows Server 2022 Datacenter 10.0.20348","cpu":"AMD EPYC 7763 64-Core Processor                 (2 cores, 4 logical)","memoryGB":16.0,"physicalDisks":["Msft Virtual Disk [Unspecified, SAS, 150 GB]","Msft Virtual Disk [Unspecified, SAS, 256 GB]"],"defenderRealtime":false,"fixtureFiles":1000000}
fixture: {"Root":"E:\\bench","Files":1000000,"Directories":20000,"HardLinks":10000,"BytesWritten":615236100,"Seconds":66.7357794}
probe-baseline: {"Label":"baseline (no scan)","Seconds":20.019959,"ReadLatency":{"Count":123971,"Min":0.1252,"P50":0.1502,"P95":0.2086,"P99":0.291,"Max":13.7468,"Mean":0.16107871518339234},"TimerLateness":{"Count":1301,"Min":0.1482000000000001,"P50":14.6243,"P95":14.6731,"P99":14.8574,"Max":15.2436,"Mean":14.380800153727876},"Reads":123971}
probe-during-fast: {"Label":"during fast scan (cold)","Seconds":14.2688359,"ReadLatency":{"Count":32638,"Min":0.1286,"P50":0.225,"P95":0.8483,"P99":1.1934,"Max":12.7183,"Mean":0.4271163949996913},"TimerLateness":{"Count":919,"Min":0.14080000000000004,"P50":14.6091,"P95":14.6744,"P99":15.0887,"Max":15.4511,"Mean":14.50058857453753},"Reads":32638}
probe-during-low-impact: {"Label":"during low-impact scan (cold)","Seconds":21.130016,"ReadLatency":{"Count":79537,"Min":0.1275,"P50":0.2498,"P95":0.3743,"P99":0.5289,"Max":15.9181,"Mean":0.25578464991135286},"TimerLateness":{"Count":1360,"Min":0.1806000000000001,"P50":14.6105,"P95":14.6749,"P99":15.0626,"Max":15.188099999999999,"Mean":14.51642544117647},"Reads":79537}
scan-fast-cold-2cpu: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":10.60894,"Entries":1010000,"Directories":20742,"EntriesPerSecond":95202.72524870533,"BaselineWorkingSet":23724032,"BaselinePrivateBytes":8179712,"PeakWorkingSet":77176832,"PeakPrivateBytes":56303616,"PeakManagedHeap":39412832,"AfterWorkingSet":51798016,"AfterPrivateBytes":30822400,"PeakOpenDirectories":251,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold-with-probe: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":11.9795812,"Entries":1010000,"Directories":20742,"EntriesPerSecond":84310.12596667402,"BaselineWorkingSet":23760896,"BaselinePrivateBytes":8200192,"PeakWorkingSet":80752640,"PeakPrivateBytes":61100032,"PeakManagedHeap":39913448,"AfterWorkingSet":53362688,"AfterPrivateBytes":32595968,"PeakOpenDirectories":468,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":9.7341913,"Entries":1010000,"Directories":20742,"EntriesPerSecond":103757.97730624011,"BaselineWorkingSet":23724032,"BaselinePrivateBytes":8187904,"PeakWorkingSet":82472960,"PeakPrivateBytes":62586880,"PeakManagedHeap":39530856,"AfterWorkingSet":55463936,"AfterPrivateBytes":34570240,"PeakOpenDirectories":462,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}

### bench 1000000 win25-vs2026 [2]
| baseline (no scan) | 20.0 | 149,639 | 0.12 | 0.17 | 0.24 | 23.7 | 3.06 / 14.90 / 15.10 |
| during low-impact scan (cold) | 32.3 | 184,788 | 0.14 | 0.26 | 0.32 | 27.8 | 12.61 / 14.91 / 15.07 |
| during fast scan (cold) | 11.8 | 29,400 | 0.51 | 0.71 | 0.88 | 25.6 | 12.70 / 15.01 / 15.14 |

### bench 1000000 win25-vs2026 [1]
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
| Fast | 1.7 | 5/8 | 166.4 | 176.2 | 176.2 |
| LowImpact | 3.2 | 8/8 | 103.3 | 246.1 | 246.1 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|

### raw bench 1000000 win25-vs2026 [4]
startup-warm-9: {"ProcessStartToMainMs":61.1469,"MainToReadyMs":37.7339,"ProcessStartToReadyMs":98.8808,"ReadyWorkingSet":27656192,"ReadyPrivateBytes":23031808,"IdleWorkingSet":27893760,"IdlePrivateBytes":8429568,"ExternalWallMs":5157.4158}

### raw bench 1000000 win25-vs2026 [3]
scan-low-impact-warm-links-all: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"All","CacheState":"warm","Seconds":57.7057064,"Entries":1010000,"Directories":20742,"EntriesPerSecond":17502.601787749714,"BaselineWorkingSet":25522176,"BaselinePrivateBytes":8179712,"PeakWorkingSet":84480000,"PeakPrivateBytes":63393792,"PeakManagedHeap":43407680,"AfterWorkingSet":56791040,"AfterPrivateBytes":33976320,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-links-none: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"None","CacheState":"warm","Seconds":3.0501876,"Entries":1010000,"Directories":20742,"EntriesPerSecond":331127.1739482516,"BaselineWorkingSet":25501696,"BaselinePrivateBytes":8159232,"PeakWorkingSet":82071552,"PeakPrivateBytes":61321216,"PeakManagedHeap":39518400,"AfterWorkingSet":54501376,"AfterPrivateBytes":31961088,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
startup-cold: {"ProcessStartToMainMs":181.5019,"MainToReadyMs":70.0714,"ProcessStartToReadyMs":251.5733,"ReadyWorkingSet":27529216,"ReadyPrivateBytes":22745088,"IdleWorkingSet":27783168,"IdlePrivateBytes":8163328,"ExternalWallMs":5336.5684}
startup-warm-1: {"ProcessStartToMainMs":61.9048,"MainToReadyMs":37.7611,"ProcessStartToReadyMs":99.6659,"ReadyWorkingSet":27574272,"ReadyPrivateBytes":22863872,"IdleWorkingSet":27799552,"IdlePrivateBytes":8261632,"ExternalWallMs":5156.9028}
startup-warm-10: {"ProcessStartToMainMs":65.7416,"MainToReadyMs":37.2243,"ProcessStartToReadyMs":102.9659,"ReadyWorkingSet":27660288,"ReadyPrivateBytes":23007232,"IdleWorkingSet":27860992,"IdlePrivateBytes":8392704,"ExternalWallMs":5163.7732}
startup-warm-2: {"ProcessStartToMainMs":58.5502,"MainToReadyMs":37.5359,"ProcessStartToReadyMs":96.0861,"ReadyWorkingSet":27639808,"ReadyPrivateBytes":22999040,"IdleWorkingSet":27873280,"IdlePrivateBytes":8417280,"ExternalWallMs":5155.5131}
startup-warm-3: {"ProcessStartToMainMs":58.7529,"MainToReadyMs":41.6201,"ProcessStartToReadyMs":100.373,"ReadyWorkingSet":27516928,"ReadyPrivateBytes":22822912,"IdleWorkingSet":27738112,"IdlePrivateBytes":8220672,"ExternalWallMs":5164.7267}
startup-warm-4: {"ProcessStartToMainMs":60.7053,"MainToReadyMs":36.3268,"ProcessStartToReadyMs":97.0321,"ReadyWorkingSet":27627520,"ReadyPrivateBytes":22904832,"IdleWorkingSet":27856896,"IdlePrivateBytes":8310784,"ExternalWallMs":5158.9838}
startup-warm-5: {"ProcessStartToMainMs":58.7904,"MainToReadyMs":36.4049,"ProcessStartToReadyMs":95.1953,"ReadyWorkingSet":27578368,"ReadyPrivateBytes":22867968,"IdleWorkingSet":27799552,"IdlePrivateBytes":8273920,"ExternalWallMs":5152.2035}
startup-warm-6: {"ProcessStartToMainMs":59.5168,"MainToReadyMs":37.4286,"ProcessStartToReadyMs":96.9454,"ReadyWorkingSet":27738112,"ReadyPrivateBytes":23146496,"IdleWorkingSet":27963392,"IdlePrivateBytes":8556544,"ExternalWallMs":5155.0977}
startup-warm-7: {"ProcessStartToMainMs":60.9702,"MainToReadyMs":36.3775,"ProcessStartToReadyMs":97.3477,"ReadyWorkingSet":27590656,"ReadyPrivateBytes":22896640,"IdleWorkingSet":27820032,"IdlePrivateBytes":8278016,"ExternalWallMs":5154.3731}
startup-warm-8: {"ProcessStartToMainMs":63.4291,"MainToReadyMs":39.0273,"ProcessStartToReadyMs":102.4564,"ReadyWorkingSet":27582464,"ReadyPrivateBytes":22855680,"IdleWorkingSet":27803648,"IdlePrivateBytes":8232960,"ExternalWallMs":5164.1079}

### raw bench 1000000 win25-vs2026 [2]
scan-fast-warm-1: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":1.6931165,"Entries":1010000,"Directories":20742,"EntriesPerSecond":596533.0796788053,"BaselineWorkingSet":25497600,"BaselinePrivateBytes":8146944,"PeakWorkingSet":89239552,"PeakPrivateBytes":68612096,"PeakManagedHeap":44292920,"AfterWorkingSet":61988864,"AfterPrivateBytes":39620608,"PeakOpenDirectories":468,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-warm-2: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":1.6651392,"Entries":1010000,"Directories":20742,"EntriesPerSecond":606555.8963478849,"BaselineWorkingSet":25505792,"BaselinePrivateBytes":8167424,"PeakWorkingSet":88866816,"PeakPrivateBytes":67747840,"PeakManagedHeap":46821408,"AfterWorkingSet":57544704,"AfterPrivateBytes":35061760,"PeakOpenDirectories":468,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-2cpu: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":15.3807977,"Entries":1010000,"Directories":20742,"EntriesPerSecond":65666.29505828557,"BaselineWorkingSet":25468928,"BaselinePrivateBytes":8077312,"PeakWorkingSet":81707008,"PeakPrivateBytes":60674048,"PeakManagedHeap":43198856,"AfterWorkingSet":54177792,"AfterPrivateBytes":31391744,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-with-probe: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":17.2630247,"Entries":1010000,"Directories":20742,"EntriesPerSecond":58506.548971108175,"BaselineWorkingSet":25497600,"BaselinePrivateBytes":8146944,"PeakWorkingSet":81833984,"PeakPrivateBytes":60948480,"PeakManagedHeap":42231608,"AfterWorkingSet":54181888,"AfterPrivateBytes":31551488,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":12.5458467,"Entries":1010000,"Directories":20742,"EntriesPerSecond":80504.72990396096,"BaselineWorkingSet":25550848,"BaselinePrivateBytes":8212480,"PeakWorkingSet":81559552,"PeakPrivateBytes":60416000,"PeakManagedHeap":41824536,"AfterWorkingSet":53948416,"AfterPrivateBytes":31088640,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-1: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":3.0248079,"Entries":1010000,"Directories":20742,"EntriesPerSecond":333905.5018998066,"BaselineWorkingSet":25522176,"BaselinePrivateBytes":8200192,"PeakWorkingSet":82178048,"PeakPrivateBytes":61427712,"PeakManagedHeap":42332672,"AfterWorkingSet":54562816,"AfterPrivateBytes":32133120,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-2: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":3.0984535,"Entries":1010000,"Directories":20742,"EntriesPerSecond":325969.06811736885,"BaselineWorkingSet":25546752,"BaselinePrivateBytes":8200192,"PeakWorkingSet":82067456,"PeakPrivateBytes":60989440,"PeakManagedHeap":41704392,"AfterWorkingSet":54407168,"AfterPrivateBytes":31641600,"PeakOpenDirectories":139,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}

### raw bench 1000000 win25-vs2026 [1]
cancel-fast: {"Mode":"Fast","FullScanSeconds":1.6878302,"Latency":{"Count":5,"Min":64.6027,"P50":166.4331,"P95":176.2358,"P99":176.2358,"Max":176.2358,"Mean":146.92852}}
cancel-low-impact: {"Mode":"LowImpact","FullScanSeconds":3.2364139,"Latency":{"Count":8,"Min":53.4753,"P50":103.2882,"P95":246.1366,"P99":246.1366,"Max":246.1366,"Mean":137.64295}}
environment: {"kind":"CI: GitHub-hosted win25-vs2026 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.","os":"Microsoft Windows Server 2025 Datacenter 10.0.26100","cpu":"AMD EPYC 9V74 80-Core Processor                 (2 cores, 4 logical)","memoryGB":16.0,"physicalDisks":["Msft Virtual Disk [Unspecified, SAS, 150 GB]","Msft Virtual Disk [Unspecified, SAS, 150 GB]"],"defenderRealtime":false,"fixtureFiles":1000000}
fixture: {"Root":"E:\\bench","Files":1000000,"Directories":20000,"HardLinks":10000,"BytesWritten":615236100,"Seconds":85.1131266}
probe-baseline: {"Label":"baseline (no scan)","Seconds":20.0097123,"ReadLatency":{"Count":149639,"Min":0.0965,"P50":0.1249,"P95":0.1715,"P99":0.2366,"Max":23.666,"Mean":0.1334387505930805},"TimerLateness":{"Count":2420,"Min":0.1472,"P50":3.0571,"P95":14.8984,"P99":15.0963,"Max":15.928899999999999,"Mean":7.264052685950413},"Reads":149639}
probe-during-fast: {"Label":"during fast scan (cold)","Seconds":11.8285072,"ReadLatency":{"Count":29400,"Min":0.1071,"P50":0.5105,"P95":0.7068,"P99":0.8817,"Max":25.5936,"Mean":0.3912685442176872},"TimerLateness":{"Count":1158,"Min":0.1016999999999999,"P50":12.7007,"P95":15.0062,"P99":15.1391,"Max":15.526399999999999,"Mean":9.189648272884291},"Reads":29400}
probe-during-low-impact: {"Label":"during low-impact scan (cold)","Seconds":32.2949388,"ReadLatency":{"Count":184788,"Min":0.096,"P50":0.1364,"P95":0.2603,"P99":0.3208,"Max":27.835,"Mean":0.16631864027966178},"TimerLateness":{"Count":3333,"Min":0.1057999999999999,"P50":12.6132,"P95":14.9145,"P99":15.0735,"Max":18.5917,"Mean":8.670116921692184},"Reads":184788}
scan-fast-cold-2cpu: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":9.471853,"Entries":1010000,"Directories":20742,"EntriesPerSecond":106631.72243065851,"BaselineWorkingSet":25460736,"BaselinePrivateBytes":8085504,"PeakWorkingSet":81858560,"PeakPrivateBytes":60030976,"PeakManagedHeap":43539488,"AfterWorkingSet":54456320,"AfterPrivateBytes":31653888,"PeakOpenDirectories":251,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold-with-probe: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":10.4990379,"Entries":1010000,"Directories":20742,"EntriesPerSecond":96199.2907940641,"BaselineWorkingSet":25571328,"BaselinePrivateBytes":8200192,"PeakWorkingSet":84103168,"PeakPrivateBytes":62881792,"PeakManagedHeap":39742104,"AfterWorkingSet":56672256,"AfterPrivateBytes":33775616,"PeakOpenDirectories":468,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":8.5035343,"Entries":1010000,"Directories":20742,"EntriesPerSecond":118774.14312305413,"BaselineWorkingSet":25509888,"BaselinePrivateBytes":8159232,"PeakWorkingSet":84615168,"PeakPrivateBytes":63234048,"PeakManagedHeap":43222344,"AfterWorkingSet":57102336,"AfterPrivateBytes":34734080,"PeakOpenDirectories":467,"LinkRecords":1010000,"HardLinks":"Reconciled None","Complete":true}

### bench 3000000 win25-vs2026 [2]
| baseline (no scan) | 20.0 | 115,915 | 0.16 | 0.22 | 0.33 | 27.9 | 12.65 / 14.94 / 15.10 |
| during low-impact scan (cold) | 65.8 | 215,678 | 0.29 | 0.38 | 0.55 | 29.0 | 12.62 / 14.92 / 15.10 |
| during fast scan (cold) | 39.5 | 64,821 | 0.70 | 0.91 | 1.22 | 26.0 | 12.74 / 15.03 / 15.12 |

### bench 3000000 win25-vs2026 [1]
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
| Fast | 4.0 | 4/5 | 197.0 | 635.6 | 635.6 |
| LowImpact | 9.3 | 5/5 | 282.2 | 515.9 | 515.9 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|

### raw bench 3000000 win25-vs2026 [4]
startup-warm-7: {"ProcessStartToMainMs":60.0017,"MainToReadyMs":37.7597,"ProcessStartToReadyMs":97.7614,"ReadyWorkingSet":27627520,"ReadyPrivateBytes":22921216,"IdleWorkingSet":28033024,"IdlePrivateBytes":8478720,"ExternalWallMs":5155.451}
startup-warm-8: {"ProcessStartToMainMs":59.7972,"MainToReadyMs":37.8861,"ProcessStartToReadyMs":97.6833,"ReadyWorkingSet":27615232,"ReadyPrivateBytes":22904832,"IdleWorkingSet":27979776,"IdlePrivateBytes":8425472,"ExternalWallMs":5156.6034}
startup-warm-9: {"ProcessStartToMainMs":57.3472,"MainToReadyMs":38.7465,"ProcessStartToReadyMs":96.0937,"ReadyWorkingSet":27549696,"ReadyPrivateBytes":22822912,"IdleWorkingSet":27963392,"IdlePrivateBytes":8396800,"ExternalWallMs":5154.8397}

### raw bench 3000000 win25-vs2026 [3]
scan-low-impact-warm-2: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":10.9836096,"Entries":3030000,"Directories":61539,"EntriesPerSecond":275865.59522290376,"BaselineWorkingSet":25542656,"BaselinePrivateBytes":8183808,"PeakWorkingSet":104632320,"PeakPrivateBytes":83034112,"PeakManagedHeap":65313216,"AfterWorkingSet":53207040,"AfterPrivateBytes":30486528,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-links-all: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"All","CacheState":"warm","Seconds":220.749674,"Entries":3030000,"Directories":61539,"EntriesPerSecond":13725.954585101676,"BaselineWorkingSet":25571328,"BaselinePrivateBytes":8220672,"PeakWorkingSet":105979904,"PeakPrivateBytes":85200896,"PeakManagedHeap":65662824,"AfterWorkingSet":67608576,"AfterPrivateBytes":45674496,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-links-none: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"None","CacheState":"warm","Seconds":9.2625354,"Entries":3030000,"Directories":61539,"EntriesPerSecond":327124.25584899785,"BaselineWorkingSet":25518080,"BaselinePrivateBytes":8155136,"PeakWorkingSet":107917312,"PeakPrivateBytes":86560768,"PeakManagedHeap":66375840,"AfterWorkingSet":53014528,"AfterPrivateBytes":30535680,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
startup-cold: {"ProcessStartToMainMs":257.453,"MainToReadyMs":90.2858,"ProcessStartToReadyMs":347.7388,"ReadyWorkingSet":27602944,"ReadyPrivateBytes":22908928,"IdleWorkingSet":28020736,"IdlePrivateBytes":8491008,"ExternalWallMs":5482.2422}
startup-warm-1: {"ProcessStartToMainMs":65.5294,"MainToReadyMs":39.399,"ProcessStartToReadyMs":104.9284,"ReadyWorkingSet":27525120,"ReadyPrivateBytes":22753280,"IdleWorkingSet":27987968,"IdlePrivateBytes":8380416,"ExternalWallMs":5164.0825}
startup-warm-10: {"ProcessStartToMainMs":65.8749,"MainToReadyMs":37.1404,"ProcessStartToReadyMs":103.0153,"ReadyWorkingSet":27504640,"ReadyPrivateBytes":22757376,"IdleWorkingSet":27910144,"IdlePrivateBytes":8298496,"ExternalWallMs":5160.4621}
startup-warm-2: {"ProcessStartToMainMs":64.7808,"MainToReadyMs":39.4048,"ProcessStartToReadyMs":104.1856,"ReadyWorkingSet":27525120,"ReadyPrivateBytes":22765568,"IdleWorkingSet":27955200,"IdlePrivateBytes":8327168,"ExternalWallMs":5172.9502}
startup-warm-3: {"ProcessStartToMainMs":60.6634,"MainToReadyMs":41.366,"ProcessStartToReadyMs":102.0294,"ReadyWorkingSet":27525120,"ReadyPrivateBytes":22769664,"IdleWorkingSet":27926528,"IdlePrivateBytes":8302592,"ExternalWallMs":5176.3636}
startup-warm-4: {"ProcessStartToMainMs":71.7159,"MainToReadyMs":45.9821,"ProcessStartToReadyMs":117.698,"ReadyWorkingSet":27561984,"ReadyPrivateBytes":22798336,"IdleWorkingSet":27959296,"IdlePrivateBytes":8359936,"ExternalWallMs":5175.4688}
startup-warm-5: {"ProcessStartToMainMs":79.084,"MainToReadyMs":42.2268,"ProcessStartToReadyMs":121.3108,"ReadyWorkingSet":27582464,"ReadyPrivateBytes":22851584,"IdleWorkingSet":27979776,"IdlePrivateBytes":8417280,"ExternalWallMs":5179.467}
startup-warm-6: {"ProcessStartToMainMs":57.0859,"MainToReadyMs":38.1268,"ProcessStartToReadyMs":95.2127,"ReadyWorkingSet":27553792,"ReadyPrivateBytes":22822912,"IdleWorkingSet":27992064,"IdlePrivateBytes":8441856,"ExternalWallMs":5157.4517}

### raw bench 3000000 win25-vs2026 [2]
scan-fast-warm-1: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":3.9283178,"Entries":3030000,"Directories":61539,"EntriesPerSecond":771322.5238548673,"BaselineWorkingSet":25530368,"BaselinePrivateBytes":8167424,"PeakWorkingSet":111792128,"PeakPrivateBytes":90234880,"PeakManagedHeap":64575280,"AfterWorkingSet":55455744,"AfterPrivateBytes":32772096,"PeakOpenDirectories":672,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-warm-2: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":3.9532138,"Entries":3030000,"Directories":61539,"EntriesPerSecond":766464.9961507268,"BaselineWorkingSet":25501696,"BaselinePrivateBytes":8155136,"PeakWorkingSet":109654016,"PeakPrivateBytes":87916544,"PeakManagedHeap":67545912,"AfterWorkingSet":55275520,"AfterPrivateBytes":32419840,"PeakOpenDirectories":672,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-2cpu: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":54.234832,"Entries":3030000,"Directories":61539,"EntriesPerSecond":55868.154989398696,"BaselineWorkingSet":25505792,"BaselinePrivateBytes":8101888,"PeakWorkingSet":106602496,"PeakPrivateBytes":85110784,"PeakManagedHeap":66305624,"AfterWorkingSet":48779264,"AfterPrivateBytes":26152960,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-with-probe: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":63.3993979,"Entries":3030000,"Directories":61539,"EntriesPerSecond":47792.25198288516,"BaselineWorkingSet":25501696,"BaselinePrivateBytes":8134656,"PeakWorkingSet":106020864,"PeakPrivateBytes":84213760,"PeakManagedHeap":64413824,"AfterWorkingSet":50475008,"AfterPrivateBytes":27590656,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":48.9041831,"Entries":3030000,"Directories":61539,"EntriesPerSecond":61957.89006032902,"BaselineWorkingSet":25485312,"BaselinePrivateBytes":8146944,"PeakWorkingSet":106397696,"PeakPrivateBytes":85123072,"PeakManagedHeap":67637824,"AfterWorkingSet":48955392,"AfterPrivateBytes":26554368,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-1: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":11.6634734,"Entries":3030000,"Directories":61539,"EntriesPerSecond":259785.39120258976,"BaselineWorkingSet":25518080,"BaselinePrivateBytes":8192000,"PeakWorkingSet":104448000,"PeakPrivateBytes":83120128,"PeakManagedHeap":67665376,"AfterWorkingSet":65593344,"AfterPrivateBytes":43257856,"PeakOpenDirectories":199,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}

### raw bench 3000000 win25-vs2026 [1]
cancel-fast: {"Mode":"Fast","FullScanSeconds":4.0388222,"Latency":{"Count":4,"Min":133.6517,"P50":197.0191,"P95":635.6042,"P99":635.6042,"Max":635.6042,"Mean":351.145625}}
cancel-low-impact: {"Mode":"LowImpact","FullScanSeconds":9.276758,"Latency":{"Count":5,"Min":62.8726,"P50":282.1674,"P95":515.8815,"P99":515.8815,"Max":515.8815,"Mean":293.67476}}
environment: {"kind":"CI: GitHub-hosted win25-vs2026 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.","os":"Microsoft Windows Server 2025 Datacenter 10.0.26100","cpu":"AMD EPYC 7763 64-Core Processor                 (2 cores, 4 logical)","memoryGB":16.0,"physicalDisks":["Msft Virtual Disk [Unspecified, SAS, 150 GB]","Msft Virtual Disk [Unspecified, SAS, 150 GB]"],"defenderRealtime":false,"fixtureFiles":3000000}
fixture: {"Root":"E:\\bench","Files":3000000,"Directories":60000,"HardLinks":30000,"BytesWritten":1844738795,"Seconds":290.1596014}
probe-baseline: {"Label":"baseline (no scan)","Seconds":20.0183841,"ReadLatency":{"Count":115915,"Min":0.1217,"P50":0.1572,"P95":0.2218,"P99":0.3308,"Max":27.879,"Mean":0.17215813052668955},"TimerLateness":{"Count":2041,"Min":0.3446,"P50":12.6508,"P95":14.9355,"P99":15.1037,"Max":15.549499999999998,"Mean":8.801016805487505},"Reads":115915}
probe-during-fast: {"Label":"during fast scan (cold)","Seconds":39.4842298,"ReadLatency":{"Count":64821,"Min":0.1226,"P50":0.6973,"P95":0.9134,"P99":1.2222,"Max":26.0275,"Mean":0.5959267629317659},"TimerLateness":{"Count":3780,"Min":0.0948,"P50":12.736,"P95":15.026800000000001,"P99":15.1233,"Max":16.1951,"Mean":9.423693439153427},"Reads":64821}
probe-during-low-impact: {"Label":"during low-impact scan (cold)","Seconds":65.8015741,"ReadLatency":{"Count":215678,"Min":0.1224,"P50":0.2851,"P95":0.3826,"P99":0.5498,"Max":29.0446,"Mean":0.2918563427887853},"TimerLateness":{"Count":6860,"Min":0.1491,"P50":12.6215,"P95":14.9248,"P99":15.102799999999998,"Max":22.6289,"Mean":8.571152521865923},"Reads":215678}
scan-fast-cold-2cpu: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":33.0529566,"Entries":3030000,"Directories":61539,"EntriesPerSecond":91671.07308034283,"BaselineWorkingSet":25505792,"BaselinePrivateBytes":8142848,"PeakWorkingSet":109232128,"PeakPrivateBytes":88780800,"PeakManagedHeap":67258432,"AfterWorkingSet":48840704,"AfterPrivateBytes":26611712,"PeakOpenDirectories":359,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold-with-probe: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":37.4780204,"Entries":3030000,"Directories":61539,"EntriesPerSecond":80847.38648576007,"BaselineWorkingSet":25505792,"BaselinePrivateBytes":8138752,"PeakWorkingSet":104943616,"PeakPrivateBytes":83214336,"PeakManagedHeap":67288264,"AfterWorkingSet":49143808,"AfterPrivateBytes":26357760,"PeakOpenDirectories":668,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":30.8784883,"Entries":3030000,"Directories":61539,"EntriesPerSecond":98126.5653474364,"BaselineWorkingSet":25518080,"BaselinePrivateBytes":8171520,"PeakWorkingSet":117395456,"PeakPrivateBytes":95801344,"PeakManagedHeap":72333296,"AfterWorkingSet":61837312,"AfterPrivateBytes":39153664,"PeakOpenDirectories":664,"LinkRecords":3030000,"HardLinks":"Reconciled None","Complete":true}

### bench 100000 win25-vs2026 [2]
| during low-impact scan (cold) | 3.2 | 10,371 | 0.28 | 0.42 | 0.65 | 24.5 | 12.72 / 14.96 / 15.12 |
| during fast scan (cold) | 2.9 | 7,690 | 0.19 | 0.96 | 1.30 | 26.7 | 12.72 / 15.02 / 15.39 |

### bench 100000 win25-vs2026 [1]
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
| Fast | 0.4 | 1/8 | 40.9 | 40.9 | 40.9 |
| LowImpact | 0.9 | 4/8 | 59.3 | 220.5 | 220.5 |

### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)

| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |
|---|---|---|---|---|---|---|---|
| baseline (no scan) | 20.0 | 105,968 | 0.17 | 0.23 | 0.38 | 25.8 | 12.65 / 14.95 / 15.10 |

### raw bench 100000 win25-vs2026 [4]
startup-warm-9: {"ProcessStartToMainMs":58.7716,"MainToReadyMs":38.032,"ProcessStartToReadyMs":96.8036,"ReadyWorkingSet":27746304,"ReadyPrivateBytes":23011328,"IdleWorkingSet":27975680,"IdlePrivateBytes":8400896,"ExternalWallMs":5158.0292}

### raw bench 100000 win25-vs2026 [3]
scan-low-impact-warm-links-all: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"All","CacheState":"warm","Seconds":9.0119955,"Entries":101000,"Directories":2167,"EntriesPerSecond":11207.284779491956,"BaselineWorkingSet":25653248,"BaselinePrivateBytes":8380416,"PeakWorkingSet":54501376,"PeakPrivateBytes":44167168,"PeakManagedHeap":30341008,"AfterWorkingSet":48582656,"AfterPrivateBytes":26353664,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-links-none: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"None","CacheState":"warm","Seconds":0.663039,"Entries":101000,"Directories":2167,"EntriesPerSecond":152328.8976968172,"BaselineWorkingSet":25612288,"BaselinePrivateBytes":8269824,"PeakWorkingSet":52453376,"PeakPrivateBytes":40189952,"PeakManagedHeap":30630536,"AfterWorkingSet":46530560,"AfterPrivateBytes":25100288,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
startup-cold: {"ProcessStartToMainMs":280.5751,"MainToReadyMs":96.8429,"ProcessStartToReadyMs":377.418,"ReadyWorkingSet":27627520,"ReadyPrivateBytes":22867968,"IdleWorkingSet":27852800,"IdlePrivateBytes":8220672,"ExternalWallMs":5474.5834}
startup-warm-1: {"ProcessStartToMainMs":58.1192,"MainToReadyMs":38.0457,"ProcessStartToReadyMs":96.1649,"ReadyWorkingSet":27672576,"ReadyPrivateBytes":22908928,"IdleWorkingSet":27889664,"IdlePrivateBytes":8282112,"ExternalWallMs":5153.8544}
startup-warm-10: {"ProcessStartToMainMs":60.6099,"MainToReadyMs":37.3929,"ProcessStartToReadyMs":98.0028,"ReadyWorkingSet":27684864,"ReadyPrivateBytes":22986752,"IdleWorkingSet":27934720,"IdlePrivateBytes":8335360,"ExternalWallMs":5154.3771}
startup-warm-2: {"ProcessStartToMainMs":62.5292,"MainToReadyMs":41.1962,"ProcessStartToReadyMs":103.7254,"ReadyWorkingSet":27721728,"ReadyPrivateBytes":22962176,"IdleWorkingSet":27918336,"IdlePrivateBytes":8339456,"ExternalWallMs":5161.6497}
startup-warm-3: {"ProcessStartToMainMs":57.9793,"MainToReadyMs":37.5299,"ProcessStartToReadyMs":95.5092,"ReadyWorkingSet":27717632,"ReadyPrivateBytes":22958080,"IdleWorkingSet":27934720,"IdlePrivateBytes":8331264,"ExternalWallMs":5152.5347}
startup-warm-4: {"ProcessStartToMainMs":58.5253,"MainToReadyMs":37.7563,"ProcessStartToReadyMs":96.2816,"ReadyWorkingSet":27828224,"ReadyPrivateBytes":23142400,"IdleWorkingSet":28041216,"IdlePrivateBytes":8531968,"ExternalWallMs":5153.3438}
startup-warm-5: {"ProcessStartToMainMs":59.5115,"MainToReadyMs":38.8679,"ProcessStartToReadyMs":98.3794,"ReadyWorkingSet":27684864,"ReadyPrivateBytes":22990848,"IdleWorkingSet":27951104,"IdlePrivateBytes":8364032,"ExternalWallMs":5155.9191}
startup-warm-6: {"ProcessStartToMainMs":59.7145,"MainToReadyMs":38.7335,"ProcessStartToReadyMs":98.448,"ReadyWorkingSet":27725824,"ReadyPrivateBytes":23027712,"IdleWorkingSet":28008448,"IdlePrivateBytes":8421376,"ExternalWallMs":5156.0957}
startup-warm-7: {"ProcessStartToMainMs":58.109,"MainToReadyMs":37.5064,"ProcessStartToReadyMs":95.6154,"ReadyWorkingSet":27721728,"ReadyPrivateBytes":22953984,"IdleWorkingSet":27938816,"IdlePrivateBytes":8314880,"ExternalWallMs":5152.635}
startup-warm-8: {"ProcessStartToMainMs":61.3987,"MainToReadyMs":37.8728,"ProcessStartToReadyMs":99.2715,"ReadyWorkingSet":27648000,"ReadyPrivateBytes":22855680,"IdleWorkingSet":27881472,"IdlePrivateBytes":8269824,"ExternalWallMs":5156.7724}

### raw bench 100000 win25-vs2026 [2]
scan-fast-warm-1: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":0.418492,"Entries":101000,"Directories":2167,"EntriesPerSecond":241342.72578687288,"BaselineWorkingSet":25530368,"BaselinePrivateBytes":8192000,"PeakWorkingSet":52715520,"PeakPrivateBytes":41291776,"PeakManagedHeap":25704144,"AfterWorkingSet":46931968,"AfterPrivateBytes":26267648,"PeakOpenDirectories":221,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-warm-2: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":0.328939,"Entries":101000,"Directories":2167,"EntriesPerSecond":307047.8112963194,"BaselineWorkingSet":25579520,"BaselinePrivateBytes":8237056,"PeakWorkingSet":52559872,"PeakPrivateBytes":41082880,"PeakManagedHeap":25183176,"AfterWorkingSet":46714880,"AfterPrivateBytes":26058752,"PeakOpenDirectories":221,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-2cpu: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":2.3496734,"Entries":101000,"Directories":2167,"EntriesPerSecond":42984.6973626207,"BaselineWorkingSet":25534464,"BaselinePrivateBytes":8187904,"PeakWorkingSet":57131008,"PeakPrivateBytes":43900928,"PeakManagedHeap":29394304,"AfterWorkingSet":50880512,"AfterPrivateBytes":28422144,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold-with-probe: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":2.5689943,"Entries":101000,"Directories":2167,"EntriesPerSecond":39314.99575534286,"BaselineWorkingSet":25538560,"BaselinePrivateBytes":8155136,"PeakWorkingSet":57126912,"PeakPrivateBytes":43839488,"PeakManagedHeap":29009752,"AfterWorkingSet":50868224,"AfterPrivateBytes":28360704,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-cold: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":1.9887877,"Entries":101000,"Directories":2167,"EntriesPerSecond":50784.70668337299,"BaselineWorkingSet":25505792,"BaselinePrivateBytes":8167424,"PeakWorkingSet":57139200,"PeakPrivateBytes":44142592,"PeakManagedHeap":28679944,"AfterWorkingSet":50884608,"AfterPrivateBytes":28659712,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-1: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":0.6662692,"Entries":101000,"Directories":2167,"EntriesPerSecond":151590.378183473,"BaselineWorkingSet":25518080,"BaselinePrivateBytes":8192000,"PeakWorkingSet":52555776,"PeakPrivateBytes":40505344,"PeakManagedHeap":31072088,"AfterWorkingSet":46628864,"AfterPrivateBytes":25415680,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-low-impact-warm-2: {"Root":"E:\\bench","Mode":"LowImpact","LinkCounts":"RetainedFiles","CacheState":"warm","Seconds":0.6649729,"Entries":101000,"Directories":2167,"EntriesPerSecond":151885.8888835921,"BaselineWorkingSet":25530368,"BaselinePrivateBytes":8196096,"PeakWorkingSet":51757056,"PeakPrivateBytes":39829504,"PeakManagedHeap":31349272,"AfterWorkingSet":45830144,"AfterPrivateBytes":24739840,"PeakOpenDirectories":65,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}

### raw bench 100000 win25-vs2026 [1]
cancel-fast: {"Mode":"Fast","FullScanSeconds":0.3847551,"Latency":{"Count":1,"Min":40.9459,"P50":40.9459,"P95":40.9459,"P99":40.9459,"Max":40.9459,"Mean":40.9459}}
cancel-low-impact: {"Mode":"LowImpact","FullScanSeconds":0.9101323,"Latency":{"Count":4,"Min":36.2719,"P50":59.3249,"P95":220.4713,"P99":220.4713,"Max":220.4713,"Mean":97.209325}}
environment: {"kind":"CI: GitHub-hosted win25-vs2026 VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.","os":"Microsoft Windows Server 2025 Datacenter 10.0.26100","cpu":"AMD EPYC 7763 64-Core Processor                 (2 cores, 4 logical)","memoryGB":16.0,"physicalDisks":["Msft Virtual Disk [Unspecified, SAS, 150 GB]","Msft Virtual Disk [Unspecified, SAS, 150 GB]"],"defenderRealtime":false,"fixtureFiles":100000}
fixture: {"Root":"E:\\bench","Files":100000,"Directories":2000,"HardLinks":1000,"BytesWritten":61452937,"Seconds":11.217376}
probe-baseline: {"Label":"baseline (no scan)","Seconds":20.0178417,"ReadLatency":{"Count":105968,"Min":0.1442,"P50":0.1707,"P95":0.2254,"P99":0.3794,"Max":25.8177,"Mean":0.1884785925939872},"TimerLateness":{"Count":2050,"Min":0.34709999999999996,"P50":12.6458,"P95":14.9467,"P99":15.103100000000001,"Max":15.45,"Mean":8.757479609756093},"Reads":105968}
probe-during-fast: {"Label":"during fast scan (cold)","Seconds":2.890395,"ReadLatency":{"Count":7690,"Min":0.1495,"P50":0.1893,"P95":0.9567,"P99":1.305,"Max":26.6522,"Mean":0.36387885565669725},"TimerLateness":{"Count":282,"Min":0.7192000000000001,"P50":12.7178,"P95":15.015,"P99":15.392399999999999,"Max":15.444199999999999,"Mean":9.211024822695032},"Reads":7690}
probe-during-low-impact: {"Label":"during low-impact scan (cold)","Seconds":3.2498969,"ReadLatency":{"Count":10371,"Min":0.1543,"P50":0.2806,"P95":0.4192,"P99":0.6487,"Max":24.5316,"Mean":0.29946068845820095},"TimerLateness":{"Count":311,"Min":0.034399999999999986,"P50":12.7218,"P95":14.9602,"P99":15.120000000000001,"Max":15.3418,"Mean":9.414781350482317},"Reads":10371}
scan-fast-cold-2cpu: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":2.095078,"Entries":101000,"Directories":2167,"EntriesPerSecond":48208.2290014978,"BaselineWorkingSet":25501696,"BaselinePrivateBytes":8167424,"PeakWorkingSet":57032704,"PeakPrivateBytes":44191744,"PeakManagedHeap":29850240,"AfterWorkingSet":50778112,"AfterPrivateBytes":28708864,"PeakOpenDirectories":117,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold-with-probe: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":1.8811149,"Entries":101000,"Directories":2167,"EntriesPerSecond":53691.56344463594,"BaselineWorkingSet":25554944,"BaselinePrivateBytes":8192000,"PeakWorkingSet":56700928,"PeakPrivateBytes":43794432,"PeakManagedHeap":28664416,"AfterWorkingSet":50757632,"AfterPrivateBytes":28770304,"PeakOpenDirectories":221,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}
scan-fast-cold: {"Root":"E:\\bench","Mode":"Fast","LinkCounts":"RetainedFiles","CacheState":"cold","Seconds":1.5390705,"Entries":101000,"Directories":2167,"EntriesPerSecond":65624.02437055352,"BaselineWorkingSet":25518080,"BaselinePrivateBytes":8175616,"PeakWorkingSet":55853056,"PeakPrivateBytes":43401216,"PeakManagedHeap":29577856,"AfterWorkingSet":49983488,"AfterPrivateBytes":28377088,"PeakOpenDirectories":221,"LinkRecords":101000,"HardLinks":"Reconciled None","Complete":true}

