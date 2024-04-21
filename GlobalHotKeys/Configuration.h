#pragma once
#include "HotKeyConfig.h"

#include <filesystem>
#include <functional>
#include <optional>
#include <vector>

namespace sgrottel {
	class ISimpleLog;
}

class Configuration
{
public:
	Configuration(sgrottel::ISimpleLog& log);
	~Configuration();

	const std::filesystem::path& GetFilePath() const
	{
		return m_configFile;
	}

	bool SetFilePath(std::filesystem::path const& path, std::optional<std::function<void(std::wstring const&)>> errorMessageReceiver = std::nullopt);

	inline std::vector<HotKeyConfig> const& GetHotKeys() const
	{
		return m_hotKeys;
	}
	inline bool GetBell() const
	{
		return m_bell;
	}

private:
	sgrottel::ISimpleLog& m_log;
	std::filesystem::path m_configFile;
	bool m_configFileLoaded;
	std::vector<HotKeyConfig> m_hotKeys;

	bool m_bell;

	void LoadConfigFilePathFromRegistry();
	void SaveConfigFilePathInRegistry();
};

