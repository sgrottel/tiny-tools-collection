#include "pch.h"

#include "Menu.h"
#include "MainWindow.h"
#include "SimpleLog/SimpleLog.hpp"

#include <shellapi.h>

Menu::Menu(sgrottel::ISimpleLog& log, HINSTANCE hInstance)
	: m_log{ log }
{
	m_hMenu = LoadMenu(hInstance, MAKEINTRESOURCE(101));
	if (m_hMenu == NULL)
	{
		sgrottel::SimpleLog::Error(log, "Failed LoadMenu returned NULL");
		return;
	}

	m_hSubMenu = GetSubMenu(m_hMenu, 0);
	if (m_hSubMenu == NULL)
	{
		sgrottel::SimpleLog::Error(log, "Failed GetSubMenu returned NULL");
		return;
	}

}

Menu::~Menu()
{
	if (m_hSubMenu != NULL)
	{
		DestroyMenu(m_hSubMenu);
		m_hSubMenu = NULL;
	}
	if (m_hMenu != NULL)
	{
		DestroyMenu(m_hMenu);
		m_hMenu = NULL;
	}
}

void Menu::Popup(const MainWindow& wnd, const POINT& p)
{
	if (m_hSubMenu == NULL)
	{
		return;
	}

	SetForegroundWindow(wnd.GetHandle());
	BOOL rv = TrackPopupMenuEx(m_hSubMenu, TPM_RIGHTBUTTON, p.x, p.y, wnd.GetHandle(), NULL);
	if (rv == 0) 
	{
		sgrottel::SimpleLog::Error(m_log, "Failed to TrackPopupMenuEx: %d", static_cast<int>(GetLastError()));
	}
}

void Menu::Call(WORD menuItemID)
{
	switch (static_cast<int>(menuItemID))
	{
	case MI_ENABLE_ALL_HOTKEYS:
		m_onEnableAllHotKeys();
		break;

	case MI_DISABLE_ALL_HOTKEYS:
		m_onDisableAllHotKeys();
		break;

	case MI_SELECT_CONFIG:
		m_onSelectConfig();
		break;

	case MI_RELOAD_CONFIG:
		m_onReloadConfig();
		break;

	case MI_OPEN_LOG:
	{
		std::filesystem::path logFile = dynamic_cast<sgrottel::SimpleLog&>(m_log).GetFilePath();
		if (logFile.empty())
		{
			MessageBox(NULL, L"Failed to identify log file path", MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
			break;
		}

		ShellExecuteW(NULL, L"open", logFile.wstring().c_str(), NULL, NULL, SW_SHOW);
		break;
	}

	case MI_EXPLORE_LOG_DIR:
	{
		std::filesystem::path logFile = dynamic_cast<sgrottel::SimpleLog&>(m_log).GetFilePath();
		if (logFile.empty())
		{
			MessageBox(NULL, L"Failed to identify log file path", MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
			break;
		}
		logFile = std::filesystem::canonical(logFile);

		std::unique_ptr<__unaligned ITEMIDLIST, decltype(&ILFree)> fileIdl{ ILCreateFromPath(logFile.wstring().c_str()), &ILFree };

		if (!fileIdl)
		{
			MessageBox(NULL, L"Failed to access log file path identifier", MainWindow::c_WindowName, MB_ICONERROR | MB_OK);
			break;
		}

		SHOpenFolderAndSelectItems(fileIdl.get(), 0, 0, 0);
		break;
	}

	case MI_EXIT:
		PostQuitMessage(0);
		break;

	case MI_SHOW_ABOUT:
		m_onShowAbout();
		break;

	default:
		sgrottel::SimpleLog::Warning(m_log, "Triggered menu item call of not-implemented id %d", static_cast<int>(menuItemID));
		break;
	}

	SetForegroundWindow(GetTopWindow(NULL));
}

void Menu::SetMenuItemEnabled(int id, bool enabled)
{
	EnableMenuItem(m_hMenu, id, MF_BYCOMMAND | (enabled ? MF_ENABLED : MF_GRAYED));
}
