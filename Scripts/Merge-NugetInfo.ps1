#
# $csprojs = Get-Content "C:\Downloads\all-csproj.txt"  -- paths to *.csproj files
# $csprojs | Select-Object -First 10 | Foreach-Object { C:\dev\tiny-tools\Scripts\Get-NugetInfo.ps1 $_ } | C:\dev\tiny-tools\Scripts\Merge-NugetInfo.ps1
# $csprojs | Foreach-Object { C:\dev\tiny-tools\Scripts\Get-NugetInfo.ps1 $_ } | C:\dev\tiny-tools\Scripts\Merge-NugetInfo.ps1 | ConvertTo-Yaml
#
param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [array]$NugetInfo,
    [switch]$OnlyMultiVersions
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
    }
}
end {
    if ($OnlyMultiVersions) {
        foreach ($key in $nugets.Keys.Clone()) {
            if ($nugets[$key].Count -le 1) {
                $nugets.Remove($key)
            }
        }
    }
    $nugets    
}
