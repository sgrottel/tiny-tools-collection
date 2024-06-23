#
# Creates a minimal project README.md
#
param(
    [string]$proj,
    [string]$outFile
)

$mainReadMe = Join-Path $PSScriptRoot "..\README.md"
$mainLicense = Join-Path $PSScriptRoot "..\LICENSE"

Write-Host "Loading $mainReadMe"
$lines = [string[]](get-content $mainReadMe)

$in = $false
$details = ''
$pushedEmpty = $true
foreach ($line in $lines)
{
    if ($line.StartsWith('##'))
    {
        $in = $line.SubString(2).Trim().StartsWith($proj)
    }
    elseif ($in)
    {
        if ($line -match '^\s*\[!\[[^\]]+\]\(.*github\.com/.*/actions/workflows') { continue; }
        if ($line -eq '')
        {
            if (-not $pushedEmpty)
            {
                $details += "`n";
            }
            $pushedEmpty = $true
        }
        else
        {
            $details += $line + "`n";
            $pushedEmpty = $false
        }
    }
}

$readme = "# $proj`nPart of SGrottel's Tiny Tools Collection`nhttps://github.com/sgrottel/tiny-tools-collection`n`n$details"

$projReadme = Join-Path $PSScriptRoot ".." $proj "README.md"
if (Test-Path $projReadme -PathType Leaf)
{
    Write-Host "Checking project README.md"
    $lines = [string[]](get-content $projReadme)
    $include = $false
    foreach ($line in $lines)
    {
        if ($line -match '^<!--\s*START\s+INCLUDE\s+IN\s+PACKAGE\s+README\s*-->\s*$')
        {
            $include = $true;
            continue;
        }
        if ($line -match '^<!--\s*STOP\s+INCLUDE\s+IN\s+PACKAGE\s+README\s*-->\s*$')
        {
            $include = $false;
            continue;
        }
        if ($include)
        {
            $readme += "$line`n"
        }
    }
}

$readme += "`n## MIT License`nThis project is freely available under the terms of the MIT license (see LICENSE file).`n`n"

Write-Host "Including License"
$lines = [string[]](get-content $mainLicense)
foreach ($line in $lines)
{
    $readme += '    ' + ($line.Trim()) + "`n"
}

Write-Host "Writing $outFile"
Set-Content -Value $readme -LiteralPath $outFile -Encoding Utf8
