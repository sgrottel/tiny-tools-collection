#pragma once

#include <string>
#include <vector>

struct HotKeyConfig
{
	static constexpr const uint32_t c_invalidVirtualKeyCode = 0xffffffffu;

	static uint32_t ParseVirtualKeyCode(std::wstring str);

	// https://learn.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
	uint32_t virtualKeyCode;

	bool modAlt;

	bool modCtrl;

	bool modShift;

	std::wstring executable;

	std::wstring workingDirectory;

	std::vector<std::wstring> arguments;

	std::wstring GetKeyWString() const;
};

