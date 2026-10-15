# Runs the complete test suite from a clean Release build and publishes the outcome as annotations.
param([Parameter(Mandatory)] [string] $OutDir)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'CiReport.psm1') -Force
trap { Write-CiError $_; exit 1 }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

git clean -xdf -e TestResults | Out-Null
& dotnet build -c Release 2>&1 | Tee-Object -FilePath (Join-Path $OutDir 'build.log') | Out-Null
if ($LASTEXITCODE -ne 0) { Write-CiAnnotation -Title "build failed $env:ImageOS" -Lines (Get-Content (Join-Path $OutDir 'build.log') | Select-String ' error ' | ForEach-Object { $_.Line }); exit 1 }

$log = Join-Path $OutDir 'test.log'
& dotnet test -c Release --no-build --blame-hang-timeout 8m --logger "console;verbosity=normal" --logger "trx" --results-directory (Join-Path $OutDir 'trx') 2>&1 | Tee-Object -FilePath $log | Out-Null
$testExit = $LASTEXITCODE

$lines = @("## Test suite: $env:ImageOS (SI_PBT_ITER=$env:SI_PBT_ITER)", '')
$lines += Get-Content $log | Select-String -Pattern '(Passed!|Failed!|Skipped!) ' | ForEach-Object { '- ' + $_.Line.Trim() }
$hang = Get-Content $log | Select-String -Pattern '(hang|Blame|crash|aborted|The active test run was aborted)' | ForEach-Object { $_.Line.Trim() } | Select-Object -First 40
if ($hang) { $lines += '', '### Hang / abort diagnostics', ''; $lines += $hang }
$timing = Get-Content $log | Select-String -Pattern '^\s+Passed .* \[(\d+ m )?\d+ s\]' | ForEach-Object { '- ' + $_.Line.Trim() }
if ($timing) { $lines += '', '### Slow tests (>= 1 s)', ''; $lines += $timing }
$failures = Get-Content $log | Select-String -Pattern '^\s+Failed ' -Context 0, 12 | ForEach-Object { $_.Line.Trim(); $_.Context.PostContext | ForEach-Object { '    ' + $_.Trim() } }
if ($failures) { $lines += '', '### Failures', ''; $lines += $failures | Select-Object -First 200 }
$skipped = Get-Content $log | Select-String -Pattern '^\s+Skipped ' | ForEach-Object { '- ' + $_.Line.Trim() }
if ($skipped) { $lines += '', '### Skipped', ''; $lines += $skipped }
Write-CiAnnotation -Title "tests $env:ImageOS" -Lines $lines
exit $testExit
