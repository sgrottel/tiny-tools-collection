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

#include "TraceFile.h"

#include <stdexcept>
#include <regex>

#include <shellapi.h>
#include <Objbase.h>
#include <shlwapi.h>
#include <Winerror.h>

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

	// https://docs.microsoft.com/en-us/windows/win32/api/securitybaseapi/nf-securitybaseapi-checktokenmembership
	bool isUserAdmin()
		/*++
		Routine Description: This routine returns TRUE if the caller's
		process is a member of the Administrators local group. Caller is NOT
		expected to be impersonating anyone and is expected to be able to
		open its own process and process token.
		Arguments: None.
		Return Value:
		   TRUE - Caller has Administrators local group.
		   FALSE - Caller does not have Administrators local group. --
		*/
	{
		BOOL b;
		SID_IDENTIFIER_AUTHORITY ntAuthority = SECURITY_NT_AUTHORITY;
		PSID administratorsGroup;
		b = AllocateAndInitializeSid(
			&ntAuthority,
			2,
			SECURITY_BUILTIN_DOMAIN_RID,
			DOMAIN_ALIAS_RID_ADMINS,
			0, 0, 0, 0, 0, 0,
			&administratorsGroup);
		if (b) {
			if (!CheckTokenMembership(NULL, administratorsGroup, &b)) {
				b = false;
			}
			FreeSid(administratorsGroup);
		}

		return b != false;
	}

}

void Config::init(const TCHAR* cmdLine) {
	m_continue = true;

	loadFromRegistry();

	if (cmdLine[0] == 0) {
		// empty command line
		if (!fileExists(m_kdbxFile)) {
			throw std::runtime_error(toUtf8((_tstring{ _T("Unable to open file:\n\"") } + m_kdbxFile + _T("\"")).c_str()));
		}

		if (m_keePassExe.empty()) {
			tryFindKeePassExe();
		}

		if (!fileExists(m_keePassExe)) {
			throw std::runtime_error(toUtf8((_tstring{ _T("KeePass not found or not accessible:\n\"") } + m_keePassExe + _T("\"")).c_str()));
		}

		m_continue = true;
		return;
	}

	// parse non-empty command line
	bool isMatch = false;
	std::match_results<const TCHAR*> result;

#pragma region -help
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
#pragma endregion

#pragma region -config <file> <exe> 
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

		writeToRegistry(cmdLine);
		MessageBox(NULL,
			(_tstringstream{} << _T("Wrote config to Windows Registry.\nFile: ")
				<< m_kdbxFile << _T("\nKeePass: ")
				<< m_keePassExe).str().c_str(),
			k_caption,
			MB_OK | MB_ICONINFORMATION);

		m_continue = false;
		return;
	}
#pragma endregion

#pragma region -startsound (off|on)
	isMatch = std::regex_match(
		cmdLine,
		result,
		std::basic_regex<TCHAR>{
		_T(R"(\s*(?:-{1,2}|/)startsound\s+(\S+).*)"),
			std::regex_constants::ECMAScript | std::regex_constants::icase}
	);
	if (isMatch) {
		_tstring flag = result[1].str();
		if (_tcsicmp(flag.c_str(), _T("on")) == 0) {
			m_playStartSound = true;
		}
		else if (_tcsicmp(flag.c_str(), _T("off")) == 0) {
			m_playStartSound = false;
		}
		else {
			throw std::runtime_error(toUtf8((_tstring{ _T("Invalid argument to configure startsound: ") } + flag).c_str()));
		}

		writeToRegistry(cmdLine);
		MessageBox(NULL,
			(_tstringstream{} << _T("Wrote startsound configuration to Windows Registry: ")
				<< (m_playStartSound ? _T("on") : _T("off"))).str().c_str(),
			k_caption,
			MB_OK | MB_ICONINFORMATION);

		m_continue = false;
		return;
	}
#pragma endregion

	throw std::runtime_error(toUtf8((_tstring{ _T("Unexpected command line:\n\"") } + cmdLine + _T("\"")).c_str()));
}

void Config::showHelp() {
	MessageBox(NULL, _T(R"(Syntax:

KeePassHotKey.exe (-help | -config <file> <exe> | -startsound (off|on) )

Use '-help' to show this text.

Use '-config' to configure the kdbx file to open and (optionally) the KeePass executable.

Use '-startsound' to configure the tool to play or not to play a sound at normal start.

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
#pragma warning(suppress: 4311 4302)
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

void Config::loadFromRegistry() {
	// load file and keepass from config
	TCHAR file[MAX_PATH + 1];
	DWORD fileLen = MAX_PATH;

	LSTATUS rr = RegGetValue(HKEY_CURRENT_USER, REGKEY_APP_KEYNAME, _T("tracefile"), RRF_RT_REG_SZ, NULL, file, &fileLen);
	if (rr == ERROR_SUCCESS) {
		if (fileLen > MAX_PATH) fileLen = MAX_PATH;
		file[fileLen] = 0;
		TraceFile::Instance().setFile(file);
	}

	rr = RegGetValue(HKEY_CURRENT_USER, REGKEY_APP_KEYNAME, _T("kdbx"), RRF_RT_REG_SZ, NULL, file, &fileLen);
	if (rr == ERROR_SUCCESS) {
		if (fileLen > MAX_PATH) fileLen = MAX_PATH;
		file[fileLen] = 0;
		m_kdbxFile = file;
	}

	TCHAR exe[MAX_PATH + 1];
	DWORD exeLen = MAX_PATH;

	rr = RegGetValue(HKEY_CURRENT_USER, REGKEY_APP_KEYNAME, _T("keepass"), RRF_RT_REG_SZ, NULL, exe, &exeLen);
	if (rr == ERROR_SUCCESS) {
		if (exeLen > MAX_PATH) exeLen = MAX_PATH;
		exe[exeLen] = 0;
		m_keePassExe = exe;
	}

	DWORD dw = 0;
	DWORD dws = sizeof(DWORD);
	rr = RegGetValue(HKEY_CURRENT_USER, REGKEY_APP_KEYNAME, _T("confirmautotype"), RRF_RT_REG_DWORD, NULL, &dw, &dws);
	if (rr == ERROR_SUCCESS) {
		m_needConfirmationForAutoType = dw != 0;
	}

	dw = 0;
	dws = sizeof(DWORD);
	rr = RegGetValue(HKEY_CURRENT_USER, REGKEY_APP_KEYNAME, _T("startsound"), RRF_RT_REG_DWORD, NULL, &dw, &dws);
	if (rr == ERROR_SUCCESS) {
		m_playStartSound = dw != 0;
	}
}

void Config::writeToRegistry(const TCHAR* cmdLine)
{
	_tstringstream error;
	HKEY appKey;
	bool accessDenied = false;
	LSTATUS rr = RegCreateKeyEx(
		HKEY_CURRENT_USER,
		REGKEY_APP_KEYNAME,
		0, NULL, 0, KEY_WRITE, NULL, &appKey, NULL);
	if (rr == ERROR_SUCCESS) {

		rr = RegSetValueExW(
			appKey, _T("kdbx"), 0, REG_SZ,
			reinterpret_cast<const BYTE*>(m_kdbxFile.c_str()),
			static_cast<DWORD>((m_kdbxFile.size() + 1) * sizeof(TCHAR)));
		if (rr != ERROR_SUCCESS) {
			error << _T("\nFailed to store kdbx file value: ") << rr;
			accessDenied |= (rr == ERROR_ACCESS_DENIED);
		}

		rr = RegSetValueExW(
			appKey, _T("keepass"), 0, REG_SZ,
			reinterpret_cast<const BYTE*>(m_keePassExe.c_str()),
			static_cast<DWORD>((m_keePassExe.size() + 1) * sizeof(TCHAR)));
		if (rr != ERROR_SUCCESS) {
			error << _T("\nFailed to store keepass path value: ") << rr;
			accessDenied |= (rr == ERROR_ACCESS_DENIED);
		}

		DWORD dw = m_needConfirmationForAutoType ? 1 : 0;
		rr = RegSetValueExW(
			appKey, _T("confirmautotype"), 0, REG_DWORD,
			reinterpret_cast<const BYTE*>(&dw), sizeof(DWORD));
		if (rr != ERROR_SUCCESS) {
			error << _T("\nFailed to store confirmautotype settings flag: ") << rr;
			accessDenied |= (rr == ERROR_ACCESS_DENIED);
		}

		dw = m_playStartSound ? 1 : 0;
		rr = RegSetValueExW(
			appKey, _T("startsound"), 0, REG_DWORD,
			reinterpret_cast<const BYTE*>(&dw), sizeof(DWORD));
		if (rr != ERROR_SUCCESS) {
			error << _T("\nFailed to store startsound settings flag: ") << rr;
			accessDenied |= (rr == ERROR_ACCESS_DENIED);
		}

		RegCloseKey(appKey);
	}
	else {
		error << _T("\nFailed to open/create registry key: ") << rr;
		accessDenied |= (rr == ERROR_ACCESS_DENIED);
	}

	if (accessDenied) {
		if (!isUserAdmin()) {
			// elevate
			TCHAR path[MAX_PATH + 1];
			DWORD len = GetModuleFileName(NULL, path, MAX_PATH);
			if (len > MAX_PATH) len = MAX_PATH;
			path[len] = 0;

			SHELLEXECUTEINFO sei;
			ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
			sei.cbSize = sizeof(SHELLEXECUTEINFO);

			sei.lpVerb = L"runas"; // elevate as admin
			sei.lpFile = path;
			sei.lpParameters = cmdLine;
			sei.hwnd = NULL;
			sei.nShow = SW_NORMAL;

			BOOL ser = ::ShellExecuteEx(&sei);

			if (ser == TRUE) {
				return;
			}
			else
			{
				error << _T("\nFailed to start application with elevated rights: ") << GetLastError();
			}
		}
	}

	if (error.tellp() != 0) {
		std::string msg{ toUtf8((_tstringstream {} << _T("ERROR: Failed to write to Windows Registry: ") << error.str()
			<< _T("\nFile: ") << m_kdbxFile
			<< _T("\nKeePass : ") << m_keePassExe
			<< _T("\nStartSound: ") << (m_playStartSound ? _T("on") : _T("off"))
			).str().c_str()) };
		throw std::runtime_error(msg.c_str());
	}
}
