# MakeIco.ps1
#
param(
	[Parameter(Mandatory)][string]$IcoFile,
	[Parameter(Mandatory)][string[]]$Files,
	[switch]$Force = $False,
	[Parameter(HelpMessage="Path to the IcoCat.exe tool")]
	[string]$IcoCat = $null
)

if ([string]::IsNullOrEmpty($IcoCat))
{
	$IcoCat = (Get-Command "IcoCat.exe" -ErrorAction SilentlyContinue).Source
}
if (-not (Test-Path $IcoCat))
{
	Write-Error "IcoCat.exe not available"
	exit
}

if (Test-Path $IcoFile)
{
	if (-not $Force)
	{
		Write-Error "Ico file $IcoFile already exists"
		exit
	}
	else
	{
		Remove-Item $IcoFile
	}
}

$IsWP = [System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($Files)
if ($IsWP) { 
    $Files = Get-ChildItem $Files | % { $_.FullName }
}

$files | foreach {
	$img = [System.Drawing.Image]::FromFile($_)
	$size = [Math]::max($img.Width, $img.Height)
	$img.Dispose()
	@{
		Size = $size
		file = $_
	}
} | Sort-Object -Property Size -Descending | foreach {
	$enc = if ($_.size -gt 64) { "PNG" } else { "Bitmap" }
	& $IcoCat -i $IcoFile -s $_.file -e $enc
}
