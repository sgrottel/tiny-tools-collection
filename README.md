# SGrottel's Tiny Tools Collection
<img align="right" src="./_doc/swiss%20army%20cheese.png" alt="Swiss Army Cheese">

Assortment of tiny, tiny tools.

These are independent tools.
Each one is too tiny to justify a repository of it's own.

<br clear="both"/>

## AllGitsInfo
Powershell script to iterate over all git repositories and print some configuration values, like user, origin, signing.

## Beep
`MessageBeep(MB_OK);`

## CallStackUtility
Little C++ helper class to fetch a call stack, e.g. used for better error reporting and logging.

## ConProgBar
A c++ progress bar for console applications.
As usually, only semi-robust, but good enough.

## Dib
Desktop Icon Backup

## DimMon
C# app to dim monitors by displaying an transparent black overlaying window.
The idea is to dim "other" monitors when watching a video or playing a game one only one of the monitors.

## GithubOverview
A powershell script to print an overview of my stuff on Github.
[Github CLI](https://cli.github.com/) must be installed.

## HWndToFront
Brings a window to front in Windows 11.
Can also start an application and bring that's window to front.

## LoginWhen
Queries the Windows event log to print when the user sessions logged in and logged out.

## MakeIco
Powershell script using [IcoTools](https://github.com/jtippet/IcoTools) to make an `.ico` file from multiple image files in one call.

## scfeu
Source Code Files Encoding Unifier -- encoding and line endings fixed in code files. Sort of.

## Scripts
Unsorted useful scripts, e.g. Pwsh.

## shutdownplannergui
A small GUI, slapped together in C#, around the Shutdown command-line utility.

## SimpleLog
A very simple log implementation.
Nothing fancy, only log strings to log files and log file retention.
Usable as source component, not a lib.

## StartPwsh
Trivial tool to start Pwsh.exe and provide an icon. Because...

## ToggleDisplay
Command line tool to enable/disable/toggle connected displays.
My scenario is to toggle a TV connected to my PC as `\\.\DISPLAY3`

## UrlCollector
Tool to collect urls from clipboard, one by one.

## _doc
Some more generic documentation files, e.g. including images.

## License
All tools are open source and can be used freely.

In general, and explicitly for all tools not specifying a license in their subdirectory, all code within this repositiory is published under the [MIT license](./LICENSE).

Some tools explicitly specify a license, mostly Apache License version 2.0.
In those cases, consider those tools to be dual licensed under [MIT license](./LICENSE) and the explicitly specifed license.
You can then use the tool under the terms of one of those licences, as you choose.

I doubt, feel free to ask.
