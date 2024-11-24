param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [alias ("Target")]
    [array]$Targets 
)
begin {
    $txt = "<table>`n<thead><tr>";
    $headers = @("name","issuesCnt","hotIssuesCnt","prCnt","isFork","isArchived","isPrivate","isForkBehind","updatedAt")
    foreach ($h in $headers) {
        $txt += "<th><small><small><small>$h</small></small></small></th>";
    }
    $txt += "</tr></thead>`n<tbody>`n";
}
process {
    $txt += "<tr>";
    $o = $Targets[0]
    $defaultTags = "<small><small><small>";
    if ($o.hotIssuesCnt + $o.prCnt -gt 0) {
        $defaultTags += "<strong>";
    }
    for ($i = 0; $i -lt $headers.Length; $i++) {
        $tags = $defaultTags;
        $s = ($o.($headers[$i])).ToString().Trim()
        if ((($headers[$i] -eq "hotIssuesCnt") -or ($headers[$i] -eq "prCnt")) -and ($s -ne "0")) {
            $tags += "<mark>";
        }
        $endTags = $tags -replace "<","</";
        if ($headers[$i] -eq "name") {
            $tags += "<a href=`"" + $o.url + "`">";
            $endTags = "</a>" + $endTags;
        }
        $txt += "<td>$tags$s$endTags</td>";
    }
    $txt += "</tr>`n";
}
end {
    $txt += "</tbody>`n</table>`n"
    $txt
}

