// poltermouse.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include <cstdio>
#include <random>

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

bool TryEnableVirtualTerminalSequences()
{
	HANDLE hStdout = GetStdHandle(STD_OUTPUT_HANDLE);
	if (hStdout == INVALID_HANDLE_VALUE) return false;
	// CloseHandle is not necessary

	DWORD conMode = 0;
	if (!GetConsoleMode(hStdout, &conMode)) return false;

	conMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
	if (!SetConsoleMode(hStdout, conMode)) return false;

	if (!GetConsoleMode(hStdout, &conMode)) return false;

	return (conMode & ENABLE_VIRTUAL_TERMINAL_PROCESSING) == ENABLE_VIRTUAL_TERMINAL_PROCESSING;
}

int main()
{
	bool cool = TryEnableVirtualTerminalSequences();

	printf("Welcome to %spoltermouse%s\n",
		(cool ? "\x1b[40m\x1b[38;2;155;255;205m\x1b[1m" : "'"),
		(cool ? "\x1b[39m\x1b[49m" : "'"));
	printf("Move the mouse yourself to exit...");

	POINT lastPos;
	GetCursorPos(&lastPos);
	POINT pos;

	std::default_random_engine gen;
	std::uniform_int_distribution<int> xRnd(0, GetSystemMetrics(SM_CXSCREEN));
	std::uniform_int_distribution<int> yRnd(0, GetSystemMetrics(SM_CYSCREEN));

	while (GetCursorPos(&pos), pos.x == lastPos.x && pos.y == lastPos.y)
	{
		SetCursorPos(
			(pos.x * 15 + xRnd(gen)) / 16,
			(pos.y * 15 + yRnd(gen)) / 16);
		GetCursorPos(&lastPos);

		Sleep(125);
	}

	printf("\n");
}
