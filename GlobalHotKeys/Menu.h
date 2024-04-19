#pragma once

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

namespace sgrottel {
	class ISimpleLog;
}
class MainWindow;

class Menu
{
public:
	Menu(sgrottel::ISimpleLog& log);
	~Menu();

	void Popup(const MainWindow& wnd, const POINT& p);

	void Call(WORD menuItemID);

private:
	sgrottel::ISimpleLog& m_log;

	HMENU m_hMenu;
};

