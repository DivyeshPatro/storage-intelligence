# M0b feasibility probe: which test volumes can be created on this runner, and which fixture features
# work on each. Writes capabilities.json and a Markdown summary. Unsupported cases are reported explicitly.
param([Parameter(Mandatory)] [string] $OutDir)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'TestVolumes.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'CiReport.psm1') -Force
trap { Write-CiError $_; exit 1 }
Assert-DisposableEnvironment
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$os = Get-CimInstance Win32_OperatingSystem
$result = [ordered]@{
    runner     = $env:ImageOS
    os         = "$($os.Caption) $($os.Version) build $($os.BuildNumber)"
    timestamp  = (Get-Date).ToUniversalTime().ToString('o')
    volumes    = @()
}


Add-Type -Namespace SiProbe -Name Native -MemberDefinition @'
[DllImport("wofutil.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
public static extern int WofIsExternalFile(string FilePath, out int IsExternalFile, out uint Provider, IntPtr ExternalFileInfo, ref uint BufferLength);
[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
public static extern uint GetCompressedFileSizeW(string lpFileName, out uint lpFileSizeHigh);
'@

# WOF hides its own reparse point from FSCTL queries, so fsutil cannot detect it; ask the WOF API instead.
function Get-WofState([string] $Path) {
    [int] $external = 0; [uint32] $provider = 0; [uint32] $len = 0
    $hr = [SiProbe.Native]::WofIsExternalFile($Path, [ref] $external, [ref] $provider, [IntPtr]::Zero, [ref] $len)
    [uint32] $high = 0
    $low = [SiProbe.Native]::GetCompressedFileSizeW($Path, [ref] $high)
    $compressed = ([uint64] $high -shl 32) -bor $low
    return "hr=0x{0:X8} external={1} provider={2} logical={3} compressedSize={4}" -f $hr, $external, $provider, (Get-Item $Path).Length, $compressed
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
        if ($fs -eq 'NTFS') {
            $entry.features += Test-Feature 'wof_filter_attach' {
                Invoke-Native fltmc.exe @('attach', 'wof', $vol.Root.Substring(0, 2))
            }
        }
        $entry.features += Test-Feature 'wof_compression' {
            $w = Join-Path $dir 'wof.txt'
            Set-Content -Path $w -Value ('windows overlay filter ' * 50000) -Encoding ascii
            Invoke-Native compact.exe @('/c', '/exe:xpress4k', $w) | Out-Null
            $state = Get-WofState $w
            if ($state -notmatch 'external=1') { throw "Not WOF-backed after compact: $state" }
            $state
        }
        $entry.features += Test-Feature 'filter_instances' {
            $letter = $vol.Root.Substring(0, 2)
            $filters = (& fltmc.exe instances -v $letter 2>&1 | Out-String)
            if ($filters -notmatch '(?im)^\s*wof\b') { throw "wof filter not attached: $filters" }
            'wof attached'
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

# WOF on the runner's own volumes, where the WOF filter is normally attached.
$extra = [ordered]@{ fileSystem = 'runner volumes'; created = $true; error = $null; clusterSize = $null; features = @() }
foreach ($base in @($env:RUNNER_TEMP, [IO.Path]::GetTempPath())) {
    $w = Join-Path $base ("si-wof-{0}.txt" -f [guid]::NewGuid().ToString('N').Substring(0, 8))
    $extra.features += Test-Feature "wof_compression ($([IO.Path]::GetPathRoot($w)))" {
        Set-Content -Path $w -Value ('windows overlay filter ' * 50000) -Encoding ascii
        try {
            Invoke-Native compact.exe @('/c', '/exe:xpress4k', $w) | Out-Null
            $state = Get-WofState $w
            if ($state -notmatch 'external=1') { throw "Not WOF-backed after compact: $state" }
            $state
        }
        finally { Remove-Item $w -Force -ErrorAction SilentlyContinue }
    }
}
$result.volumes += $extra

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
        if ($detail.Length -gt 400) { $detail = $detail.Substring(0, 400) + '...' }
        $md += "| $($v.fileSystem) | yes (cluster $($v.clusterSize)) | $($f.feature) | $($f.status) | $detail |"
    }
}
Set-Content -Path (Join-Path $OutDir 'capabilities.md') -Value $md -Encoding utf8
if ($env:GITHUB_STEP_SUMMARY) { Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $md }
$md | Write-Output
Write-CiAnnotation -Title "volume-capabilities $env:ImageOS" -Lines $md
exit 0
