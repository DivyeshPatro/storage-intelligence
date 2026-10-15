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
    & $Tool fixture --root $vol.Root --seed 1 --fs $FileSystem --manifest $manifest 2>&1 | Tee-Object -FilePath (Join-Path $OutDir 'fixture.log') | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "fixture generation failed: $(Get-Content (Join-Path $OutDir 'fixture.log') -Raw)" }
    $notes += Get-Content (Join-Path $OutDir 'fixture.log')

    & $Tool validate --root $vol.Root --fs $FileSystem --manifest $manifest --out-dir $OutDir 2>&1 | Tee-Object -FilePath (Join-Path $OutDir 'validate.log') | Out-Null
    $validateExit = $LASTEXITCODE
    if ($validateExit -ne 0) { $exit = 1 }

    & $Tool allocation --root $vol.Root --fs $FileSystem --cluster $vol.ClusterSize --out (Join-Path $OutDir 'allocation.json') 2>&1 | Tee-Object -FilePath (Join-Path $OutDir 'allocation.log') | Out-Null
    if ($LASTEXITCODE -ne 0) { $exit = 1 }

    & $Tool release-denials --manifest $manifest | Out-Null

    $lines = @("validate exit code: $validateExit", '') + $notes + @('') + (Get-Content (Join-Path $OutDir 'validate.log')) + @('') + (Get-Content (Join-Path $OutDir 'allocation.log'))
    Write-CiAnnotation -Title $title -Lines $lines
}
finally {
    Remove-TestVolume -Vhd $vol.Vhd
}
exit $exit
