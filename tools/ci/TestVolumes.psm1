# Disposable test volumes for CI. Creates VHDX-backed volumes under a temp root and removes them.
#
# SAFETY: this module partitions and formats disks. It refuses to run unless it is on a GitHub Actions
# runner AND SI_ALLOW_TEST_VOLUMES=1 is set, and it only ever touches disks backed by VHDX files it created.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-DisposableEnvironment {
    if ($env:GITHUB_ACTIONS -ne 'true' -or $env:SI_ALLOW_TEST_VOLUMES -ne '1') {
        throw 'Refusing to create or modify volumes: only allowed on a CI runner with SI_ALLOW_TEST_VOLUMES=1.'
    }
}

function Invoke-DiskPart([string[]] $Lines) {
    $script = Join-Path ([IO.Path]::GetTempPath()) ("si-diskpart-{0}.txt" -f [guid]::NewGuid().ToString('N'))
    Set-Content -Path $script -Value $Lines -Encoding ascii
    try {
        $output = & diskpart.exe /s $script 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) { throw "diskpart failed ($LASTEXITCODE): $output" }
        return $output
    }
    finally { Remove-Item $script -Force -ErrorAction SilentlyContinue }
}

function Get-VhdDisk([string] $VhdPath) {
    for ($i = 0; $i -lt 20; $i++) {
        $disk = Get-Disk | Where-Object { $_.Location -eq $VhdPath } | Select-Object -First 1
        if ($disk) { return $disk }
        Start-Sleep -Milliseconds 500
    }
    throw "Attached VHDX not found as a disk: $VhdPath"
}

<#
.SYNOPSIS Creates, attaches, partitions and formats a VHDX-backed test volume.
.OUTPUTS Object with Vhd, Root (e.g. 'T:\'), FileSystem, ClusterSize.
#>
function New-TestVolume {
    param(
        [Parameter(Mandatory)][ValidateSet('NTFS', 'ReFS', 'FAT32', 'exFAT')] [string] $FileSystem,
        [Parameter(Mandatory)] [string] $Root,
        [int] $SizeMB = 2048
    )
    Assert-DisposableEnvironment
    New-Item -ItemType Directory -Force -Path $Root | Out-Null
    $vhd = Join-Path (Resolve-Path $Root) ("si-{0}-{1}.vhdx" -f $FileSystem.ToLowerInvariant(), [guid]::NewGuid().ToString('N').Substring(0, 8))

    Invoke-DiskPart @(
        "create vdisk file=`"$vhd`" maximum=$SizeMB type=expandable",
        "select vdisk file=`"$vhd`"",
        'attach vdisk'
    ) | Out-Null

    try {
        $disk = Get-VhdDisk $vhd
        Initialize-Disk -Number $disk.Number -PartitionStyle GPT -ErrorAction Stop | Out-Null
        $partition = New-Partition -DiskNumber $disk.Number -UseMaximumSize -AssignDriveLetter -ErrorAction Stop
        $volume = Format-Volume -Partition $partition -FileSystem $FileSystem -NewFileSystemLabel ("SI" + $FileSystem) -Confirm:$false -Force -ErrorAction Stop
        # Marker checked by the validation tool's TestRootGuard before it writes anything to this volume.
        Set-Content -Path "$($partition.DriveLetter):\.si-disposable-volume" -Value "Disposable CI test volume backed by $vhd" -Encoding ascii
        return [pscustomobject]@{
            Vhd         = $vhd
            Root        = "$($partition.DriveLetter):\"
            FileSystem  = $volume.FileSystem
            ClusterSize = $volume.AllocationUnitSize
        }
    }
    catch {
        $err = $_
        Remove-TestVolume -Vhd $vhd
        throw $err
    }
}

function Remove-TestVolume {
    param([Parameter(Mandatory)] [string] $Vhd)
    Assert-DisposableEnvironment
    try { Invoke-DiskPart @("select vdisk file=`"$Vhd`"", 'detach vdisk') | Out-Null } catch { Write-Warning "detach failed: $_" }
    Remove-Item -LiteralPath $Vhd -Force -ErrorAction SilentlyContinue
}

Export-ModuleMember -Function New-TestVolume, Remove-TestVolume, Assert-DisposableEnvironment
