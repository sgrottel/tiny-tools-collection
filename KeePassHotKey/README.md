# KeePass HotKey
Wrapper utility to open a [KeePass](https://keepass.info/) DB or trigger the Auto-Type Feature

<img src="./KeePass_Square_BW.ico" alt="KeePass_Square_BW" width="64px" />

This utility is _very_ specific to my use case:

- I want to trigger one action from a dedicated hardware key on my keyboard
- This action should either open a specific KeePass data base file, configured by the user, or
- Trigger the "auto-type selected" feature of KeePass, if a KeePass instance is open, running, and has a selected entry.

Note, that for the condition of triggering the "auto-type selected" feature, it is not required that the running KeePass instance has the same data base opened as is configured for the first workflow.


## Usage

Assign a hardware key to start the executable without any additional parameters.
```
.\KeePassHotKey.exe
```
preferably, specify the _full path_ to the app.
The examples in this README.md use relative paths only to improve readability.

After the app has been configured (see below), the key will trigger the following actions:

1. If KeePass is not running, or no data base is opened, or no entry is selected in the right ListView of KeePass...
	- Then the configred `.kdbx` file will be opened.
	- The dialog for entering the master password should automatically appear quickly.
2. If KeePass is running, has _any_ data base opened, and any entry is selected...
	- The app will show a dialog asking for confirmation to auto-type the selected entry.
	(Since triggering auto-typing a password with a very single key type sounds dangerous.)
	- Press the app hot key a second time _after_ this confirmation dialog appeared.
	- Then KeePass will be instructed to trigger auto-type of the selected entry.

You can call the app with
```
.\KeePassHotKey.exe -?
```
to show a help dialog with the possible command line options.

### Configuration

To configure the application, call
```
.\KeePassHotKey.exe -config <file> <exe>
```

Replace `<file>` with the _full path_ to the `.kdbx` file you want to open if no KeePass instance is running.

Replace `<exe>` with the _full path_ to the `keepass.exe` you are using.
If you omit this parameter, the app will try to auto-detect the path of the executable.

If needed the app will request elevated access rights for your user account to write the configuration to the windows registry.

There is no way to _unconfigure_ the app.
The configuration is stored in the Windows registry, under `HKEY_CURRENT_USER\Software\SGrottel\KeePassHotKey`.
Delete this key and it's values if you want to remove the configuration.


## Dependencies

This app requires the Microsoft Visual C runtime to be installed:

https://visualstudio.microsoft.com/de/vs/older-downloads/#microsoft-visual-c-redistributable-for-visual-studio-2017


## Building from Source

This app is a native Windows application, written in C++.

You will need Microsoft Visual Studio 2019 or newer.
The Community Edition will work nicely.

You need a recent Windows SDK, which is usually bundled with Visual Studio itself.

The open the solution and build the solution or project.

There are no further dependencies.


## Acknowledgements

The icon `KeePass_Square_BW.ico` is based on the original icon image files from the KeePass repository:
* Files: `KeePass_Square_Blue_*.png`
* https://sourceforge.net/projects/keepass/files/KeePass%202.x/2.50/KeePass-2.50-Source.zip/download
* https://keepass.info/


## License

The project is freely available under the [Apache 2 License](./LICENSE).

> Copyright 2022 SGrottel (https://www.sgrottel.de)
>
> Licensed under the Apache License, Version 2.0 (the "License");
> you may not use this file except in compliance with the License.
> You may obtain a copy of the License at
>
> http ://www.apache.org/licenses/LICENSE-2.0
>
> Unless required by applicable law or agreed to in writing, software
> distributed under the License is distributed on an "AS IS" BASIS,
> WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
> See the License for the specific language governing permissionsand
> limitations under the License.
