# M0c benchmarks on a GitHub-hosted Windows runner. All results are CI (shared Azure VM, SSD-backed); none of them
# describe physical or HDD hardware. Cold-cache measurements detach and re-attach the fixture's VHDX volume, which
# empties that volume's cache without a reboot (operating-system files stay cached).
param(
    [Parameter(Mandatory)] [long] $Files,
    [Parameter(Mandatory)] [string] $OutDir,
    [int] $WarmRepeats = 2,
    [int] $CancelTrials = 8
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'TestVolumes.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'CiReport.psm1') -Force
trap { Write-CiError $_; exit 1 }
Assert-DisposableEnvironment
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$title = "bench $Files $env:ImageOS"

function Progress([string] $m) { [Console]::Error.WriteLine("[$((Get-Date).ToUniversalTime().ToString('HH:mm:ss'))] $m") }

# --- Environment record -------------------------------------------------------------------------------------------
$cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
$os = Get-CimInstance Win32_OperatingSystem
$disks = Get-PhysicalDisk | ForEach-Object { "$($_.FriendlyName) [$($_.MediaType), $($_.BusType), $([math]::Round($_.Size / 1GB)) GB]" }
$defender = try { (Get-MpComputerStatus).RealTimeProtectionEnabled } catch { 'unknown' }
$environment = [ordered]@{
    kind               = "CI: GitHub-hosted $env:ImageOS VM (Azure, shared, SSD-backed). Not physical hardware; not an HDD."
    os                 = "$($os.Caption) $($os.Version)"
    cpu                = "$($cpu.Name) ($($cpu.NumberOfCores) cores, $($cpu.NumberOfLogicalProcessors) logical)"
    memoryGB           = [math]::Round($os.TotalVisibleMemorySize / 1MB, 1)
    physicalDisks      = $disks
    defenderRealtime   = $defender
    fixtureFiles       = $Files
}
$environment | ConvertTo-Json | Set-Content (Join-Path $OutDir 'environment.json')
$env:SI_BENCH_KIND = $environment.kind

# --- Volume, application and fixture --------------------------------------------------------------------------------
$vhdRoot = if ((Get-PSDrive C).Free -gt (Get-PSDrive D -ErrorAction SilentlyContinue).Free) { 'C:\si-bench-vhd' } else { Join-Path $env:RUNNER_TEMP 'si-bench-vhd' }
$vol = New-TestVolume -FileSystem NTFS -Root $vhdRoot -SizeMB 40960
$exit = 0
try {
    $root = $vol.Root
    Progress "volume $root on $($vol.Vhd)"
    $app = Join-Path $root 'app'
    & dotnet publish (Join-Path $PSScriptRoot '..\StorageIntelligence.Bench') -c Release -r win-x64 --self-contained -p:PublishReadyToRun=true -o $app 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'publish failed' }
    $bench = Join-Path $app 'si-bench.exe'
    $temp = Join-Path $env:RUNNER_TEMP 'si-bench-temp'
    New-Item -ItemType Directory -Force -Path $temp | Out-Null

    function Bench([string[]] $Arguments, [string] $Name) {
        $out = Join-Path $OutDir "$Name.json"
        & $bench @Arguments --out $out 2>> (Join-Path $OutDir 'progress.log') | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "si-bench $($Arguments -join ' ') failed ($LASTEXITCODE)" }
        return $out
    }

    function Remount {
        Dismount-DiskImage -ImagePath $vol.Vhd | Out-Null
        Mount-DiskImage -ImagePath $vol.Vhd | Out-Null
        for ($i = 0; $i -lt 40; $i++) {
            $letter = (Get-DiskImage -ImagePath $vol.Vhd | Get-Disk | Get-Partition | Where-Object DriveLetter | Select-Object -First 1).DriveLetter
            if ($letter -and (Test-Path "$($letter):\bench")) { break }
            Start-Sleep -Milliseconds 250
        }
        if (-not $letter) { throw 'volume did not come back after remount' }
        $script:root = "$($letter):\"
        $script:app = Join-Path $script:root 'app'
        $script:bench = Join-Path $script:app 'si-bench.exe'
    }

    Progress "fixture: $Files files"
    $fixtureOut = Join-Path $OutDir 'fixture.json'
    & $bench fixture --root ($root + '.') --files $Files --out $fixtureOut 2>> (Join-Path $OutDir 'progress.log') | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'fixture failed' }
    & $bench probe-file --root ($root + '.') --size-mb 1024 | Out-Null

    # --- Startup: cold (volume just re-attached) then warm ------------------------------------------------------------
    Progress 'startup'
    Remount
    foreach ($i in 0..10) {
        $name = if ($i -eq 0) { 'startup-cold' } else { "startup-warm-$i" }
        $sw = [Diagnostics.Stopwatch]::StartNew()
        Bench @('startup') $name | Out-Null
        $sw.Stop()
        $j = Get-Content (Join-Path $OutDir "$name.json") | ConvertFrom-Json
        $j | Add-Member -NotePropertyName ExternalWallMs -NotePropertyValue $sw.Elapsed.TotalMilliseconds
        $j | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $OutDir "$name.json")
    }

    # --- Scans ---------------------------------------------------------------------------------------------------------
    foreach ($mode in 'low-impact', 'fast') {
        Progress "scan $mode cold"
        Remount
        Bench @('scan', '--root', (Join-Path $root 'bench'), '--mode', $mode, '--links', 'retained', '--cache', 'cold', '--temp', $temp) "scan-$mode-cold" | Out-Null
        foreach ($r in 1..$WarmRepeats) {
            Progress "scan $mode warm $r"
            Bench @('scan', '--root', (Join-Path $root 'bench'), '--mode', $mode, '--links', 'retained', '--cache', 'warm', '--temp', $temp) "scan-$mode-warm-$r" | Out-Null
        }
    }

    foreach ($links in 'none', 'all') {
        Progress "scan low-impact warm links=$links"
        Bench @('scan', '--root', (Join-Path $root 'bench'), '--mode', 'low-impact', '--links', $links, '--cache', 'warm', '--temp', $temp) "scan-low-impact-warm-links-$links" | Out-Null
    }

    # Two-CPU variant: the process starts with affinity restricted to logical CPUs 0-1 (an approximation of a dual-core
    # machine's CPU budget only; memory, disk and cache are still the runner's).
    $env:SI_BENCH_NOTES = 'process affinity restricted to 2 logical CPUs from start (cmd start /affinity 3)'
    foreach ($mode in 'low-impact', 'fast') {
        Progress "scan $mode cold, 2-CPU affinity"
        Remount
        $out = Join-Path $OutDir "scan-$mode-cold-2cpu.json"
        & cmd.exe /c start '""' /affinity 3 /wait $bench scan --root (Join-Path $root 'bench') --mode $mode --links retained --cache cold --temp $temp --out $out
        if (-not (Test-Path $out)) { throw "2-CPU scan $mode produced no result" }
    }
    $env:SI_BENCH_NOTES = ''

    # --- Cancellation --------------------------------------------------------------------------------------------------
    foreach ($mode in 'low-impact', 'fast') {
        Progress "cancel $mode"
        Bench @('cancel', '--root', (Join-Path $root 'bench'), '--mode', $mode, '--trials', "$CancelTrials", '--temp', $temp) "cancel-$mode" | Out-Null
    }

    # --- Foreground responsiveness: probe alone, then probe while a cold scan runs -------------------------------------
    Progress 'probe baseline'
    Remount
    Bench @('probe', '--file', (Join-Path $root 'probe.bin'), '--label', 'baseline (no scan)', '--seconds', '20') 'probe-baseline' | Out-Null
    foreach ($mode in 'low-impact', 'fast') {
        Progress "probe during $mode scan"
        Remount
        $scanOut = Join-Path $OutDir "scan-$mode-cold-with-probe.json"
        $scan = Start-Process -FilePath $bench -PassThru -NoNewWindow -RedirectStandardOutput (Join-Path $OutDir "scan-$mode-with-probe.stdout") `
            -ArgumentList @('scan', '--root', (Join-Path $root 'bench'), '--mode', $mode, '--links', 'retained', '--cache', 'cold', '--temp', $temp, '--out', $scanOut)
        Bench @('probe', '--file', (Join-Path $root 'probe.bin'), '--label', "during $mode scan (cold)", '--until-pid', "$($scan.Id)") "probe-during-$mode" | Out-Null
        $scan.WaitForExit()
    }

    # --- Summary -------------------------------------------------------------------------------------------------------
    # Raw metrics first (timelines removed), so no summarizer failure can lose the measurements.
    $raw = foreach ($f in Get-ChildItem $OutDir -Filter *.json | Sort-Object Name) {
        $j = Get-Content $f.FullName -Raw | ConvertFrom-Json
        if ($j.PSObject.Properties.Name -contains 'Timeline') { $j.PSObject.Properties.Remove('Timeline') }
        if ($j.PSObject.Properties.Name -contains 'Trials') { $j.PSObject.Properties.Remove('Trials') }
        if ($j.PSObject.Properties.Name -contains 'Environment') { $j.PSObject.Properties.Remove('Environment') }
        "$($f.BaseName): $($j | ConvertTo-Json -Depth 4 -Compress)"
    }
    Write-CiAnnotation -Title "raw $title" -Lines $raw

    $env:PYTHONIOENCODING = 'utf-8'
    $summary = & python (Join-Path $PSScriptRoot '..\bench\summarize-windows.py') $OutDir
    if ($LASTEXITCODE -ne 0) { throw "summarizer failed ($LASTEXITCODE)" }
    $summary | Set-Content (Join-Path $OutDir 'summary.md') -Encoding utf8
    Write-CiAnnotation -Title $title -Lines $summary
}
catch {
    Write-CiError $_
    $tail = @("FAILED: $($_.Exception.Message)", '') + (Get-Content (Join-Path $OutDir 'progress.log') -Tail 30 -ErrorAction SilentlyContinue)
    Write-CiAnnotation -Title "FAILED $title" -Lines $tail
    $exit = 1
}
finally {
    try { Dismount-DiskImage -ImagePath $vol.Vhd -ErrorAction SilentlyContinue | Out-Null } catch { }
    Remove-Item -LiteralPath $vol.Vhd -Force -ErrorAction SilentlyContinue
}
exit $exit
