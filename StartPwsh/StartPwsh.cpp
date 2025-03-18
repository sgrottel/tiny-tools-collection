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
	_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPWSTR lpCmdLine,
	_In_ int nShowCmd
)
{
	std::wstring cmdLine;
	cmdLine += L"pwsh.exe";

	if (lpCmdLine != nullptr && lpCmdLine[0] != 0)
	{
		cmdLine += L" ";
		cmdLine += lpCmdLine;
	}

	STARTUPINFOW si;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	BOOL succ = CreateProcessW(
		NULL, // app; use NULL and specify app as first arg in `cmdline` to trigger search behavior
		const_cast<wchar_t*>(cmdLine.c_str()), // cmdline
		NULL, // procAttr
		NULL, // threadAttr
		FALSE, // inheritHandles
		0, // creationFlags
		NULL, // env
		NULL, // curDir
		&si, // startupInfo
		&pi); // processInfo

	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);

	return 0;
}
