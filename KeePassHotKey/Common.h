//
// KeePassHotKey
// Common.h
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
#pragma once

#include <tchar.h>
#include <string>
#include <sstream>
#include <locale>
#include <codecvt>

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <tchar.h>

constexpr const TCHAR* k_caption = _T("KeePass'HotKey");

typedef std::basic_string<TCHAR> _tstring;
typedef std::basic_stringstream<TCHAR> _tstringstream;

inline std::string toUtf8(const TCHAR* str) {
#ifdef _UNICODE
	int len = WideCharToMultiByte(CP_UTF8, 0, str, -1, NULL, 0, NULL, NULL);
	if (len == 0) return "";

	std::string buf(len - 1, ' ');
	WideCharToMultiByte(CP_UTF8, 0, str, -1, buf.data(), static_cast<int>(buf.size()), NULL, NULL);
	return buf;

#else /* _UNICODE */
	return std::string{ str }; // Don't care for locale at this moment.
#endif /* _UNICODE */
}

inline _tstring fromUtf8(const char* strUtf8) {
#ifdef _UNICODE
	int len = MultiByteToWideChar(CP_UTF8, 0, strUtf8, -1, NULL, 0);
	if (len == 0) return _T("");

	_tstring buf(len, _T(' '));
	MultiByteToWideChar(CP_UTF8, 0, strUtf8, -1, buf.data(), len);
	return buf;

#else /* _UNICODE */
	return _tstring{ strUtf8 }; // Don't care for locale at this moment.
#endif /* _UNICODE */
}
