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
#   .\Summary.ps1 | ConvertTo-Json -depth 20 | Set-Content .\Summary.json
#
param(
    [switch]$scripting,
    [switch]$updateForks
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
        prs=$null; # TODO IMPLEMENT!
        isFork=$_.isFork;
        isArchived=$_.isArchived;
        isPrivate=$_.isPrivate;
        isForkBehind=$false;
        updatedAt=($_.pushedAt -gt $_.updatedAt) ? $_.pushedAt : $_.updatedAt;
        issues=$null;
        workflowsCnt=0;
        workflowsLastFailed=0;
        workflows=$null;
    }

    if ($sum.isFork) {
        $forkRepo = "https://github.com/$($_.owner.login)/$($_.name).git";
        if ($updateForks) {
            gh repo sync "$($_.owner.login)/$($_.name)"
        }
        $forkHeadCommit = (git ls-remote $forkRepo HEAD)
        $parentRepo = "https://github.com/$($_.parent.owner.login)/$($_.parent.name).git";
        $parentHeadCommit = (git ls-remote $parentRepo HEAD)
        $sum.isForkBehind = ($forkHeadCommit -ne $parentHeadCommit)
    }

    if ($sum.issuesCnt -gt 0) {
        $issues = (gh issue list -L 10000 --json "title,comments,updatedAt,author" --repo $sum.name) | ConvertFrom-Json
        $hotIssues = $issues | ForEach-Object {
            return (($_.comments.length -gt 0) ? $_.comments[-1].author.login : $_.author.login)
        } | Where-Object {$_ -ne $defaultUser}
        $sum.hotIssuesCnt = $hotIssues.count
        $sum.issues = ($issues | Select-Object title,@{Name="author";Expression={$_.Author.login}},updatedAt)
    }

    $workflows = (gh workflow list --limit 10000 --json "name,id" --repo $sum.name) | ConvertFrom-Json
    $sum.workflowsCnt = $workflows.count
    $workflows = $workflows | ForEach-Object {
        $runs = (gh run list --workflow ($_.id) --limit 10 --json "name,status,conclusion,startedAt" --repo $sum.name) | ConvertFrom-Json | Where-Object status -eq "completed"
        $status = "unknown";
        $desc = "";
        $lastTime = "";
        if ($runs.length -gt 0) {
            $status = $runs[0].conclusion;
            $desc = $runs[0].name;
            $lastTime = $runs[0].startedAt;
        }
        return [PSCustomObject]@{
            name=$_.name;
            status=$status;
            desc=$desc;
            lastTime=$lastTime;
        }
    }
    $sum.workflowsLastFailed = ($workflows | Where-Object status -ne "success").count
    $sum.workflows = $workflows;

    $sum
} | Sort-Object -Property @{Expression="isArchived";Descending=$false},@{Expression="isFork";Descending=$false},@{Expression="name";Descending=$false}

$repos
