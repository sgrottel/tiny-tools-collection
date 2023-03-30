// HWndToFront
// Freely available via Apache License, v2.0; see LICENSE file
//
// Copyright 2022 SGrottel (https://www.sgrottel.de)
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
// See the License for the specific language governing permissions and
// limitations under the License.
//
#include "BringHWndToFront.h"

#include <stdio.h>
#include <stdlib.h>

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <wchar.h>

struct Config
{
	HWND hWnd;

	const wchar_t* cmd;
	const wchar_t* const* args;
	int argsCnt;
	const wchar_t* workingDir;
};

static BOOL parseArg(int argc, wchar_t const* argv[], struct Config* outConfig);

static unsigned int Start(const struct Config* config);

int wmain(int argc, wchar_t const* argv[])
{
	struct Config config;
	HWND otherHWnd;

	// Cmdline parsing
	if (!parseArg(argc, argv, &config))
	{
		// failed to parse; error already printed to stderr
		return -1;
	}

	if (config.cmd != 0)
	{
		struct PreStartInfo* psi = NULL;
		unsigned int procId;

		psi = PrepareMainWndDetectionW(config.cmd);
		if (psi == NULL)
		{
			fwprintf(stderr, L"Pre-start preparation for main window detection failed.");
			return -1;
		}

		procId = Start(&config);
		if (procId == 0)
		{
			fwprintf(stderr, L"Failed to start process.");
			return -1;
		}

		config.hWnd = DetectNewMainWnd(procId, psi, 5000);
		if (config.hWnd == 0)
		{
			fwprintf(stderr, L"New main window detection failed.");
			return -1;
		}
	}

	// validated `hWnd` is a valid handle to a top-level window
	if (!IsWindow(config.hWnd))
	{
		fwprintf(stderr, L"Specified window handle 0x%.8x is not a valid window handle.", (unsigned)((uintptr_t)config.hWnd));
		return -1;
	}
	otherHWnd = GetParent(config.hWnd);
	if (otherHWnd != NULL)
	{
		fwprintf(stderr, L"Specified window handle 0x%.8x is not a top-level window (it has a parent window).", (unsigned)((uintptr_t)config.hWnd));
		return -1;
	}

	if (BringHWndToFront(config.hWnd, TRUE))
	{
		return 0;
	}

	return -1;
}

BOOL parseArg(int argc, wchar_t const* argv[], struct Config* outConfig)
{
	if (argc < 2)
	{
		fwprintf(stderr, L"You need to specify the window handle to bring to front\n");
		return FALSE;
	}
	if (outConfig == NULL)
	{
		fwprintf(stderr, L"Out variable not initialized\n");
		return FALSE;
	}
	memset(outConfig, 0, sizeof(struct Config));

	if (_wcsicmp(argv[1], L"start") == 0)
	{
		// parse arguments as start codes
		if (argc < 3)
		{
			fwprintf(stderr, L"Executable name missing\n");
			return FALSE;
		}

		if (_wcsicmp(argv[2], L"in") == 0)
		{
			if (argc < 5)
			{
				fwprintf(stderr, L"Executable name missing\n");
				return FALSE;
			}
			outConfig->workingDir = argv[3];
			argv += 2;
			argc -= 2;
		}

		outConfig->cmd = argv[2];

		outConfig->args = argv + 3;
		outConfig->argsCnt = argc - 3;

	}
	else
	{
		// parse first argument as window handle
		if (argv[1][0] == L'x')
		{
			// parse string as hex
			int hWndValue;
			int rc = swscanf_s(argv[1], L"x%x", &hWndValue);
			outConfig->hWnd = (HWND)((intptr_t)hWndValue);
			if (rc != 1)
			{
				fwprintf(stderr, L"Input argument not parsable as hex number\n");
				return FALSE;
			}
		}
		else
		{
			// parse string as decimal
			int hWndValue;
			int rc = swscanf_s(argv[1], L"%d", &hWndValue);
			outConfig->hWnd = (HWND)((intptr_t)hWndValue);
			if (rc != 1)
			{
				fwprintf(stderr, L"Input argument `0` (invalid) or not parsable as number\n");
				return FALSE;
			}
		}
	}

	return TRUE;
}

wchar_t* BuildCmdLine(const struct Config* config)
{
	const wchar_t* exeStr = L"dummy";
	size_t exeLen = 5;
	size_t len;
	size_t fullLen;
	size_t j;
	int i;
	int cntEsc;
	BOOL hasSpace;
	wchar_t c;
	wchar_t* str;
	wchar_t* pos;

	// extract cmd file name
	if (config->cmd == NULL)
	{
		return NULL;
	}
	len = wcslen(config->cmd);
	exeStr = config->cmd;

	hasSpace = FALSE;
	cntEsc = 0;

	for (j = 0; j < len - 1; ++j)
	{
		c = config->cmd[j];
		if (c == L'\\' || c == L'/')
		{
			exeStr = config->cmd + j + 1;
			exeLen = len - j - 1;
			hasSpace = FALSE;
			cntEsc = 0;
		}
		else if (iswspace(c))
		{
			hasSpace = TRUE;
		}
		else if (c == L'"')
		{
			hasSpace = TRUE;
			cntEsc++;
		}
	}

	fullLen = exeLen;
	if (hasSpace)
	{
		fullLen += 2 + cntEsc;
	}

	// calc length of argument list
	for (i = 0; i < config->argsCnt; i++)
	{
		len = wcslen(config->args[i]);

		hasSpace = FALSE;
		cntEsc = 0;

		for (j = 0; j < len; ++j)
		{
			c = config->args[i][j];
			if (iswspace(c))
			{
				hasSpace = TRUE;
			}
			else if (c == L'"')
			{
				hasSpace = TRUE;
				cntEsc++;
			}
		}

		fullLen += 1 + len;
		if (hasSpace)
		{
			fullLen += 2 + cntEsc;
		}

	}

	fullLen++;
	str = (wchar_t*)malloc(fullLen * sizeof(wchar_t));
	if (str == NULL)
	{
		return NULL;
	}
	memset(str, 0, fullLen * sizeof(wchar_t));
	pos = str;

	hasSpace = FALSE;
	for (j = 0; j < exeLen; ++j)
	{
		if (iswspace(exeStr[j]))
		{
			hasSpace = TRUE;
			break;
		}
	}

	if (hasSpace)
	{
		*pos = L'"';
		pos++;
	}
	for (j = 0; j < exeLen; ++j)
	{
		if (exeStr[j] == L'"')
		{
			*pos = L'\\';
			pos++;
		}
		*pos = exeStr[j];
		pos++;
	}
	if (hasSpace)
	{
		*pos = L'"';
		pos++;
	}

	for (i = 0; i < config->argsCnt; i++)
	{
		len = wcslen(config->args[i]);
		hasSpace = FALSE;
		for (j = 0; j < len; ++j)
		{
			if (iswspace(config->args[i][j]))
			{
				hasSpace = TRUE;
				break;
			}
		}

		*pos = L' ';
		pos++;
		if (hasSpace)
		{
			*pos = L'"';
			pos++;
		}
		for (j = 0; j < len; ++j)
		{
			if (config->args[i][j] == L'"')
			{
				*pos = L'\\';
				pos++;
			}
			*pos = config->args[i][j];
			pos++;
		}
		if (hasSpace)
		{
			*pos = L'"';
			pos++;
		}

	}

	return str;
}

unsigned int Start(const struct Config* config)
{
	STARTUPINFOW si;
	PROCESS_INFORMATION pi;
	wchar_t* cmdLine = NULL;

	if (config == NULL)
	{
		fwprintf(stderr, L"Config variable not initialized\n");
		return 0;
	}

	ZeroMemory(&si, sizeof(STARTUPINFOW));
	si.cb = sizeof(STARTUPINFOW);

	ZeroMemory(&pi, sizeof(PROCESS_INFORMATION));

	cmdLine = BuildCmdLine(config);

	BOOL cpr = CreateProcessW(
		config->cmd,
		cmdLine,
		NULL, NULL, FALSE,
		CREATE_DEFAULT_ERROR_MODE | CREATE_NEW_PROCESS_GROUP,
		NULL,
		config->workingDir,
		&si, &pi);

	if (cmdLine)
	{
		free(cmdLine);
		cmdLine = NULL;
	}

	if (cpr == FALSE)
	{
		DWORD le = GetLastError();
		fwprintf(stderr, L"Failed to create Process: %d\n", le);
		return 0;
	}

	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);

	return pi.dwProcessId;
}
