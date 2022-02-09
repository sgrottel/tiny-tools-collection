//
// KeePassHotKey
// KeePassRunner.cpp
//
// Copyright 2022 SGrottel (https://www.sgrottel.de)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http ://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissionsand
// limitations under the License.
//
#include "KeePassRunner.h"

#include "Common.h"
#include "Config.h"

#include <shellapi.h>

#include <stdexcept>

KeePassRunner::KeePassRunner(const Config& config)
	: m_config{ config }
{
	// intentionally empty
}

void KeePassRunner::OpenKdbx() {
	SHELLEXECUTEINFO sei;
	ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
	sei.cbSize = sizeof(SHELLEXECUTEINFO);

	sei.lpFile = m_config.getKeePassExe().c_str();
	_tstring params{ _T("\"") };
	params += m_config.getKdbxFile();
	params += _T("\"");
	sei.lpParameters = params.c_str();
	sei.hwnd = NULL;
	sei.nShow = SW_NORMAL;

	BOOL ser = ::ShellExecuteEx(&sei);
	if (!ser) {
		throw std::runtime_error("Failed to open kdbx file.");
	}

}

void KeePassRunner::RunAutoTypeSelected() {
	SHELLEXECUTEINFO sei;
	ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
	sei.cbSize = sizeof(SHELLEXECUTEINFO);

	sei.lpFile = m_config.getKeePassExe().c_str();
	sei.lpParameters = _T("-auto-type-selected");
	sei.hwnd = NULL;
	sei.nShow = SW_NORMAL;

	BOOL ser = ::ShellExecuteEx(&sei);
	if (!ser) {
		throw std::runtime_error("Failed to trigger auto-type-selected.");
	}

}
