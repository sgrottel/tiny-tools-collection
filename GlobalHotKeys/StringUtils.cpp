#include "pch.h"
#include "StringUtils.h"

std::wstring ToW(char const* utf8)
{
	if (utf8 == nullptr) return L"";
	if (*utf8 == 0) return L"";

	int rv = MultiByteToWideChar(CP_UTF8, 0, utf8, -1, NULL, 0);
	if (rv == 0) return L"";

	std::wstring str(rv, L'\0');

	rv = MultiByteToWideChar(CP_UTF8, 0, utf8, -1, str.data(), static_cast<int>(str.size()));
	if (rv == 0) return L"";

	if (!str.empty() && str.back() == L'\0') str.resize(str.size() - 1);
	return std::move(str);
}

std::string ToA(std::wstring const& w)
{
	size_t len = w.length();
	std::string s = std::string(len, '?');
	for (size_t i = 0; i < len; ++i)
	{
		if (w[i] <= 127) s[i] = static_cast<char>(w[i]);
	}
	return s;
}
