// GlobalHotKeys.cpp
// GlobalHotKeys, Tiny Tools Collection
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

#include "pch.h"

#include "SimpleLog/SimpleLog.hpp"

#include "MainWindow.h"
#include "NotifyIcon.h"
#include "Menu.h"
#include "SingleInstanceGuard.h"

#include <shellapi.h>

// https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/Win7Samples/winui/shell/appshellintegration/NotificationIcon/NotificationIcon.cpp
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#pragma comment(lib, "comctl32.lib")

namespace
{
	class CoGuard {
	public:
		CoGuard() : m_inited{ false }
		{
			m_inited = SUCCEEDED(CoInitialize(NULL));
		}
		~CoGuard()
		{
			if (m_inited)
			{
				CoUninitialize();
				m_inited = false;
			}
		}
		operator bool() const
		{
			return m_inited;
		}
	private:
		bool m_inited;
	};
}

int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE /*hPrevInstance*/, PWSTR lpCmdLine, int /*nShowCmd*/)
{
	int retval = 1;

	sgrottel::SimpleLog log;
	sgrottel::SimpleLog::Write(log, "GlobalHotKeys started.");

	SingleInstanceGuard singleInstanceGuard;
	if (!singleInstanceGuard.IsSingleInstance())
	{
		sgrottel::SimpleLog::Error(log, "Another instance of GlobalHotKeys already running.");
		return 0;
	}

	CoGuard coGuard;
	if (!coGuard)
	{
		sgrottel::SimpleLog::Warning(log, "CoInitialize failed. Some functions might not work.");
	}

	MainWindow wnd{ hInstance, log };
	if (wnd.GetHandle() != NULL)
	{
		NotifyIcon notifyIcon{ log, wnd };
		Menu menu{ log, wnd.GetHInstance() };

		menu.SetOnSelectConfigCallback(
			[]()
			{
				MessageBox(NULL, L"Not implemented", MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
			});

		wnd.SetNotifyCallback(
			[&wnd, &menu, &notifyIcon]()
			{
				POINT p = notifyIcon.GetPoint();
				menu.Popup(wnd, p);
			});
		wnd.SetMenuItemCallback(std::bind(&Menu::Call, &menu, std::placeholders::_1));

		retval = wnd.RunMainLoop();

		wnd.SetNotifyCallback({});
		wnd.SetMenuItemCallback({});
	}

	sgrottel::SimpleLog::Write(log, "GlobalHotKeys exit: %d", retval);

	return retval;
}
