param(
	[string]$WinUserH
)

# Load an parse
$file = Get-Content $WinUserH -Raw
$selection = $file | Select-String '(?smi)#ifndef\s+NOVIRTUALKEYCODES(.+)#endif\s*/\*\s*!NOVIRTUALKEYCODES\s*\*/' -AllMatches | ForEach-Object { $_.Matches.Groups[1].Value } | Out-String
$lines = $selection.Split([Environment]::NewLine) | Where-Object {$_.Trim().StartsWith('#')}

# generate content for `ParseVirtualKeyCode` 1
Write-Host
$lines | ForEach-Object { if ($_ -match "#define\s+VK_(\S+)\s+(\S+)\s*") { "`tif (str == L`"$($matches[1].ToLower())`") { return VK_$($matches[1]); }" } else { $_ } }
Write-Host

# generate content for `ParseVirtualKeyCode` 2
Write-Host
$lines | ForEach-Object { if ($_ -match "#define\s+VK_(\S+)\s+(\S+)\s*") { if ($matches[1].StartsWith("OEM_")) { `
	"if (static_cast<wchar_t>(MapVirtualKeyW(VK_$($matches[1]), MAPVK_VK_TO_CHAR)) == str[0]) { return VK_$($matches[1]); }" `
} } else { $_ } }
Write-Host

# generate content for `GetKeyWString` 1
Write-Host
$lines | ForEach-Object { if ($_ -match "#define\s+VK_(\S+)\s+(\S+)\s*") { if (-not ($matches[1].StartsWith("OEM_"))) { `
	"case VK_$($matches[1]): str = L`"$($matches[1].ToLower())`"; break;" `
} } else { $_ } }
Write-Host

# generate content for `GetKeyWString` 2
Write-Host
$lines | ForEach-Object { if ($_ -match "#define\s+VK_(\S+)\s+(\S+)\s*") { if ($matches[1].StartsWith("OEM_")) { `
	"case VK_$($matches[1]): str = L`"$($matches[1].ToLower())`"; break;" `
} } else { $_ } }
Write-Host
