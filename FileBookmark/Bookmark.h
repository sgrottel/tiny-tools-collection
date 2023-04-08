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

#include <filesystem>
#include <vector>

namespace filebookmark
{

	class Bookmark
	{
	public:

		inline std::filesystem::path const& GetPath() const
		{
			return m_path;
		}
		inline std::filesystem::path const& GetBookmarkedFile() const
		{
			return m_bookmarkedFile;
		}
		inline std::filesystem::path const& GetNextFile() const
		{
			return m_nextFile;
		}

		void Set(std::filesystem::path const& file);
		void Open(std::filesystem::path const& bookmarkFile);

		// If no bookmark file is in folder, set bookmark on the first one
		// Open bookmark
		void OpenDirectory(std::filesystem::path const& directory);

	private:
		std::vector<std::filesystem::path> GetFiles(std::filesystem::path const& directory);

		std::filesystem::path m_path;
		std::filesystem::path m_bookmarkedFile;
		std::filesystem::path m_nextFile;
	};

}
