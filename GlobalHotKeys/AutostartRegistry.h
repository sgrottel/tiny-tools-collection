#pragma once

namespace sgrottel {
	class ISimpleLog;
}

class AutostartRegistry
{
public:
	AutostartRegistry(sgrottel::ISimpleLog& log);

	void Register();
	void Unregister();
	bool CanUnregister();

private:
	static constexpr const wchar_t* c_name = L"sgrottel.GlobalHotKeys";

	sgrottel::ISimpleLog& m_log;

};

