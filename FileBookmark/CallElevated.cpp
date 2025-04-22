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
#include "CallElevated.h"

#include "utility.h"

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

#include <shellapi.h>
#include <Objbase.h>

bool filebookmark::CallElevated::IsElevated()
{
	BOOL retval = FALSE;
	HANDLE hToken = NULL;

	if (OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		TOKEN_ELEVATION Elevation;
		DWORD cbSize = sizeof(TOKEN_ELEVATION);
		if (GetTokenInformation(hToken, TokenElevation, &Elevation, sizeof(Elevation), &cbSize))
		{
			retval = Elevation.TokenIsElevated;
		}
	}
	if (hToken) {
		CloseHandle(hToken);
	}

	return retval != FALSE;
}

void filebookmark::CallElevated::ReCallAs(const wchar_t* arg)
{
	CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
	std::wstring exePath = GetExecutingModuleFilePath();
	ShellExecuteW(nullptr, L"runas", exePath.c_str(), arg, nullptr, SW_SHOWNORMAL);
	CoUninitialize();
}
