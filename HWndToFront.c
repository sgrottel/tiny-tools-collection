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

BOOL parseArg(int argc, char const* argv[], HWND* outHWnd)
{
	if (argc < 2)
	{
		fprintf(stderr, "You need to specify the window handle to bring to front\n");
		return FALSE;
	}
	if (outHWnd == NULL)
	{
		fprintf(stderr, "Out variable not initialized\n");
		return FALSE;
	}

	if (argv[1][0] == 'x')
	{
		// parse string as hex
		int hWndValue;
		int rc = sscanf_s(argv[1], "x%x", &hWndValue);
		*outHWnd = (HWND)((intptr_t)hWndValue);
		if (rc != 1)
		{
			fprintf(stderr, "Input argument not parsable as hex number\n");
			return FALSE;
		}
	}
	else
	{
		// parse string as decimal
		*outHWnd = (HWND)((intptr_t)atoi(argv[1]));
		if (*outHWnd == 0)
		{
			fprintf(stderr, "Input argument `0` (invalid) or not parsable as number\n");
			return FALSE;
		}
	}

	return TRUE;
}

int main(int argc, char const* argv[])
{
	HWND hWnd, otherHWnd;

	// Cmdline parsing
	if (!parseArg(argc, argv, &hWnd))
	{
		// failed to parse; error already printed to stderr
		return -1;
	}

	// validated `hWnd` is a valid handle to a top-level window
	if (!IsWindow(hWnd))
	{
		fprintf(stderr, "Specified window handle 0x%.8x is not a valid window handle.", (unsigned)((uintptr_t)hWnd));
		return -1;
	}
	otherHWnd = GetParent(hWnd);
	if (otherHWnd != NULL)
	{
		fprintf(stderr, "Specified window handle 0x%.8x is not a top-level window (it has a parent window).", (unsigned)((uintptr_t)hWnd));
		return -1;
	}

	if (BringHWndToFront(hWnd, TRUE))
	{
		return 0;
	}

	return -1;
}
