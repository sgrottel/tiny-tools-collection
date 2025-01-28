// StarterWindow.cpp
// StarterWindow, Tiny Tools Collection
//
// Copyright 2024 SGrottel
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissionsand
// limitations under the License.

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

#include "SimpleLog/SimpleLog.hpp"

#include "Version.h"
#include "Window.h"

#include <cstdlib>
#include <memory>

// https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/Win7Samples/winui/shell/appshellintegration/NotificationIcon/NotificationIcon.cpp
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#pragma comment(lib, "comctl32.lib")

namespace
{

	std::wstring mbtow(const char* s)
	{
		size_t slen = strlen(s);
		std::wstring b;
		b.resize(slen + 1, 0);
		size_t blen{ slen };
		errno_t suc = mbstowcs_s(&blen, b.data(), b.size(), s, slen);
		b.resize(blen, 0);
		return b;
	}


	void ErrorMessageBox(const wchar_t* msg)
	{
		::MessageBoxW(nullptr, msg, L"StarterWindow", MB_OK | MB_ICONERROR);
	}

}

int APIENTRY wWinMain(
	_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR lpCmdLine,
	_In_ int nShowCmd)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);
	UNREFERENCED_PARAMETER(nShowCmd);
	int retval = 0;

	std::shared_ptr<sgrottel::ISimpleLog> log;

	try
	{
		SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

		log = std::make_shared<sgrottel::SimpleLog>();
		log->Write("StarterWindow v%d.%d.%d.%d started.", STARTERWINDOW_VER_MAJOR, STARTERWINDOW_VER_MINOR, STARTERWINDOW_VER_PATCH, STARTERWINDOW_VER_BUILD);

		Window wnd{ log, hInstance };

		wnd.MainLoop();

		// throw std::runtime_error("Not implemented");

	}
	catch (std::exception const& ex)
	{
		std::wstring msg{ L"Critical failure: " };
		msg += mbtow(ex.what());
		log->Critical(L"EXCEPTION: %s", ex.what());
		ErrorMessageBox(msg.c_str());
		retval = -1;
	}
	catch (...)
	{
		log->Critical(L"EXCEPTION: unknown");
		ErrorMessageBox(L"Critical failure: unknown");
		retval = -1;
	}

	return retval;
}
