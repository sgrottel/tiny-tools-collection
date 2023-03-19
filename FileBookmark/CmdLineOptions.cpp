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
#include "CmdLineOptions.h"

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>
#include <shellapi.h>

#include <memory>
#include <algorithm>
#include <string>
#include <cctype>
#include <filesystem>

using filebookmark::CmdLineOptions;

void CmdLineOptions::Parse(const wchar_t* pCmdLine)
{
	if (pCmdLine == nullptr || pCmdLine[0] == L'\0')
	{
		// early out.
		// Otherwise `CommandLineToArgvW` will add the executable name.
		m_mode = Mode::None;
		m_bookmarkFile.clear();
		return;
	}

	int argc = 0;
	std::unique_ptr<LPWSTR, void*(WINAPI *)(void*)> argv{CommandLineToArgvW(pCmdLine, &argc), &LocalFree};

	for (int argi = 0; argi < argc; ++argi)
	{
		LPCWSTR arg = argv.get()[argi];
		if (arg == nullptr) continue;

		size_t len = wcslen(arg);
		if (len > 2 && arg[0] == '-' && arg[1] == '-')
		{
			// likely an option
			std::wstring opt{arg};
			std::transform(opt.begin(), opt.end(), opt.begin(), [](auto c) { return std::tolower(c); });

			if (opt == L"--open")
			{
				m_mode = Mode::OpenBookmark;
				continue;
			}
			if (opt == L"--reg")
			{
				m_mode = Mode::RegisterFileType;
				continue;
			}
			if (opt == L"--unreg")
			{
				m_mode = Mode::UnregisterFileType;
				continue;
			}
		}

		// likely a file
		std::filesystem::path filePath{arg};
		if (!std::filesystem::is_regular_file(filePath))
		{
			continue;
		}
		if (!filePath.is_absolute())
		{
			filePath = std::filesystem::absolute(filePath);
		}
		filePath = std::filesystem::canonical(filePath);

		if (m_bookmarkFile.empty())
		{
			m_bookmarkFile = filePath.wstring();
		}

	}

	if (m_mode == Mode::OpenBookmark && m_bookmarkFile.empty())
	{
		m_mode = Mode::None;
	}

	if (!m_bookmarkFile.empty() && m_mode == Mode::None)
	{
		m_mode = Mode::OpenBookmark;
	}
}
