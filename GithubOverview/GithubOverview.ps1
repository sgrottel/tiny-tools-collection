
Write-Host "🦑 Checking Github CLI authentication status"
gh auth status
Write-Host "If not logged in, please run: gh auth login"

$repos = (gh repo list -L 10000 --json "description,id,isArchived,isFork,isPrivate,issues,latestRelease,licenseInfo,name,owner,pullRequests,url,updatedAt,pushedAt") | ConvertFrom-Json
$repos = $repos | Sort-Object -Property @{Expression="isArchived";Descending=$false},@{Expression="isFork";Descending=$false},@{Expression="name";Descending=$false}

function Write-RepoInfo($repo)
{
	Write-Host $repo.name -NoNewLine -F White -B Black
	if ($repo.isArchived -OR $repo.isPrivate -OR $repo.isFork) { 
		Write-Host " [" -NoNewLine -F DarkGray -B Black
		if ($repo.isArchived) { Write-Host "📦Archived" -NoNewLine -F DarkYellow -B Black }
		if ($repo.isPrivate) { Write-Host "🔒Private" -NoNewLine -F DarkRed -B Black }
		if ($repo.isFork) { Write-Host "🪧Fork" -NoNewLine -F DarkCyan -B Black }
		Write-Host "]" -NoNewLine -F DarkGray -B Black
	}
	Write-Host " - " -NoNewLine -F DarkGray -B Black
	$age = -(New-TimeSpan -Start (Get-Date) -End ([datetime]$repo.pushedAt)).TotalDays
	$ageCol = 'DarkGray'
	if ($age -lt 1) { $age = "Today (<1 day)"; $ageCol = 'darkgreen' }
	elseif ($age -lt 2) { $age = "Yesterday (<2 days)"; $ageCol = 'darkgreen' }
	elseif ($age -lt 30) { $age = ("{0:n0} days" -f $age); $ageCol = 'darkgreen' }
	elseif ($age -gt 365) {
		$age = "{0:n1} years" -f ($age / 365)
	} else {
		$age = "{0:n1} months" -f ($age / 30.5)
	}
	Write-Host $age -F $ageCol -B Black

	$issues = [int]($repo.issues.totalCount)
	if ($issues -gt 0) {
		if ($issues -eq 1) { $issues = "1 open Issue" }
		else { $issues = "{0} open Issues" -f $issues }
		Write-Host "   " $issues
		$issueInfo = (gh issue list -R "$($repo.owner.login)/$($repo.name)" --json "number,title,author,assignees") | ConvertFrom-Json
		$issueInfo | foreach {
			Write-Host "        #$($_.number)  $($_.title)" -NoNewLine
			if ($_.author.login -ne "sgrottel") {
				Write-Host " (from: $($_.author.login))" -NoNewLine -F DarkGray -B Black
			}
			Write-Host
		}
	}
	$pullRequests = [int]$repo.pullRequests.totalCount
	if ($pullRequests -gt 0) {
		if ($pullRequests -eq 1) { $pullRequests = "1 open Pull Request" }
		else { $pullRequests = "{0} open Pull Requests" -f $pullRequests }
		Write-Host "   " $pullRequests
	}
}

Write-Host
$repos | foreach { Write-RepoInfo $_ }
