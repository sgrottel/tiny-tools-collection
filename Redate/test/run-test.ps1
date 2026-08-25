param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$redateExe
)

.\generate-data.ps1

$dir = Join-Path $PSScriptRoot "data"

throw "Not implemented"
