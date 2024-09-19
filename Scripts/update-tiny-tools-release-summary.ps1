#
# Update Tiny Tools Release Summary
#
param (
    [string]$ReportDirectory,
    [string]$ReleaseTag = $null
)
$ErrorActionPreference = "Stop"

$index = [ordered]@{};
$indexJson = Join-Path $ReportDirectory "index.json"

if (Test-Path $indexJson -PathType Leaf)
{
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

$releaseInfo.Assets | ForEach-Object {
    if ($_.name -match "^(.+)-([\d\.]+)\.zip$") {
        $key = $matches[1]
        $versionStr = $matches[2]

        # Write-Host "$key => $versionStr"
        if (-not $index.ContainsKey($key)) {
            $index.Add($key, @{})
        }

        if (-not ($index[$key]["version"] -eq $versionStr)) {
            $index[$key].Add("version", $versionStr)
            $index[$key].Add("date", $null)
            $index[$key].Add("hash", $null)
        }

        if (-not ($index[$key]["date"] -or $index[$key]["hash"])) {
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
