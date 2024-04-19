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

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN

#include "SimpleLog/SimpleLog.hpp"

#include "MainWindow.h"
#include "NotifyIcon.h"
#include "Menu.h"

#include <Windows.h>
#include <shellapi.h>

// https://github.com/microsoft/Windows-classic-samples/blob/main/Samples/Win7Samples/winui/shell/appshellintegration/NotificationIcon/NotificationIcon.cpp
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#pragma comment(lib, "comctl32.lib")

int APIENTRY wWinMain(HINSTANCE hInstance, HINSTANCE /*hPrevInstance*/, PWSTR lpCmdLine, int /*nShowCmd*/)
{
	int retval = 1;

	sgrottel::SimpleLog log;
	sgrottel::SimpleLog::Write(log, "GlobalHotKeys started.");

	MainWindow wnd{ hInstance, log };
	if (wnd.GetHandle() != NULL)
	{
		NotifyIcon notifyIcon{ log, wnd };
		Menu menu{ log };

		wnd.SetNotifyCallback(
			[&wnd, &menu, &notifyIcon]()
			{
				POINT p = notifyIcon.GetPoint();
				menu.Popup(wnd, p);
			});
		wnd.SetMenuItemCallback(
			[&menu](WORD menuItemID)
			{
				menu.Call(menuItemID);
			});

		retval = wnd.RunMainLoop();

		wnd.SetNotifyCallback({});
		wnd.SetMenuItemCallback({});
	}

	sgrottel::SimpleLog::Write(log, "GlobalHotKeys exit: %d", retval);

	return retval;
}
