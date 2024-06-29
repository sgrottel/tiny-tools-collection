// Window.h
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

#pragma once

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>
#include "SimpleLog/SimpleLog.hpp"

struct Proxy;

class Window
{
public:
	Window(sgrottel::SimpleLog& log, HINSTANCE hInst);
	~Window();

	void MainLoop();

	inline operator bool() const
	{
		return m_hwnd != nullptr;
	}

private:
	static LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

	sgrottel::SimpleLog& m_log;
	HWND m_hwnd;

};

