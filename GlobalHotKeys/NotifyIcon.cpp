#include "pch.h"

#include "NotifyIcon.h"
#include "MainWindow.h"

#include <shellapi.h>

namespace
{
	// {9610BE00-2A8C-41FF-9D78-B4ECFF5FC164}
	constexpr GUID c_NotifyIconGuid =
	{ 0x9610be00, 0x2a8c, 0x41ff, { 0x9d, 0x78, 0xb4, 0xec, 0xff, 0x5f, 0xc1, 0x64 } };
}

NotifyIcon::NotifyIcon(sgrottel::ISimpleLog& log, MainWindow& mainWnd)
	: m_log{ log }, m_mainWnd{ mainWnd }
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.hWnd = mainWnd.GetHandle();
	nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_GUID | NIF_TIP | NIF_SHOWTIP;
	nid.guidItem = c_NotifyIconGuid;

	nid.uCallbackMessage = c_Message;

	wcscpy_s(nid.szTip, MainWindow::c_WindowName);

	LoadIconMetric(mainWnd.GetHInstance(), MAKEINTRESOURCE(100), LIM_SMALL, &nid.hIcon);

	if (Shell_NotifyIcon(NIM_ADD, &nid) != TRUE)
	{
		sgrottel::SimpleLog::Error(log, "Failed Shell_NotifyIcon NIM_ADD");
	}

	// NOTIFYICON_VERSION_4 is prefered
	nid.uVersion = NOTIFYICON_VERSION_4;
	Shell_NotifyIcon(NIM_SETVERSION, &nid);
}

NotifyIcon::~NotifyIcon()
{
	NOTIFYICONDATA nid = { sizeof(nid) };
	nid.uFlags = NIF_GUID;
	nid.guidItem = c_NotifyIconGuid;
	Shell_NotifyIcon(NIM_DELETE, &nid);
}

POINT NotifyIcon::GetPoint()
{
	NOTIFYICONIDENTIFIER id{ sizeof(NOTIFYICONIDENTIFIER) };
	id.hWnd = NULL;
	id.uID = 0;
	id.guidItem = c_NotifyIconGuid;

	RECT r{};
	Shell_NotifyIconGetRect(&id, &r);

	return { r.left, r.top };
}
