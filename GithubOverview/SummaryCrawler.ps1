#
# Script to crawl and publish summary
#
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-Location $PSScriptRoot

if (Test-Path log01.txt -PathType Leaf) {
    if ((Get-Item log01.txt).Length -gt (1024 * 1024)) {
        if (Test-Path log02.txt) {
            Remove-Item log02.txt
        }
        Move-Item log01.txt log02.txt
    }
}
Add-Content -Path log01.txt -Value ("Start - " + ([DateTime]::Now))

gh auth status 2>&1 | Out-Null
if ($LastExitCode -ne 0) {
    Add-Content -Path log01.txt -Value "Not logged in gh"
    if (Test-Path 'ghtoken.txt' -PathType Leaf) {
        Add-Content -Path log01.txt -Value "ghtoken.txt found"
        Get-Content 'ghtoken.txt' | gh auth login --with-token
    }
}

gh auth status 2>&1 | Out-Null
if ($LastExitCode -eq 0) {
    Add-Content -Path log01.txt -Value "Collecting summary"
    $sum = ./Summary.ps1 -scripting
    # $sum = Get-Content summary.json | ConvertFrom-Json
    Add-Content -Path log01.txt -Value "Summary collected"
} else {
    Add-Content -Path log01.txt -Value "Still not logged in gh"
    $sum = "ERROR: failed to login gh"
}

if (-not (($sum -is [string]) -and ($sum.StartsWith('ERROR:')))) {
    Add-Content -Path log01.txt -Value "Formatting summary"
    $sum = $sum | .\SummaryHtmlFormat.ps1
} else {
    Add-Content -Path log01.txt -Value "Formatting summary error"
    $sum = "<div style=`"background-color:black;padding:0.5em;color:crimson`"><pre><code>" + $sum + "</code></pre></div>";
}

if (Test-Path "publishconfig.json" -PathType Leaf) {
    $pubConf = Get-Content "publishconfig.json" | ConvertFrom-Json
    Add-Content -Path log01.txt -Value "Publishing data"
    $resp = Invoke-WebRequest $pubConf.url -Method POST -Body $sum -Headers @{ Authorization = ("Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($pubConf.user + ":" + $pubConf.pass)))}
    Add-Content -Path log01.txt -Value (" > Resp: " + $resp)
} else {
    Add-Content -Path log01.txt -Value "Publishing error: no config"
}

Add-Content -Path log01.txt -Value "done."
