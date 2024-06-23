# Redate
![Redate Icon](images/redate_128x.png)

Rewrite dates of files.

<!--[![Build status](https://ci.appveyor.com/api/projects/status/dbkm60f719k2eho8?svg=true)](https://ci.appveyor.com/project/s_grottel/redate)-->

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

This will create and initialize the `redate-file`.

The file name extension `.redate` is **not** automatically added.
Remember to write it yourself.
                         
You will need to specify one or more `source-directories`.
Those should be full file paths!

Redate, will now crawl all source directories recursively, and write the state of all files found in those directories into the redate JSON file.
This state includes the _file size_, _last write date_, _file attributes_, and _md5 hash_ of the files' contents.
If the source directories are in sub directories from the location the `.redate` file is being created, then the file names will be stored as relative paths.
Else, the files will be stored in absolute paths.

It is not supported to have the `.redate` file located in one of the source directories.
Doing so might result in undefined behavior.

MD5 hashes will be computed for all files in the source directories.
If you have larger files or many files, this might be a slow process.


### Run a Redate
```
.\Redate.exe run <redate-file>
```

When you run a redate, the source directories specified within the `.redate` file are crawled recursively for all files.
For all files the state within the file system and the stored stated within in the `.redate` file is compared.

* _New_ files only present in the file system will be added to the stored state.
* _Deleted_ files will also removed from the stored state.
* For _existing_ files, the _size_, and _content hash_ will be compared.
    * If the file is _unchanged_, the _last write date_ in the file system will be reset to the value stored in the `.redate` file.
    * If the file is _changed_, the stored state is updated. 

After this process, files which have been written or re-created, but have the same content as before, will be reset to their previous _last write date_.
This recreates the appearance of the original file.


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
This project is dual licensed. You choose either the terms of the [MIT LICENSE](../LICENSE) or the [Apache LICENSE v2.0](./LICENSE).

> Copyright 2021-2024 SGrottel (https://www.sgrottel.de)
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
