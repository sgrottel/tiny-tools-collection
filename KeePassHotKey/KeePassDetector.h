//
// KeePassHotKey
// KeePassDetector.h
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

class Config;

class KeePassDetector
{
public:
	enum class Result {
		Unknown,
		Found,
		Incomplete
	};

	KeePassDetector(const Config& config);

	void Detect();

	inline Result getResult() const { return m_result; }

private:
	const Config& m_config;
	Result m_result;
};

