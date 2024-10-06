param (
    [parameter(Position=1, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [alias ("Target")]
    [array]$Targets 
)
begin {
    $icons = @();
    $urls = @();
    $names = @();
    $issues = @();
    $hotIssues = @();
    $pullrequests = @();
}
process {
    $o = $Targets[0];
    $urls += $o.url;
    $names += $o.name;

    $ico = "";
    if ($o.isPrivate) { $ico += "<span title=`"Private`">🔒</span>"; }
    if ($o.isArchived) { $ico += "<span title=`"Archived`">📦</span>"; }
    if ($o.isFork) {
        if ($o.isForkBehind) { $ico += "<span title=`"ForkBehind`">🔀</span>"; }
        else { $ico += "<span title=`"Fork`">↗️</span>"; }
    }
    $icons += $ico;

    $issues += $o.issuesCnt
    $hotIssues += $o.hotIssuesCnt
    $pullrequests += $o.prCnt
}
end {
    $iconPadLen = ($icons | ForEach-Object { ($_ | Select-String -Pattern "<span" -AllMatches).Matches.Count } | Measure-Object -Maximum).Maximum
    $namePadLen = ($names | ForEach-Object { $_.Length } | Measure-Object -Maximum).Maximum
    $iPadLen = ($issues | ForEach-Object { "$_".Length } | Measure-Object -Maximum).Maximum
    $hiPadLen = ($hotIssues | ForEach-Object { "$_".Length } | Measure-Object -Maximum).Maximum
    if ($hiPadLen -gt 0) { $hiPadLen += 2; }
    $prPadLen = ($pullrequests | ForEach-Object { "$_".Length } | Measure-Object -Maximum).Maximum

    $txt = "<small>`n";
    for ($i = 0; $i -lt $icons.Length; $i++) {
        $ico = $icons[$i];
        for ($icoLen = ($ico | Select-String -Pattern "<span" -AllMatches).Matches.Count; $icoLen -lt $iconPadLen; $icoLen++) {
            $ico += "<span>🔹</span>";
        }

        $name = $names[$i];
        $namePad = $namePadLen - $name.Length;
        if ($namePad -le 0) { $namePad = ""; }
        elseif ($namePad -eq 1) { $namePad = "&nbsp;"; }
        elseif ($namePad -gt 1) { $namePad = " " + "&nbsp;" * ($namePad - 1); }
        $url = $urls[$i];

        $iCnt = $issues[$i].ToString().PadLeft($iPadLen, '_') -replace '_',"&nbsp;"
        $hiCnt = $hotIssues[$i]
        if ($hiCnt -eq 0) { $hiCnt = ""; } else { $hiCnt = " !$hiCnt"; }
        $iCnt += $hiCnt.PadRight($hiPadLen, '_') -replace '_',"&nbsp;"
        if ($hotIssues[$i] -gt 0) { $iCnt = "<mark>$iCnt</mark>"; }

        $prCnt = $pullrequests[$i].ToString().PadLeft($prPadLen, '_') -replace '_',"&nbsp;"
        if ($prCnt -ne "0") { $prCnt = "<mark>$prCnt</mark>"; }

        $strong = ($hotIssues[$i] + $pullrequests[$i]) -gt 0;
        if ($strong) { $txt += "<strong>"; }
        $txt += "<code>$ico</code> <code><a href=`"$url`">$name</a>$namePad</code> &nbsp; <code>Issues:&nbsp;$iCnt</code> &nbsp; <code>PRs:&nbsp;$prCnt</code>";
        if ($strong) { $txt += "</strong>"; }

        $txt += "<br>`n";
    }
    $txt += "</small>"
    $txt
}

