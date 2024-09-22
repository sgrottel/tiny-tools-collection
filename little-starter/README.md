# ✨ Little Starter™
Automate the typical "open apps &amp; files" task when you start your working session.

The idea is to simplify the stuff I do (most) every time I start my days, after booting and logging into the system.

<!--- START STRIP -->
[![Build Action](https://github.com/sgrottel/little-starter/actions/workflows/dotnet-desktop.yaml/badge.svg)](https://github.com/sgrottel/little-starter/actions/workflows/dotnet-desktop.yaml)

<!--- END STRIP -->
## How to Use
You need to write a [YAML configuration file (see: `doc/config-yaml.md`)](./doc/config-yaml.md).

To install the tool, just download a release zip and extract all files into a dedicated folder.
For example:
```
C:\tools\little-starter\
```

To auto-start the tool create a shortcut to the tool into the auto-run start menu folder for your user.
For example:
```
C:\Users\#####\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\Little Starter™.lnk
```

<!--- START STRIP -->
## How to Build
Using Visual Studio you can use the [build instructions (see: `doc/build.md`)](./doc/build.md) for a straight forward process.

<!--- END STRIP -->
## License
This project is freely available under the terms of the [Apache License v.2.0](LICENSE):

> Copyright 2022 SGrottel
>
> Licensed under the Apache License, Version 2.0 (the "License");
> you may not use this file except in compliance with the License.
> You may obtain a copy of the License at
>
> http://www.apache.org/licenses/LICENSE-2.0
>
> Unless required by applicable law or agreed to in writing, software
> distributed under the License is distributed on an "AS IS" BASIS,
> WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
> See the License for the specific language governing permissions and
> limitations under the License.
