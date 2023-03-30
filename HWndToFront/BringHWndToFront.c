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

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <tlhelp32.h>

#include <malloc.h>
#include <stdio.h>

struct WindowOverlapTest
{
	HWND hWnd;
	RECT rect;
	BOOL overlap;
};

static BOOL TestWindowsForOverlap(HWND oHWnd, struct WindowOverlapTest* test)
{
	RECT r;
	if (GetWindowRect(oHWnd, &r))
	{
		if ((r.left <= test->rect.right)
			&& (r.right >= test->rect.left)
			&& (r.top <= test->rect.bottom)
			&& (r.bottom >= test->rect.top))
		{
			test->overlap = TRUE;
		}
	}

	return !test->overlap; // continue
}

static BOOL IsObscured(HWND hWnd)
{
	struct WindowOverlapTest test;

	test.hWnd = hWnd;
	test.overlap = FALSE;

	if (!GetWindowRect(hWnd, &test.rect))
	{
		return TRUE; // unable to determine
	}

	EnumWindows((WNDENUMPROC)&TestWindowsForOverlap, (LPARAM)&test);

	return test.overlap;
}

BOOL IsForegroundHWnd(HWND hWnd)
{
	HWND otherHWnd;
	otherHWnd = GetForegroundWindow();
	return (otherHWnd == hWnd);
}

BOOL BringHWndToFront(HWND hWnd, BOOL okIfUnobscured)
{
	// check if already in front
	if (IsForegroundHWnd(hWnd))
	{
		return TRUE;
	}

	// call legacy bring to front
	SetForegroundWindow(hWnd);
	Sleep(100);
	if (IsForegroundHWnd(hWnd))
	{
		return TRUE;
	}

	SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
	Sleep(100);
	if (IsForegroundHWnd(hWnd))
	{
		return TRUE;
	}

	// check if non-obscured, which is good enough
	if (okIfUnobscured)
	{
		if (!IsObscured(hWnd))
		{
			return TRUE;
		}
	}

	// modern-hacky bring-to-front dance: minimize, restore
	ShowWindow(hWnd, SW_SHOW);
	if (IsForegroundHWnd(hWnd))
	{
		return TRUE;
	}

	BOOL maximized = IsZoomed(hWnd);
	ShowWindow(hWnd, SW_MINIMIZE);
	ShowWindow(hWnd, SW_SHOWMINIMIZED);
	ShowWindow(hWnd, SW_SHOWNORMAL);
	if (maximized)
	{
		ShowWindow(hWnd, SW_SHOWMAXIMIZED);
	}

	return IsForegroundHWnd(hWnd);
}

struct WindowList
{
	HWND hWnd;
	int depth;
	wchar_t title[MAX_PATH + 1];
	struct WindowList* next;
};

static void FreeWindowList(struct WindowList** windows)
{
	struct WindowList* next;
	while (*windows != NULL)
	{
		next = (*windows)->next;
		free(*windows);
		*windows = next;
	}
}

struct PreStartInfo
{
	struct WindowList* windows;
};

static int WindowZ(HWND hWnd)
{
	int z = 0;
	HWND iWnd;
	for (iWnd = hWnd; (iWnd = GetWindow(iWnd, GW_HWNDPREV)) != NULL; ++z);
	return z;
}

struct PreStartInfo* PrepareMainWndDetectionA(const char* executable)
{
	int exeLen;
	wchar_t* newStr;
	int newLen;
	struct PreStartInfo* rv;
	
	exeLen = (int)strlen(executable);
	newLen = MultiByteToWideChar(CP_THREAD_ACP, 0, executable, exeLen, NULL, 0);
	newLen++;
	newStr = (wchar_t*)malloc(newLen * sizeof(wchar_t));
	newLen = MultiByteToWideChar(CP_THREAD_ACP, 0, executable, exeLen, newStr, newLen);
	newStr[newLen] = 0;
	rv = PrepareMainWndDetectionW(newStr);
	free(newStr);

	return rv;
}

struct WindowOfProcessImageNameSearch
{
	const wchar_t* path;
	struct WindowList* windows;
};

static BOOL FindWindowsForProcess(HWND hWnd, struct WindowOfProcessImageNameSearch* search)
{
	DWORD procId;
	HANDLE proc;
	wchar_t moduleName[MAX_PATH + 1];
	DWORD moduleNameLen;

	if (!IsWindowVisible(hWnd))
	{
		// skip invisible windows, as they are not for user interaction
		return TRUE;
	}

	GetWindowThreadProcessId(hWnd, &procId);
	proc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION | PROCESS_VM_READ, FALSE, procId);
	if (proc == NULL)
	{
		return TRUE;
	}

	moduleNameLen = sizeof(moduleName);
	if (QueryFullProcessImageNameW(proc, 0, moduleName, &moduleNameLen))
	{
		moduleName[moduleNameLen] = 0;
		if (_wcsicmp(moduleName, search->path) == 0)
		{
			struct WindowList* w = (struct WindowList*)malloc(sizeof(struct WindowList));
			w->hWnd = hWnd;
			w->depth = WindowZ(hWnd);
			memset(w->title, 0, MAX_PATH + 1 * sizeof(wchar_t));
			GetWindowTextW(hWnd, w->title, MAX_PATH);
			w->next = search->windows;
			search->windows = w;
		}
	}

	CloseHandle(proc);
	return TRUE;
}

#define MAX_PROCESSIDS_SIZE 1024

struct WindowOfProcessIdSearch
{
	DWORD processIds[MAX_PROCESSIDS_SIZE];
	unsigned int processIdsCount;
	HWND hWnd;
};

static BOOL FindWindowsForProcessId(HWND hWnd, struct WindowOfProcessIdSearch* search)
{
	DWORD procId;
	unsigned int i;
	if (!IsWindowVisible(hWnd))
	{
		// skip invisible windows, as they are not for user interaction
		return TRUE;
	}
	GetWindowThreadProcessId(hWnd, &procId);
	for (i = 0; i < search->processIdsCount; ++i)
	{
		if (procId == search->processIds[i])
		{
			search->hWnd = hWnd;
			return FALSE;
		}
	}
	return TRUE;
}

struct PreStartInfo* PrepareMainWndDetectionW(const wchar_t* executable)
{
	struct WindowOfProcessImageNameSearch searchInfo;
	searchInfo.path = executable;
	searchInfo.windows = NULL;

	// enumerate top-level windows
	EnumWindows((WNDENUMPROC)&FindWindowsForProcess, (LPARAM)&searchInfo);

	struct PreStartInfo* retval;
	retval = (struct PreStartInfo*)malloc(sizeof(struct PreStartInfo));
	retval->windows = searchInfo.windows;

	return retval;
}

HWND DetectNewMainWnd(unsigned int processId, struct PreStartInfo* preStart, int timeOutMs)
{
	const int sleepTimeMs = 20;
	int wait = max(timeOutMs, sleepTimeMs) / sleepTimeMs;
	HWND hWnd = 0;
	struct WindowList* w;
	wchar_t name[MAX_PATH + 1];
	int i, z;
	int candidateZ = MAXINT;
	int candidate = 4;
	HANDLE processSnapshot;
	PROCESSENTRY32 pe32;
	BOOLEAN found, parentFound;
	struct WindowOfProcessIdSearch procIdWndSearch;

	procIdWndSearch.processIds[0] = processId;
	procIdWndSearch.processIdsCount = 1;

	pe32.dwSize = sizeof(PROCESSENTRY32);

	for (; wait > 0; --wait)
	{
		Sleep(sleepTimeMs);

		// check for visible main window of new process
		procIdWndSearch.hWnd = NULL;
		EnumWindows((WNDENUMPROC)&FindWindowsForProcessId, (LPARAM)&procIdWndSearch);
		if (procIdWndSearch.hWnd != NULL)
		{
			hWnd = procIdWndSearch.hWnd;
			break;
		}

		// or check for changes in the visible top-level windows of sister processes
		if (preStart != NULL)
		{
			for (w = preStart->windows; w != NULL; w = w->next)
			{
				z = WindowZ(w->hWnd);
				memset(name, 0, MAX_PATH + 1 * sizeof(wchar_t));
				GetWindowTextW(w->hWnd, name, MAX_PATH);

				if (z < w->depth || _wcsicmp(w->title, name) != 0)
				{
					if (z <= candidateZ)
					{
						candidateZ = z;
						if (hWnd == w->hWnd)
						{
							if (--candidate <= 0)
							{
								break;
							}
						}
						else
						{
							hWnd = w->hWnd;
							candidate = 4;
						}
					}
				}
			}
		}

		// stop looking if we found anything
		if (hWnd != NULL)
		{
			break;
		}

		// try to extend search to child processes
		processSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
		if (processSnapshot != INVALID_HANDLE_VALUE)
		{
			if (Process32First(processSnapshot, &pe32))
			{
				do
				{
					found = parentFound = FALSE;
					for (i = 0; i < (int)procIdWndSearch.processIdsCount; ++i)
					{
						if (pe32.th32ProcessID == procIdWndSearch.processIds[i])
						{
							found = TRUE;
							break;
						}
						if (pe32.th32ParentProcessID == procIdWndSearch.processIds[i])
						{
							parentFound = TRUE;
						}
					}
					if (found) continue;
					if (parentFound)
					{
						// depending on the order of the processes in the snapshot we could
						// either add children of children ... in one go, or not.
						// Both is ok, as this is an interative, heuristic check anyway.
						if (procIdWndSearch.processIdsCount < MAX_PROCESSIDS_SIZE)
						{
							procIdWndSearch.processIds[procIdWndSearch.processIdsCount++] = pe32.th32ProcessID;
						}
					}

				} while (Process32Next(processSnapshot, &pe32));
			}
			CloseHandle(processSnapshot);
		}

	}

	if (preStart != NULL)
	{
		FreeWindowList(&(preStart->windows));
		free(preStart);
	}

	return hWnd;
}
