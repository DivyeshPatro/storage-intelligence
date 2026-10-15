#!/usr/bin/env python3
"""Rebuilds per-measurement JSON files from the 'raw bench' check-run annotations of an m0c-benchmarks run
(used because the session cannot download artifacts). Usage: extract-raw-annotations.py <annotations.md> <out-dir>"""
import json, os, re, sys

text = open(sys.argv[1], encoding='utf-8').read()
chunks = {}
for block in re.split(r'^### ', text, flags=re.M):
    title, _, body = block.partition('\n')
    m = re.match(r'raw bench (\d+) (\S+) \[(\d+)\]$', title.strip())
    if m:
        chunks.setdefault((m.group(1), m.group(2)), {})[int(m.group(3))] = body
for (files, image), parts in chunks.items():
    out = os.path.join(sys.argv[2], f'{image}-{files}')
    os.makedirs(out, exist_ok=True)
    for line in ''.join(parts[k] for k in sorted(parts)).splitlines():
        m = re.match(r'^([a-z0-9-]+): (\{.*\})$', line.strip())
        if m:
            with open(os.path.join(out, m.group(1) + '.json'), 'w', encoding='utf-8') as f:
                json.dump(json.loads(m.group(2)), f, indent=1)
    print(out, len(os.listdir(out)), 'files')
