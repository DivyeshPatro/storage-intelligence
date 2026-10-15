#!/usr/bin/env python3
"""Summarizes one Invoke-Benchmarks.ps1 output directory as Markdown. Usage: summarize-windows.py <dir>"""
import glob
import json
import os
import statistics
import sys

d = sys.argv[1]
MB = 1024 * 1024


def load(name):
    with open(os.path.join(d, name), encoding='utf-8-sig') as f:
        return json.load(f)


def mb(v):
    return f"{v / MB:.1f}"


env = load('environment.json')
fixture = load('fixture.json')
print(f"## Benchmarks: {env['fixtureFiles']:,} files — {env['kind']}")
print()
print(f"- OS: {env['os']}; CPU: {env['cpu']}; RAM: {env['memoryGB']} GB; Defender real-time: {env['defenderRealtime']}")
print(f"- Disks: {'; '.join(env['physicalDisks']) if isinstance(env['physicalDisks'], list) else env['physicalDisks']}")
print(f"- Fixture: {fixture['Files']:,} files + {fixture['HardLinks']:,} hard links in {fixture['Directories']:,} leaf directories, "
      f"{fixture['BytesWritten'] / MB:.0f} MB of data, created in {fixture['Seconds']:.0f} s")
print()

cold = load('startup-cold.json')
warm = [load(os.path.basename(p)) for p in sorted(glob.glob(os.path.join(d, 'startup-warm-*.json')))]
print('### Startup (console host; no UI)')
print()
print('| | Process start → ready (ms) | External wall incl. exit (ms) | Working set at ready (MB) | Idle working set (MB) | Idle private bytes (MB) |')
print('|---|---|---|---|---|---|')
print(f"| Cold (volume re-attached) | {cold['ProcessStartToReadyMs']:.0f} | {cold['ExternalWallMs']:.0f} | {mb(cold['ReadyWorkingSet'])} | {mb(cold['IdleWorkingSet'])} | {mb(cold['IdlePrivateBytes'])} |")
if warm:
    med = lambda k: statistics.median(w[k] for w in warm)
    print(f"| Warm, median of {len(warm)} (max) | {med('ProcessStartToReadyMs'):.0f} ({max(w['ProcessStartToReadyMs'] for w in warm):.0f}) | "
          f"{med('ExternalWallMs'):.0f} | {mb(med('ReadyWorkingSet'))} | {mb(med('IdleWorkingSet'))} | {mb(med('IdlePrivateBytes'))} |")
print()

print('### Scans')
print()
print('| Run | Mode | Links | Cache | CPUs | Time (s) | Entries/s | Baseline WS (MB) | Peak WS (MB) | Peak private (MB) | Peak managed heap (MB) | After scan WS / private (MB) | Peak open dirs | Hard links |')
print('|---|---|---|---|---|---|---|---|---|---|---|---|---|---|')
for path in sorted(glob.glob(os.path.join(d, 'scan-*.json'))):
    s = load(os.path.basename(path))
    cpus = '2 (affinity)' if 'affinity' in (s['Environment'].get('Notes') or '') else 'all'
    name = os.path.basename(path)[5:-5]
    print(f"| {name} | {s['Mode']} | {s['LinkCounts']} | {s['CacheState']} | {cpus} | {s['Seconds']:.1f} | {s['EntriesPerSecond']:,.0f} | "
          f"{mb(s['BaselineWorkingSet'])} | {mb(s['PeakWorkingSet'])} | {mb(s['PeakPrivateBytes'])} | {mb(s['PeakManagedHeap'])} | "
          f"{mb(s['AfterWorkingSet'])} / {mb(s['AfterPrivateBytes'])} | {s['PeakOpenDirectories']} | {s['HardLinks']} |")
print()

print('### Cancellation (time from Cancel() to the scan returning; warm cache)')
print()
print('| Mode | Full scan (s) | Trials cancelled | p50 (ms) | p95 (ms) | Max (ms) |')
print('|---|---|---|---|---|---|')
for path in sorted(glob.glob(os.path.join(d, 'cancel-*.json'))):
    c = load(os.path.basename(path))
    lat = c['Latency']
    print(f"| {c['Mode']} | {c['FullScanSeconds']:.1f} | {lat['Count']}/{len(c['Trials'])} | {lat['P50']:.1f} | {lat['P95']:.1f} | {lat['Max']:.1f} |")
print()

print('### Foreground responsiveness (separate probe process; 4 KiB unbuffered random reads at QD1 on the same volume, and 1 ms timer lateness)')
print()
print('| Probe | Duration (s) | Reads | Read p50 (ms) | Read p95 (ms) | Read p99 (ms) | Read max (ms) | Timer lateness p50 / p95 / p99 (ms) |')
print('|---|---|---|---|---|---|---|---|')
for name in ['probe-baseline.json', 'probe-during-low-impact.json', 'probe-during-fast.json']:
    if not os.path.exists(os.path.join(d, name)):
        continue
    p = load(name)
    r, t = p['ReadLatency'], p['TimerLateness']
    print(f"| {p['Label']} | {p['Seconds']:.1f} | {p['Reads']:,} | {r['P50']:.2f} | {r['P95']:.2f} | {r['P99']:.2f} | {r['Max']:.1f} | {t['P50']:.2f} / {t['P95']:.2f} / {t['P99']:.2f} |")
