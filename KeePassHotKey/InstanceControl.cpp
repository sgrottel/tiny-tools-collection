//
// KeePassHotKey
// InstanceControl.cpp
//
// Copyright 2022 SGrottel (https://www.sgrottel.de)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http ://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissionsand
// limitations under the License.
//
#include "InstanceControl.h"

#include "TraceFile.h"

InstanceControl::~InstanceControl() {
	deinit();
}

bool InstanceControl::initOrSignal() {
	if (m_instanceSemaphore != INVALID_HANDLE_VALUE) return true; // already locked

	m_instanceSemaphore = CreateSemaphore(NULL, 0, 1, _T("sgrottel_keepasshotkey_instance_handle_lock"));

	if (m_instanceSemaphore == NULL) {
		m_instanceSemaphore = INVALID_HANDLE_VALUE;
		throw std::runtime_error("Failed to obtail instance semaphore lock");
	}

	DWORD le = GetLastError();
	if (le == ERROR_ALREADY_EXISTS) {
		TraceFile::Instance().log(_T("ReleaseSemaphore(m_instanceSemaphore)"));
		BOOL rsr = ReleaseSemaphore(m_instanceSemaphore, 1, NULL);
		TraceFile::Instance().log() << _T("\t") << rsr;
		return false;
	}

	return true;
}

void InstanceControl::deinit() {
	if (m_instanceSemaphore != INVALID_HANDLE_VALUE) {
		CloseHandle(m_instanceSemaphore);
		m_instanceSemaphore = INVALID_HANDLE_VALUE;
	}
}

void InstanceControl::clearSignaled() {
	if (m_instanceSemaphore == INVALID_HANDLE_VALUE) return;

	DWORD rv = WAIT_OBJECT_0;
	while (rv == WAIT_OBJECT_0) {
		rv = WaitForSingleObject(m_instanceSemaphore, 0);
		if (rv == WAIT_FAILED) {
			throw std::runtime_error("Failed to wait on instance semaphore");
		}
	}
}

bool InstanceControl::tryGetSignaled() {
	if (m_instanceSemaphore == INVALID_HANDLE_VALUE) {
		throw std::logic_error("Cannot query signaled state when semaphore is not initialized");
	}

	DWORD rv = WaitForSingleObject(m_instanceSemaphore, 0);
	if (rv == WAIT_FAILED) {
		throw std::runtime_error("Failed to wait on instance semaphore");
	}

	return rv == WAIT_OBJECT_0;
}
