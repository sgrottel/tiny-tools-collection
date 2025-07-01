#
# Script to crawl and publish summary
#
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-Location $PSScriptRoot

gh auth status 2>&1 | Out-Null
if ($LastExitCode -ne 0) {
    Write-Host "Not logged in gh"
    if (Test-Path 'ghtoken.txt' -PathType Leaf) {
        Write-Host "ghtoken.txt found"
        Get-Content 'ghtoken.txt' | gh auth login --with-token
    }
}

gh auth status 2>&1 | Out-Null
if ($LastExitCode -eq 0) {
    Write-Host "Collecting summary"
    $sum = ./Summary.ps1 -updateForks
    $sum | ConvertTo-Json -Depth 20 | Set-Content summary.json
    # $sum = Get-Content summary.json | ConvertFrom-Json
    Write-Host "Summary collected"
} else {
    Write-Error "Still not logged in gh" -ErrorAction Continue
    $sum = "ERROR: failed to login gh"
    $title = "Github Summary - $sum"
}

if (-not (($sum -is [string]) -and ($sum.StartsWith('ERROR:')))) {
    Write-Host "Formatting summary"
    $numTotalHotIssues = $sum | Select-Object -ExpandProperty hotIssuesCnt | Measure-Object -Sum | Select-Object -ExpandProperty Sum
    $numTotalPRs = $sum | Select-Object -ExpandProperty prCnt | Measure-Object -Sum | Select-Object -ExpandProperty Sum
    $numTotalCIFails = $sum | Where-Object { -not ($_.isFork -or $_.isArchived -or $_.isPrivate) } | Select-Object -ExpandProperty workflowsLastFailed | Measure-Object -Sum | Select-Object -ExpandProperty Sum
    $title = "Github Summary - $numTotalHotIssues !Issues, $numTotalPRs PRs, $numTotalCIFails CIFails";
    $sum = $sum | .\SummaryHtmlReport.ps1
} else {
    Write-Host "Formatting summary error"
    $sum = "<div style=`"background-color:black;padding:0.5em;color:crimson`"><pre><code>$sum</code></pre></div>";
}

if (Test-Path "publishconfig.json" -PathType Leaf) {
    $pubConf = Get-Content "publishconfig.json" | ConvertFrom-Json
    Write-Host "Publishing data"
    $resp = Invoke-WebRequest $pubConf.url -Method POST -Body (@{ title = $title; desc = $sum } | ConvertTo-Json) -ContentType "application/json" -Headers @{ Authorization = ("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($pubConf.user + ":" + $pubConf.pass)))}
    Write-Host (" > Resp: " + $resp)
} else {
    Write-Error "Publishing error: no config" -Erroraction Continue
}
