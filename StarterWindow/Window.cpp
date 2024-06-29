// Window.cpp
// StarterWindow, Tiny Tools Collection
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

#include "Window.h"


Window::Window(sgrottel::SimpleLog& log, HINSTANCE hInst)
	: m_log{ log }, m_hwnd{ nullptr }
{
	// Register the window class.
	constexpr wchar_t CLASS_NAME[] = L"SGrottelStarterWindow";

	WNDCLASS wc = { };

	wc.lpfnWndProc = WindowProc;
	wc.hInstance = hInst;
	wc.lpszClassName = CLASS_NAME;
	wc.hIcon = reinterpret_cast<HICON>(LoadImageW(hInst, MAKEINTRESOURCEW(100), IMAGE_ICON, LR_DEFAULTSIZE, LR_DEFAULTSIZE, LR_DEFAULTSIZE));

	ATOM c = RegisterClassW(&wc);
	if (c == 0)
	{
		sgrottel::SimpleLog::Error(m_log, "Failed to register window class: %d\n", GetLastError());
		return;
	}

	// Create the window.
	m_hwnd = CreateWindowExW(
		0,
		CLASS_NAME,
		L"StarterWindow",
		WS_BORDER | WS_POPUP | WS_CAPTION | WS_SYSMENU,

		// Size and position
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		800,
		200,

		nullptr,
		nullptr,
		hInst,
		nullptr
	);

	if (m_hwnd == nullptr)
	{
		sgrottel::SimpleLog::Error(m_log, "Failed to create window: %d\n", GetLastError());
		return;
	}

	ShowWindow(m_hwnd, SW_SHOWNORMAL);
}

Window::~Window()
{
	PostQuitMessage(0);
}

void Window::MainLoop()
{
	sgrottel::SimpleLog::Write(m_log, "main loop started");

	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0) > 0)
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	sgrottel::SimpleLog::Write(m_log, "main loop left");
}

LRESULT CALLBACK Window::WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_DESTROY:
		PostQuitMessage(0);
		return 0;

	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hwnd, &ps);

		// All painting occurs here, between BeginPaint and EndPaint.

		FillRect(hdc, &ps.rcPaint, (HBRUSH)(COLOR_WINDOW + 1));

		EndPaint(hwnd, &ps);
	}
	return 0;

	}

	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}
