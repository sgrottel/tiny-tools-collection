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

template<class T>
std::string toUtf8(const T* str) {
	using convert_type = std::codecvt_utf8<T>;
	std::wstring_convert<convert_type, T> converter;
	return converter.to_bytes(str);
}

template<class T>
std::basic_string<T> fromUtf8(const char* strUtf8) {
	using convert_type = std::codecvt_utf8<T>;
	std::wstring_convert<convert_type, T> converter;
	return converter.from_bytes(strUtf8);
}
