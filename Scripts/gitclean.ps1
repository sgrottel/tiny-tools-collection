#
# Removes (almost) all untracked or ignored files for the specified git repository
#
param(
    [string]$path = (Get-Location).Path
)

try
{
Push-Location

$path = (Resolve-Path $path).Path
Write-Host "Cleaning $path"
Write-Host

cd $path
git clean -dx --dry-run -e desktop.ini

if ($LASTEXITCODE -ne 0)
{
    exit;
}

Write-Host
Write-Host "Waiting for Confirmation or Abort (Ctrl+C)"
Timeout 10

git clean -dxf -e desktop.ini

}
finally
{
Pop-Location
}
