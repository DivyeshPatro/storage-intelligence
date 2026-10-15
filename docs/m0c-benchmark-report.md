# M0c preliminary benchmark report

Status: **preliminary; for review.** No result in this report was measured on physical hardware or on an HDD.
No UI exists yet, so "idle" and "startup" describe the console host only.

## Where every number comes from

| Source | Label in results | What it is | What it cannot show |
|---|---|---|---|
| **Linux development container** | `Linux cloud container (development environment; not Windows, not physical)` | This session's 4-vCPU Xeon container, Ubuntu 24.04, .NET 10.0.12. Runs the platform-independent accounting core only, with synthetic events and no disk I/O. | Anything about Windows, disks, or the scanner's I/O |
| **Windows CI** | `CI: GitHub-hosted <image> VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD.` | GitHub-hosted Windows Server 2025 (build 26100) and 2022 (build 20348) runners: 2 cores / 4 logical CPUs (AMD EPYC), 16 GB RAM, virtual SAS disks. **Defender real-time protection was off.** | Physical HDD seek behaviour, low-end CPUs, 4 GB RAM, Windows 11 client, antivirus interference |
| **2-CPU variant** (CI) | `process affinity restricted to 2 logical CPUs` | The scan process was started with affinity to 2 logical CPUs (`cmd start /affinity 3`). | A real dual-core machine: memory, disk and caches are still the runner's |
| **Resource-constrained Windows VM** | none | **Not available.** No Windows VM could be created from this environment, and the hosted runners do not support nested virtualization. | n/a |
| **Physical HDD** | none | **Pending**: no hardware. | n/a |

## How to reproduce

```
# Accounting core (any OS)
dotnet build tools/StorageIntelligence.Bench -c Release
dotnet tools/StorageIntelligence.Bench/bin/Release/net10.0-windows/si-bench.dll core --entries 3000000 --store parents --temp <dir> --out <file>
python3 tools/bench/summarize-core.py <dir-with-core-*.json>

# Windows (CI only: creates disposable VHDX volumes)
.github/workflows/m0c-benchmarks.yml  →  tools/ci/Invoke-Benchmarks.ps1 -Files <n> -OutDir <dir>
tools/ci/run-annotations.sh <run-id> > annotations.md
python3 tools/bench/extract-raw-annotations.py annotations.md <out-dir>
python3 tools/bench/summarize-windows.py <out-dir>/<image>-<files>
```

**Data:**
- `docs/benchmarks/linux-container-core/`: 18 core runs.
- `docs/benchmarks/windows-ci-run-38739859200/`: 4 jobs × 30 measurements, each with a `summary.md`.

**Method notes:**
- **Fixtures:** synthetic, 3 directory levels with 50 files per leaf folder. 70% of files are empty and 30% hold 1–4096 bytes. 1% of files have a second hard link.
- **Cold cache:** the fixture volume's VHDX is detached and re-attached before each cold run, which empties that volume's cache. Operating-system files stay cached.
- **Process isolation:** each measurement runs in a fresh process.
- **Memory:** sampled every 50 ms. Peak *working set* is resident memory; *private bytes* is committed private memory.

## 1. Memory

### 1a. Scanner on Windows (console host, workstation non-concurrent GC). CI, windows-2025 unless marked.

Low-impact mode, cold cache, link counts measured for retained files (the default):

| Entries | Folders | Baseline WS | Peak WS | Peak private | Peak managed heap | Peak WS − baseline | After scan WS / private |
|---|---|---|---|---|---|---|---|
| 100,000 | 2,000 | 24.3 MB | 54.5 MB | 42.1 MB | 27.4 MB | 30.2 MB | 48.5 / 27.3 MB |
| 1,000,000 | 20,000 | 24.4 MB | 77.8 MB | 57.6 MB | 39.9 MB | 53.4 MB | 51.4 / 29.6 MB |
| 1,000,000 (windows-2022) | 20,000 | 22.6 MB | 78.1 MB | 59.9 MB | 41.7 MB | 55.5 MB | 51.8 / 32.0 MB |
| 3,000,000 | 60,000 | 24.3 MB | 101.5 MB | 81.2 MB | 64.5 MB | 77.2 MB | 46.7 / 25.3 MB |

- **Highest peak observed:** 112.0 MB working set (fast mode, cold, 3M entries).
- **Scanner memory is not flat as size grows.** Peak above baseline rises by about 24 MB from 1M to 3M entries (about 12 bytes per entry, or about 600 bytes per folder). It returns to roughly baseline + 25 MB after the scan.
- The growth matches the cause found in 1b: the in-memory directory store and path map keep one record per folder. The hard-link sort buffer (256k records, about 14 MB, allocated once) and the per-volume tables are fixed.

### 1b. Accounting core alone (Linux container; no disk I/O; median of 3)

| Store | Entries | Peak WS − baseline | Peak managed heap | Hard-link run files |
|---|---|---|---|---|
| full in-memory records | 100k / 1M / 3M | 27.6 / 88.4 / 172.3 MB | 24.9 / 65.6 / 153.8 MB | 1.1 / 11.2 / 35.2 MB |
| parents only (≈ planned SQLite store) | 100k / 1M / 3M | 25.6 / 48.2 / **53.9 MB** | 23.1 / 32.9 / 38.6 MB | 1.1 / 11.2 / 35.2 MB |

- **With a store that keeps only parent links, the core's memory is close to flat from 1M to 3M** (+5.7 MB).
- The growth seen in the full variant (and in 1a) comes from keeping every directory record in memory.
- Run files take about **12 bytes per entry** on disk.

### 1c. Idle (console host only; no UI exists)

**Idle working set: 24.5–26.7 MB. Idle private bytes: 7.7–8.1 MB.** Measured 5 s after startup, after a full collection, on all four CI jobs.

## 2. Startup (self-contained ReadyToRun console host; "ready" = after a first small scan has run)

| Job | Cold: process start → ready | Warm: median (max) of 10 |
|---|---|---|
| windows-2025, 100k | 377 ms | 97 ms (104) |
| windows-2025, 1M | 252 ms | 98 ms (103) |
| windows-2025, 3M | 348 ms | 103 ms (121) |
| windows-2022, 1M | 274 ms | 91 ms (102) |

"Cold" here means the application files were read from a freshly re-attached volume; the OS and its own files were warm. The external wall-clock figure in the data includes a deliberate 5-second idle period and is not a startup time.

## 3. Scan throughput (entries per second; CI SSD)

| Entries | Low-impact cold | Fast cold | Low-impact warm | Fast warm | Low-impact cold, 2 CPUs | Fast cold, 2 CPUs |
|---|---|---|---|---|---|---|
| 100k | 50,785 (2.0 s) | 65,624 (1.5 s) | 151,590 | 307,048 | 42,985 | 48,208 |
| 1M | 80,505 (12.5 s) | 118,774 (8.5 s) | 333,906 | 606,556 | 65,666 | 106,632 |
| 1M (2022) | 68,949 (14.6 s) | 103,758 (9.7 s) | 315,759 | 557,047 | 66,823 | 95,203 |
| 3M | 61,958 (48.9 s) | 98,127 (30.9 s) | 259,785 | 771,323 | 55,868 | 91,671 |

**Cost of measuring link counts** (low-impact, warm): with none measured, 3.1 s at 1M. With retained files only (the default), 3.0 s. With every file measured, **57.7 s at 1M and 220.7 s at 3M (about 19× slower).** The default adds no measurable time; measuring every file is impractical for routine scans.

## 4. Cancellation (time from Cancel() to the scan returning; warm cache)

| Job | Low-impact p50 / p95 / max | Fast p50 / p95 / max |
|---|---|---|
| windows-2025, 100k | 59 / 221 / 221 ms (4 cancelled) | 41 ms (1 cancelled) |
| windows-2025, 1M | 103 / 246 / 246 ms (8 cancelled) | 166 / 176 / 176 ms (5 cancelled) |
| windows-2022, 1M | 87 / 171 / 171 ms (7 cancelled) | 159 / 204 / 204 ms (5 cancelled) |
| windows-2025, 3M | 282 / 516 / 516 ms (5 cancelled) | 197 / 636 / 636 ms (4 cancelled) |

Only trials where the scan was still running when cancellation arrived are counted; in the others the warm scan finished first. Trial totals were not preserved (section 7).

## 5. Foreground responsiveness

A separate probe process issued 4 KiB unbuffered random reads at queue depth 1 against its own 1 GiB file on the same volume, while a cold scan ran in another process.

| Job | Baseline read p95 | During low-impact scan: p95 (change) | During fast scan: p95 (change) | Probe read rate: low-impact / fast vs baseline |
|---|---|---|---|---|
| windows-2025, 100k | 0.23 ms | 0.42 ms (**+83%**) | 0.96 ms (+317%) | short scan; small sample |
| windows-2025, 1M | 0.17 ms | 0.26 ms (**+53%**) | 0.71 ms (+318%) | −24% / −67% |
| windows-2022, 1M | 0.21 ms | 0.37 ms (**+76%**) | 0.85 ms (+305%) | −36% / −62% |
| windows-2025, 3M | 0.22 ms | 0.38 ms (**+73%**) | 0.91 ms (+314%) | −43% / −72% |

- **The v0.2 target (low-impact adds at most 25% to foreground p95 latency) was not met on CI.** Low-impact mode was 3.5–6× gentler than fast mode, but still added 53–83%.
- Possible contributors, none of them verified:
  - Background I/O priority may not be honoured by the virtual storage stack.
  - Azure disk IOPS caps make any concurrent I/O visible.
  - The fixture's many tiny folders mean many metadata reads.
- **The timer-lateness probe was invalid as designed.** Its median was 12–15 ms even with no scan, which reflects Windows' default ~15.6 ms timer granularity. It cannot detect scanner impact; its numbers are in the data but are not evidence.

## 6. Recommended budgets (for review)

| Budget | Recommendation | Basis | Confidence |
|---|---|---|---|
| Whole-app peak working set | **≤ 250 MB** (unchanged) | Scanner peak ≤ 112 MB at 3M on CI | The UI's fixed cost is unmeasured, so the remaining ~138 MB for the UI is untested |
| Scanner memory above process baseline | **≤ 64 MB, flat as size grows** | Core with a parents-only store: 54 MB at 3M. The current in-memory store reaches 77 MB and grows | Requires replacing the in-memory directory store and path map with the planned on-disk store before R1 |
| Idle working set | **≤ 100 MB** (unchanged) | Console host idles at 27 MB | The UI is unmeasured |
| Cancellation | **p95 ≤ 1 s, max ≤ 2 s** | Max observed 636 ms | CI SSD only. HDD pending |
| Startup to ready (console host) | Cold ≤ 1 s, warm ≤ 250 ms | 252–377 ms cold, ≤ 121 ms warm | OS files were warm; the UI is unmeasured |
| Foreground impact, low-impact mode | **Keep ≤ +25% p95 read latency. Not met (+53% to +83% on CI).** | Section 5 | Needs adaptive throttling and physical-hardware measurement before it can be claimed |
| Default link-count measurement | Keep "retained files only"; never "all" by default | All files is about 19× slower | High (CI) |

## 7. Limitations and problems found

- **Nothing here is physical, nothing is an HDD, nothing is Windows 11.** No resource-constrained VM was available.
- **Defender real-time protection was off on the runners.** Real users' antivirus is likely to slow scans and change memory and latency.
- **Fixtures are synthetic and uniform:** tiny files and uniform folders. Real drives have deeper trees, large files and more varied names.
- **Peaks are 50 ms samples**, so a brief spike between samples could be missed. The per-process peak counter (`PeakWorkingSet64`) was not recorded; it should be for M1.
- **The timer-lateness probe is invalid** (section 5); a replacement must request a higher timer resolution or measure differently.
- **Artifacts can't be downloaded in this session.** Results were rebuilt from check-run annotations. The first attempt lost summary sections to GitHub's per-step annotation limit and its trial details; raw metrics were published first, so no measurement was lost. Annotation chunks are now larger to stay within the limit.
- Each configuration ran **once per job** (warm scans twice); there is no variance analysis across runners beyond the 2022/2025 1M pair.

## 8. Open items carried forward

- **FAT32 intermittent uncertainty difference** (M0b section 6): open; the investigation is specified there.
- **WOF, NTFS-compressed and ReFS allocation:** uncertain, apart from the specifically validated cases in M0b section 2.
- **Low-impact foreground target not met:** needs a design decision (adaptive throttling, verification of I/O priority) and physical-hardware measurement.
- **Directory store memory growth:** the planned on-disk store must replace the in-memory one before the scanner-memory budget can be met at scale.
