# ConProgBar
A simple single-file C++/H progress bar for the text console.

This is based on an older C# implementation.
I rewrote the important parts in C++.

## Usage
Just add `ConProgBar.h` to your project.

## Example
```cpp
#include "ConProgBar.h"

// ...

sgconutil::ConProgBar<int> bar;

// Start the bar with any progress value range.
// The max value indicates process completion.
bar.Start(0, 10, 0);

for (int i = 0; i < 10; ++i) {
	bar.SetVal(i);

	// Do your stuff

}

// And, we are done.
bar.Complete();
```

All three function calls will report their progress information to `std::cout`.
For best, consistent output, your application should not output any information (except for critical errors, of course).
If you do, be sure to add a new line to your output, as a subsequence output from the ConProgBar will start with a `\r`.


## License
The code is freely available under terms of the Apache License V2 (see [LICENSE](./License))

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
