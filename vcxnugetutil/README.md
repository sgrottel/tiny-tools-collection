# VCX Nuget Util
Native cpp projects with vcxproj can use native nuget packages to pull in dependencies.

Currently, dependabot is not able to scan vcxproj to detect updates of those dependencies:

* [Support .vcxproj for NuGet dependencies (dependabot-core/issues/#9711)](https://github.com/dependabot/dependabot-core/issues/9711)
* [Dependabot no longer updates NuGet packages in a repository with only a Directory.Packages.props file (dependabot-core/issues/#8590)](https://github.com/dependabot/dependabot-core/issues/8590)
* [https://github.com/willbush/dependabot-cpm-updates](https://github.com/willbush/dependabot-cpm-updates)

As a recommendation, csproj-sentinel projects can duplicated the depenceny references and will be visible to dependabot.
