#
# DiffYaml.ps1
# Prints or formats YAML text based on comparing two YAML
#
# GithubOverview.ps1
# sgrottel/tiny-tools-collection
#
# Usage example:
#   DiffYaml.ps1 (gc .\firstfile.yaml) (gc .\secondfile.yaml)
#
# This is purely text based logic, following the assumption that both input
# texts are equally formatted yaml.
#
param(
    [Parameter(Position=0, mandatory=$true)][array] $firstLines,
    [Parameter(Position=1, mandatory=$true)][array] $secondLines,
    [switch]$html
)

$alen = $firstLines.length;
$blen = $secondLines.length;

$aInd = ($firstLines | ForEach-Object { ($_ -match '^([\s]*)') ? (($Matches.1).length) : 0 });
$bInd = ($secondLines | ForEach-Object { ($_ -match '^([\s]*)') ? (($Matches.1).length) : 0 });

$aMap = ($firstLines | ForEach-Object { -1 });
$bMap = ($secondLines | ForEach-Object { -1 });

$inds = ($aInd + $bInd) | Sort-Object -Unique;
foreach ($ind in $inds) {
    $mapFailed = @();
    # First, try to find perfect matches
    for ($i = 0; $i -lt $alen; $i++) {
        if ($aInd[$i] -ne $ind)  { continue; }
        if ($aMap[$i] -ge 0) { continue; }
        if ($firstLines[$i].empty) { continue; }
        $r = @();
        for ($j = 0; $j -lt $blen; $j++) {
            if ($bInd[$j] -ne $ind) { continue; }
            if ($bMap[$j] -ge 0) { continue; }
            if ($secondLines[$j].empty) { continue; }
            if ($firstLines[$i] -eq $secondLines[$j]) {
                $r += $j;
            }
        }
        if ($r.length -eq 1) {
            # Double check that candidate is unique in both directions
            $cnt = 0;
            for ($k = 0; $k -lt $alen; $k++) {
                if ($aInd[$k] -ne $ind)  { continue; }
                if ($aMap[$k] -ge 0) { continue; }
                if ($firstLines[$k].empty) { continue; }
                if ($firstLines[$k] -eq $secondLines[$r[0]]) {
                    $cnt++;
                }
            }
            if ($cnt -eq 1) {
                $aMap[$i] = $r[0];
                $bMap[$r[0]] = $i;
            } else {
                $mapFailed += $i;
            }
        } else {
            $mapFailed += $i;
        }
    }
    if ($mapFailed.length -gt 0) {
        # If some mapping fail, try mapping in context
        foreach ($i in $mapFailed) {
            $ai = $i;
            for (; ($aMap[$ai] -lt 0) -and ($ai -ge 0); $ai--) {}
            if ($ai -lt 0) { $a = 0; } else { $ai =  $aMap[$ai]; }
            $bi = $i;
            for (; ($aMap[$bi] -lt 0) -and ($bi -lt $alen); $bi++) {}
            if ($bi -lt $alen) { $bi = $aMap[$bi]; } else { $bi = $blen; }

            $r = @();
            for ($j = $ai; $j -le $bi; $j++) {
                if ($bInd[$j] -ne $ind) { continue; }
                if ($bMap[$j] -ge 0) { continue; }
                if ($secondLines[$j].empty) { continue; }
                if ($firstLines[$i] -eq $secondLines[$j]) {
                    $r += $j;
                }
            }
            if ($r.length -eq 1) {
                $aMap[$i] = $r[0];
                $bMap[$r[0]] = $i;
            }
        }
    }
}

$lines = @();
$j = 0;
for ($i = 0; $i -lt $blen; $i++) {
    $m = $bMap[$i];
    for (; $j -lt $m; $j++) {
        $lines += [PSCustomObject]@{n=-1; t='x'; i=$aInd[$j]; c=$firstLines[$j]}
    }
    if ($j -eq $m) { $j++; }
    $lines += [PSCustomObject]@{n=$i; t=(($bMap[$i] -lt 0) ? 'a' : '-'); i=$bInd[$i]; c=$secondLines[$i]}
}
for (; $j -lt $alen; $j++) {
    $lines += [PSCustomObject]@{n=-1; t='x'; i=$aInd[$j]; c=$firstLines[$j]}
}

$m = -1;
for ($i = $lines.length - 1; $i -ge 0; $i--) {
    if ($m -ge 0) {
        if ($lines[$i].i -lt $m) {
            if ($lines[$i].t -eq '-') {
                $lines[$i].t = 'H';
            }
            $m = $lines[$i].i;
            if ($lines[$i].c.TrimStart().StartsWith('- ')) {
                $m += 0.5;
            }
        } else {
            if ($lines[$i].t -eq '-') {
                $lines[$i].t = 'h';
            }
        }
        if ($lines[$i].i -eq 0) {
            $m = -1;
        }
    } elseif ($lines[$i].t -ne '-') {
        $m = $lines[$i].i;
        if ($lines[$i].c.TrimStart().StartsWith('- ')) {
            $m += 0.5;
        }
    }
}

$m = 0;
for ($i = 0; $i -lt $lines.length; $i++) {
    if ($lines[$i].i -eq 0) {
        $m = 0;
    }
    if ($lines[$i].t -ne '-') {
        $m = 1;
    }
    if ($m -ne 0 -and $lines[$i].t -eq '-') {
        $lines[$i].t = 'h';
    }
}

if ($html) {
    $text = "<code><pre style=`"color: grey; background-color: black`">";
    $em = $false;
    $strong = $false;
    $ins = $false;
    $del = $false;
    foreach ($line in $lines) {
        switch -CaseSensitive ($line.t) {
            'H' {
                if (-not $em) { $text += "<em style=`"color: lightgrey; font-style: normal;`">"; $em = $true; }
                if (-not $strong) { $text += "<strong style=`"color: LightYellow; font-weight: normal;`">"; $strong = $true; }
                if ($ins) { $text += "</ins>"; $ins = $false; }
                if ($del) { $text += "</del>"; $del = $false; }
            }
            'h' {
                if (-not $em) { $text += "<em style=`"color: lightgrey; font-style: normal;`">"; $em = $true; }
                if ($strong) { $text += "</strong>"; $strong = $false; }
                if ($ins) { $text += "</ins>"; $ins = $false; }
                if ($del) { $text += "</del>"; $del = $false; }
            }
            'a' {
                if (-not $em) { $text += "<em style=`"color: lightgrey; font-style: normal;`">"; $em = $true; }
                if ($strong) { $text += "</strong>"; $strong = $false; }
                if (-not $ins) { $text += "<ins style=`"color: limegreen; text-decoration: none;`">"; $ins = $true; }
                if ($del) { $text += "</del>"; $del = $false; }
            }
            'x' {
                if (-not $em) { $text += "<em style=`"color: lightgrey; font-style: normal;`">"; $em = $true; }
                if ($strong) { $text += "</strong>"; $strong = $false; }
                if ($ins) { $text += "</ins>"; $ins = $false; }
                if (-not $del) { $text += "<del style=`"color: firebrick; text-decoration: none;`">"; $del = $true; }
            }
            default {
                if ($em) { $text += "</em>"; $em = $false; }
                if ($strong) { $text += "</strong>"; $strong = $false; }
                if ($ins) { $text += "</ins>"; $ins = $false; }
                if ($del) { $text += "</del>"; $del = $false; }
            }
        }
        $text += [System.Web.HttpUtility]::HtmlEncode($line.c) + "`n";
    }
    if ($em) { $text += "</em>"; $em = $false; }
    if ($strong) { $text += "</strong>"; $strong = $false; }
    if ($ins) { $text += "</ins>"; $ins = $false; }
    if ($del) { $text += "</del>"; $del = $false; }
    $text += "</pre></code>";
    Write-Output $text;
} else {
    foreach ($line in $lines) {
        $fc = switch -CaseSensitive ($line.t) {
            'H' { [System.ConsoleColor]::Yellow }
            'h' { [System.ConsoleColor]::Gray }
            'a' { [System.ConsoleColor]::Green }
            'x' { [System.ConsoleColor]::Red }
            default { [System.ConsoleColor]::DarkGray }
        }
        $prefix = $line.t;
        if ($prefix -ne 'a' -and $prefix -ne 'x') { $prefix = ' '; }
        Write-Host $prefix $line.c -ForegroundColor $fc -BackgroundColor Black
    }
}
