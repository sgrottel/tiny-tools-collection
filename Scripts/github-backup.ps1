#
# GithubOverview.ps1
# sgrottel/tiny-tools-collection
#
# Requires:
#   Github.cli "gh" to be installed and authenticated
#
param([Parameter(Mandatory=$true)][string]$outDir)
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

if (-not (Test-Path $outDir -PathType Container))
{
	Write-Error("Output directory does not exist");
}

$myRepos = (gh repo list -L 10000 --json "url,name") | ConvertFrom-Json
foreach ($repo in $myRepos)
{
	Write-Host ">" $repo.name
}
