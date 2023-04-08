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
#include "Bookmark.h"

#include <regex>
#include <string>
#include <vector>
#include <fstream>
#include <algorithm>

using filebookmark::Bookmark;

void Bookmark::Set(std::filesystem::path const& file)
{
	// TODO: Implement the real thing

	m_path.clear();
	m_bookmarkedFile.clear();
	m_nextFile.clear();
	if (file.empty() || !std::filesystem::is_regular_file(file))
	{
		return;
	}

	std::wregex isBookmark{ L"^.*\\.bookmark$", std::wregex::ECMAScript | std::wregex::icase };
	{
		std::wstring filename{file.filename()};
		if (std::regex_match(filename, isBookmark))
		{
			// TODO: redirect to bookmarked file
			return;
		}
	}

	// Preliminary alpha implementation:
	//
	// Delete all bookmark files from the folder
	// Then create a new empty bookmark file for the specified file

	std::vector<std::filesystem::path> files;
	for (auto const& file : std::filesystem::directory_iterator{ file.parent_path() })
	{
		std::wstring filename{file.path().filename().wstring()};
		if (std::regex_match(filename, isBookmark)) {
			files.push_back(file.path());
		}
	}

	for (auto const& file : files)
	{
		std::filesystem::remove(file);
	}

	std::wstring newBookmark{ file.wstring() + L".bookmark" };
	std::ofstream newFile{newBookmark};
	if (newFile.is_open())
	{
		newFile << "empty for now";
		newFile.close();

		Open(newBookmark);
	}
}

void Bookmark::Open(std::filesystem::path const& bookmarkFile)
{
	// TODO: Implement the real thing

	m_path.clear();
	m_bookmarkedFile.clear();
	m_nextFile.clear();
	if (bookmarkFile.empty() || !std::filesystem::is_regular_file(bookmarkFile))
	{
		return;
	}

	// Preliminary alpha implementation:
	//
	// Sort all files in the folder.
	// The file directly before the bookmark is the bookmarked file
	// The file after the bookmark is the next file

	std::vector<std::filesystem::path> files{ GetFiles(bookmarkFile.parent_path()) };

	std::filesystem::path before;
	std::filesystem::path after;

	bool found = false;
	for (std::filesystem::path const& p : files)
	{
		if (p == bookmarkFile)
		{
			found = true;
		}
		else
		{
			if (!found) before = p;
			else
			{
				after = p;
				break;
			}
		}
	}

	if (found)
	{
		m_path = bookmarkFile;
		m_bookmarkedFile = before;
		m_nextFile = after;
	}
}

void Bookmark::OpenDirectory(std::filesystem::path const& directory)
{
	std::vector<std::filesystem::path> files{ GetFiles(directory) };
	if (files.empty()) return; // nothing to bookmark

	for (auto const& file : files)
	{
		std::wstring ext{file.extension()};
		std::transform(ext.begin(), ext.end(), ext.begin(), [](auto c) { return std::tolower(c); });
		if (ext == L".bookmark")
		{
			Open(file);
			return;
		}
	}
	Set(files.front());
}

std::vector<std::filesystem::path> Bookmark::GetFiles(std::filesystem::path const& directory)
{
	// number-aware sorting
	std::vector<std::filesystem::path> files;

	std::wregex splitter{ L"^([^\\d]+)(\\d+)(.*)$" };
	std::vector<std::vector<std::pair<std::wstring, std::wstring>>> filesSegs;
	for (auto file : std::filesystem::directory_iterator{ directory })
	{
		std::wstring filename{ file.path().filename().wstring() };
		if (filename.empty()) continue;
		std::vector<std::pair<std::wstring, std::wstring>> fileSplits;

		std::wcmatch matches;
		while (std::regex_match(filename.c_str(), matches, splitter))
		{
			fileSplits.push_back(std::pair<std::wstring, std::wstring>{ matches[1].str(), matches[2].str() });
			filename = matches[3].str();
		}
		fileSplits.push_back(std::pair<std::wstring, std::wstring>{ filename, L"0" });
		filesSegs.push_back(std::move(fileSplits));
	}
	std::sort(
		filesSegs.begin(),
		filesSegs.end(),
		[](std::vector<std::pair<std::wstring, std::wstring>> const& a, std::vector<std::pair<std::wstring, std::wstring>> const& b) {
			size_t aSize = a.size();
			size_t bSize = b.size();

			for (size_t i = 0; i <= std::max(aSize, bSize); ++i)
			{
				if (aSize <= i) return true;
				if (bSize <= i) return false;

				auto const& aSeg = a[i];
				auto const& bSeg = b[i];

				if (aSeg.first < bSeg.first) return true;
				if (aSeg.first > bSeg.first) return false;

				int aI = _wtoi(aSeg.second.c_str());
				int bI = _wtoi(bSeg.second.c_str());

				if (aI < bI) return true;
				if (aI > bI) return false;
			}

			return false;
		});

	files.resize(filesSegs.size());
	std::transform(
		filesSegs.begin(),
		filesSegs.end(),
		files.begin(),
		[&directory](std::vector<std::pair<std::wstring, std::wstring>> const& a) {
			std::wstring c;
			for (auto const& p : a)
			{
				c += p.first + p.second;
			}
			return directory / c.substr(0, c.size() - 1);
		});

	return files;
}
