// KeePassHotKey
// Config.cpp
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
#include "Config.h"

#include <stdexcept>
#include <regex>

#include <shellapi.h>
#include <Objbase.h>
#include <shlwapi.h>

namespace {

	_tstring cleanup(const _tstring& str) {
		size_t len = str.size();
		if (len > 1 && str[0] == _T('"') && str[len - 1] == _T('"')) {
			_tstring s{ str.begin() + 1, str.end() - 1 };

			size_t start_pos = 0;
			while ((start_pos = s.find(_T("\"\""), start_pos)) != std::string::npos) {
				s.replace(start_pos, 2 /* len of old substring */, _T("\""));
				start_pos += 1 /* len of new substring */;
			}

			return s;
		}

		return str;
	}

	void makeAbsolutePath(_tstring& inoutStr) {
		size_t tarLen = GetFullPathName(inoutStr.c_str(), 0, NULL, NULL);
		_tstring tarStr(tarLen, _T(' '));
		GetFullPathName(inoutStr.c_str(), static_cast<DWORD>(tarStr.size()), const_cast<TCHAR*>(tarStr.data()), NULL);
		while (tarStr[tarStr.size() - 1] == 0) tarStr.resize(tarStr.size() - 1);
		inoutStr = tarStr;
	}

	bool fileExists(const _tstring& path) {
		HANDLE hFile = CreateFile(path.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
		if (hFile == INVALID_HANDLE_VALUE) return false;
		CloseHandle(hFile);
		return true;
	}

}

void Config::init(const TCHAR* cmdLine) {

	if (cmdLine[0] == 0) {
		// empty command line
		// load file and keepass from config

		// TODO: load from windows registry

		if (!fileExists(m_kdbxFile)) {
			throw std::runtime_error(toUtf8((_tstring{ _T("Unable to open file:\n\"") } + m_kdbxFile + _T("\"")).c_str()));
		}

		if (m_keePassExe.empty()) {
			tryFindKeePassExe();
		}

		if (!fileExists(m_keePassExe)) {
			throw std::runtime_error(toUtf8((_tstring{ _T("KeePass not found or not accessible:\n\"") } + m_keePassExe + _T("\"")).c_str()));
		}

	}
	else
	{
		bool isMatch = false;
		std::match_results<const TCHAR*> result;

		isMatch = std::regex_match(
			cmdLine,
			result,
			std::basic_regex<TCHAR>{
			_T(R"(\s*(?:-{1,2}|/)(?:(?:help)|\?).*)"),
				std::regex_constants::ECMAScript | std::regex_constants::icase}
		);
		if (isMatch) {
			showHelp();
			m_continue = false;
			return;
		}

		isMatch = std::regex_match(
			cmdLine,
			result,
			std::basic_regex<TCHAR>{
			_T(R"(\s*(?:-{1,2}|/)config\s+((?:[^"]\S*)|(?:"(?:(?:"")|[^"])*"))(?:\s+((?:[^"]\S*)|(?:"(?:(?:"")|[^"])*")))?.*)"),
				std::regex_constants::ECMAScript | std::regex_constants::icase}
		);
		if (isMatch) {
			m_kdbxFile = cleanup(result[1].str());
			if (!m_kdbxFile.empty()) {
				makeAbsolutePath(m_kdbxFile);
				if (!fileExists(m_kdbxFile)) {
					throw std::runtime_error(toUtf8((_tstring{ _T("Unable to open file:\n\"") } + m_kdbxFile + _T("\"")).c_str()));
				}
			}
			else
			{
				throw std::runtime_error(toUtf8(_T("You must specify the kdbx file, the file must exist, and the user must have read access permissions.")));
			}

			m_keePassExe = cleanup(result[2].str());
			if (!m_keePassExe.empty()) {
				makeAbsolutePath(m_keePassExe);
			}
			else
			{
				tryFindKeePassExe();
			}

			if (!fileExists(m_keePassExe)) {
				throw std::runtime_error(toUtf8((_tstring{ _T("KeePass not found or not accessible:\n\"") } + m_keePassExe + _T("\"")).c_str()));
			}

			// TODO: elevate if needed

			// TODO: write to windows registry

			MessageBox(NULL,
				(_tstringstream{} << _T("Wrote config to Windows Registry.\nFile: ") << m_kdbxFile << _T("\nKeePass: ") << m_keePassExe).str().c_str(),
				k_caption,
				MB_OK | MB_ICONINFORMATION);
			m_continue = false;
			return;
		}

		throw std::runtime_error(toUtf8((_tstring{ _T("Unexpected command line:\n\"") } + cmdLine + _T("\"")).c_str()));

	}
}

void Config::showHelp() {
	MessageBox(NULL, _T(R"(Syntax:

KeePassHotKey.exe (-help|-config <file> <exe>)

Use '-help' to show this text.

Use '-config' to configure the kdbx file to open and (optionally) the KeePass executable.

Use without any arguments to perform standard operation:
- open the configured kdbx file if no KeePass instance is running, or
- perform auto type of selected KeePass entry
)"), k_caption, MB_OK | MB_ICONQUESTION);
}

void Config::tryFindKeePassExe() {
	constexpr const TCHAR* keePassExeName = _T("KeePass.exe");

	wchar_t outPath[MAX_PATH + 1];
	DWORD outPathLen = MAX_PATH;

	// via file association 
	{
		// (only works if m_kdbxFile exists)
		HINSTANCE exe = FindExecutable(m_kdbxFile.c_str(), NULL, outPath);
		if (reinterpret_cast<DWORD>(exe) > 32) {
			m_keePassExe = outPath;
			CloseHandle(exe);
			return;
		}

		HRESULT hr = AssocQueryString(ASSOCF_INIT_FOR_FILE,
			ASSOCSTR_EXECUTABLE,
			_T(".kdbx"),
			NULL,
			outPath,
			&outPathLen);
		if (hr == S_OK) {
			outPath[outPathLen] = 0;
			m_keePassExe = outPath;
			return;
		}
	}


	// search in path
	{
		DWORD rs = SearchPath(NULL, _T("KeePass"), _T(".exe"), MAX_PATH, outPath, NULL);
		if (rs > 0) {
			outPath[rs] = 0;
			m_keePassExe = outPath;
			return;
		}
	}

	// seems we did not find anything.
}
