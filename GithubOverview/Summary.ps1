#
# Summary.ps1
# sgrottel/tiny-tools-collection
#
# Requires:
#   Github.cli "gh" to be installed and authenticated
#
# Example:
#   .\Summary.ps1 | Format-Table
#   .\Summary.ps1 | Format-Table name,url,issuesCnt,@{Name="hotIssuesCnt";Expression={$esc=[char]27;($_.hotIssuesCnt -gt 0) ? "$esc[31m$($_.hotIssuesCnt)$esc[0m": "0"};Alignment='Right'},@{Name="prCnt";Expression={$esc=[char]27;($_.prCnt -gt 0) ? "$esc[31m$($_.prCnt)$esc[0m": "0"};Alignment='Right'},isFork,isArchived,isPrivate,isForkBehind
#
param(
    [switch]$scripting
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# check github.cli authentication status
$authStat = (gh auth status 2>&1) | Out-String
if ($LastExitCode -ne 0) {
    if ($scripting) {
        "ERROR: gh not logged in"
    } else {
        Write-Error "You are not logged in with the github.cli 'gh'`nPlease, run: gh auth login"
    }
    exit
}
if ($authStat -match 'Logged in to github.com account (\w+)')
{
    $defaultUser = $Matches[1].Trim()
}
elseif ($authStat -match 'Logged in to github.com as (\w+)')
{
    $defaultUser = $Matches[1].Trim()
}
else
{
    if ($scripting) {
        "ERROR: gh user detection failed"
    } else {
        Write-Error "Failed to identify default user from 'gh auth status'"
    }
    exit
}

# collect high-level info on repositories
# Write-Host "Collecting repository list" -ForegroundColor DarkGray -BackgroundColor Black
$repoJsonFields = "isArchived,isFork,isPrivate,issues,name,owner,pullRequests,url,updatedAt,pushedAt,parent";

$repos = (gh repo list -L 10000 --json $repoJsonFields) | ConvertFrom-Json
# $repos = $repos | Where-Object { $_.name -notin $myUserReposToIgnore }

# sort owner, making default user empty name => first in list
$repos = $repos | ForEach-Object {
    $sum = [PSCustomObject]@{
        name=$_.owner.login + "/" + $_.name;
        url=$_.url;
        issuesCnt=$_.issues.totalCount;
        hotIssuesCnt=0;
        prCnt=$_.pullRequests.totalCount;
        isFork=$_.isFork;
        isArchived=$_.isArchived;
        isPrivate=$_.isPrivate;
        isForkBehind=$false;
        updatedAt=($_.pushedAt -gt $_.updatedAt) ? $_.pushedAt : $_.updatedAt;
    }

    if ($sum.isFork) {
        $forkRepo = "https://github.com/$($_.owner.login)/$($_.name).git";
        $forkHeadCommit = (git ls-remote $forkRepo HEAD)
        $parentRepo = "https://github.com/$($_.parent.owner.login)/$($_.parent.name).git";
        $parentHeadCommit = (git ls-remote $parentRepo HEAD)
        $sum.isForkBehind = ($forkHeadCommit -ne $parentHeadCommit)
    }

    if ($sum.issuesCnt -gt 0) {
        $issues = (gh issue list -L 10000 --json "comments,updatedAt,author" --repo $sum.name) | ConvertFrom-Json
        $hotIssues = $issues | ForEach-Object {
            return (($_.comments.length -gt 0) ? $_.comments[-1].author.login : $_.author.login)
        } | Where-Object {$_ -ne $defaultUser}
        $sum.hotIssuesCnt = $hotIssues.count
    }

    $sum
} | Sort-Object -Property @{Expression="isArchived";Descending=$false},@{Expression="isFork";Descending=$false},@{Expression="name";Descending=$false}

$repos
