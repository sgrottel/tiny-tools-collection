//
// KeePassHotKey
// TraceFile.h
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
#pragma once

#include "Common.h"
#include <vector>


class TraceFile
{
public:

	class LogStream : public _tstringstream {
	public:
		LogStream(LogStream const& src) = delete;
		LogStream& operator=(LogStream const& src) = delete;
		LogStream(LogStream&& src) = default;
		LogStream& operator=(LogStream&& src) = default;

		~LogStream();
	private:
		LogStream(TraceFile& owner);

		TraceFile& m_owner;
		friend TraceFile;
	};

	static TraceFile& Instance();

	void setFile(_tstring const& path);

	LogStream log();
	void log(_tstring const& msg);

private:

	std::string m_prefix;
	_tstring m_file;
	std::vector<_tstring> m_lineBuf;
};

