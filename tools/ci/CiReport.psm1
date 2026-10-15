# Publishes result text as GitHub check-run annotations, readable via the REST API
# (repos/{owner}/{repo}/check-runs/{job_id}/annotations) when logs/artifacts are not downloadable.
Set-StrictMode -Version Latest

function Write-CiAnnotation {
    param([Parameter(Mandatory)] [string] $Title, [Parameter(Mandatory)] [string[]] $Lines)
    $chunk = New-Object System.Collections.Generic.List[string]
    $size = 0
    $part = 1
    foreach ($line in $Lines + @($null)) {
        if ($null -eq $line -or ($size + $line.Length) -gt 3500) {
            if ($chunk.Count -gt 0) {
                $msg = ($chunk -join "`n").Replace('%', '%25').Replace("`r", '%0D').Replace("`n", '%0A')
                Write-Output "::notice title=$Title [$part]::$msg"
                $part++
            }
            $chunk.Clear(); $size = 0
            if ($null -eq $line) { break }
        }
        $chunk.Add($line); $size += $line.Length + 1
    }
}

Export-ModuleMember -Function Write-CiAnnotation
