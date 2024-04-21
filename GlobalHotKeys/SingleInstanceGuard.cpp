#include "pch.h"
#include "SingleInstanceGuard.h"

namespace
{
	constexpr const wchar_t* c_AppSingleInstanceMutexName = L"SGR_GLOBALHOTKEYS_SINGLEINSTANCE";
}

SingleInstanceGuard::SingleInstanceGuard()
	: m_isSingleInstance{ true }
{
	m_hMutex = CreateMutex(NULL, FALSE, c_AppSingleInstanceMutexName);
	if (m_hMutex == NULL) {
		m_isSingleInstance = false;
	}
	else
	{
		DWORD le = GetLastError();
		m_isSingleInstance = le != ERROR_ALREADY_EXISTS;
	}
}

SingleInstanceGuard::~SingleInstanceGuard()
{
	if (m_hMutex != NULL) {
		CloseHandle(m_hMutex);
		m_hMutex = NULL;
	}
}
