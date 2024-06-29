#
# GithubOverview.ps1
# sgrottel/tiny-tools-collection
#
# Requires:
#   Github.cli "gh" to be installed and authenticated
#
# Usage example:
#   .\GithubOverview.ps1 | ConvertTo-Json | Out-File "GithubOverview-dump.json"
#
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# configuration
$myUserReposToIgnore = @("clumsy", "FreshRSS")
$extraRepos = @("jtippet/IcoTools", "mifi/lossless-cut", "FreshRSS/FreshRSS")

# TODO: only show PRs/issues if:
#  - I am the repo owner
#  - I am author of PR/issue or assigned
#  - I participated in discussion
# For now, just remove PRs/issues in foreigh repos:
$reposNoIssues = @("jtippet/IcoTools", "mifi/lossless-cut", "FreshRSS/FreshRSS")
$reposNoPRs = @("jtippet/IcoTools", "mifi/lossless-cut", "FreshRSS/FreshRSS")

$defaultUser = "sgrottel"
$maxIssues = 10
$maxMsgLen = 60

# check github.cli authentication status
gh auth status | Out-Null
if ($LastExitCode -ne 0) {
	Write-Error "You are not logged in with the github.cli 'gh'`nPlease, run: gh auth login"
	exit
}

# collect high-level info on repositories
Write-Host "Collecting repository list" -ForegroundColor DarkGray -BackgroundColor Black

$repos = @()
$repoJsonFields = "description,id,isArchived,isFork,isPrivate,issues,latestRelease,licenseInfo,name,owner,pullRequests,url,updatedAt,pushedAt";

$myUserRepos = (gh repo list -L 10000 --json $repoJsonFields) | ConvertFrom-Json
$myUserRepos = $myUserRepos | Where-Object { $_.name -notin $myUserReposToIgnore }

$repos += $myUserRepos
$extraRepos | ForEach-Object { $repos += (gh repo view $_ --json $repoJsonFields) | ConvertFrom-Json }

$repos = $repos | Sort-Object -Property @{Expression="isArchived";Descending=$false},@{Expression="isFork";Descending=$false},@{Expression="name";Descending=$false}

function IssueInfo {
	param(
		$issue
	)
	$o = [ordered]@{}
	
	$o["id"] = $issue.number
	$o["title"] = $issue.title
	$o["url"] = $issue.url
	$o["date"] = [datetime]$issue.createdAt
	$o["author"] = "$($issue.author.login) ($($issue.author.name))"

	if ($issue.assignees) {
		$o["assignees"] = $issue.assignees | ForEach-Object { "$($_.login) ($($_.name))"}
	}

	$updated = [datetime]$issue.updatedAt
	if ($updated -gt $o["date"]) {
		$o["last_updated"] = $updated
	}

	$count = [int]($issue.comments.count)
	if ($count -gt 0) {
		$o["comments"] = [ordered]@{}
		$o["comments"]["count"] = $count
		$lastComment = ([object[]]($issue.comments | ForEach-Object {
			$date = [datetime]$_.createdAt
			if ($_.updatedAt) {
				$d = [datetime]$_.updatedAt
				if ($date -lt $d) {
					$date = $d
				}
			}
			@{date=$date;comment=$_}
		} | Sort date))[-1]
		# $o["comments"]["last_raw"] = $lastComment
		$o["comments"]["last_date"] = $lastComment["date"]
		$o["comments"]["last_by"] = "$($lastComment["comment"].author.login) ($($lastComment["comment"].author.name))"		
	}
	return $o
}

function RepoInfo {
	param(
		$repo
	)
	$o = [ordered]@{}

	# name, type, id
	$name = $repo.name
	if ($repo.owner.login -ne $defaultUser) {
		$name = $repo.owner.login + "/" + $name
	}
	$o["name"] = $name
	$namePrefix = "";
	if ($repo.isArchived) {
		$o["isArchived"] = $true
		$namePrefix += "📦"
	}
	if ($repo.isPrivate) {
		$o["isPrivate"] = $true
		$namePrefix += "🔒"
	}
	if ($repo.isFork) {
		$o["isFork"] = $true
		$namePrefix += "🪧"
	}
	if ($namePrefix) {
		$o["name"] = $namePrefix + ": " + $o["name"]
	}
	$o["url"] = $repo.url

	# last commit
	$lastCommitInfo = (gh api "repos/$($repo.owner.login)/$($repo.name)/commits/HEAD") | ConvertFrom-Json
	if ($lastCommitInfo) {
		$olci = [ordered]@{}
		$olci["hash"] = ([Uri]$lastCommitInfo.commit.url).Segments[-1]
		$olci["author"] = "$($lastCommitInfo.commit.author.name) <$($lastCommitInfo.commit.author.email)>"
		$o["lastCommit_date"] = [datetime]$lastCommitInfo.commit.author.date
		if ($lastCommitInfo.commit.committer -and $lastCommitInfo.commit.committer.name -and $lastCommitInfo.commit.committer.email) {
			if ($lastCommitInfo.commit.committer.name -ne $lastCommitInfo.commit.author.name -or $lastCommitInfo.commit.committer.email -ne $lastCommitInfo.commit.author.email) {
				$olci["committer"] = "$($lastCommitInfo.commit.committer.name) <$($lastCommitInfo.commit.committer.email)>"
				if ($lastCommitInfo.commit.committer.date) {
					$d = [datetime]$lastCommitInfo.commit.committer.date
					if ($o["lastCommit_date"] -lt $d) {
						$o["lastCommit_date"] = $d
					}
				}
			}
		}
		$olci["msg"] = $lastCommitInfo.commit.message.Split("`n")[0]
		if ($olci["msg"].length -gt $maxMsgLen) {
			$olci["msg"] = $olci["msg"].substring(0, $maxMsgLen - 3) + "..."
		}

		# $olci["raw"] = $lastCommitInfo.commit
		$o["lastCommit_info"] = $olci
	}

	# last release
	if ($repo.latestRelease) {
		$o["lastRelease"] = [ordered]@{}
		$o["lastRelease"]["name"] = $repo.latestRelease.name
		$o["lastRelease"]["tag"] = $repo.latestRelease.tagName
		$o["lastRelease"]["date"] = [datetime]$repo.latestRelease.publishedAt
	}

	# issues
	$issueJsonFields = "number,title,author,assignees,comments,createdAt,updatedAt,url";
	if (-not ($name -in $reposNoIssues)) {	
		$count = [int]($repo.issues.totalCount)
		if ($count -gt $maxIssues) {
			$o["issues_totalCount"] = $count
		}
		$o["issues"] = @()
		if ($count -gt 0) {
			$issueInfo = ((gh issue list -R "$($repo.owner.login)/$($repo.name)" -L ([Math]::Min($count, $maxIssues)) --json $issueJsonFields) | ConvertFrom-Json)
			$o["issues"] += [object[]]($issueInfo | ForEach-Object { IssueInfo $_ })
		}
	}

	# pullRequests
	if (-not ($name -in $reposNoPRs)) {
		$count = [int]($repo.pullRequests.totalCount)
		if ($count -gt $maxIssues) {
			$o["pullRequests_totalCount"] = $count
		}
		$o["pullRequests"] = @()
		if ($count -gt 0) {
			$prInfo = ((gh pr list -R "$($repo.owner.login)/$($repo.name)" -L ([Math]::Min($count, $maxIssues)) --json $issueJsonFields) | ConvertFrom-Json)
			$o["pullRequests"] += [object[]]($prInfo | ForEach-Object { IssueInfo $_ })
		}
	}

	return $o
}

Write-Host "$($repos.length) repositories" -ForegroundColor DarkGray -BackgroundColor Black
$repos = $repos | ForEach-Object {
	Write-Host "loading $($_.owner.login)/$($_.name)" -ForegroundColor DarkGray -BackgroundColor Black
	RepoInfo $_
}

$repos
