//
// KeePassHotKey
// TraceFile.cpp
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
#include "TraceFile.h"

#include <iomanip>
#include <ctime>

namespace {
	void appendLine(_tstring const& file, _tstring const& msg, std::string const& prefix) {
		HANDLE hFile = CreateFile(file.c_str(), GENERIC_WRITE, FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, 0, NULL);
		if (hFile == INVALID_HANDLE_VALUE) {
			throw std::runtime_error((std::stringstream{} << "Failed to create specified trace file:\n" << toUtf8(file.c_str())).str());
		}
		SetFilePointer(hFile, 0, 0, FILE_END);

		std::stringstream pre;
		std::time_t t = std::time(nullptr);
		std::tm tm;
		localtime_s(&tm, &t);
		pre << std::put_time(&tm, "%Y.%m.%dT%T") << "|";
		std::string pp = pre.str();
		WriteFile(hFile, pp.c_str(), static_cast<DWORD>(pp.size()), NULL, NULL);

		WriteFile(hFile, prefix.c_str(), static_cast<DWORD>(prefix.size()), NULL, NULL);
		std::string msgBuf = toUtf8(msg.c_str());
		WriteFile(hFile, msgBuf.c_str(), static_cast<DWORD>(msgBuf.size()), NULL, NULL);
		WriteFile(hFile, "\n", 1, NULL, NULL);

		CloseHandle(hFile);
	}
}

TraceFile::LogStream::LogStream(TraceFile &owner) : _tstringstream(), m_owner(owner) {
	// intentionally empty
}

TraceFile::LogStream::~LogStream() {
	m_owner.log(this->str());
}

TraceFile& TraceFile::Instance() {
	static TraceFile inst;
	return inst;
}

void TraceFile::setFile(_tstring const& path) {
	m_file = path;

	std::stringstream pre;
	pre << std::hex << std::setw(8) << std::setfill('0') << GetCurrentProcessId() << "| ";
	m_prefix = pre.str();

	for (_tstring& s : m_lineBuf) {
		appendLine(m_file, s, m_prefix);
	}
}

TraceFile::LogStream TraceFile::log() {
	return LogStream{ *this };
}

void TraceFile::log(_tstring const& msg) {
	if (m_file.empty()) {
		m_lineBuf.push_back(msg);
		return;
	}
	appendLine(m_file, msg, m_prefix);
}
