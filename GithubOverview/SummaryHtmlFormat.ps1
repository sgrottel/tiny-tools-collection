param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [alias ("Target")]
    [array]$Targets 
)
begin {
    $txt = "<table>`n<tr>";
    $headers = @("name","issuesCnt","hotIssuesCnt","prCnt","isFork","isArchived","isPrivate","isForkBehind","updatedAt")
    foreach ($h in $headers) {
        $txt += "<th><code>$h</code></th>";
    }
    $txt += "</tr>`n";
}
process {
    $txt += "<tr>";
    $o = $Targets[0]
    for ($i = 0; $i -lt $headers.Length; $i++) {
        $s = ($o.($headers[$i])).ToString().Trim()
        $txt += "<td><code>$s</code></td>";
    }
    $txt += "</tr>`n";
}
end {
    $txt += "</table>`n"
    $txt
}

