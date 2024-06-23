param(
	[Parameter(Mandatory=$true)][string]$version
	)

dotnet publish -c release /p:CopyOutputSymbolsToPublishDirectory=false

# write-host $version
rm ".\bin\Release\*.zip" -ErrorAction Ignore
rm ".\bin\Release\Redate" -recurse -force -ErrorAction Ignore
mkdir ".\bin\Release\Redate"
copy ".\bin\Release\net5.0\publish\*" ".\bin\Release\Redate" -recurse -force
Compress-Archive -Path ".\bin\Release\Redate" -DestinationPath (".\bin\Release\Redate-" + $version + ".zip") -force
