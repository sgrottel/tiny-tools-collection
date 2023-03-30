//
// StartPwsh.cpp
// Main application file
// Open Source under MIT License
//
// Icon file is copyright by Microsoft
//

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <string>

int __stdcall wWinMain(
	HINSTANCE hInstance,
	HINSTANCE hPrevInstance,
	LPWSTR lpCmdLine,
	int nShowCmd
)
{
	std::wstring cmdLine;
	cmdLine += L"pwsh.exe";

	STARTUPINFOW si;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	CreateProcessW(NULL,
		const_cast<wchar_t*>(cmdLine.c_str()),
		NULL, NULL, FALSE, 0, NULL, NULL, & si, & pi);

	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);

	return 0;
}
