// GlobalHotKeys.cpp
// GlobalHotKeys, Tiny Tools Collection
//
// Copyright 2025 SGrottel
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
#include "Configuration.h"
#include "HotKeyManager.h"
#include "Version.h"
#include "StringUtils.h"

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

	void ShowAboutDlg(HINSTANCE hInstance)
	{

		std::wstring text
			= std::wstring{ MainWindow::c_WindowName }
			+ L"\nVersion: "
			+ std::to_wstring(GLOBALHOTKEYS_VER_MAJOR) + L"."
			+ std::to_wstring(GLOBALHOTKEYS_VER_MINOR) + L"."
			+ std::to_wstring(GLOBALHOTKEYS_VER_PATCH) + L"."
			+ std::to_wstring(GLOBALHOTKEYS_VER_BUILD)
			+ L"  -  Copyright " + ToW(GLOBALHOTKEYS_VER_YEARSTR) + L" SGrottel\n"
			+ L"\n"
			+ ToW(GLOBALHOTKEYS_VER_DESCIPTION)
			+ L"\nTiny Tools Collection  -  https://go.sgrottel.de/tinytools";

		MSGBOXPARAMSW boxParams{ sizeof(MSGBOXPARAMSW) };
		boxParams.hInstance = hInstance;
		boxParams.lpszText = text.c_str();
		boxParams.lpszCaption = MainWindow::c_WindowName;
		boxParams.dwStyle = MB_OK | MB_USERICON | MB_TOPMOST | MB_SETFOREGROUND;
		boxParams.lpszIcon = MAKEINTRESOURCE(100);

		MessageBoxIndirectW(&boxParams);
	}
}

int APIENTRY wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE /*hPrevInstance*/, _In_ PWSTR lpCmdLine, _In_ int /*nShowCmd*/)
{
	int retval = 1;

	sgrottel::SimpleLog log;
	log.Write("GlobalHotKeys started.");

	SingleInstanceGuard singleInstanceGuard;
	if (!singleInstanceGuard.IsSingleInstance())
	{
		log.Error("Another instance of GlobalHotKeys already running.");
		return 0;
	}

	CoGuard coGuard;
	if (!coGuard)
	{
		log.Warning("CoInitialize failed. Some functions might not work.");
	}
	Configuration config{ log };

	MainWindow wnd{ hInstance, log };
	if (wnd.GetHandle() != NULL)
	{
		std::unique_ptr<NotifyIcon> notifyIcon = std::make_unique<NotifyIcon>(log, wnd);
		Menu menu{ log, wnd.GetHInstance() };
		HotKeyManager keys{ log, wnd };
		keys.SetHotKeys(config.GetHotKeys());
		keys.SetBell(config.GetBell());

		menu.SetOnShowAboutCallback([hInstance]() { ShowAboutDlg(hInstance); });

		menu.SetOnEnableAllHotKeysCallback(std::bind(&HotKeyManager::EnableAllHotKeys, &keys));
		menu.SetOnDisableAllHotKeysCallback(std::bind(&HotKeyManager::DisableAllHotKeys, &keys));
		auto configLoadErrorMessageBox = [&config](std::wstring const& error)
			{
				MessageBox(
					NULL,
					(L"Failed to load configuration\n"
						+ config.GetFilePath().wstring()
						+ L"\n"
						+ error).c_str(),
					MainWindow::c_WindowName,
					MB_ICONERROR | MB_OK);
			};
		auto selectConfig = [&log, &config, &configLoadErrorMessageBox, &keys]()
			{
				std::unique_ptr<IFileOpenDialog, std::function<void(IFileOpenDialog*)>> dlg;
				{
					IFileOpenDialog* rawdlg;
					HRESULT result = CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_ALL, IID_IFileOpenDialog, reinterpret_cast<void**>(&rawdlg));
					if (FAILED(result)) {
						std::wstring error{ L"Failed to CoCreateInstance(CLSID_FileOpenDialog): " + std::to_wstring(static_cast<unsigned int>(result)) };
						log.Error(error);
						MessageBox(NULL, error.c_str(), MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
						return;
					}
					dlg = std::move(std::unique_ptr<IFileOpenDialog, std::function<void(IFileOpenDialog*)>>(rawdlg, [](IFileOpenDialog* dlg) { dlg->Release(); }));
				}

				dlg->SetFileName(config.GetFilePath().wstring().c_str());
				dlg->SetTitle((std::wstring{ MainWindow::c_WindowName } + L" Select Configuration...").c_str());
				COMDLG_FILTERSPEC fileTypeFilters[]{
					{ L"YAML Files", L"*.yaml;*.yml" },
					{ L"All Files", L"*.*" }
				};
				dlg->SetFileTypes(ARRAYSIZE(fileTypeFilters), fileTypeFilters);
				dlg->SetOptions(FOS_FORCEFILESYSTEM | FOS_PATHMUSTEXIST | FOS_FILEMUSTEXIST);

				// Show the open file dialog window
				HRESULT res = dlg->Show(NULL);
				if (res == HRESULT_FROM_WIN32(ERROR_CANCELLED))
				{
					// user cancelled
					return;
				}
				if (FAILED(res)) {
					std::wstring error{ L"Failed to Show FileOpenDialog: " + std::to_wstring(static_cast<unsigned int>(res)) };
					log.Error(error);
					MessageBox(NULL, error.c_str(), MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
					return;
				}

				// Retrieve the selected file name
				IShellItem* files;
				res = dlg->GetResult(&files);
				if (FAILED(res)) {
					std::wstring error{ L"Failed to fetch FileOpenDialog result: " + std::to_wstring(static_cast<unsigned int>(res)) };
					log.Error(error);
					MessageBox(NULL, error.c_str(), MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
					return;
				}
				// Get the file path
				PWSTR path;
				res = files->GetDisplayName(SIGDN_FILESYSPATH, &path);
				if (FAILED(res)) {
					files->Release();
					std::wstring error{ L"Failed to fetch FileOpenDialog result path: " + std::to_wstring(static_cast<unsigned int>(res)) };
					log.Error(error);
					MessageBox(NULL, error.c_str(), MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
					return;
				}

				std::filesystem::path p{ path };

				CoTaskMemFree(path);
				files->Release();

				config.SetFilePath(p, configLoadErrorMessageBox);
				keys.SetHotKeys(config.GetHotKeys());
				keys.SetBell(config.GetBell());
			};
		menu.SetOnSelectConfigCallback(selectConfig);
		menu.SetOnReloadConfigCallback(
			[&config, &selectConfig, &configLoadErrorMessageBox, &keys]()
			{
				if (config.GetFilePath().empty()
					|| !std::filesystem::is_regular_file(config.GetFilePath()))
				{
					selectConfig();
					return;
				}

				config.SetFilePath(config.GetFilePath(), configLoadErrorMessageBox);
				keys.SetHotKeys(config.GetHotKeys());
				keys.SetBell(config.GetBell());
			});

		wnd.SetNotifyCallback(
			[&wnd, &menu, &notifyIcon, &config, &keys]()
			{
				menu.SetEnableAllHotkeysEnabled(keys.CanEnableAllHotKeys());
				menu.SetDisableAllHotkeysEnabled(keys.CanDisableAllHotKeys());
				menu.SetReloadConfigurationEnabled(
					!config.GetFilePath().empty()
					&& std::filesystem::is_regular_file(config.GetFilePath()));

				POINT p = notifyIcon->GetPoint();
				menu.Popup(wnd, p);
			});
		wnd.SetMenuItemCallback(std::bind(&Menu::Call, &menu, std::placeholders::_1));
		wnd.SetRefreshNotifyIconCallback(
			[&notifyIcon, &log, &wnd]()
			{
				notifyIcon.reset(); // first dtor
				notifyIcon = std::make_unique<NotifyIcon>(log, wnd); // then ctor
			});

		wnd.SetHotKeyCallback(std::bind(&HotKeyManager::HotKeyTriggered, &keys, std::placeholders::_1));

		retval = wnd.RunMainLoop();

		wnd.SetNotifyCallback({});
		wnd.SetMenuItemCallback({});
	}

	log.Write("GlobalHotKeys exit: %d", retval);

	return retval;
}
