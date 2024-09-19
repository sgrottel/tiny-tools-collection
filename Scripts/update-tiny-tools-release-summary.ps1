#
# Update Tiny Tools Release Summary
#
param (
    [string]$ReportDirectory,
    [string]$ReleaseTag = $null
)
$ErrorActionPreference = "Stop"


#
# Update 'index.json' based on latest/selected release assets
#
$index = [ordered]@{};
$indexJson = Join-Path $ReportDirectory "index.json"
if (Test-Path $indexJson -PathType Leaf)
{
    Write-Host "Loading 'index.json'..."
    $index = Get-Content $indexJson | ConvertFrom-Json -AsHashtable
}

$githubApiHeaders = @{
    "Accept"="application/vnd.github+json"
};

if ($ReleaseTag)
{
    Write-Host "Load release '$ReleaseTag'"
    $releaseInfo = Invoke-WebRequest "https://api.github.com/repos/sgrottel/tiny-tools-collection/releases/tags/$ReleaseTag" -Headers $githubApiHeaders
}
else
{
    Write-Host "Load latest release"
    $releaseInfo = Invoke-WebRequest "https://api.github.com/repos/sgrottel/tiny-tools-collection/releases/latest" -Headers $githubApiHeaders
}
if ($releaseInfo.StatusCode -ne 200)
{
    throw "Failed to load release information from Github api"
}
$releaseInfo = $releaseInfo.Content | ConvertFrom-Json


#
# For all assets, check if the releases are known
#
$releaseInfo.Assets | ForEach-Object {
    if ($_.name -match "^(.+)-([\d\.]+)\.zip$") {
        $key = $matches[1]
        $versionStr = $matches[2]

        # Write-Host "$key => $versionStr"
        if (-not $index.ContainsKey($key)) {
            $index.Add($key, @{})
        }

        if (-not ($index[$key]["version"] -eq $versionStr)) {
            Write-Host "New release: $key $versionStr"
            $index[$key]["version"] = $versionStr;
            $index[$key]["date"] = $null;
            $index[$key]["hash"] = $null;
        }

        if (-not ($index[$key]["date"] -and $index[$key]["hash"])) {
            Write-Host "Searching Release date for $key ($versionStr)..."
            # Search for release date
            # Check build actions
            $pp = 20
            $maxP = 1
            $artifactHash = $null
            $artifactDate = $null
            for ($p = 1; $p -le $maxP; $p++) {
                Write-Host "Searching runs[$p]..."
                $runs = Invoke-WebRequest "https://api.github.com/repos/sgrottel/tiny-tools-collection/actions/runs?per_page=$pp&page=$p" -Headers $githubApiHeaders
                $runs = $runs.Content | ConvertFrom-Json
                if ($maxP -eq 1) {
                    $maxP = [Math]::Ceiling([int]($runs.total_count) / $pp);
                }
                $runs.workflow_runs | ForEach-Object {
                    if ($artifactDate) { return; } # alread found
                    if (-not ($_.status -eq "completed" -and $_.conclusion -eq "success")) { return; }
                    $hash = $_.head_sha;
                    $artifacts_url = $_.artifacts_url
                    $artifacts = Invoke-WebRequest $artifacts_url -Headers $githubApiHeaders
                    $artifacts = $artifacts.Content | ConvertFrom-Json
                    if ($artifacts.total_count -lt 1) { return; }
                    $artifacts.artifacts | ForEach-Object {
                        if ($artifactDate) { return; } # alread found
                        if (-not ($_.name -match "(.+)-([\d\.]+)")) { return; }
                        if ($matches[1] -ne $key -or $matches[2] -ne $versionStr) { return; }
                        Write-host "Release found $key $versionStr"
                        $artifactDate = $_.updated_at
                    }
                    if ($artifactDate) {
                        $artifactHash = $hash;
                    }
                }
                if ($artifactDate) { break; } # alread found
            }

            if ($artifactDate) {
                $index[$key]["date"] = $artifactDate;
            }
            if ($artifactHash) {
                $index[$key]["hash"] = $artifactHash;
            }
        }
    }
}

$index | ConvertTo-Json -Depth 100 | Set-Content $indexJson


#
# Build / update badges
#
function FetchOrUpdateBadge
{
    param(
        [string]$filename,
        [string]$label,
        [string]$value
    )
    $file = Join-Path $ReportDirectory $filename
    if (Test-Path $file -PathType Leaf) {
        $c = [string](Get-Content $file -Raw)
        if ($c.Contains("${label}: $value")) {
            # Badge is up to date
            return
        }
    }

    Write-Host "Creating Badge $filename := $value"

    $escapedLabel = $label -replace ' ', '_'
    $escapedValue = $value -replace '-', '--'
    $h = Get-Random -Minimum 90 -Maximum 331 # no red-ish
    $s = Get-Random -Minimum 60 -Maximum 81
    $l = Get-Random -Minimum 30 -Maximum 71
    $color = [System.Uri]::EscapeUriString("hsl($h,$s%,$l%)")
    $url = "https://img.shields.io/badge/$escapedLabel-$escapedValue-$color"
    Invoke-WebRequest $url -OutFile $file
}

foreach($key in $index.Keys) {
    $ver = $index[$key]["version"]
    $date = ([DateTime]$index[$key]["date"]).ToString("yyyy-MM-dd")

    FetchOrUpdateBadge "$key-ver.svg" "Latest Release" ("v" + ([char]0x200A) + "$ver")
    FetchOrUpdateBadge "$key-date.svg" "Release Date" $date
}


#
# Build / update overview page
#
$urlPath = "https://raw.githubusercontent.com/wiki/sgrottel/tiny-tools-collection/releases/"
$txt = @'
# Tools Releases
This page shows an overview over all tools wiithin the Tiny Tools Collection, their latest release version, date of the latest release, and the git commit hash.

All assets are available within the [latest release](https://github.com/sgrottel/tiny-tools-collection/releases/latest).

The following table is sorted by release date, newest to oldest release.

| Name | Last Release | Git Hash |
| --- | --- | --- |

'@
foreach($key in $index.Keys | Sort-Object { $index[$_]["date"] } -Descending) {
    $hash = $index[$key]["hash"]
    $ver = $index[$key]["version"]
    $date = ([DateTime]$index[$key]["date"]).ToString("yyyy-MM-dd")
    $txt += "| $key | ![v$ver]($urlPath$key-ver.svg) ![$date]($urlPath$key-date.svg) | " + `
    "[$hash](https://github.com/sgrottel/tiny-tools-collection/commit/$hash) |`n"
}
Set-Content (Join-Path $ReportDirectory "Tools Releases.md") $txt
