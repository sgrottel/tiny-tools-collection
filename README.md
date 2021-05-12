# Redate

Rewrite dates of files.

Uses _redate_ files to memorize the state, i.e. size, file attributes, content hash, and last write date, of files within input source directories.
The tool can then reset file attributes and last write date of files, which have been rewritten with the same content as before, i.e. same size and same content hash.

More information is available in this [Blog post (https://go.grottel.net/redate-blog-post)](https://go.grottel.net/redate-blog-post)


## How to use

This is a command line utility app.
Run it from a command line shell, like Powershell or CMD.

Syntax:
```
.\Redate.exe <command> <redate-file> <source-directories>
```

The parameters `redate-file` and `source-directories` might be omitted, depending on the command.
The following subsections provide details about the possible commands.


### Init a Redate File
```
.\Redate.exe init <redate-file> <source-directories>
```

TODO


### Run a Redate
```
.\Redate.exe run <redate-file>
```

TODO


### Register the Redate File Type
```
.\Redate.exe reg
```

Adds entries to the Windows Registry to register this instance of `Redate.exe` with the file type `*.redate`.
This allows to automatically run the `Run` command on those files, by simply double-clicking, e.g. in the Windows Explorer.

This command most likely requires elevated user rights, and must be run with administrator privileges.


### Unregister the Redate File Type
```
.\Redate.exe unreg
```

Removes the entries from the Windows Registry, which have been created by the command `reg`.

This command most likely requires elevated user rights, and must be run with administrator privileges.

If the file type is not registered, the operation will not fail.

The operation will not check, if the entries in the Windows Registry point to this specific instance of `Redate.exe` or any other path.
The entries will always be removed.


## How to build

Redate is written in CSharp, and set up as a Visual Studio project for DotNet 5.0.

Open the `Redate.sln` Visual Studio solution file, e.g., in Visual Studio Community Edition.


### Dependencies

* Microsoft.Win32.Registry -- is used to implement the commands `reg` and `unreg`.
As a result, Redate is platform-specific to Microsoft Windows.
* Newtonsoft.Json -- is used to serialize and deserialize the `.redate` files, which are in JSON format.

All dependencies are installed via Nuget packages.
When building the application from within Visual Studio, those packages should be restored automatically.
If not, trigger _Restore Nuget Packages_ on the solution, e.g. by right-clicking in the Solution Explorer on the Solution node.


## License

Copyright 2021 SGrottel (https://www.sgrottel.de)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

For details, see [LICENSE](./LICENSE) file.
