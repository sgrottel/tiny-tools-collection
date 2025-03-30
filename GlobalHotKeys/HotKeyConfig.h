#pragma once

#include <string>
#include <vector>

struct HotKeyConfig
{
	static constexpr const uint32_t c_invalidVirtualKeyCode = 0xffffffffu;

	static uint32_t ParseVirtualKeyCode(std::wstring str);

	// https://learn.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes
	uint32_t virtualKeyCode{ 0 };

	bool modAlt{ false };

	bool modCtrl{ false };

	bool modShift{ false };

	std::wstring executable{};

	std::wstring workingDirectory{};

	std::vector<std::wstring> arguments{};

	bool isRelExePath{ false };

	bool noFileCheck{ false };

	std::wstring GetKeyWString() const;
};

