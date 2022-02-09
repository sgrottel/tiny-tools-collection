//
// KeePassHotKey
// ConfirmationDialog.cpp
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
#include "ConfirmationDialog.h"

#include "Config.h"
#include "InstanceControl.h"

#include <Commctrl.h>

#if defined _M_IX86
#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='x86' publicKeyToken='6595b64144ccf1df' language='*'\"")
#elif defined _M_IA64
#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='ia64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#elif defined _M_X64
#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='amd64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#else
#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif

namespace {

	struct CallbackData {
		InstanceControl& m_instanceControl;
	};

	HRESULT CALLBACK dlgCallback(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam, LONG_PTR lpRefData) {
		CallbackData* data = reinterpret_cast<CallbackData*>(lpRefData);
		constexpr const int timeoutSec = 5;

		switch (msg) {

		case TDN_CREATED:
			data->m_instanceControl.clearSignaled();
			{
				LONG_PTR exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
				exStyle |= WS_EX_NOACTIVATE | WS_EX_TOPMOST;
				SetWindowLongPtr(hwnd, GWL_EXSTYLE, exStyle);
			}
			break;

		case TDN_TIMER:

			if (data->m_instanceControl.tryGetSignaled()) {
				PostMessage(hwnd, TDM_CLICK_BUTTON, IDOK, 0);
			}

			PostMessage(hwnd, TDM_SET_PROGRESS_BAR_POS, wParam / (timeoutSec * 10), 0);

			if (wParam >= timeoutSec * 1000) {
				PostMessage(hwnd, TDM_CLICK_BUTTON, IDCANCEL, 0);
			}

			break;
		}

		return S_OK;
	}

}

ConfirmationDialog::ConfirmationDialog(const Config& config, InstanceControl& instanceControl)
	: m_config{ config }, m_instanceControl{ instanceControl }
{
	// intentionally empty
}

bool ConfirmationDialog::confirm(HINSTANCE hinst) {

	CallbackData data{
		m_instanceControl
	};

	TASKDIALOGCONFIG dlg;
	ZeroMemory(&dlg, sizeof(TASKDIALOGCONFIG));
	dlg.cbSize = sizeof(TASKDIALOGCONFIG);
	dlg.dwFlags = TDF_ALLOW_DIALOG_CANCELLATION | TDF_CALLBACK_TIMER | TDF_SHOW_PROGRESS_BAR;
	dlg.hInstance = hinst;
	dlg.dwCommonButtons = TDCBF_OK_BUTTON | TDCBF_CANCEL_BUTTON;
	dlg.pszWindowTitle = L"KeePass'HotKey";
	dlg.pszMainIcon = MAKEINTRESOURCE(100);
	dlg.pszContent = L"Press Hotkey again to confirm.\nDialog will auto-cancel in 5 seconds.";
	dlg.pfCallback = &dlgCallback;
	dlg.lpCallbackData = reinterpret_cast<LONG_PTR>(&data);

	int btn;

	if (TaskDialogIndirect(&dlg, &btn, NULL, NULL) != S_OK) {
		throw std::runtime_error("Failed to open confirmation UI");
	}

	return btn == IDOK;
}
