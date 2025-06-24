param(
    [string]$csproj
)
$ErrorActionPreference = 'SilentlyContinue'

[xml]$xml = Get-Content -Path $csproj
$packages = $xml.SelectNodes("/Project/ItemGroup/PackageReference[@Include and @Version]")

$map = @{}
foreach ($package in $packages) {
    $include = $package.Include
    $version = $package.Version
    $nuget = @{}
    $nuget[$version] = @($csproj)
    $map[$include] = $nuget
}
$map
