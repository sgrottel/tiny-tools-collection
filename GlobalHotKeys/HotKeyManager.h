#pragma once
#include "HotKeyConfig.h"
#include <vector>

namespace sgrottel {
	class ISimpleLog;
}
class MainWindow;

class HotKeyManager
{
public:
	HotKeyManager(sgrottel::ISimpleLog& log, MainWindow& wnd);
	~HotKeyManager();

	void SetHotKeys(std::vector<HotKeyConfig> const& hotKeys);
	inline void SetBell(bool bell)
	{
		m_bell = bell;
	}

	bool CanEnableAllHotKeys();
	bool CanDisableAllHotKeys();
	void EnableAllHotKeys();
	void DisableAllHotKeys();

	void HotKeyTriggered(uint32_t id);

private:
	struct HotKey : public HotKeyConfig
	{
		uint32_t m_activeId;
	};

	MainWindow& m_wnd;
	sgrottel::ISimpleLog& m_log;
	std::vector<HotKey> m_hotKeys;
	bool m_bell;
};

