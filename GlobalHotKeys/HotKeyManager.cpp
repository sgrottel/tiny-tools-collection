#include "pch.h"
#include "HotKeyManager.h"
#include "MainWindow.h"
#include "SimpleLog/SimpleLog.hpp"
#include "StringUtils.h"

namespace
{
	constexpr const uint32_t c_idOffset = 42;
}

HotKeyManager::HotKeyManager(sgrottel::ISimpleLog& log, MainWindow& wnd)
	: m_wnd{ wnd }, m_log{ log }, m_bell{ false }
{
}

HotKeyManager::~HotKeyManager()
{
	DisableAllHotKeys();
}

void HotKeyManager::SetHotKeys(std::vector<HotKeyConfig> const& hotKeys, std::filesystem::path const& configDir)
{
	bool allPrevKeysDiabled = false;
	if (!m_hotKeys.empty())
	{
		allPrevKeysDiabled = true;
		for (auto const& hk : m_hotKeys)
		{
			if (hk.m_activeId != 0)
			{
				allPrevKeysDiabled = false;
				break;
			}
		}
	}
	DisableAllHotKeys();

	m_hotKeys.clear();
	for (auto const& hkc : hotKeys)
	{
		m_hotKeys.push_back({ hkc });
		m_hotKeys.back().m_activeId = 0;
	}
	m_configDir = configDir;

	if (!allPrevKeysDiabled)
	{
		EnableAllHotKeys();
	}
}

bool HotKeyManager::CanEnableAllHotKeys()
{
	for (auto const& hk : m_hotKeys)
	{
		if (hk.m_activeId == 0) return true;
	}
	return false;
}

bool HotKeyManager::CanDisableAllHotKeys()
{
	for (auto const& hk : m_hotKeys)
	{
		if (hk.m_activeId != 0) return true;
	}
	return false;
}

void HotKeyManager::EnableAllHotKeys()
{
	uint32_t nextId = c_idOffset;
	for (auto const& hk : m_hotKeys)
	{
		if (hk.m_activeId >= nextId)
		{
			nextId = hk.m_activeId;
		}
	}

	for (auto& hk : m_hotKeys)
	{
		if (hk.m_activeId == 0)
		{
			m_log.Write(L"RegisterHotKey(..., %u, %s)", nextId, hk.GetKeyWString().c_str());

			UINT mods = 0;
			if (hk.modAlt) mods |= MOD_ALT;
			if (hk.modCtrl) mods |= MOD_CONTROL;
			if (hk.modShift) mods |= MOD_SHIFT;
			BOOL res = RegisterHotKey(m_wnd.GetHandle(), nextId, mods, hk.virtualKeyCode);
			if (res == 0)
			{
				m_log.Error(L"RegisterHotKey failed: %d", static_cast<int>(GetLastError()));
			}
			else
			{
				hk.m_activeId = nextId;
			}

			nextId++;
			if (nextId > 0xBFFF)
			{
				m_log.Error("RegisterHotKey id limit hit. Abort.");
				break;
			}
		}
	}
}

void HotKeyManager::DisableAllHotKeys()
{
	for (auto& hk : m_hotKeys)
	{
		if (hk.m_activeId != 0)
		{
			m_log.Write("UnregisterHotKey(..., %u)", hk.m_activeId);
			UnregisterHotKey(m_wnd.GetHandle(), hk.m_activeId);
			hk.m_activeId = 0;
		}
	}
}

namespace
{
	std::filesystem::path SearchExePath(wchar_t const* dir, std::filesystem::path const& exe)
	{
		std::vector<wchar_t> p(MAX_PATH, L'\0');

		DWORD res = SearchPathW(dir, exe.wstring().c_str(), NULL, static_cast<DWORD>(p.size()), p.data(), NULL);
		if (res == 0) return {};
		if (res > p.size())
		{
			p.resize(res, L'\0');
			res = SearchPathW(dir, exe.wstring().c_str(), NULL, static_cast<DWORD>(p.size()), p.data(), NULL);
			if (res == 0) return {};
		}

		return std::filesystem::path{ p.begin(), p.begin() + res };
	}
}

void HotKeyManager::HotKeyTriggered(uint32_t id)
{
	m_log.Write("HotKeyTriggered(%u)", id);

	auto hk = std::find_if(m_hotKeys.begin(), m_hotKeys.end(), [&id](auto const& key) { return key.m_activeId == id; });
	if (hk == m_hotKeys.end())
	{
		m_log.Error("HotKey(%u) not found", id);
		if (m_bell)
		{
			MessageBeep(MB_ICONERROR);
		}
		return;
	}

	std::filesystem::path exe{ hk->executable };

	std::filesystem::path wd{ hk->workingDirectory };
	if (!wd.empty() && !wd.is_absolute()) wd = std::filesystem::absolute(wd);

	if (!exe.is_absolute())
	{
		if (hk->isRelExePath)
		{
			std::filesystem::path e2 = std::filesystem::canonical(m_configDir / exe);
			if (e2.is_absolute() && std::filesystem::exists(e2) && std::filesystem::is_regular_file(e2)) exe = e2;

			if (!exe.is_absolute())
			{
				e2 = std::filesystem::canonical(std::filesystem::current_path() / exe);
				if (e2.is_absolute() && std::filesystem::exists(e2) && std::filesystem::is_regular_file(e2)) exe = e2;
			}
		}

		if (!exe.is_absolute())
		{
			if (!hk->workingDirectory.empty())
			{
				std::filesystem::path e2 = SearchExePath(wd.wstring().c_str(), exe);
				if (!e2.empty() && e2.is_absolute()) exe = e2;
			}
			if (!exe.is_absolute())
			{
				std::filesystem::path e2 = SearchExePath(nullptr, exe);
				if (!e2.empty() && e2.is_absolute()) exe = e2;
			}
			exe = std::filesystem::absolute(exe);
		}
	}

	if (exe.empty())
	{
		m_log.Error(L"HotKey(%u) executable %s not found", id, hk->executable.c_str());
		if (m_bell)
		{
			MessageBeep(MB_ICONERROR);
		}
		return;
	}

	if (!hk->noFileCheck)
	{
		std::error_code ec;
		if (!std::filesystem::is_regular_file(exe, ec))
		{
			if (ec)
			{
				m_log.Error(L"HotKey(%u) executable %s not accessible: %s", id, exe.wstring().c_str(), ToW(ec.message().c_str()).c_str());
			}
			else
			{
				m_log.Error(L"HotKey(%u) executable %s not found", id, hk->executable.c_str());
			}
			if (m_bell)
			{
				MessageBeep(MB_ICONERROR);
			}
			return;
		}
	}

	m_log.Write(L"Found HotKey(%u) executable %s", id, exe.wstring().c_str());

	if (!wd.empty() && !std::filesystem::is_directory(wd))
	{
		m_log.Write(L"HotKey(%u) working directory not found %s", id, wd.wstring().c_str());
		wd.clear();
	}

	std::vector<wchar_t> arguments;
	arguments.push_back(L' ');

	for (auto const& arg : hk->arguments)
	{
		std::wstring as{ arg };

		bool needEsc = false;
		size_t pos = 0;
		while ((pos = as.find(L'"', pos)) != std::string::npos) {
			as.replace(pos, 1, L"\\\"");
			pos += 2; // Skip the newly added escape character
			needEsc = true;
		}

		if (arguments.capacity() < arguments.size() + 3 + as.size())
		{
			arguments.reserve(arguments.size() + 3 + as.size());
		}

		if (needEsc) arguments.push_back(L'"');
		for (wchar_t c : as) arguments.push_back(c);
		if (needEsc) arguments.push_back(L'"');
		arguments.push_back(L' ');
	}
	arguments.push_back(0);

	m_log.Write(L"HotKey(%u) args: %s", id, arguments.data());

	if (m_bell)
	{
		MessageBeep(MB_ICONINFORMATION);
	}

	STARTUPINFO si = { sizeof(STARTUPINFO) };
	PROCESS_INFORMATION pi;

	if (CreateProcessW(
		exe.wstring().c_str(),
		arguments.data(),
		nullptr,
		nullptr,
		FALSE,
		CREATE_NEW_PROCESS_GROUP | CREATE_NEW_CONSOLE,
		nullptr,
		wd.empty() ? nullptr : wd.wstring().c_str(),
		&si,
		&pi
		))
	{
		CloseHandle(pi.hProcess);
		CloseHandle(pi.hThread);
	}
	else
	{
		m_log.Error(L"HotKey(%u) executable could not be started: %d", id, static_cast<int>(GetLastError()));
		if (m_bell)
		{
			MessageBeep(MB_ICONERROR);
		}
		return;
	}

}
