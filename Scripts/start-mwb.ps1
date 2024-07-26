$ErrorActionPreference = "Stop"
Write-Host Starting Service MouseWithoutBordersSvc
Start-Service MouseWithoutBordersSvc
Sleep -Seconds 1
Write-Host Stopping Service MouseWithoutBordersSvc
Stop-Service MouseWithoutBordersSvc
Write-Host done.
