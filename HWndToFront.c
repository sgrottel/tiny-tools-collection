// HWndToFront
// Freely available via Apache License, v2.0; see LICENSE file
//
#include <stdio.h>
#include <stdlib.h>

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>


BOOL parseArg(int argc, char const* argv[], HWND* outHWnd)
{
	if (argc < 2) {
		fprintf(stderr, "You need to specify the window handle to bring to front\n");
		return FALSE;
	}
	if (outHWnd == NULL)
	{
		fprintf(stderr, "Out variable not initialized\n");
		return FALSE;
	}

	if (argv[1][0] == 'x') {
		// parse string as hex
		int hWndValue;
		int rc = sscanf_s(argv[1], "x%x", &hWndValue);
		*outHWnd = (HWND)((intptr_t)hWndValue);
		if (rc != 1) {
			fprintf(stderr, "Input argument not parsable as hex number\n");
			return FALSE;
		}
	}
	else {
		// parse string as decimal
		*outHWnd = (HWND)((intptr_t)atoi(argv[1]));
		if (*outHWnd == 0) {
			fprintf(stderr, "Input argument `0` (invalid) or not parsable as number\n");
			return FALSE;
		}
	}

	return TRUE;
}


struct WINDOW_TEST {
	HWND hWnd;
	RECT rect;
	BOOL overlap;
};


BOOL TestWindowsForOverlap(HWND oHWnd, struct WINDOW_TEST* test)
{
	RECT r;
	if (GetWindowRect(oHWnd, &r)) {
		if ((r.left <= test->rect.right)
			&& (r.right >= test->rect.left)
			&& (r.top <= test->rect.bottom)
			&& (r.bottom >= test->rect.top)) {
			test->overlap = TRUE;
		}
	}

	return !test->overlap; // continue
}


BOOL IsObscured(HWND hWnd)
{
	struct WINDOW_TEST test;

	test.hWnd = hWnd;
	test.overlap = FALSE;

	if (!GetWindowRect(hWnd, &test.rect)) {
		return TRUE; // unable to determine
	}

	EnumWindows((WNDENUMPROC)&TestWindowsForOverlap, (LPARAM)&test);

	return test.overlap;
}


int main(int argc, char const* argv[])
{
	HWND hWnd, otherHWnd;

	// Cmdline parsing
	if (!parseArg(argc, argv, &hWnd)) {
		// failed to parse; error already printed to stderr
		return -1;
	}

	// validated `hWnd` is a valid handle to a top-level window
	if (!IsWindow(hWnd)) {
		fprintf(stderr, "Specified window handle 0x%.8x is not a valid window handle.", (unsigned)((uintptr_t)hWnd));
		return -1;
	}
	otherHWnd = GetParent(hWnd);
	if (otherHWnd != NULL) {
		fprintf(stderr, "Specified window handle 0x%.8x is not a top-level window (it has a parent window).", (unsigned)((uintptr_t)hWnd));
		return -1;
	}

	// check if already in front
	otherHWnd = GetForegroundWindow();
	if (otherHWnd == hWnd) {
		return 0;
	}

	// call legacy bring to front
	SetForegroundWindow(hWnd);
	Sleep(100);
	otherHWnd = GetForegroundWindow();
	if (otherHWnd == hWnd) {
		return 0;
	}

	SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
	Sleep(100);
	otherHWnd = GetForegroundWindow();
	if (otherHWnd == hWnd) {
		return 0;
	}

	// check if non-obscured, which is good enough
	if (!IsObscured(hWnd)) {
		return 0;
	}

	// modern-hacky bring-to-front dance: minimize, restore
	ShowWindow(hWnd, SW_SHOW);
	if (otherHWnd == hWnd) {
		return 0;
	}

	BOOL maximized = IsZoomed(hWnd);
	ShowWindow(hWnd, SW_MINIMIZE);
	ShowWindow(hWnd, SW_SHOWMINIMIZED);
	ShowWindow(hWnd, SW_SHOWNORMAL);
	if (maximized) {
		ShowWindow(hWnd, SW_SHOWMAXIMIZED);
	}

	return -1;
}
