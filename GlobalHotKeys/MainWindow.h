#pragma once

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

#include <functional>

namespace sgrottel {
	class ISimpleLog;
}

class MainWindow
{
public:
	static constexpr const wchar_t* c_WindowName = L"GlobalHotKeys";

	MainWindow(HINSTANCE hInstance, sgrottel::ISimpleLog& log);
	~MainWindow();

	int RunMainLoop();

	inline HINSTANCE GetHInstance() const
	{
		return m_hInstance;
	}
	inline HWND GetHandle() const
	{
		return m_hWnd;
	}

	inline void SetNotifyCallback(std::function<void()> cb)
	{
		m_notifyCallback = std::move(cb);
	}
	inline void SetMenuItemCallback(std::function<void(WORD)> cb)
	{
		m_menuItemCallback = std::move(cb);
	}

private:
	static LRESULT CALLBACK wndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

	HINSTANCE m_hInstance;
	sgrottel::ISimpleLog& m_log;
	HWND m_hWnd;

	std::function<void()> m_notifyCallback;
	std::function<void(WORD)> m_menuItemCallback;
};

