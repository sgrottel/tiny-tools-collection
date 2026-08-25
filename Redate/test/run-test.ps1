param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$redateExe
)

$ErrorActionPreference = 'Stop'

.\generate-data.ps1

$dir = Join-Path $PSScriptRoot "data"

if ((@(
    ((Get-Item (Join-Path $dir "1.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:30:00Z").ToUniversalTime()),
    ((Get-Item (Join-Path $dir "2.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:40:00Z").ToUniversalTime()),
    ((Get-Item (Join-Path $dir "a\3.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T11:50:00Z").ToUniversalTime()),
    ((Get-Item (Join-Path $dir "b\4.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T12:00:00Z").ToUniversalTime()),
    ((Get-Item (Join-Path $dir "b\c\5.txt")).LastWriteTimeUtc.ToUniversalTime() - [DateTime]::Parse("2026-08-25T12:10:00Z").ToUniversalTime())
    ) | ForEach-Object { [Math]::Abs($_.TotalSeconds) -lt 1 }) -contains $false) {
    throw "Initial file time setup unexpected"
}
Write-Host "✅ Initial file time setup ok"


throw "Not implemented"
