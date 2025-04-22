// FileBookmark
// Copyright 2023, SGrottel
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#include "utility.h"

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

#include <vector>

std::wstring filebookmark::GetExecutingModuleFilePath()
{
	std::vector<wchar_t> path;
	size_t size = _MAX_PATH;
	path.resize(size);
	size = static_cast<size_t>(GetModuleFileNameW(nullptr, path.data(), static_cast<DWORD>(path.size())));
	while (size == path.size() && GetLastError() == ERROR_INSUFFICIENT_BUFFER)
	{
		size += 100;
		path.resize(size);
		size = static_cast<size_t>(GetModuleFileNameW(nullptr, path.data(), static_cast<DWORD>(path.size())));
	}
	return std::wstring(path.data(), size);
}
