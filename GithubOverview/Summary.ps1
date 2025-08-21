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
    [switch]$updateForks,
    [switch]$runEmptyWorkflows
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# check github.cli authentication status
$authStat = (gh auth status 2>&1) | Out-String
if ($LastExitCode -ne 0) {
    Write-Error "You are not logged in with the github.cli 'gh'`nPlease, run: gh auth login"
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
    Write-Error "Failed to identify default user from 'gh auth status'"
    exit
}

# collect high-level info on repositories
# Write-Host "Collecting repository list" -ForegroundColor DarkGray -BackgroundColor Black
$repoJsonFields = "isArchived,isFork,isPrivate,issues,name,owner,pullRequests,url,updatedAt,pushedAt,parent";

$repos = (gh repo list -L 10000 --json $repoJsonFields) | ConvertFrom-Json
# $repos = $repos | Where-Object { $_.name -notin $myUserReposToIgnore }
# $repos = $repos | Where-Object { $_.name -eq "tiny-tools-collection" }

# sort owner, making default user empty name => first in list
$repos = $repos | ForEach-Object {
    $sum = [PSCustomObject]@{
        name=$_.owner.login + "/" + $_.name;
        url=$_.url;
        issuesCnt=$_.issues.totalCount;
        hotIssuesCnt=0;
        prCnt=$_.pullRequests.totalCount;
        prs=$null;
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
        $issues = (gh issue list -L 10000 --json "title,comments,updatedAt,author,number" --repo $sum.name) | ConvertFrom-Json
        $hotIssues = $issues | ForEach-Object {
            return (($_.comments.length -gt 0) ? $_.comments[-1].author.login : $_.author.login)
        } | Where-Object {$_ -ne $defaultUser}
        $sum.hotIssuesCnt = $hotIssues.count
        $sum.issues = [array]($issues | Select-Object number,title,@{Name="author";Expression={$_.Author.login}},updatedAt)
    }

    if ($sum.prCnt -gt 0) {
        $prs = (gh pr list -L 10000 --json "number,title,updatedAt,author,createdAt" --repo $sum.name) | ConvertFrom-Json
        $sum.prs = [array]($prs | Select-Object number,title,@{Name="author";Expression={$_.Author.login}},createdAt,updatedAt)
    }

    $workflows = (gh workflow list --limit 10000 --json "name,id,path" --repo $sum.name) | ConvertFrom-Json
    $sum.workflowsCnt = $workflows.count
    $workflows = $workflows | ForEach-Object {
        $runs = (gh run list --workflow ($_.id) --limit 10 --json "name,status,conclusion,startedAt,databaseId" --repo $sum.name) | ConvertFrom-Json | Where-Object status -eq "completed"
        $status = "unknown";
        $desc = "";
        $lastTime = "";
        $id = "";

        if (($runs.length -eq 0) -and ($runEmptyWorkflows))
        {
            # workflow has no runs, so trigger dispatch runs (if possible) for the next update to get info (auto-healing)
            gh workflow run ($_.id) --repo ($sum.name)
        }

        if ($runs.length -gt 0) {
            $status = $runs[0].conclusion;
            $desc = $runs[0].name;
            $lastTime = $runs[0].startedAt;
            $id = $runs[0].databaseId;
        }
        return [PSCustomObject]@{
            name=$_.name;
            status=$status;
            desc=$desc;
            lastTime=$lastTime;
            id=$id;
        }
    }
    $sum.workflowsLastFailed = ($workflows | Where-Object { (($_.status -ne "success") -and ($_.status -ne "unknown")) }).count
    $sum.workflows = [array]($workflows);

    $sum
} | Sort-Object -Property @{Expression="isArchived";Descending=$false},@{Expression="isFork";Descending=$false},@{Expression="name";Descending=$false}

$repos
