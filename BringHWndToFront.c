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

struct WINDOW_TEST
{
	HWND hWnd;
	RECT rect;
	BOOL overlap;
};

static BOOL TestWindowsForOverlap(HWND oHWnd, struct WINDOW_TEST* test)
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
	struct WINDOW_TEST test;

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
		return 0;
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
