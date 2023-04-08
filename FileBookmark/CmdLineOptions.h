// FileBookmark
// Copyright 2023, SGrottel
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#pragma once

#include <string>

namespace filebookmark
{
	class CmdLineOptions
	{
	public:

		enum class Mode {
			None,
			OpenBookmark,
			RegisterFileType,
			UnregisterFileType,
			SetBookmark,
			SetBookmarkAndOpen,
			OpenDirectory
		};

		inline Mode GetMode() const {
			return m_mode;
		}
		inline std::wstring const& GetPath() const {
			return m_path;
		}

		void Parse(const wchar_t* pCmdLine);

	private:
		Mode m_mode{ Mode::None };
		std::wstring m_path{};
	};

}