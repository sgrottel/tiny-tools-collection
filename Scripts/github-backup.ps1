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

# fetch repositories names
$myRepos = (gh repo list -L 10000 --json "url,name") | ConvertFrom-Json

# Test target dirs
$hasError = $false;
foreach ($repo in $myRepos)
{
	$dir = Join-Path $outDir ($repo.name + ".git");
	if (Test-Path $dir)
	{
		Write-Error "Repository `"$($repo.name)`" checkout directory exists: $dir" -ErrorAction Continue
		$hasError = $true;
	}
}
if ($hasError)
{
	exit;
}

# Fetch repositories
$cwd = Get-Location
foreach ($repo in $myRepos)
{
	Write-Host "Cloning repository" $repo.name
	Set-Location $outDir
	Write-Host "git clone --mirror $($repo.url)" -ForegroundColor DarkGray -BackgroundColor Black
	git clone --mirror ($repo.url)
	if ($LASTEXITCODE -ne 0)
	{
		Set-Location $cwd
		Write-Error "Clone operation failed"
	}

	Set-Location (Join-Path $outDir ($repo.name + ".git"))
	Write-Host "git lfs fetch --all" -ForegroundColor DarkGray -BackgroundColor Black
	git lfs fetch --all
	if ($LASTEXITCODE -ne 0)
	{
		Set-Location $cwd
		Write-Error "Lfs fetch failed"
	}

	Write-Host "git lfs checkout" -ForegroundColor DarkGray -BackgroundColor Black
	git lfs checkout
	if ($LASTEXITCODE -ne 0)
	{
		Set-Location $cwd
		Write-Error "Lfs checkout failed"
	}

	Write-Host "git branch -r" -ForegroundColor DarkGray -BackgroundColor Black
	git branch -r
	if ($LASTEXITCODE -ne 0)
	{
		Set-Location $cwd
		Write-Error "branch report failed"
	}

	Write-Host "git tag" -ForegroundColor DarkGray -BackgroundColor Black
	git tag --list | Write-Host
	if ($LASTEXITCODE -ne 0)
	{
		Set-Location $cwd
		Write-Error "Tag report failed"
	}

	Write-Host "git lfs ls-files" -ForegroundColor DarkGray -BackgroundColor Black
	git lfs ls-files
	if ($LASTEXITCODE -ne 0)
	{
		Set-Location $cwd
		Write-Error "Lfs report failed"
	}

}
Set-Location $cwd
