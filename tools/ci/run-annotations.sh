#!/usr/bin/env bash
# Prints the notice annotations of every job in a workflow run (used where logs/artifacts are not reachable).
# Usage: tools/ci/run-annotations.sh <run-id> [title-filter]
set -euo pipefail
run="$1"; filter="${2:-}"
repo="$(git remote get-url origin | sed -E 's#^(https://github.com/|git@github.com:)##; s#\.git$##')"
for job in $(gh api "repos/$repo/actions/runs/$run/jobs" --jq '.jobs[].id'); do
  gh api "repos/$repo/check-runs/$job/annotations" --paginate \
    --jq ".[] | select(.annotation_level == \"notice\") | select(.title | contains(\"$filter\")) | \"### \(.title)\n\(.message)\n\""
done
