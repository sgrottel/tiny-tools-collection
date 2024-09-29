param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [alias ("Target")]
    [array]$Targets 
)
begin {
    $headers = @("name","issuesCnt","hotIssuesCnt","prCnt","isFork","isArchived","isPrivate","isForkBehind","updatedAt")
    $lengths = $headers | ForEach-Object { $_.Length }
    $rows = ,$headers
    $rows += ,@("====","=========","============","=====","======","==========","=========","============","=========")
    $cols = @("lightgreen", "lightgreen")
}
process {
    $vals = @()
    $col = "silver"
    $o = $Targets[0]
    for ($i = 0; $i -lt $headers.Length; $i++) {
        $s = ($o.($headers[$i])).ToString().Trim()
        $vals += ,$s
        if ($lengths[$i] -lt $s.Length) { $lengths[$i] = $s.Length }
    }
    if (($o.issuesCnt + $o.hotIssuesCnt + $o.prCnt) -eq 0) {
        $col = "gray"
    }
    if (($o.hotIssuesCnt + $o.prCnt) -gt 0) {
        $col = "fuchsia"
    }
    $rows += ,$vals
    $cols += ,$col
}
end {
    $txt = "<div style=`"background-color:black;padding:0.5em`"><pre><code>"
    $r = 0;
    for ($r = 0; $r -lt $rows.Length; $r++)
    {
        $c = $cols[$r]
        $txt += "<span style=`"color:$c`">"
        for ($i = 0; $i -lt $lengths.Length; $i++) {
            $s = $rows[$r][$i].ToString()
            if ($s -match "\d+") {
                $s = $s.PadLeft($lengths[$i])
            } else {
                $s = $s.PadRight($lengths[$i])
            }
            if ($i -gt 0) {
                $s = ' ' + $s
            }
            $txt += $s
        }
        $txt += "</span>`n"
    }
    $txt += "</code></pre></div>"
    $txt
}

