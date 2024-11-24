param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [alias ("Target")]
    [array]$Targets 
)
begin {
    $myarray = @()
}
process {
    $myarray += ,$Targets
}
end {
        
    $format = @{Name="name";Expression={$esc=[char]27;(($_.hotIssuesCnt+$_.prCnt -gt 0) ? "$esc[45m" : "") + $_.name}},`
    @{Name="issuesCnt";Expression={$esc=[char]27;(($_.issuesCnt -eq 0) ? "$esc[0;90m" : "$esc[36m") + $_.issuesCnt};Alignment="Right"},`
    @{Name="hotIssues";Expression={$esc=[char]27;(($_.hotIssuesCnt -gt 0) ? "$esc[45m" : "") + $_.hotIssuesCnt};Alignment="Right"},`
    @{Name="pullRequests";Expression={$esc=[char]27;(($_.prCnt -gt 0) ? "$esc[45m" : "") + $_.prCnt};Alignment="Right"},`
    "isArchived","isPrivate","isFork","isForkBehind","updatedAt";
    $myarray | Format-Table $format
}

