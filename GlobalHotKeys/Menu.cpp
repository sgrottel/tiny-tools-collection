#include "Menu.h"

#include "MainWindow.h"

#include <shlobj_core.h>
#include "SimpleLog/SimpleLog.hpp"

Menu::Menu(sgrottel::ISimpleLog& log)
	: m_log{ log }
{
	m_hMenu = CreatePopupMenu();
	if (m_hMenu == NULL)
	{
		sgrottel::SimpleLog::Error(log, "Failed CreatePopupMenu returned NULL");
		return;
	}

	InsertMenu(m_hMenu, 0, MF_BYPOSITION | MF_STRING, 8, L"Play");
	InsertMenu(m_hMenu, 1, MF_BYPOSITION | MF_STRING, 7, L"Exit");

}

Menu::~Menu()
{
	if (m_hMenu != NULL)
	{
		DestroyMenu(m_hMenu);
		m_hMenu = NULL;
	}
}

void Menu::Popup(const MainWindow& wnd, const POINT& p)
{
	if (m_hMenu == NULL)
	{
		return;
	}

	SetForegroundWindow(wnd.GetHandle());
	TrackPopupMenuEx(m_hMenu, TPM_RIGHTBUTTON, p.x, p.y, wnd.GetHandle(), NULL);
}

void Menu::Call(WORD menuItemID)
{
	PostQuitMessage(0);
}
