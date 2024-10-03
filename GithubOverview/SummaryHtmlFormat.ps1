param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [alias ("Target")]
    [array]$Targets 
)
begin {
    $txt = "<table>`n<tr>";
    $headers = @("name","issuesCnt","hotIssuesCnt","prCnt","isFork","isArchived","isPrivate","isForkBehind","updatedAt")
    foreach ($h in $headers) {
        $txt += "<th><small><small><small>$h</small></small></small></th>";
    }
    $txt += "</tr>`n";
}
process {
    $txt += "<tr>";
    $o = $Targets[0]
    for ($i = 0; $i -lt $headers.Length; $i++) {
        $s = ($o.($headers[$i])).ToString().Trim()
        $txt += "<td><small><small><small>$s</small></small></small></td>";
    }
    $txt += "</tr>`n";
}
end {
    $txt += "</table>`n"
    $txt
}

