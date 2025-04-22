// FileBookmark
// Copyright 2023, SGrottel
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#include "CmdLineOptions.h"
#include "Registation.h"
#include "CallElevated.h"
#include "Bookmark.h"
#include "DialogWindowPlacer.h"

#define WIN32_LEAN_AND_MEAN
#define VC_EXTRALEAN
#include <Windows.h>

#if defined _WIN64
#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='amd64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#else
#pragma comment(linker, "/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif
#include <Commctrl.h>
#include <commdlg.h>

#include <shellapi.h>
#include <Objbase.h>

#include <stdexcept>
#include <sstream>
#include <filesystem>

namespace
{
	static const wchar_t* appName = L"FileBookmark";
	static HINSTANCE hInstance;
}

void AskForAction();
void RegisterFileType();
void UnregisterFileType();
void MainWithBookmarkFile(std::wstring const& filepath);

int WINAPI wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ PWSTR pCmdLine, _In_ int nCmdShow)
{
	using filebookmark::CmdLineOptions;

	::hInstance = hInstance;

	SetProcessDPIAware();
	CmdLineOptions cmdLine;
	try {
		cmdLine.Parse(pCmdLine);

		switch (cmdLine.GetMode())
		{
		case CmdLineOptions::Mode::None:
			AskForAction();
			break;

		case CmdLineOptions::Mode::RegisterFileType:
			RegisterFileType();
			break;

		case CmdLineOptions::Mode::UnregisterFileType:
			UnregisterFileType();
			break;

		case CmdLineOptions::Mode::OpenBookmark:
			if (cmdLine.GetPath().empty())
			{
				throw std::runtime_error{"Bookmark file not specified"};
			}
			MainWithBookmarkFile(cmdLine.GetPath());
			break;

		case CmdLineOptions::Mode::SetBookmark:
			// no break;
		case CmdLineOptions::Mode::SetBookmarkAndOpen:
			if (cmdLine.GetPath().empty())
			{
				throw std::runtime_error{"File to bookmark not specified"};
			}
			{
				filebookmark::Bookmark bookmark;
				bookmark.Set(cmdLine.GetPath());

				if (bookmark.GetPath().empty())
				{
					throw std::runtime_error{"Failed to set bookmark"};
				}
				if (!std::filesystem::is_regular_file(bookmark.GetPath()))
				{
					throw std::runtime_error{"Failed to set bookmark -- type error"};
				}

				if (cmdLine.GetMode() == CmdLineOptions::Mode::SetBookmarkAndOpen)
				{
					// continue and open in GUI
					MainWithBookmarkFile(bookmark.GetPath());
				}
			}
			break;

		case CmdLineOptions::Mode::OpenDirectory:
			if (cmdLine.GetPath().empty())
			{
				throw std::runtime_error{"Directory not specified"};
			}
			{
				filebookmark::Bookmark bookmark;
				bookmark.OpenDirectory(cmdLine.GetPath());

				if (bookmark.GetPath().empty())
				{
					throw std::runtime_error{"Failed to set bookmark"};
				}
				if (!std::filesystem::is_regular_file(bookmark.GetPath()))
				{
					throw std::runtime_error{"Failed to set bookmark -- type error"};
				}

				MainWithBookmarkFile(bookmark.GetPath());
			}
			break;

		default:
			std::string msg{ "Unsupported `Mode` encountered after parsing the application's command line: " };
			msg += std::to_string(static_cast<int>(cmdLine.GetMode()));
			throw std::logic_error(msg);
		}

	}
	catch (std::exception const& ex)
	{
		std::wstringstream msg;
		msg << L"An unrecoverable error was encountered:\n" << ex.what();

		MessageBoxW(nullptr, msg.str().c_str(), appName, MB_ICONERROR | MB_OK);
	}

	return 0;
}

std::wstring SetBookmarkViaFileDlg()
{
	wchar_t filename[MAX_PATH + 1];
	filename[0] = 0;

	OPENFILENAMEW dlg{ 0 };
	dlg.lStructSize = sizeof(OPENFILENAMEW);
	dlg.hInstance = hInstance;
	dlg.lpstrFilter = L"All Files\0*.*\0\0";
	dlg.lpstrFile = filename;
	dlg.nMaxFile = MAX_PATH;
	dlg.lpstrTitle = L"Set Bookmark On...";
	dlg.Flags = OFN_FILEMUSTEXIST;

	filebookmark::DialogWindowPlacer placer;

	// control on which monitor the dlg opens
	if (GetOpenFileNameW(&dlg))
	{
		filebookmark::Bookmark bookmark;
		bookmark.Set(filename);
		return bookmark.GetPath();
	}

	return L"";
}

std::wstring OpenBookmarkFileDlg()
{
	wchar_t filename[MAX_PATH + 1];
	filename[0] = 0;

	OPENFILENAMEW dlg{ 0 };
	dlg.lStructSize = sizeof(OPENFILENAMEW);
	dlg.hInstance = hInstance;
	dlg.lpstrFilter = L"Bookmark Files\0*.bookmark\0All Files\0*.*\0\0";
	dlg.lpstrFile = filename;
	dlg.nMaxFile = MAX_PATH;
	dlg.lpstrTitle = L"Open Bookmark...";
	dlg.Flags = OFN_FILEMUSTEXIST;

	filebookmark::DialogWindowPlacer placer;

	// control on which monitor the dlg opens
	if (GetOpenFileNameW(&dlg))
	{
		filebookmark::Bookmark bookmark;
		bookmark.Open(filename);
		return bookmark.GetPath();
	}

	return L"";
}

void AskForAction()
{
	int buttonPressed = 0;

	TASKDIALOGCONFIG config = { 0 };

	std::wstring regText{L"Register the \".bookmark\" File Type"};
	std::wstring unregText{L"Unregister the \".bookmark\" File Type"};

	if (!filebookmark::Registation::HasAccess())
	{
		std::wstring warning;
		if (filebookmark::CallElevated::IsElevated())
		{
			warning = L"\nAccess rights seem to be restricted. Operation might fail.";
		}
		else
		{
			warning = L"\nElevated access rights will be requested.";
		}
		regText += warning;
		unregText += warning;
	}

	const TASKDIALOG_BUTTON buttons[] = {
		{ 102, L"Open a \".bookmark\" File..." },
		{ 103, L"Set \".bookmark\" on a File..." },
		{ 100, regText.c_str() },
		{ 101, unregText.c_str() },
	};

	config.cbSize = sizeof(config);
	config.hInstance = hInstance;
	config.dwCommonButtons = TDCBF_CANCEL_BUTTON;
	config.dwFlags = TDF_USE_COMMAND_LINKS | TDF_USE_HICON_MAIN;
	config.pszWindowTitle = appName;
	config.hMainIcon = LoadIconW(hInstance, MAKEINTRESOURCEW(100));
	config.pszMainInstruction = L"File Bookmark Actions";
	config.pszContent = L"You did not open a bookmark file. In this mode, you can register the file type.";
	config.nDefaultButton = IDCANCEL;
	config.pButtons = buttons;
	config.cButtons = ARRAYSIZE(buttons);

	filebookmark::DialogWindowPlacer placer;

	// control on which monitor the dlg opens
	HRESULT res = TaskDialogIndirect(&config, &buttonPressed, nullptr, nullptr);
	if (res != S_OK)
	{
		throw std::runtime_error{"Failed to open UI Dialog: " + std::to_string(static_cast<int>(res))};
	}

	switch (buttonPressed)
	{
	case 100:
		RegisterFileType();
		break;

	case 101:
		UnregisterFileType();
		break;

	case 102:
		{
			std::wstring path = OpenBookmarkFileDlg();
			if (!path.empty())
			{
				MainWithBookmarkFile(path);
			}
		}
		break;

	case 103:
		{
			std::wstring path = SetBookmarkViaFileDlg();
			if (!path.empty())
			{
				MainWithBookmarkFile(path);
			}
		}
		break;

	case IDCANCEL:
		break;

	default:
		throw std::logic_error{"Handling of UI Dialog result not implemented: " + std::to_string(buttonPressed)};
	}
}

void RegisterFileType()
{
	using filebookmark::Registation;
	using filebookmark::CallElevated;
	if (!Registation::HasAccess() && !CallElevated::IsElevated())
	{
		CallElevated c;
		c.ReCallAs(L"--reg");
		return;
	}

	Registation reg;
	reg.Register();

	MessageBoxW(nullptr, L"Successfully registered the file type `.bookmark`.", appName, MB_OK);
}

void UnregisterFileType()
{
	using filebookmark::Registation;
	using filebookmark::CallElevated;
	if (!Registation::HasAccess() && !CallElevated::IsElevated())
	{
		CallElevated c;
		c.ReCallAs(L"--unreg");
		return;
	}

	Registation reg;
	reg.Unregister();

	MessageBoxW(nullptr, L"Successfully unregistered the file type `.bookmark`.", appName, MB_OK);
}

void OpenBookmarkedFile(std::wstring const& file)
{
	CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
	ShellExecuteW(nullptr, L"open", file.c_str(), nullptr, nullptr, SW_SHOWNORMAL);
	CoUninitialize();
}

void MainWithBookmarkFile(std::wstring const& filepath)
{

	std::filesystem::path path{filepath};
	if (path.empty() || !std::filesystem::is_regular_file(path))
	{
		throw std::runtime_error{"Bookmark file error"};
	}

	while (!path.empty() && std::filesystem::is_regular_file(path))
	{
		int buttonPressed = 0;

		std::wstring filename = path.filename().wstring();
		std::wstring context = L"in: " + path.parent_path().wstring();

		filebookmark::Bookmark bookmark;
		bookmark.Open(path);

		std::vector<TASKDIALOG_BUTTON> buttons;
		buttons.reserve(4);

		std::wstring openFileStr, nextFileStr;
		if (!bookmark.GetBookmarkedFile().empty() && std::filesystem::is_regular_file(bookmark.GetBookmarkedFile()))
		{
			openFileStr = L"Open Bookmarked File\n" + bookmark.GetBookmarkedFile().wstring();
			buttons.push_back(TASKDIALOG_BUTTON{ 100, openFileStr.c_str() });
		}
		else
		{
			context += L"\nThe bookmark is invalid and does not reference an file.";
		}

		if (!bookmark.GetNextFile().empty() && std::filesystem::is_regular_file(bookmark.GetNextFile()))
		{
			nextFileStr = L"Bookmark Next File\n" + bookmark.GetNextFile().wstring();
			buttons.push_back(TASKDIALOG_BUTTON{ 101, nextFileStr.c_str() });
		}

		buttons.push_back(TASKDIALOG_BUTTON{ 102, L"Open a \".bookmark\" File..." });
		buttons.push_back(TASKDIALOG_BUTTON{ 103, L"Set \".bookmark\" on a File..." });

		// TODO: Button for Bookmark-Detail-App

		TASKDIALOGCONFIG config = { 0 };
		config.cbSize = sizeof(config);
		config.hInstance = hInstance;
		config.dwCommonButtons = TDCBF_CANCEL_BUTTON;
		config.dwFlags = TDF_USE_COMMAND_LINKS | TDF_USE_HICON_MAIN;
		config.pszWindowTitle = appName;
		config.hMainIcon = LoadIconW(hInstance, MAKEINTRESOURCEW(100));
		config.pszMainInstruction = filename.c_str();
		config.pszContent = context.c_str();
		config.nDefaultButton = IDCANCEL;
		config.pButtons = buttons.data();
		config.cButtons = static_cast<unsigned int>(buttons.size());
		config.cxWidth = 0;

		filebookmark::DialogWindowPlacer placer;

		// control on which monitor the dlg opens
		HRESULT res = TaskDialogIndirect(&config, &buttonPressed, nullptr, nullptr);

		switch (buttonPressed)
		{
		case 100:
			OpenBookmarkedFile(bookmark.GetBookmarkedFile());
			path.clear();
			break;

		case 101:
			path = bookmark.GetNextFile();
			bookmark.Set(path);
			path = bookmark.GetPath();
			break;

		case 102:
			path = OpenBookmarkFileDlg();
			break;

		case 103:
			path = SetBookmarkViaFileDlg();
			break;

		case IDCANCEL:
			path.clear();
			break;

		default:
			throw std::logic_error{"Handling of UI Dialog result not implemented: " + std::to_string(buttonPressed)};
		}
	}
}
