#include "pch.h"

#include "AutostartRegistry.h"

#include "MainWindow.h"

#include <SimpleLog/SimpleLog.hpp>

namespace
{
	constexpr const wchar_t* c_keypath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	std::wstring GetExecutablePath()
	{
		std::wstring path;
		DWORD size = MAX_PATH;

		while (true)
		{
			path.resize(size);

			DWORD len = GetModuleFileNameW(nullptr, path.data(), size);

			if (len == 0)
				return L""; // error

			if (len < size)
			{
				path.resize(len);
				return path;
			}

			// Buffer was too small, grow and retry
			size *= 2;
		}
	}
}

AutostartRegistry::AutostartRegistry(sgrottel::ISimpleLog& log)
	: m_log{ log }
{
}

void AutostartRegistry::Register()
{
	std::wstring exe = GetExecutablePath();
	m_log.Write(L"Registering in Startup: %s", exe.c_str());

	const auto stat = RegSetKeyValueW(HKEY_CURRENT_USER, c_keypath, c_name, REG_SZ, exe.c_str(), (exe.size() + 1) * sizeof(wchar_t));
	if (stat == ERROR_SUCCESS)
	{
		std::wstring msg{ L"Application registered for auto Startup: " + exe + L"\nCheck in 'Task Manager' if the application might be disabled" };
		MessageBox(NULL, msg.c_str(), MainWindow::c_WindowName, MB_ICONINFORMATION | MB_OK);
		m_log.Detail("ok");
	}
	else
	{
		std::wstring error{ L"Failed to register for auto Startup: " + exe + L"\nFailure code: " + std::to_wstring(static_cast<unsigned int>(stat)) };
		m_log.Error(error);
		MessageBox(NULL, error.c_str(), MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
	}
}

void AutostartRegistry::Unregister()
{
	m_log.Write("Unregistering from Startup");
	const auto stat = RegDeleteKeyValueW(HKEY_CURRENT_USER, c_keypath, c_name);
	if (stat == ERROR_SUCCESS)
	{
		std::wstring msg{ L"Application removed from auto Startup" };
		MessageBox(NULL, msg.c_str(), MainWindow::c_WindowName, MB_ICONINFORMATION | MB_OK);
		m_log.Write(msg);
	}
	else
	{
		std::wstring error{ L"Failed to removed for auto Startup\nFailure code: " + std::to_wstring(static_cast<unsigned int>(stat)) };
		m_log.Error(error);
		MessageBox(NULL, error.c_str(), MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
	}
}

bool AutostartRegistry::CanUnregister()
{
	HKEY hKey = nullptr;
	if (RegOpenKeyExW(HKEY_CURRENT_USER, c_keypath, 0, KEY_READ, &hKey) != ERROR_SUCCESS)
	{
		return false;
	}

	LSTATUS stat = RegQueryValueExW(hKey, c_name, NULL, NULL, NULL, NULL);

	RegCloseKey(hKey);

	return stat == ERROR_SUCCESS;
}
