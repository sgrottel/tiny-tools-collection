#
# AllGitsInfo
# by SGrottel (www.sgrottel.de)
# as Open Source under the MIT License
#
# Little powershell script to summarize setup of all git repositories on the machine.
#
# Examples:
#   .\AllGitsInfo.ps1 | Format-Table
#   .\AllGitsInfo.ps1 | Export-Csv allGits.csv
#

function getConfigValue([string]$name, [string]$scope)
{
	$v = (& git config "$scope" --get "$name")
	return $v
}

function getLocalConfigValue([string]$name)
{
	$v = (& git config --show-scope -z --get "$name") -Split "`0"
	if ($v[0])
	{
		$scope = $v[0].trim().toLower()
		if ($scope -eq 'global' -or $scope -eq 'system')
		{
			return ' <'+$scope[0]+'> '
		}
	}

	return ($v[1])
}

# The everything search command line client
# Borrowed from another one of my tools (https://go.grottel.net/checkouts-overview)
$es = "C:\tools\Checkouts-Overview\es.exe"; 

$repos = (& $es -r ^.git$ -ww /ad)

$table = New-Object System.Data.Datatable

[void]$table.Columns.Add('scope')
[void]$table.Columns.Add('user.name')
[void]$table.Columns.Add('user.email')
[void]$table.Columns.Add('user.signingkey')
[void]$table.Columns.Add('commit.gpgsign')
[void]$table.Columns.Add('remote.origin.url')

[void]$table.Rows.Add(
	'system',
	(getConfigValue 'user.name' '--system'),
	(getConfigValue 'user.email' '--system'),
	(getConfigValue 'user.signingkey' '--system'),
	(getConfigValue 'commit.gpgsign' '--system'),
	(getConfigValue 'remote.origin.url' '--system')
)

[void]$table.Rows.Add(
	'global',
	(getConfigValue 'user.name' '--global'),
	(getConfigValue 'user.email' '--global'),
	(getConfigValue 'user.signingkey' '--global'),
	(getConfigValue 'commit.gpgsign' '--global'),
	(getConfigValue 'remote.origin.url' '--global')
)

$repos | foreach {
	$p = Split-Path $_ -parent
	push-location $p
	[void]$table.Rows.Add(
		$p,
		(getLocalConfigValue 'user.name'),
		(getLocalConfigValue 'user.email'),
		(getLocalConfigValue 'user.signingkey'),
		(getLocalConfigValue 'commit.gpgsign'),
		(getLocalConfigValue 'remote.origin.url')
	)
	pop-location
}

return $table
