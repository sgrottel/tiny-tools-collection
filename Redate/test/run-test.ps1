param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$redateExe
)
Write-Host "Testing: $redateExe"

$ErrorActionPreference = 'Stop'

function Assert-FileDates() {
    param([Parameter(Mandatory)][string]$name)
    $dataDir = Join-Path $PSScriptRoot "data"

    if ((@(
        ((Get-Item (Join-Path $dataDir "1.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:30:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "2.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:40:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "a\3.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:50:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "b\4.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T12:00:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "b\c\5.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T12:10:00Z").ToUniversalTime())
        ) | ForEach-Object { [Math]::Abs($_.TotalSeconds) -lt 1 }) -contains $false) {
        throw "$name unexpected"
    }
    Write-Host "✅ $name ok"
}

function Assert-FileDates2() {
    param([Parameter(Mandatory)][string]$name)
    $dataDir = Join-Path $PSScriptRoot "data"

    if ((@(
        ((Get-Item (Join-Path $dataDir "1.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:30:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "a\3.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T13:06:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "b\4.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T13:07:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "b\c\5.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T13:05:00Z").ToUniversalTime())
        ) | ForEach-Object { [Math]::Abs($_.TotalSeconds) -lt 1 }) -contains $false) {
        throw "$name unexpected"
    }
    Write-Host "✅ $name ok"
}

function Assert-FileDates3() {
    param([Parameter(Mandatory)][string]$name)
    $dataDir = Join-Path $PSScriptRoot "data"

    if ((@(
        ((Get-Item (Join-Path $dataDir "1.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:30:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "a\3.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T13:06:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "b\4.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T13:07:00Z").ToUniversalTime()),
        ((Get-Item (Join-Path $dataDir "b\c\5.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T12:10:00Z").ToUniversalTime())
        ) | ForEach-Object { [Math]::Abs($_.TotalSeconds) -lt 1 }) -contains $false) {
        throw "$name unexpected"
    }
    Write-Host "✅ $name ok"
}

function Normalize-Json {
    param([Parameter(Mandatory)]$Value)

    switch ($Value) {
        # Arrays / lists
        { $_ -is [System.Collections.IEnumerable] -and -not ($_ -is [string]) } {
            # Keep array order, but normalize each element
            $normalized = @()
            foreach ($item in $Value) {
                $normalized += Normalize-Json $item
            }
            return ,$normalized
        }

        # Objects (PSCustomObject)
        { $_ -is [pscustomobject] } {
            $ordered = [ordered]@{}
            # Get properties, sort by name to ignore original order
            $props = $Value.PSObject.Properties |
                     Where-Object { $_.MemberType -eq 'NoteProperty' } |
                     Sort-Object Name

            foreach ($p in $props) {
                $ordered[$p.Name] = Normalize-Json $p.Value
            }
            return $ordered
        }

        # Hashtables (if any)
        { $_ -is [hashtable] } {
            $ordered = [ordered]@{}
            foreach ($key in ($Value.Keys | Sort-Object)) {
                $ordered[$key] = Normalize-Json $Value[$key]
            }
            return $ordered
        }

        # Scalars (string, int, bool, null, etc.)
        default {
            return $Value
        }
    }
}

function Compare-JsonFiles {
    param(
        [Parameter(Mandatory)]$fileA,
        [Parameter(Mandatory)]$fileB
    )

    $leftRaw  = Get-Content -Path $fileA -Raw
    $rightRaw = Get-Content -Path $fileB -Raw

    $leftObj  = $leftRaw  | ConvertFrom-Json -Depth 100
    $rightObj = $rightRaw | ConvertFrom-Json -Depth 100

    $leftObj.FileDate = "ignore";
    $rightObj.FileDate = "ignore";

    $leftNorm  = Normalize-Json $leftObj
    $rightNorm = Normalize-Json $rightObj

    $leftCanon  = $leftNorm  | ConvertTo-Json -Depth 100 -Compress
    $rightCanon = $rightNorm | ConvertTo-Json -Depth 100 -Compress

    if ($leftCanon -eq $rightCanon) {
        Write-Host "✅ JSON files contain same data."
    } else {
        Write-Host "Gen:`n $leftRaw"
        throw "JSON files differ."
    }
}

& (Join-Path $PSScriptRoot "generate-data.ps1")
$dataDir = Join-Path $PSScriptRoot "data"
Assert-FileDates "Initial file time setup"

$genRedate = (Join-Path $PSScriptRoot "gen.redate")
if (Test-Path $genRedate) {
    Remove-Item $genRedate -Force
    if (Test-Path $genRedate) {
        throw "Failed to remove $genRedate"
    }
}

& $redateExe init $genRedate $dataDir

if (-not (Test-Path $genRedate)) {
    throw "Failed to init $genRedate"
}
Assert-FileDates "File time stamps unchanged after init"

Compare-JsonFiles $genRedate (Join-Path $PSScriptRoot "reference1.redate")

Remove-Item (Join-Path $dataDir "2.txt") -Force
(Get-Item (Join-Path $dir "b\c\5.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T13:05:00Z").ToUniversalTime()
Set-Content -Path (Join-Path $dir "a\3.txt") -Value "Now new 3"
(Get-Item (Join-Path $dir "a\3.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T13:06:00Z").ToUniversalTime()
Set-Content -Path (Join-Path $dir "b\4.txt") -Value "This is 4 edited"
(Get-Item (Join-Path $dir "b\4.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T13:07:00Z").ToUniversalTime()

Assert-FileDates2 "Edited File's time stamps"

& $redateExe run $genRedate

Assert-FileDates3 "Edited & restored File's time stamps"

Compare-JsonFiles $genRedate (Join-Path $PSScriptRoot "reference2.redate")

Write-Host "✅ done."
