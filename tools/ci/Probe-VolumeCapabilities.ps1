# M0b feasibility probe: which test volumes can be created on this runner, and which fixture features
# work on each. Writes capabilities.json and a Markdown summary. Unsupported cases are reported explicitly.
param([Parameter(Mandatory)] [string] $OutDir)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'TestVolumes.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'CiReport.psm1') -Force
Assert-DisposableEnvironment
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$os = Get-CimInstance Win32_OperatingSystem
$result = [ordered]@{
    runner     = $env:ImageOS
    os         = "$($os.Caption) $($os.Version) build $($os.BuildNumber)"
    timestamp  = (Get-Date).ToUniversalTime().ToString('o')
    volumes    = @()
}

function Test-Feature([string] $Name, [scriptblock] $Body) {
    try {
        $detail = & $Body
        return [ordered]@{ feature = $Name; status = 'supported'; detail = "$detail".Trim() }
    }
    catch {
        return [ordered]@{ feature = $Name; status = 'unsupported'; detail = "$($_.Exception.Message)".Trim() }
    }
}

function Invoke-Native([string] $Exe, [string[]] $Arguments) {
    $out = & $Exe @Arguments 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { throw "$Exe $($Arguments -join ' ') failed ($LASTEXITCODE): $out" }
    return $out
}

foreach ($fs in 'NTFS', 'ReFS', 'FAT32', 'exFAT') {
    $entry = [ordered]@{ fileSystem = $fs; created = $false; error = $null; clusterSize = $null; features = @() }
    $vol = $null
    try {
        $vol = New-TestVolume -FileSystem $fs -Root (Join-Path $env:RUNNER_TEMP 'si-vhd')
        $entry.created = $true
        $entry.clusterSize = $vol.ClusterSize
        $dir = Join-Path $vol.Root 'probe'
        New-Item -ItemType Directory -Path $dir | Out-Null
        $plain = Join-Path $dir 'plain.bin'
        [IO.File]::WriteAllBytes($plain, [byte[]]::new(100000))

        $entry.features += Test-Feature 'file_id' { Invoke-Native fsutil @('file', 'queryfileid', $plain) }
        $entry.features += Test-Feature 'hard_link' {
            Invoke-Native fsutil @('hardlink', 'create', (Join-Path $dir 'plain-link.bin'), $plain)
        }
        $entry.features += Test-Feature 'junction' { Invoke-Native cmd.exe @('/c', 'mklink', '/J', (Join-Path $dir 'junction'), $dir) }
        $entry.features += Test-Feature 'symlink_file' { Invoke-Native cmd.exe @('/c', 'mklink', (Join-Path $dir 'symlink.bin'), $plain) }
        $entry.features += Test-Feature 'sparse' {
            $sparse = Join-Path $dir 'sparse.bin'
            Invoke-Native fsutil @('file', 'createnew', $sparse, '67108864') | Out-Null
            Invoke-Native fsutil @('sparse', 'setflag', $sparse) | Out-Null
            Invoke-Native fsutil @('sparse', 'setrange', $sparse, '0', '67108864') | Out-Null
            Invoke-Native fsutil @('sparse', 'queryflag', $sparse)
        }
        $entry.features += Test-Feature 'ntfs_compression' {
            $c = Join-Path $dir 'compressed.txt'
            Set-Content -Path $c -Value ('storage intelligence ' * 50000) -Encoding ascii
            Invoke-Native compact.exe @('/c', $c) | Out-Null
            if (-not ((Get-Item $c).Attributes -band [IO.FileAttributes]::Compressed)) { throw 'Compressed attribute not set' }
            'Compressed attribute set'
        }
        $entry.features += Test-Feature 'wof_compression' {
            $w = Join-Path $dir 'wof.txt'
            Set-Content -Path $w -Value ('windows overlay filter ' * 50000) -Encoding ascii
            Invoke-Native compact.exe @('/c', '/exe:xpress4k', $w) | Out-Null
            $q = Invoke-Native fsutil @('reparsepoint', 'query', $w)
            if ($q -notmatch '0x80000017') { throw "WOF reparse tag not present: $q" }
            'IO_REPARSE_TAG_WOF present'
        }
        $entry.features += [ordered]@{ feature = 'cloud_placeholder'; status = 'not_probed'; detail = 'Requires a cloud sync provider; not available on hosted runners.' }
        $entry.features += [ordered]@{ feature = 'block_cloning'; status = 'not_probed'; detail = 'ReFS block cloning is treated as uncertain by design.' }
    }
    catch {
        $entry.error = "$($_.Exception.Message)".Trim()
    }
    finally {
        if ($vol) { Remove-TestVolume -Vhd $vol.Vhd }
    }
    $result.volumes += $entry
}

$json = $result | ConvertTo-Json -Depth 6
Set-Content -Path (Join-Path $OutDir 'capabilities.json') -Value $json -Encoding utf8

$md = @("## Test volume capabilities: $($result.os)", '', '| Filesystem | Created | Feature | Status | Detail |', '|---|---|---|---|---|')
foreach ($v in $result.volumes) {
    if (-not $v.created) {
        $md += "| $($v.fileSystem) | **no** | - | - | $($v.error -replace '\|', '/' -replace '\r?\n', ' ') |"
        continue
    }
    foreach ($f in $v.features) {
        $detail = ("$($f.detail)" -replace '\|', '/' -replace '\r?\n', ' ')
        if ($detail.Length -gt 120) { $detail = $detail.Substring(0, 120) + '...' }
        $md += "| $($v.fileSystem) | yes (cluster $($v.clusterSize)) | $($f.feature) | $($f.status) | $detail |"
    }
}
Set-Content -Path (Join-Path $OutDir 'capabilities.md') -Value $md -Encoding utf8
if ($env:GITHUB_STEP_SUMMARY) { Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $md }
$md | Write-Output
Write-CiAnnotation -Title "volume-capabilities $env:ImageOS" -Lines $md
