// GlobalHotKeys.cpp
// GlobalHotKeys, Tiny Tools Collection
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

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN

#include "SimpleLog/SimpleLog.hpp"

#include <Windows.h>

sgrottel::SimpleLog g_log;

int WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
	sgrottel::SimpleLog::Write(g_log, "Hello World.");

	// TODO: Implement

	return 0;
}
