# HWndToFront
Utility to bring a Window to Front

## Window Mode
Syntax:
```ps
HWndToFront.exe {DEC}
```
```ps
HWndToFront.exe x{HEX}
```
Specify the [window handle (HWND)]() of the top-level window you want to pull in front either as decimal number or as hex number prefixed by an `x`

Example:
```ps
HWndToFront.exe x00000000000509C2
```
Note: leading zeros are optional, e.g. from copying the handle value from another software.

## Start Call Mode
Syntax:
```ps
HWndToFront.exe start (in {wd}) {exec} ({args} ...)
```
The first argument must be the string `start`.

The next two arguments can be optionally the string `in` and the path to be used as working directory.
If omitted, the current directory will be used as working directory.

The next argument is the path of the executable to run.
Specify the executable in a way that it can be directly found, either as full path (recommended) or relative path.
HWndToFront will *not* search for the executable using the system's path environment variable.

All remaining arguments will be passed on to the specified executable when starting.

Example:
```ps
start in C:\dev "C:\Program Files\Notepad++\notepad++.exe" "C:\dev\HWndToFront\README.md"
```

## Build
Open the project solution in Visual Studio 2022 (Community Edition) or newer.
Make sure you have C/C++ Desktop development tools and a recent Windows SDK installed.
The solution should build as is.

## License
> Copyright 2022 SGrottel (https://github.com/sgrottel/HWndToFront)
>
> Licensed under the [Apache License, Version 2.0 (the "License")](./LICENSE);
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
