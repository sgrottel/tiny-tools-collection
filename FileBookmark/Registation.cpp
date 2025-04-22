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
#include "Registation.h"

#include "utility.h"

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

#include <stdexcept>
#include <vector>

namespace
{
	class RegKey
	{
	public:
		~RegKey() {
			Close();
		}

		inline operator HKEY() const {
			return m_hKey;
		}
		inline void Close()
		{
			if (m_hKey != 0)
			{
				RegCloseKey(m_hKey);
				m_hKey = 0;
			}
		}
		inline HKEY* Put() {
			Close();
			return &m_hKey;
		}

		DWORD CreateOrOpen(HKEY parent, const wchar_t* name, DWORD options = 0);

		bool SetValue(const wchar_t* name, const std::wstring& value);

	private:
		HKEY m_hKey{ 0 };
	};

	DWORD RegKey::CreateOrOpen(HKEY parent, const wchar_t* name, DWORD options)
	{
		Close();
		DWORD disposition{ REG_OPENED_EXISTING_KEY };
		LSTATUS stat = RegCreateKeyExW(parent, name, 0, nullptr, options, KEY_ALL_ACCESS, nullptr, &m_hKey, &disposition);
		if (stat != ERROR_SUCCESS) throw std::runtime_error{"Failed to create or open key"};
		return disposition;
	}

	bool RegKey::SetValue(const wchar_t* name, const std::wstring& value)
	{
		if (m_hKey == 0) throw std::logic_error{"RegKey not open"};
		LSTATUS stat = RegSetValueExW(m_hKey, name, 0, REG_SZ, reinterpret_cast<const BYTE*>(value.c_str()), static_cast<DWORD>((value.size() + 1) * sizeof(wchar_t)));
		return stat == ERROR_SUCCESS;
	}

}

bool filebookmark::Registation::HasAccess()
{
	const wchar_t* tryKeyName{ L".bookmark-volatile-try" };
	RegKey keyDotBookmarkVolatileTry;
	try {
		keyDotBookmarkVolatileTry.CreateOrOpen(HKEY_CLASSES_ROOT, tryKeyName, REG_OPTION_VOLATILE);
		RegDeleteKeyW(HKEY_CLASSES_ROOT, tryKeyName);
		return true;
	}
	catch (...) {}
	return false;
}

void filebookmark::Registation::Register()
{
	std::wstring exePath = GetExecutingModuleFilePath();
	std::wstring FileBookmarkFileTypeName{L"FileBookmark.bookmark"};

	{
		RegKey keyDotBookmark;
		keyDotBookmark.CreateOrOpen(HKEY_CLASSES_ROOT, L".bookmark");
		keyDotBookmark.SetValue(nullptr, FileBookmarkFileTypeName);
	}
	{
		RegKey keyBookmarkFile;
		keyBookmarkFile.CreateOrOpen(HKEY_CLASSES_ROOT, FileBookmarkFileTypeName.c_str());
		{
			RegKey keyDefaultIcon;
			keyDefaultIcon.CreateOrOpen(keyBookmarkFile, L"DefaultIcon");
			keyDefaultIcon.SetValue(nullptr, L"\"" + exePath + L"\",-100");
		}
		{
			RegKey keyShell;
			keyShell.CreateOrOpen(keyBookmarkFile, L"shell");
			keyShell.SetValue(nullptr, L"open");
			{
				RegKey keyShellOpen;
				keyShellOpen.CreateOrOpen(keyShell, L"open");
				{
					RegKey keyShellOpenCommand;
					keyShellOpenCommand.CreateOrOpen(keyShellOpen, L"command");
					keyShellOpenCommand.SetValue(nullptr, L"\"" + exePath + L"\" --open \"%1\"");
				}
			}
		}
	}
}

void filebookmark::Registation::Unregister()
{
	LSTATUS stat = RegDeleteTreeW(HKEY_CLASSES_ROOT, L".bookmark");
	if (stat != ERROR_SUCCESS) throw std::runtime_error{"Failed to delete registry key"};
	stat = RegDeleteTreeW(HKEY_CLASSES_ROOT, L"FileBookmark.bookmark");
	if (stat != ERROR_SUCCESS) throw std::runtime_error{"Failed to delete registry key"};
}
