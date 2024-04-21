#pragma once

class SingleInstanceGuard
{
public:
	SingleInstanceGuard();
	~SingleInstanceGuard();

	inline bool IsSingleInstance() const
	{
		return m_isSingleInstance;
	}

private:
	HANDLE m_hMutex;
	bool m_isSingleInstance;
};

