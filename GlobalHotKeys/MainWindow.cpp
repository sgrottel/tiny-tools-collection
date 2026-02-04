#include "pch.h"

#include "MainWindow.h"
#include "NotifyIcon.h"
#include "SimpleLog/SimpleLog.hpp"

#include <string_view>
#include <string>
#include <unordered_map>

using namespace std::string_view_literals;

namespace
{
	constexpr const wchar_t* c_MessageOnlyWindowsClass = L"SgrGlobalHotKeysMainWindow";

	// {8E6A03F1-9F62-499F-9516-4BC6A533C2B2}
	constexpr const uint64_t c_initGuid1 = 0x8e6a03f19f62499fu;
	constexpr const uint64_t c_initGuid2 = 0x95164bc6a533c2b2u;

	struct InitObjectRef
	{
		uint64_t initGuid1;
		uint64_t initGuid2;
		MainWindow* that;
	};
}

MainWindow::MainWindow(HINSTANCE hInstance, sgrottel::ISimpleLog& log)
	: m_hInstance{ hInstance }, m_log{ log }, m_hWnd{ 0 }
{
	WNDCLASSEX wcex = { sizeof(wcex) };

	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = wndProc;
	wcex.hInstance = m_hInstance;
	wcex.hIcon = LoadIcon(m_hInstance, MAKEINTRESOURCE(100));
	wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = c_MessageOnlyWindowsClass;

	auto atom = RegisterClassEx(&wcex);
	if (atom == 0)
	{
		m_log.Error("Failed RegisterClassEx returned 0");
		return;
	}

	// https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features?redirectedfrom=MSDN#message-only-windows
	InitObjectRef initObjRef
	{
		c_initGuid1,
		c_initGuid2,
		this
	};
	m_hWnd = CreateWindowEx(
		WS_EX_NOACTIVATE | WS_EX_NOREDIRECTIONBITMAP,
		c_MessageOnlyWindowsClass,
		c_WindowName,
		WS_DISABLED,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		NULL,
		NULL,
		m_hInstance,
		static_cast<void*>(&initObjRef));

	if (m_hWnd == NULL)
	{
		m_log.Error("Failed CreateWindowEx returned NULL");
		return;
	}

	ShowWindow(m_hWnd, SW_HIDE);

	m_msgTaskbarCreated = RegisterWindowMessageW(L"TaskbarCreated");
}

MainWindow::~MainWindow()
{
	if (m_hWnd != NULL)
	{
		DestroyWindow(m_hWnd);
		m_hWnd = NULL;
	}
}

int MainWindow::RunMainLoop()
{
	if (m_hWnd == NULL)
	{
		return 1;
	}

	MSG msg;
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}

LRESULT CALLBACK MainWindow::wndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	if (uMsg == WM_CREATE)
	{
		const LPCREATESTRUCT p = reinterpret_cast<LPCREATESTRUCT>(lParam);
		const InitObjectRef* ior = static_cast<InitObjectRef*>(p->lpCreateParams);

		if (ior->initGuid1 != c_initGuid1 || ior->initGuid2 != c_initGuid2)
		{
			// initialization struct validation failed!
			return -1;
		}

		SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(ior->that));
		return 0;
	}

	MainWindow* that = reinterpret_cast<MainWindow*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));

	switch (uMsg)
	{
	case WM_DESTROY:
		SetWindowLongPtr(hwnd, GWLP_USERDATA, 0);
		return 0;

	case NotifyIcon::c_Message:
	{
		UINT innerMsg = LOWORD(lParam);
		switch (innerMsg)
		{
		case WM_LBUTTONUP:
		case WM_MBUTTONUP:
		case WM_RBUTTONUP:
		case WM_CONTEXTMENU:
			that->m_notifyCallback();
			break;
		}
		return 0;
	}

	case WM_COMMAND:
	{
		WORD menuItemID = LOWORD(wParam);
		that->m_menuItemCallback(menuItemID);
		return 0;
	}

	case WM_HOTKEY:
	{
		that->m_hotKeyCallback(static_cast<uint32_t>(wParam));
		return 0;
	}

	case WM_SETTINGCHANGE:
		if (lParam && std::wstring_view{ reinterpret_cast<const wchar_t*>(lParam) } == L"Environment"sv)
		{
			that->ReloadEnvironment();
		}
		break;
	}

	if (that != nullptr && that->m_msgTaskbarCreated != 0 && uMsg == that->m_msgTaskbarCreated)
	{
		that->m_refreshNotifyIconCallback();
	}

	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

void MainWindow::ReloadEnvironment()
{
	m_log.Write("Reloading environment variables from system settings");

	std::unordered_map<std::wstring, std::wstring> env;

	const struct
	{
		HKEY root;
		const wchar_t* path;
	} roots[] =
	{
		{
			HKEY_LOCAL_MACHINE,
			L"SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment"
		},
		{
			HKEY_CURRENT_USER,
			L"Environment"
		}
	};

	std::vector<wchar_t> nameBuf(1024);
	std::vector<BYTE> valueBuf(1024 * 2);

	for (const auto& r : roots)
	{
		HKEY hKey = nullptr;
		if (RegOpenKeyExW(r.root, r.path, 0, KEY_READ, &hKey) != ERROR_SUCCESS)
			continue;

		DWORD valueCount = 0;
		DWORD maxNameLen = 0;
		DWORD maxValueLen = 0;

		// Query number of keys
		if (RegQueryInfoKeyW(hKey, nullptr, nullptr, nullptr, nullptr, nullptr, nullptr, &valueCount, &maxNameLen, &maxValueLen, nullptr, nullptr) != ERROR_SUCCESS)
		{
			RegCloseKey(hKey);
			continue;
		}

		// Allocate buffers
		nameBuf.resize(maxNameLen + 1);
		valueBuf.resize(maxValueLen + 2);

		for (DWORD i = 0; i < valueCount; ++i)
		{
			DWORD nameLen = maxNameLen + 1;
			DWORD valueLen = maxValueLen + 2;
			DWORD type = 0;

			if (RegEnumValueW(hKey, i, nameBuf.data(), &nameLen, nullptr, &type, valueBuf.data(), &valueLen) != ERROR_SUCCESS)
			{
				continue;
			}
			if (type != REG_SZ && type != REG_EXPAND_SZ)
			{
				continue;
			}

			std::wstring name(nameBuf.data(), nameLen);
			std::wstring nameUC{ name };
			CharUpperBuffW(nameUC.data(), static_cast<DWORD>(nameUC.size()));

			std::wstring value(reinterpret_cast<wchar_t*>(valueBuf.data()), valueLen / sizeof(wchar_t));

			// Remove trailing nulls
			while (!value.empty() && value.back() == L'\0')
			{
				value.pop_back();
			}

			// Expand REG_EXPAND_SZ
			if (type == REG_EXPAND_SZ)
			{
				DWORD needed = ExpandEnvironmentStringsW(value.c_str(), nullptr, 0);
				if (needed > 0)
				{
					std::wstring expanded(needed, L'\0');
					ExpandEnvironmentStringsW(value.c_str(), expanded.data(), needed);
					// Remove trailing null
					if (!expanded.empty() && expanded.back() == L'\0')
					{
						expanded.pop_back();
					}
					value = std::move(expanded);
				}
			}

			if (nameUC == L"USERNAME"sv)
			{
				// do not change the user name
			}
			else if (nameUC == L"PATH"sv)
			{
				// merge path info
				if (env.find(L"Path") == env.end())
				{
					env[L"Path"] = L"";
				}
				auto& path = env[L"Path"];
				if (!path.empty())
				{
					path += L";";
				}
				path += value;
			}
			else
			{
				env[name] = value;
			}

		}

		RegCloseKey(hKey);
	}

	for (auto const& [name, value] : env)
	{
		// Read current process value
		DWORD curSize = GetEnvironmentVariableW(name.c_str(), nullptr, 0);
		std::wstring current;
		if (curSize > 0)
		{
			current.resize(curSize);
			GetEnvironmentVariableW(name.c_str(), current.data(), curSize);
			if (!current.empty() && current.back() == L'\0')
			{
				current.pop_back();
			}
		}

		// Compare and update if different
		if (current != value)
		{
			m_log.Detail(L"Updating %s = %s\n", name.c_str(), value.c_str());
			SetEnvironmentVariableW(name.c_str(), value.c_str());
		}

	}

}
