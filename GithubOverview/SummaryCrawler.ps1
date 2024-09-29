#
# Script to crawl and publish summary
#
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-Location $PSScriptRoot

gh auth status 2>&1 | Out-Null
if ($LastExitCode -ne 0) {
    if (Test-Path 'ghtoken.txt' -PathType Leaf) {
        Get-Content 'ghtoken.txt' | gh auth login --with-token
    }
}

gh auth status 2>&1 | Out-Null
if ($LastExitCode -eq 0) {
    $sum = ./Summary.ps1 -scripting
} else {
    $sum = "ERROR: failed to login gh"
}

if (-not (($sum -is [string]) -and ($sum.StartsWith('ERROR:')))) {
    $sum = $sum | .\SummaryHtmlFormat.ps1
}

# TODO: Publish
$sum
