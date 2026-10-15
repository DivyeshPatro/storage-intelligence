#!/usr/bin/env python3
"""Summarizes si-bench `core` results: median and range per (store, entries). Usage: summarize-core.py <dir>"""
import glob, json, statistics, sys

MB = 1024 * 1024
rows = {}
for f in glob.glob(sys.argv[1] + '/core-*.json'):
    d = json.load(open(f))
    rows.setdefault((d['Store'], d['Entries']), []).append(d)

print('| Store | Entries | Dirs | Runs | Time s (median) | Entries/s (median) | Baseline WS MB | Peak WS MB (min–max) | Peak WS − baseline MB | Peak managed heap MB | Run files MB | Peak open dirs |')
print('|---|---|---|---|---|---|---|---|---|---|---|---|')
for key in sorted(rows):
    r = rows[key]
    med = lambda k: statistics.median([x[k] for x in r])
    print(f"| {key[0]} | {key[1]:,} | {r[0]['Directories']:,} | {len(r)} | {med('Seconds'):.2f} | {med('EntriesPerSecond'):,.0f} | "
          f"{med('BaselineWorkingSet')/MB:.1f} | {min(x['PeakWorkingSet'] for x in r)/MB:.1f}–{max(x['PeakWorkingSet'] for x in r)/MB:.1f} | "
          f"{statistics.median([x['PeakWorkingSet']-x['BaselineWorkingSet'] for x in r])/MB:.1f} | {med('PeakManagedHeap')/MB:.1f} | "
          f"{r[0]['RunBytes']/MB:.1f} | {r[0]['PeakOpenDirectories']} |")
print()
print('Environment:', json.dumps(next(iter(rows.values()))[0]['Environment']))
