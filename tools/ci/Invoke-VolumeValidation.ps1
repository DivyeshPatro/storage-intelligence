# M0b: on a disposable volume of the given filesystem, generate a fixture, validate the production scanner against
# the independent reference scanner, and measure allocation semantics. Results are written to $OutDir and published
# as annotations. A volume type that cannot be created is reported as NOT TESTED, never as passing.
param([Parameter(Mandatory)] [ValidateSet('NTFS', 'ReFS', 'FAT32', 'exFAT')] [string] $FileSystem,
      [Parameter(Mandatory)] [string] $OutDir,
      [Parameter(Mandatory)] [string] $Tool)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'TestVolumes.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'CiReport.psm1') -Force
trap { Write-CiError $_; exit 1 }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$title = "validation $FileSystem $env:ImageOS"

# Runs the tool with a watchdog. On timeout the process is killed and the tail of its output is published,
# so a hang is diagnosable instead of silently consuming the job.
function Invoke-Tool([string] $Name, [string[]] $Arguments, [int] $TimeoutSeconds = 300) {
    $out = Join-Path $OutDir "$Name.log"
    $err = Join-Path $OutDir "$Name.progress.log"
    $p = Start-Process -FilePath $Tool -ArgumentList $Arguments -NoNewWindow -PassThru -RedirectStandardOutput $out -RedirectStandardError $err
    if (-not $p.WaitForExit($TimeoutSeconds * 1000)) {
        $p.Kill($true)
        $tail = @("TIMEOUT after $TimeoutSeconds s in '$Name'", '', 'progress:') + (Get-Content $err -Tail 40) + @('', 'output:') + (Get-Content $out -Tail 20)
        Write-CiAnnotation -Title "TIMEOUT $title" -Lines $tail
        throw "$Name timed out"
    }
    return $p.ExitCode
}

try {
    $vol = New-TestVolume -FileSystem $FileSystem -Root (Join-Path $env:RUNNER_TEMP 'si-vhd') -SizeMB 4096
}
catch {
    Write-CiAnnotation -Title "NOT TESTED $title" -Lines @("Volume could not be created: $($_.Exception.Message)")
    exit 0
}

$exit = 0
try {
    $notes = @("Volume $($vol.Root) $($vol.FileSystem), cluster $($vol.ClusterSize), VHDX $($vol.Vhd)")
    if ($FileSystem -eq 'NTFS') {
        $attach = & fltmc.exe attach wof $vol.Root.Substring(0, 2) 2>&1 | Out-String
        $notes += "fltmc attach wof: $($attach.Trim())"
    }

    $manifest = Join-Path $OutDir 'manifest.json'
    # 'E:\.' rather than 'E:\': a trailing backslash before a closing quote breaks Windows argument parsing.
    $root = $vol.Root + '.'
    $code = Invoke-Tool 'fixture' @('fixture', '--root', $root, '--seed', '1', '--fs', $FileSystem, '--manifest', $manifest)
    if ($code -ne 0) { throw "fixture generation failed ($code): $(Get-Content (Join-Path $OutDir 'fixture.log') -Raw) $(Get-Content (Join-Path $OutDir 'fixture.progress.log') -Raw)" }
    $notes += Get-Content (Join-Path $OutDir 'fixture.log')

    $validateExit = Invoke-Tool 'validate' @('validate', '--root', $root, '--fs', $FileSystem, '--manifest', $manifest, '--out-dir', $OutDir)
    if ($validateExit -ne 0) { $exit = 1 }

    $allocationExit = Invoke-Tool 'allocation' @('allocation', '--root', $root, '--fs', $FileSystem, '--cluster', "$($vol.ClusterSize)", '--out', (Join-Path $OutDir 'allocation.json'))
    if ($allocationExit -ne 0) { $exit = 1 }

    & $Tool release-denials --manifest $manifest | Out-Null

    $lines = @("validate exit code: $validateExit, allocation exit code: $allocationExit", '') + $notes + @('') + (Get-Content (Join-Path $OutDir 'validate.log')) + @('') + (Get-Content (Join-Path $OutDir 'allocation.log'))
    Write-CiAnnotation -Title $title -Lines $lines
}
finally {
    Remove-TestVolume -Vhd $vol.Vhd
}
exit $exit
