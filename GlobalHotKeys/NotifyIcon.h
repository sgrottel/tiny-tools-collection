#pragma once

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

namespace sgrottel {
	class ISimpleLog;
}
class MainWindow;

class NotifyIcon
{
public:
	static constexpr UINT c_Message = WM_APP + 1;

	NotifyIcon(sgrottel::ISimpleLog& log, MainWindow& mainWnd);
	~NotifyIcon();

	POINT GetPoint();

private:
	sgrottel::ISimpleLog& m_log;
	MainWindow& m_mainWnd;
};

