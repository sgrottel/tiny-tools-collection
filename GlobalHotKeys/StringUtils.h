#pragma once
#include <string>

std::wstring ToW(char const* utf8);

inline std::wstring ToW(unsigned char const* utf8)
{
	return std::move(ToW(reinterpret_cast<const char*>(utf8)));
}

std::string ToA(std::wstring const& w);
