| Store | Entries | Dirs | Runs | Time s (median) | Entries/s (median) | Baseline WS MB | Peak WS MB (min–max) | Peak WS − baseline MB | Peak managed heap MB | Run files MB | Peak open dirs |
|---|---|---|---|---|---|---|---|---|---|---|---|
| full | 100,000 | 2,147 | 3 | 0.31 | 326,588 | 28.0 | 55.5–55.7 | 27.6 | 24.9 | 1.1 | 4 |
| full | 1,000,000 | 20,537 | 3 | 2.43 | 411,651 | 28.0 | 115.4–127.4 | 88.4 | 65.6 | 11.2 | 4 |
| full | 3,000,000 | 60,931 | 3 | 4.12 | 728,942 | 28.0 | 178.8–203.8 | 172.3 | 153.8 | 35.2 | 4 |
| parents | 100,000 | 2,147 | 3 | 0.40 | 251,764 | 27.9 | 53.6–56.8 | 25.6 | 23.1 | 1.1 | 4 |
| parents | 1,000,000 | 20,537 | 3 | 2.14 | 466,725 | 28.0 | 75.5–76.4 | 48.2 | 32.9 | 11.2 | 4 |
| parents | 3,000,000 | 60,931 | 3 | 3.26 | 919,585 | 28.1 | 82.0–83.4 | 53.9 | 38.6 | 35.2 | 4 |

Environment: {"Machine": "vm", "Kind": "Linux cloud container (development environment; not Windows, not physical)", "Os": "Ubuntu 24.04.5 LTS", "LogicalProcessors": 4, "Processor": "X64", "PhysicalMemoryBytes": 10757766144, "Runtime": ".NET 10.0.12", "GcMode": "workstation, latency Batch", "ProcessAffinityMask": -1, "Notes": ""}
