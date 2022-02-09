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
#include <Commctrl.h>

#include <functional>
#include <vector>
#include <regex>

namespace {

	struct WndInfo {
		HWND hwnd;
		_tstring caption;
		_tstring classname;
		_tstring process;
	};

	struct SearchWindowCallback {
		std::function<bool(WndInfo&)> callback;
	};

	_tstring _tGetClassName(HWND hwnd) {
		constexpr const int k_strBufLen = 200;
		TCHAR classname[k_strBufLen + 1];
		DWORD len = GetClassName(hwnd, classname, k_strBufLen);
		if (len > k_strBufLen) len = k_strBufLen;
		classname[len] = 0;
		return classname;
	}

	BOOL CALLBACK detectKeePassMainWindow(HWND hwnd, LPARAM lParam) {
		SearchWindowCallback* cb = reinterpret_cast<SearchWindowCallback*>(lParam);
		WndInfo info{ hwnd };

		constexpr const int k_strBufLen = 200;

		TCHAR caption[k_strBufLen + 1];
		DWORD len = GetWindowText(hwnd, caption, k_strBufLen);
		if (len > k_strBufLen) len = k_strBufLen;
		caption[len] = 0;
		info.caption = caption;

		info.classname = _tGetClassName(hwnd);

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

	struct SearchListViewCallback {
		std::vector<HWND> listViews;
		std::basic_regex<TCHAR> classReg = std::basic_regex<TCHAR>{
			_T(R"(.*ListView32.*)"),
			std::regex_constants::ECMAScript | std::regex_constants::icase };
	};

	BOOL CALLBACK detectKeePassListView(HWND hwnd, LPARAM lParam) {
		SearchListViewCallback* cb = reinterpret_cast<SearchListViewCallback*>(lParam);

		_tstring className = _tGetClassName(hwnd);
		bool isMatch = std::regex_match(className, cb->classReg);
		if (isMatch) {
			cb->listViews.push_back(hwnd);
		}

		//OutputDebugStringW((std::wstringstream{}
		//	<< (isMatch ? "!" : "")
		//	<< "[" << reinterpret_cast<unsigned long long>(hwnd) << "] "
		//	<< "<" << reinterpret_cast<unsigned long long>(GetParent(hwnd)) << "> "
		//	<< className << "\n").str().c_str());
		return TRUE;
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

	SearchWindowCallback windowSearcher{
		[&keePassPath, &keePassName, &window](WndInfo& wi) {
			bool procFilenameMatch = 0 == _tcsicmp(getFilename(wi.process).c_str(), keePassName.c_str());

			if (procFilenameMatch) {
				// there will be several top-level windows (message only, tooltips, etc.)
				bool isVisible = IsWindowVisible(wi.hwnd) != FALSE;
				bool noOwner = GetWindow(wi.hwnd, GW_OWNER) == 0;
				bool procPathMatch = 0 == _tcsicmp(wi.process.c_str(), keePassPath.c_str());

				if (isVisible && noOwner) {
					// most likely the process' main window

					window = wi;

					if (procPathMatch) return false; // and really the real executable. Stop searching
				}
			}
			return true; // continue search
		}
	};
	EnumWindows(&detectKeePassMainWindow, reinterpret_cast<LPARAM>(&windowSearcher));

	if (window.hwnd == 0) {
		m_result = Result::WindowNotFound;
		return;
	}

	SearchListViewCallback cb{};
	EnumChildWindows(window.hwnd, &detectKeePassListView, reinterpret_cast<LPARAM>(&cb));
	if (cb.listViews.empty()) {
		m_result = Result::ListViewNotFound;
		return;
	}

	// check that each detected listview (which should only be one), has at least one selected item
	bool allListViewsHaveSelectedItems = true;
	for (HWND hListView : cb.listViews) {
		LRESULT res = SendMessage(hListView, LVM_GETNEXTITEM, -1, LVNI_FOCUSED | LVNI_SELECTED);
		if (res < 0) {
			allListViewsHaveSelectedItems = false;
			break;
		}
	}

	if (!allListViewsHaveSelectedItems) {
		m_result = Result::NoSelection;
		return;
	}

	// all tests succeeded!
	m_result = Result::FoundOk;
}
