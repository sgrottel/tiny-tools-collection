//
// KeePassHotKey
// KeePassDetector.cpp
//
// Copyright 2022 SGrottel (https://www.sgrottel.de)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http ://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissionsand
// limitations under the License.
//
#include "KeePassDetector.h"

#include "Common.h"
#include "Config.h"

#include <psapi.h>

#include <functional>

namespace {

	struct WndInfo {
		HWND hwnd;
		_tstring caption;
		_tstring classname;
		_tstring process;
	};

	struct SearchCallback {
		std::function<bool(WndInfo&)> callback;
	};

	BOOL CALLBACK detectKeePassMainWindow(HWND hwnd, LPARAM lParam) {
		SearchCallback* cb = reinterpret_cast<SearchCallback*>(lParam);
		WndInfo info{ hwnd };

		constexpr const int k_strBufLen = 200;

		TCHAR caption[k_strBufLen + 1];
		DWORD len = GetWindowText(hwnd, caption, k_strBufLen);
		if (len > k_strBufLen) len = k_strBufLen;
		caption[len] = 0;
		info.caption = caption;

		TCHAR classname[k_strBufLen + 1];
		len = GetClassName(hwnd, classname, k_strBufLen);
		if (len > k_strBufLen) len = k_strBufLen;
		classname[len] = 0;
		info.classname = classname;

		DWORD pid = 0;
		DWORD tid = GetWindowThreadProcessId(hwnd, &pid);
		HANDLE hPro = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
		if (hPro != INVALID_HANDLE_VALUE) {
			TCHAR path[MAX_PATH + 1];
			len = GetModuleFileNameEx(hPro, 0, path, MAX_PATH);
			if (len > MAX_PATH) len = MAX_PATH;
			path[len] = 0;
			info.process = path;

			CloseHandle(hPro);
		}
		else
		{
			info.process.clear();
		}

		bool rv = cb->callback(info);

		return rv ? TRUE : FALSE; // continue search
	}

	_tstring getFilename(const _tstring& path) {
		auto pos = path.find_last_of(_T('\\'));
		if (pos == _tstring::npos) return path;
		return path.substr(pos + 1);
	}
}

KeePassDetector::KeePassDetector(const Config& config) 
	: m_config{ config },
	m_result{ Result::Unknown } {
	// intentionally empty
}

void KeePassDetector::Detect() {
	m_result = Result::Unknown;

	WndInfo window{ 0 };

	_tstring keePassPath{ m_config.getKeePassExe() };
	_tstring keePassName{ getFilename(keePassPath) };

	SearchCallback searcher{
		[&keePassPath, &keePassName, &window](WndInfo& wi) {
			bool procFilenameMatch = 0 == _tcsicmp(getFilename(wi.process).c_str(), keePassName.c_str());
			bool procPathMatch = 0 == _tcsicmp(wi.process.c_str(), keePassPath.c_str());

			if (procFilenameMatch) {
				// there will be several top-level windows (message only, tooltips, etc.)



				OutputDebugStringW((
					std::wstringstream{} << wi.process << L"\n"
					<< wi.caption << L"\n"
					<< wi.classname << L"\n"
					<< "Path-match: " << procPathMatch << "\n"
					).str().c_str());

			}
			return true; // continue search
		}
	};
	EnumWindows(&detectKeePassMainWindow, reinterpret_cast<LPARAM>(&searcher));

}
