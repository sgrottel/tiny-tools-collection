#pragma once

#include <string>
#include <vector>
#include <unordered_map>

struct HotKeyConfig
{
	static constexpr const uint32_t c_invalidVirtualKeyCode = 0xffffffffu;

	struct ResolveArgConfig
	{
		bool isRelPath{ false };
	};

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

	bool createNoWindow{ true };

	std::unordered_map<uint32_t, ResolveArgConfig> resolveArgsPaths{};

	std::wstring GetKeyWString() const;
};

