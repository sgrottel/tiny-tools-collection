//
// KeePassHotKey
// Config.h
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

class Config {
public:

	void init(const TCHAR* cmdLine);

	inline const _tstring& getKeePassExe() const { return m_keePassExe; }
	inline const _tstring& getKdbxFile() const { return m_kdbxFile; }
	inline bool continueProgram() const { return m_continue; }

private:

	void showHelp();
	void tryFindKeePassExe();

	_tstring m_keePassExe;
	_tstring m_kdbxFile;
	bool m_continue;
};

