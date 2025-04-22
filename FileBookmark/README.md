# 🔖 FileBookmark
A simply way to bookmark a file in a directory.

[![GitHub](https://img.shields.io/github/license/sgrottel/FileBookmark)](/LICENSE)
[![Build Native](https://github.com/sgrottel/FileBookmark/actions/workflows/build_native.yaml/badge.svg)](https://github.com/sgrottel/FileBookmark/actions/workflows/build_native.yaml)

This is not a Windows Explorer shell extension.
It is a simple, normal application which writes to the right places in the registry.
Simple, not elegant, but working.

## FileBookmark.exe
The C++ application to mainly handle `.bookmark` files, quickly and directly.
It can:

* Register and unregister the `.bookmark` file type
* It can open a `.bookmark` file, and offers multiple quick actions:
    * Opening the bookmarked file
    * Moving the bookmark to the next file and optionally opening it.
    * Unbookmarking the current file, but not removing the `.bookmark` file from the directory.
      This is useful to keep the history of the bookmarked files, but not marking any file as current.
    * Opening the FileBookmark UI for additional interactions.


## FileBookmarkUI.exe
The CSharp application to show the contents of a `.bookmark` file graphically.
This way, the user can see the history of a bookmark.

TODO


## `.bookmark` Files
The `.bookmark` files are YAML files storing the history of all files bookmarked in this directory.
Core assumption is that there is only one `.bookmark` file in each directory.
When a file is bookmarked, the `.bookmark` file is changed to reference the file from it's content and is renamed to match the file name for the file bookmarked.


## Contributing
Contributions are welcome to this project in all forms:
bug reports, feature suggestions, bug fixed, documentation, etc.
In doubt, feel free to contact me with any questions.

## License
This project is freely available as open source under the terms of the [Apache License, Version 2.0](LICENSE)

> Copyright 2011-2023, SGrottel
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
