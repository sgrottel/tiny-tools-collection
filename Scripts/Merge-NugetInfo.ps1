#
# $csprojs = Get-Content "C:\Downloads\all-csproj.txt"  -- paths to *.csproj files
# $csprojs | Select-Object -First 10 | Foreach-Object { C:\dev\tiny-tools\Scripts\Get-NugetInfo.ps1 $_ } | C:\dev\tiny-tools\Scripts\Merge-NugetInfo.ps1
# $csprojs | Foreach-Object { C:\dev\tiny-tools\Scripts\Get-NugetInfo.ps1 $_ } | C:\dev\tiny-tools\Scripts\Merge-NugetInfo.ps1 | ConvertTo-Yaml
# $csprojs | Foreach-Object { C:\dev\tiny-tools\Scripts\Get-NugetInfo.ps1 $_ } | C:\dev\tiny-tools\Scripts\Merge-NugetInfo.ps1 -AddNewestVersions | ConvertTo-Yaml -OutFile C:\Downloads\csproj-nuget.yaml -Force
#
param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [array]$NugetInfo,
    [switch]$OnlyMultiVersions,
    [switch]$AddNewestVersions
)
begin {
    $nugets = @{}
}
process {
    foreach ($e in $NugetInfo[0].GetEnumerator()) {
        $vers = ($nugets[$e.Key] = $nugets[$e.Key] ?? @{})
        foreach ($v in $e.Value.GetEnumerator()) {
            $vers[$v.Key] = ($vers[$v.Key] ?? @()) + @($v.Value)
        }
        $nugets[$e.Key] = $vers
    }
}
end {
    if ($AddNewestVersions) {
        foreach ($key in $nugets.Keys.Clone()) {
            $packageName = $key.ToLowerInvariant()
            $url = "https://api.nuget.org/v3-flatcontainer/$packageName/index.json"
            $response = Invoke-RestMethod -Uri $url
            $allVersions = $response.versions
            $latestVersion = $allVersions[-1]  # Last item is the latest
            $nugets[$key][$latestVersion] = ($nugets[$key][$latestVersion] ?? @()) + @("*newest*preview*")
            $latestStable = ($allVersions | Where-Object { $_ -notmatch "-" })[-1]
            $nugets[$key][$latestStable] = ($nugets[$key][$latestStable] ?? @()) + @("*newest*stable*")
        }
    }
    if ($OnlyMultiVersions) {
        foreach ($key in $nugets.Keys.Clone()) {
            if ($nugets[$key].Count -le 1) {
                $nugets.Remove($key)
            }
        }
    }
    $nugets    
}
