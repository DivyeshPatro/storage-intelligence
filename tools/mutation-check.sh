#!/usr/bin/env bash
# Deliberate-bug check for the accounting core. Each mutation is applied to a clean tree, the accounting tests are
# run (dotnet test rebuilds), the source is restored, and at the end the solution is rebuilt from clean and the full
# suite is run again, so no mutant binary can be left behind.
#
# Usage: tools/mutation-check.sh <log-dir>   (each mutation: "file @@ sed-expression @@ label")
# A mutation must be "killed" (at least one failing test). "survived" means a test gap, or an equivalent mutant
# that the log must explain.
set -euo pipefail
log_dir="${1:?usage: tools/mutation-check.sh <log-dir>}"
mkdir -p "$log_dir"
cd "$(git rev-parse --show-toplevel)"
if ! git diff --quiet || ! git diff --cached --quiet; then
  echo "working tree must be clean" >&2
  exit 2
fi

A=src/StorageIntelligence.Accounting
# Whatever happens (including an aborted run), never leave a mutation in the source tree.
trap 'git checkout -q -- "$A"' EXIT
mutations=(
  "$A/HardLinkReconciler.cs @@ s/1 - a.Count,/0,/ @@ distinct-file delta dropped"
  "$A/HardLinkReconciler.cs @@ s/bool shared = a.Count < totalLinks;/bool shared = a.Count <= totalLinks;/ @@ shared uses <="
  "$A/HardLinkReconciler.cs @@ s/bool verified = !shared \&\& linkCountMeasured;/bool verified = !shared;/ @@ unmeasured links claimed exclusive"
  "$A/HardLinkReconciler.cs @@ s/return candidate.AllocatedSize > current.AllocatedSize;/return candidate.AllocatedSize < current.AllocatedSize;/ @@ canonical picks smallest"
  "$A/HardLinkReconciler.cs @@ s/long totalLinks = Math.Max(group.Count, reportedLinks);/long totalLinks = group.Count;/ @@ measured link count ignored"
  "$A/HardLinkReconciler.cs @@ s/flags |= MeasurementCaveats.InconsistentLinkSizes;/flags |= MeasurementCaveats.None;/ @@ inconsistency not flagged"
  "$A/Primitives.cs @@ s/(effectiveFlags \& (UncertainReasons | AlwaysUncertain)) != 0/(effectiveFlags \& UncertainReasons) != 0/ @@ policy can trust inconsistent observations"
  "$A/Primitives.cs @@ s/!FileSystemSupportsHardLinks || (hasFileId \&\& measuredLinkCount == 1)/!FileSystemSupportsHardLinks || hasFileId/ @@ any file ID treated as single link"
  "$A/ScanAggregator.cs @@ s/parent.SubtreeDirectories += record.SubtreeDirectories + 1;/parent.SubtreeDirectories += record.SubtreeDirectories;/ @@ directory count off"
  "$A/ScanAggregator.cs @@ s/parent.ChildCoverage += subtreeCoverage;/parent.ChildCoverage += record.OwnCoverage;/ @@ coverage not rolled up"
  "$A/HardLinkRuns.cs @@ s/return c != 0 ? c : a.Directory.Value.CompareTo(b.Directory.Value);/return 0;/ @@ sort comparator broken"
  "$A/HardLinkRuns.cs @@ s/pos += WriteVarint(buffer.AsSpan(pos), highDelta == 0 ? r.FileId.Low - previous.Low : r.FileId.Low);/pos += WriteVarint(buffer.AsSpan(pos), r.FileId.Low);/ @@ ID delta encoding broken"
  "$A/HardLinkRuns.cs @@ s/if (available - worstCase < _guard.ReserveBytes)/if (false)/ @@ space guard disabled"
  "$A/Model.cs @@ s/SkippedReparseDirectories == 0;/true;/ @@ skipped reparse directories ignored by completeness"
)

summary="$log_dir/mutation-summary.txt"
: > "$summary"
for entry in "${mutations[@]}"; do
  # Fields are separated by ' @@ ' because sed expressions may contain '|'.
  file="${entry%% @@ *}"; rest="${entry#* @@ }"; expr="${rest%% @@ *}"; label="${rest#* @@ }"
  sed -i "$expr" "$file"
  if git diff --quiet; then
    echo "NOT APPLIED  $label" | tee -a "$summary"
    continue
  fi
  safe=$(echo "$label" | tr ' ' '-' | tr -cd '[:alnum:]-')
  if dotnet test tests/StorageIntelligence.Accounting.Tests > "$log_dir/mutation-$safe.log" 2>&1; then
    echo "SURVIVED     $label" | tee -a "$summary"
  else
    echo "killed       $label  ($(grep -Eo 'Failed: +[0-9]+' "$log_dir/mutation-$safe.log" | head -1))" | tee -a "$summary"
  fi
  git checkout -q -- "$file"
done

# Restore and prove it: clean rebuild from the committed source, unchanged tree, full suite green.
git diff --quiet || { echo "source not restored" >&2; exit 3; }
dotnet clean -v q > /dev/null
dotnet build > "$log_dir/final-build.log" 2>&1
dotnet test --no-build > "$log_dir/final-test.log" 2>&1 && final=passed || final=FAILED
echo "final clean rebuild at $(git rev-parse --short HEAD): tests $final" | tee -a "$summary"
grep -E "Passed!|Failed!|Skipped!" "$log_dir/final-test.log" | tee -a "$summary"
[ "$final" = passed ]
