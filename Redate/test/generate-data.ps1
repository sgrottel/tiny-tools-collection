$dir = Join-Path $PSScriptRoot "data"
Write-Host "Test Dir:" $dir

if (Test-Path $dir) {
    Remove-Item $dir -Recurse -Force
}

New-Item -ItemType Directory -Path $dir | Out-Null
New-Item -ItemType Directory -Path (Join-Path $dir "a") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $dir "b") | Out-Null
New-Item -ItemType Directory -Path (Join-Path $dir "b\c") | Out-Null

Set-Content -Path (Join-Path $dir "1.txt") -Value "This is 1"
(Get-Item (Join-Path $dir "1.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T11:30:00Z")
Set-Content -Path (Join-Path $dir "2.txt") -Value "This is 2"
(Get-Item (Join-Path $dir "2.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T11:40:00Z")
Set-Content -Path (Join-Path $dir "a\3.txt") -Value "This is 3"
(Get-Item (Join-Path $dir "a\3.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T11:50:00Z")
Set-Content -Path (Join-Path $dir "b\4.txt") -Value "This is 4"
(Get-Item (Join-Path $dir "b\4.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T12:00:00Z")
Set-Content -Path (Join-Path $dir "b\c\5.txt") -Value "This is 5"
(Get-Item (Join-Path $dir "b\c\5.txt")).LastWriteTimeUtc = [DateTime]::Parse("2026-08-25T12:10:00Z")
