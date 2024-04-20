#pragma once
#include <functional>

namespace sgrottel {
	class ISimpleLog;
}
class MainWindow;

class Menu
{
	static constexpr int MI_ENABLE_ALL_HOTKEYS = 1000;
	static constexpr int MI_DISABLE_ALL_HOTKEYS = 1001;
	static constexpr int MI_SELECT_CONFIG = 1002;
	static constexpr int MI_RELOAD_CONFIG = 1003;
	static constexpr int MI_OPEN_LOG = 1004;
	static constexpr int MI_EXPLORE_LOG_DIR = 1005;
	static constexpr int MI_EXIT = 1006;

public:
	Menu(sgrottel::ISimpleLog& log, HINSTANCE hInstance);
	~Menu();

	void Popup(const MainWindow& wnd, const POINT& p);

	void Call(WORD menuItemID);

	inline void SetEnableAllHotkeysEnabled(bool enabled)
	{
		SetMenuItemEnabled(MI_ENABLE_ALL_HOTKEYS, enabled);
	}
	inline void SetDisableAllHotkeysEnabled(bool enabled)
	{
		SetMenuItemEnabled(MI_DISABLE_ALL_HOTKEYS, enabled);
	}
	inline void SetReloadConfigurationEnabled(bool enabled)
	{
		SetMenuItemEnabled(MI_RELOAD_CONFIG, enabled);
	}

	inline void SetOnEnableAllHotKeysCallback(std::function<void()> callback)
	{
		m_onEnableAllHotKeys = std::move(callback);
	}
	inline void SetOnDisableAllHotKeysCallback(std::function<void()> callback)
	{
		m_onDisableAllHotKeys = std::move(callback);
	}
	inline void SetOnSelectConfigCallback(std::function<void()> callback)
	{
		m_onSelectConfig = std::move(callback);
	}
	inline void SetOnReloadConfigCallback(std::function<void()> callback)
	{
		m_onReloadConfig = std::move(callback);
	}

private:
	sgrottel::ISimpleLog& m_log;

	void SetMenuItemEnabled(int id, bool enabled);

	HMENU m_hMenu;
	HMENU m_hSubMenu;

	std::function<void()> m_onEnableAllHotKeys;
	std::function<void()> m_onDisableAllHotKeys;
	std::function<void()> m_onSelectConfig;
	std::function<void()> m_onReloadConfig;
};

