# ✨ Little Starter™ - How to Build

* Open the Visual Studio Solution
* Make sure you have the required DotNet framework tools and sdk installed.
  * `<TargetFramework>net6.0-windows</TargetFramework>`
* Make sure the Nuget packets restore without error
  * `YamlDotNet`
* Build the project `app` targets

Note:
The build number is only injected into the version number on the CI build agents.
See [/.github/workflows/dotnet-desktop.yaml](../.github/workflows/dotnet-desktop.yaml) for details.
