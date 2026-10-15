# Publishes result text as GitHub check-run annotations, readable via the REST API
# (repos/{owner}/{repo}/check-runs/{job_id}/annotations) when logs/artifacts are not downloadable.
Set-StrictMode -Version Latest

function Write-CiAnnotation {
    param([Parameter(Mandatory)] [string] $Title, [Parameter(Mandatory)] [AllowEmptyString()] [AllowEmptyCollection()] [string[]] $Lines)
    $chunk = New-Object System.Collections.Generic.List[string]
    $size = 0
    $part = 1
    foreach ($line in $Lines + @($null)) {
        if ($null -eq $line -or ($size + $line.Length) -gt 30000) {
            if ($chunk.Count -gt 0) {
                $msg = ($chunk -join "`n").Replace('%', '%25').Replace("`r", '%0D').Replace("`n", '%0A')
                # Straight to the console: Write-Output inside a function would become its return value.
                [Console]::Out.WriteLine("::notice title=$Title [$part]::$msg")
                $part++
            }
            $chunk.Clear(); $size = 0
            if ($null -eq $line) { break }
        }
        $chunk.Add($line); $size += $line.Length + 1
    }
}

# Reports an unhandled script error as an error annotation so failures are diagnosable via the API.
function Write-CiError([Parameter(Mandatory)] $ErrorRecord) {
    $text = ($ErrorRecord | Out-String) + "`n" + $ErrorRecord.ScriptStackTrace
    $msg = $text.Replace('%', '%25').Replace("`r", '%0D').Replace("`n", '%0A')
    [Console]::Out.WriteLine("::error title=script failure::$msg")
}

Export-ModuleMember -Function Write-CiAnnotation, Write-CiError
