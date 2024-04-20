#include "pch.h"
#include "HotKeyConfig.h"

#include "Winuser.h"

namespace
{
	// trim from start (in place)
	inline void ltrim(std::wstring& s) {
		s.erase(s.begin(), std::find_if(s.begin(), s.end(), [](wchar_t ch) {
			return !std::isspace(ch);
			}));
	}

	// trim from end (in place)
	inline void rtrim(std::wstring& s) {
		s.erase(std::find_if(s.rbegin(), s.rend(), [](wchar_t ch) {
			return !std::isspace(ch);
			}).base(), s.end());
	}
}

uint32_t HotKeyConfig::ParseVirtualKeyCode(std::wstring str)
{
	if (str.empty()) return c_invalidVirtualKeyCode;
	ltrim(str);
	rtrim(str);
	if (str.empty()) return c_invalidVirtualKeyCode;

	const std::locale loc("");
	std::transform(str.begin(), str.end(), str.begin(), [&loc](wchar_t w) { return std::tolower(w, loc); });

	if (str.size() == 1)
	{
		// for basic ascii key Virtual Key code and char code are identical
		UINT uc = std::toupper(str[0], loc);
		if (MapVirtualKeyW(uc, MAPVK_VK_TO_CHAR) == uc) return uc;

		// else, try the language specific OEM codes
#if(_WIN32_WINNT >= 0x0500)
#endif /* _WIN32_WINNT >= 0x0500 */
#if(_WIN32_WINNT >= 0x0604)
#endif /* _WIN32_WINNT >= 0x0604 */
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_NEC_EQUAL, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_NEC_EQUAL; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_FJ_JISHO, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_FJ_JISHO; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_FJ_MASSHOU, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_FJ_MASSHOU; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_FJ_TOUROKU, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_FJ_TOUROKU; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_FJ_LOYA, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_FJ_LOYA; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_FJ_ROYA, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_FJ_ROYA; }
#if(_WIN32_WINNT >= 0x0500)
#endif /* _WIN32_WINNT >= 0x0500 */
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_1, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_1; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_PLUS, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_PLUS; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_COMMA, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_COMMA; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_MINUS, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_MINUS; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_PERIOD, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_PERIOD; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_2, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_2; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_3, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_3; }
#if(_WIN32_WINNT >= 0x0604)
#endif /* _WIN32_WINNT >= 0x0604 */
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_4, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_4; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_5, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_5; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_6, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_6; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_7, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_7; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_8, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_8; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_AX, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_AX; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_102, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_102; }
#if(WINVER >= 0x0400)
#endif /* WINVER >= 0x0400 */
#if(_WIN32_WINNT >= 0x0500)
#endif /* _WIN32_WINNT >= 0x0500 */
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_RESET, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_RESET; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_JUMP, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_JUMP; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_PA1, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_PA1; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_PA2, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_PA2; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_PA3, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_PA3; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_WSCTRL, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_WSCTRL; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_CUSEL, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_CUSEL; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_ATTN, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_ATTN; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_FINISH, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_FINISH; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_COPY, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_COPY; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_AUTO, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_AUTO; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_ENLW, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_ENLW; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_BACKTAB, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_BACKTAB; }
		if (static_cast<wchar_t>(MapVirtualKeyW(VK_OEM_CLEAR, MAPVK_VK_TO_CHAR)) == str[0]) { return VK_OEM_CLEAR; }

	}
	else
	{
		if (str == L"lbutton") { return VK_LBUTTON; }
		if (str == L"rbutton") { return VK_RBUTTON; }
		if (str == L"cancel") { return VK_CANCEL; }
		if (str == L"mbutton") { return VK_MBUTTON; }
#if(_WIN32_WINNT >= 0x0500)
		if (str == L"xbutton1") { return VK_XBUTTON1; }
		if (str == L"xbutton2") { return VK_XBUTTON2; }
#endif /* _WIN32_WINNT >= 0x0500 */
		if (str == L"back") { return VK_BACK; }
		if (str == L"tab") { return VK_TAB; }
		if (str == L"clear") { return VK_CLEAR; }
		if (str == L"return") { return VK_RETURN; }
		if (str == L"shift") { return VK_SHIFT; }
		if (str == L"control") { return VK_CONTROL; }
		if (str == L"menu") { return VK_MENU; }
		if (str == L"pause") { return VK_PAUSE; }
		if (str == L"capital") { return VK_CAPITAL; }
		if (str == L"kana") { return VK_KANA; }
		if (str == L"hangeul") { return VK_HANGEUL; }
		if (str == L"hangul") { return VK_HANGUL; }
		if (str == L"ime_on") { return VK_IME_ON; }
		if (str == L"junja") { return VK_JUNJA; }
		if (str == L"final") { return VK_FINAL; }
		if (str == L"hanja") { return VK_HANJA; }
		if (str == L"kanji") { return VK_KANJI; }
		if (str == L"ime_off") { return VK_IME_OFF; }
		if (str == L"escape") { return VK_ESCAPE; }
		if (str == L"convert") { return VK_CONVERT; }
		if (str == L"nonconvert") { return VK_NONCONVERT; }
		if (str == L"accept") { return VK_ACCEPT; }
		if (str == L"modechange") { return VK_MODECHANGE; }
		if (str == L"space") { return VK_SPACE; }
		if (str == L"prior") { return VK_PRIOR; }
		if (str == L"next") { return VK_NEXT; }
		if (str == L"end") { return VK_END; }
		if (str == L"home") { return VK_HOME; }
		if (str == L"left") { return VK_LEFT; }
		if (str == L"up") { return VK_UP; }
		if (str == L"right") { return VK_RIGHT; }
		if (str == L"down") { return VK_DOWN; }
		if (str == L"select") { return VK_SELECT; }
		if (str == L"print") { return VK_PRINT; }
		if (str == L"execute") { return VK_EXECUTE; }
		if (str == L"snapshot") { return VK_SNAPSHOT; }
		if (str == L"insert") { return VK_INSERT; }
		if (str == L"delete") { return VK_DELETE; }
		if (str == L"help") { return VK_HELP; }
		if (str == L"lwin") { return VK_LWIN; }
		if (str == L"rwin") { return VK_RWIN; }
		if (str == L"apps") { return VK_APPS; }
		if (str == L"sleep") { return VK_SLEEP; }
		if (str == L"numpad0") { return VK_NUMPAD0; }
		if (str == L"numpad1") { return VK_NUMPAD1; }
		if (str == L"numpad2") { return VK_NUMPAD2; }
		if (str == L"numpad3") { return VK_NUMPAD3; }
		if (str == L"numpad4") { return VK_NUMPAD4; }
		if (str == L"numpad5") { return VK_NUMPAD5; }
		if (str == L"numpad6") { return VK_NUMPAD6; }
		if (str == L"numpad7") { return VK_NUMPAD7; }
		if (str == L"numpad8") { return VK_NUMPAD8; }
		if (str == L"numpad9") { return VK_NUMPAD9; }
		if (str == L"multiply") { return VK_MULTIPLY; }
		if (str == L"add") { return VK_ADD; }
		if (str == L"separator") { return VK_SEPARATOR; }
		if (str == L"subtract") { return VK_SUBTRACT; }
		if (str == L"decimal") { return VK_DECIMAL; }
		if (str == L"divide") { return VK_DIVIDE; }
		if (str == L"f1") { return VK_F1; }
		if (str == L"f2") { return VK_F2; }
		if (str == L"f3") { return VK_F3; }
		if (str == L"f4") { return VK_F4; }
		if (str == L"f5") { return VK_F5; }
		if (str == L"f6") { return VK_F6; }
		if (str == L"f7") { return VK_F7; }
		if (str == L"f8") { return VK_F8; }
		if (str == L"f9") { return VK_F9; }
		if (str == L"f10") { return VK_F10; }
		if (str == L"f11") { return VK_F11; }
		if (str == L"f12") { return VK_F12; }
		if (str == L"f13") { return VK_F13; }
		if (str == L"f14") { return VK_F14; }
		if (str == L"f15") { return VK_F15; }
		if (str == L"f16") { return VK_F16; }
		if (str == L"f17") { return VK_F17; }
		if (str == L"f18") { return VK_F18; }
		if (str == L"f19") { return VK_F19; }
		if (str == L"f20") { return VK_F20; }
		if (str == L"f21") { return VK_F21; }
		if (str == L"f22") { return VK_F22; }
		if (str == L"f23") { return VK_F23; }
		if (str == L"f24") { return VK_F24; }
#if(_WIN32_WINNT >= 0x0604)
		if (str == L"navigation_view") { return VK_NAVIGATION_VIEW; }
		if (str == L"navigation_menu") { return VK_NAVIGATION_MENU; }
		if (str == L"navigation_up") { return VK_NAVIGATION_UP; }
		if (str == L"navigation_down") { return VK_NAVIGATION_DOWN; }
		if (str == L"navigation_left") { return VK_NAVIGATION_LEFT; }
		if (str == L"navigation_right") { return VK_NAVIGATION_RIGHT; }
		if (str == L"navigation_accept") { return VK_NAVIGATION_ACCEPT; }
		if (str == L"navigation_cancel") { return VK_NAVIGATION_CANCEL; }
#endif /* _WIN32_WINNT >= 0x0604 */
		if (str == L"numlock") { return VK_NUMLOCK; }
		if (str == L"scroll") { return VK_SCROLL; }
		if (str == L"oem_nec_equal") { return VK_OEM_NEC_EQUAL; }
		if (str == L"oem_fj_jisho") { return VK_OEM_FJ_JISHO; }
		if (str == L"oem_fj_masshou") { return VK_OEM_FJ_MASSHOU; }
		if (str == L"oem_fj_touroku") { return VK_OEM_FJ_TOUROKU; }
		if (str == L"oem_fj_loya") { return VK_OEM_FJ_LOYA; }
		if (str == L"oem_fj_roya") { return VK_OEM_FJ_ROYA; }
		if (str == L"lshift") { return VK_LSHIFT; }
		if (str == L"rshift") { return VK_RSHIFT; }
		if (str == L"lcontrol") { return VK_LCONTROL; }
		if (str == L"rcontrol") { return VK_RCONTROL; }
		if (str == L"lmenu") { return VK_LMENU; }
		if (str == L"rmenu") { return VK_RMENU; }
#if(_WIN32_WINNT >= 0x0500)
		if (str == L"browser_back") { return VK_BROWSER_BACK; }
		if (str == L"browser_forward") { return VK_BROWSER_FORWARD; }
		if (str == L"browser_refresh") { return VK_BROWSER_REFRESH; }
		if (str == L"browser_stop") { return VK_BROWSER_STOP; }
		if (str == L"browser_search") { return VK_BROWSER_SEARCH; }
		if (str == L"browser_favorites") { return VK_BROWSER_FAVORITES; }
		if (str == L"browser_home") { return VK_BROWSER_HOME; }
		if (str == L"volume_mute") { return VK_VOLUME_MUTE; }
		if (str == L"volume_down") { return VK_VOLUME_DOWN; }
		if (str == L"volume_up") { return VK_VOLUME_UP; }
		if (str == L"media_next_track") { return VK_MEDIA_NEXT_TRACK; }
		if (str == L"media_prev_track") { return VK_MEDIA_PREV_TRACK; }
		if (str == L"media_stop") { return VK_MEDIA_STOP; }
		if (str == L"media_play_pause") { return VK_MEDIA_PLAY_PAUSE; }
		if (str == L"launch_mail") { return VK_LAUNCH_MAIL; }
		if (str == L"launch_media_select") { return VK_LAUNCH_MEDIA_SELECT; }
		if (str == L"launch_app1") { return VK_LAUNCH_APP1; }
		if (str == L"launch_app2") { return VK_LAUNCH_APP2; }
#endif /* _WIN32_WINNT >= 0x0500 */
		if (str == L"oem_1") { return VK_OEM_1; }
		if (str == L"oem_plus") { return VK_OEM_PLUS; }
		if (str == L"oem_comma") { return VK_OEM_COMMA; }
		if (str == L"oem_minus") { return VK_OEM_MINUS; }
		if (str == L"oem_period") { return VK_OEM_PERIOD; }
		if (str == L"oem_2") { return VK_OEM_2; }
		if (str == L"oem_3") { return VK_OEM_3; }
#if(_WIN32_WINNT >= 0x0604)
		if (str == L"gamepad_a") { return VK_GAMEPAD_A; }
		if (str == L"gamepad_b") { return VK_GAMEPAD_B; }
		if (str == L"gamepad_x") { return VK_GAMEPAD_X; }
		if (str == L"gamepad_y") { return VK_GAMEPAD_Y; }
		if (str == L"gamepad_right_shoulder") { return VK_GAMEPAD_RIGHT_SHOULDER; }
		if (str == L"gamepad_left_shoulder") { return VK_GAMEPAD_LEFT_SHOULDER; }
		if (str == L"gamepad_left_trigger") { return VK_GAMEPAD_LEFT_TRIGGER; }
		if (str == L"gamepad_right_trigger") { return VK_GAMEPAD_RIGHT_TRIGGER; }
		if (str == L"gamepad_dpad_up") { return VK_GAMEPAD_DPAD_UP; }
		if (str == L"gamepad_dpad_down") { return VK_GAMEPAD_DPAD_DOWN; }
		if (str == L"gamepad_dpad_left") { return VK_GAMEPAD_DPAD_LEFT; }
		if (str == L"gamepad_dpad_right") { return VK_GAMEPAD_DPAD_RIGHT; }
		if (str == L"gamepad_menu") { return VK_GAMEPAD_MENU; }
		if (str == L"gamepad_view") { return VK_GAMEPAD_VIEW; }
		if (str == L"gamepad_left_thumbstick_button") { return VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON; }
		if (str == L"gamepad_right_thumbstick_button") { return VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON; }
		if (str == L"gamepad_left_thumbstick_up") { return VK_GAMEPAD_LEFT_THUMBSTICK_UP; }
		if (str == L"gamepad_left_thumbstick_down") { return VK_GAMEPAD_LEFT_THUMBSTICK_DOWN; }
		if (str == L"gamepad_left_thumbstick_right") { return VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT; }
		if (str == L"gamepad_left_thumbstick_left") { return VK_GAMEPAD_LEFT_THUMBSTICK_LEFT; }
		if (str == L"gamepad_right_thumbstick_up") { return VK_GAMEPAD_RIGHT_THUMBSTICK_UP; }
		if (str == L"gamepad_right_thumbstick_down") { return VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN; }
		if (str == L"gamepad_right_thumbstick_right") { return VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT; }
		if (str == L"gamepad_right_thumbstick_left") { return VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT; }
#endif /* _WIN32_WINNT >= 0x0604 */
		if (str == L"oem_4") { return VK_OEM_4; }
		if (str == L"oem_5") { return VK_OEM_5; }
		if (str == L"oem_6") { return VK_OEM_6; }
		if (str == L"oem_7") { return VK_OEM_7; }
		if (str == L"oem_8") { return VK_OEM_8; }
		if (str == L"oem_ax") { return VK_OEM_AX; }
		if (str == L"oem_102") { return VK_OEM_102; }
		if (str == L"ico_help") { return VK_ICO_HELP; }
		if (str == L"ico_00") { return VK_ICO_00; }
#if(WINVER >= 0x0400)
		if (str == L"processkey") { return VK_PROCESSKEY; }
#endif /* WINVER >= 0x0400 */
		if (str == L"ico_clear") { return VK_ICO_CLEAR; }
#if(_WIN32_WINNT >= 0x0500)
		if (str == L"packet") { return VK_PACKET; }
#endif /* _WIN32_WINNT >= 0x0500 */
		if (str == L"oem_reset") { return VK_OEM_RESET; }
		if (str == L"oem_jump") { return VK_OEM_JUMP; }
		if (str == L"oem_pa1") { return VK_OEM_PA1; }
		if (str == L"oem_pa2") { return VK_OEM_PA2; }
		if (str == L"oem_pa3") { return VK_OEM_PA3; }
		if (str == L"oem_wsctrl") { return VK_OEM_WSCTRL; }
		if (str == L"oem_cusel") { return VK_OEM_CUSEL; }
		if (str == L"oem_attn") { return VK_OEM_ATTN; }
		if (str == L"oem_finish") { return VK_OEM_FINISH; }
		if (str == L"oem_copy") { return VK_OEM_COPY; }
		if (str == L"oem_auto") { return VK_OEM_AUTO; }
		if (str == L"oem_enlw") { return VK_OEM_ENLW; }
		if (str == L"oem_backtab") { return VK_OEM_BACKTAB; }
		if (str == L"attn") { return VK_ATTN; }
		if (str == L"crsel") { return VK_CRSEL; }
		if (str == L"exsel") { return VK_EXSEL; }
		if (str == L"ereof") { return VK_EREOF; }
		if (str == L"play") { return VK_PLAY; }
		if (str == L"zoom") { return VK_ZOOM; }
		if (str == L"noname") { return VK_NONAME; }
		if (str == L"pa1") { return VK_PA1; }
		if (str == L"oem_clear") { return VK_OEM_CLEAR; }
	}

	return c_invalidVirtualKeyCode;
}

std::wstring HotKeyConfig::GetKeyWString() const
{
	std::wstring str;

	// first, map all but OEM key codes
	switch (virtualKeyCode)
	{
	case VK_LBUTTON: str = L"lbutton"; break;
	case VK_RBUTTON: str = L"rbutton"; break;
	case VK_CANCEL: str = L"cancel"; break;
	case VK_MBUTTON: str = L"mbutton"; break;
#if(_WIN32_WINNT >= 0x0500)
	case VK_XBUTTON1: str = L"xbutton1"; break;
	case VK_XBUTTON2: str = L"xbutton2"; break;
#endif /* _WIN32_WINNT >= 0x0500 */
	case VK_BACK: str = L"back"; break;
	case VK_TAB: str = L"tab"; break;
	case VK_CLEAR: str = L"clear"; break;
	case VK_RETURN: str = L"return"; break;
	case VK_SHIFT: str = L"shift"; break;
	case VK_CONTROL: str = L"control"; break;
	case VK_MENU: str = L"menu"; break;
	case VK_PAUSE: str = L"pause"; break;
	case VK_CAPITAL: str = L"capital"; break;
	case VK_KANA: str = L"kana"; break;
	//case VK_HANGEUL: str = L"hangeul"; break;
	//case VK_HANGUL: str = L"hangul"; break;
	case VK_IME_ON: str = L"ime_on"; break;
	case VK_JUNJA: str = L"junja"; break;
	case VK_FINAL: str = L"final"; break;
	case VK_HANJA: str = L"hanja"; break;
	//case VK_KANJI: str = L"kanji"; break;
	case VK_IME_OFF: str = L"ime_off"; break;
	case VK_ESCAPE: str = L"escape"; break;
	case VK_CONVERT: str = L"convert"; break;
	case VK_NONCONVERT: str = L"nonconvert"; break;
	case VK_ACCEPT: str = L"accept"; break;
	case VK_MODECHANGE: str = L"modechange"; break;
	case VK_SPACE: str = L"space"; break;
	case VK_PRIOR: str = L"prior"; break;
	case VK_NEXT: str = L"next"; break;
	case VK_END: str = L"end"; break;
	case VK_HOME: str = L"home"; break;
	case VK_LEFT: str = L"left"; break;
	case VK_UP: str = L"up"; break;
	case VK_RIGHT: str = L"right"; break;
	case VK_DOWN: str = L"down"; break;
	case VK_SELECT: str = L"select"; break;
	case VK_PRINT: str = L"print"; break;
	case VK_EXECUTE: str = L"execute"; break;
	case VK_SNAPSHOT: str = L"snapshot"; break;
	case VK_INSERT: str = L"insert"; break;
	case VK_DELETE: str = L"delete"; break;
	case VK_HELP: str = L"help"; break;
	case VK_LWIN: str = L"lwin"; break;
	case VK_RWIN: str = L"rwin"; break;
	case VK_APPS: str = L"apps"; break;
	case VK_SLEEP: str = L"sleep"; break;
	case VK_NUMPAD0: str = L"numpad0"; break;
	case VK_NUMPAD1: str = L"numpad1"; break;
	case VK_NUMPAD2: str = L"numpad2"; break;
	case VK_NUMPAD3: str = L"numpad3"; break;
	case VK_NUMPAD4: str = L"numpad4"; break;
	case VK_NUMPAD5: str = L"numpad5"; break;
	case VK_NUMPAD6: str = L"numpad6"; break;
	case VK_NUMPAD7: str = L"numpad7"; break;
	case VK_NUMPAD8: str = L"numpad8"; break;
	case VK_NUMPAD9: str = L"numpad9"; break;
	case VK_MULTIPLY: str = L"multiply"; break;
	case VK_ADD: str = L"add"; break;
	case VK_SEPARATOR: str = L"separator"; break;
	case VK_SUBTRACT: str = L"subtract"; break;
	case VK_DECIMAL: str = L"decimal"; break;
	case VK_DIVIDE: str = L"divide"; break;
	case VK_F1: str = L"f1"; break;
	case VK_F2: str = L"f2"; break;
	case VK_F3: str = L"f3"; break;
	case VK_F4: str = L"f4"; break;
	case VK_F5: str = L"f5"; break;
	case VK_F6: str = L"f6"; break;
	case VK_F7: str = L"f7"; break;
	case VK_F8: str = L"f8"; break;
	case VK_F9: str = L"f9"; break;
	case VK_F10: str = L"f10"; break;
	case VK_F11: str = L"f11"; break;
	case VK_F12: str = L"f12"; break;
	case VK_F13: str = L"f13"; break;
	case VK_F14: str = L"f14"; break;
	case VK_F15: str = L"f15"; break;
	case VK_F16: str = L"f16"; break;
	case VK_F17: str = L"f17"; break;
	case VK_F18: str = L"f18"; break;
	case VK_F19: str = L"f19"; break;
	case VK_F20: str = L"f20"; break;
	case VK_F21: str = L"f21"; break;
	case VK_F22: str = L"f22"; break;
	case VK_F23: str = L"f23"; break;
	case VK_F24: str = L"f24"; break;
#if(_WIN32_WINNT >= 0x0604)
	case VK_NAVIGATION_VIEW: str = L"navigation_view"; break;
	case VK_NAVIGATION_MENU: str = L"navigation_menu"; break;
	case VK_NAVIGATION_UP: str = L"navigation_up"; break;
	case VK_NAVIGATION_DOWN: str = L"navigation_down"; break;
	case VK_NAVIGATION_LEFT: str = L"navigation_left"; break;
	case VK_NAVIGATION_RIGHT: str = L"navigation_right"; break;
	case VK_NAVIGATION_ACCEPT: str = L"navigation_accept"; break;
	case VK_NAVIGATION_CANCEL: str = L"navigation_cancel"; break;
#endif /* _WIN32_WINNT >= 0x0604 */
	case VK_NUMLOCK: str = L"numlock"; break;
	case VK_SCROLL: str = L"scroll"; break;
	case VK_LSHIFT: str = L"lshift"; break;
	case VK_RSHIFT: str = L"rshift"; break;
	case VK_LCONTROL: str = L"lcontrol"; break;
	case VK_RCONTROL: str = L"rcontrol"; break;
	case VK_LMENU: str = L"lmenu"; break;
	case VK_RMENU: str = L"rmenu"; break;
#if(_WIN32_WINNT >= 0x0500)
	case VK_BROWSER_BACK: str = L"browser_back"; break;
	case VK_BROWSER_FORWARD: str = L"browser_forward"; break;
	case VK_BROWSER_REFRESH: str = L"browser_refresh"; break;
	case VK_BROWSER_STOP: str = L"browser_stop"; break;
	case VK_BROWSER_SEARCH: str = L"browser_search"; break;
	case VK_BROWSER_FAVORITES: str = L"browser_favorites"; break;
	case VK_BROWSER_HOME: str = L"browser_home"; break;
	case VK_VOLUME_MUTE: str = L"volume_mute"; break;
	case VK_VOLUME_DOWN: str = L"volume_down"; break;
	case VK_VOLUME_UP: str = L"volume_up"; break;
	case VK_MEDIA_NEXT_TRACK: str = L"media_next_track"; break;
	case VK_MEDIA_PREV_TRACK: str = L"media_prev_track"; break;
	case VK_MEDIA_STOP: str = L"media_stop"; break;
	case VK_MEDIA_PLAY_PAUSE: str = L"media_play_pause"; break;
	case VK_LAUNCH_MAIL: str = L"launch_mail"; break;
	case VK_LAUNCH_MEDIA_SELECT: str = L"launch_media_select"; break;
	case VK_LAUNCH_APP1: str = L"launch_app1"; break;
	case VK_LAUNCH_APP2: str = L"launch_app2"; break;
#endif /* _WIN32_WINNT >= 0x0500 */
#if(_WIN32_WINNT >= 0x0604)
	case VK_GAMEPAD_A: str = L"gamepad_a"; break;
	case VK_GAMEPAD_B: str = L"gamepad_b"; break;
	case VK_GAMEPAD_X: str = L"gamepad_x"; break;
	case VK_GAMEPAD_Y: str = L"gamepad_y"; break;
	case VK_GAMEPAD_RIGHT_SHOULDER: str = L"gamepad_right_shoulder"; break;
	case VK_GAMEPAD_LEFT_SHOULDER: str = L"gamepad_left_shoulder"; break;
	case VK_GAMEPAD_LEFT_TRIGGER: str = L"gamepad_left_trigger"; break;
	case VK_GAMEPAD_RIGHT_TRIGGER: str = L"gamepad_right_trigger"; break;
	case VK_GAMEPAD_DPAD_UP: str = L"gamepad_dpad_up"; break;
	case VK_GAMEPAD_DPAD_DOWN: str = L"gamepad_dpad_down"; break;
	case VK_GAMEPAD_DPAD_LEFT: str = L"gamepad_dpad_left"; break;
	case VK_GAMEPAD_DPAD_RIGHT: str = L"gamepad_dpad_right"; break;
	case VK_GAMEPAD_MENU: str = L"gamepad_menu"; break;
	case VK_GAMEPAD_VIEW: str = L"gamepad_view"; break;
	case VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON: str = L"gamepad_left_thumbstick_button"; break;
	case VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON: str = L"gamepad_right_thumbstick_button"; break;
	case VK_GAMEPAD_LEFT_THUMBSTICK_UP: str = L"gamepad_left_thumbstick_up"; break;
	case VK_GAMEPAD_LEFT_THUMBSTICK_DOWN: str = L"gamepad_left_thumbstick_down"; break;
	case VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT: str = L"gamepad_left_thumbstick_right"; break;
	case VK_GAMEPAD_LEFT_THUMBSTICK_LEFT: str = L"gamepad_left_thumbstick_left"; break;
	case VK_GAMEPAD_RIGHT_THUMBSTICK_UP: str = L"gamepad_right_thumbstick_up"; break;
	case VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN: str = L"gamepad_right_thumbstick_down"; break;
	case VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT: str = L"gamepad_right_thumbstick_right"; break;
	case VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT: str = L"gamepad_right_thumbstick_left"; break;
#endif /* _WIN32_WINNT >= 0x0604 */
	case VK_ICO_HELP: str = L"ico_help"; break;
	case VK_ICO_00: str = L"ico_00"; break;
#if(WINVER >= 0x0400)
	case VK_PROCESSKEY: str = L"processkey"; break;
#endif /* WINVER >= 0x0400 */
	case VK_ICO_CLEAR: str = L"ico_clear"; break;
#if(_WIN32_WINNT >= 0x0500)
	case VK_PACKET: str = L"packet"; break;
#endif /* _WIN32_WINNT >= 0x0500 */
	case VK_ATTN: str = L"attn"; break;
	case VK_CRSEL: str = L"crsel"; break;
	case VK_EXSEL: str = L"exsel"; break;
	case VK_EREOF: str = L"ereof"; break;
	case VK_PLAY: str = L"play"; break;
	case VK_ZOOM: str = L"zoom"; break;
	case VK_NONAME: str = L"noname"; break;
	case VK_PA1: str = L"pa1"; break;
	}

	if (str.empty())
	{
		// then, try to map character codes
		uint32_t scanCode = MapVirtualKeyW(virtualKeyCode, MAPVK_VK_TO_VSC);
		uint32_t charCode = MapVirtualKeyW(virtualKeyCode, MAPVK_VK_TO_CHAR);
		if (scanCode != 0 && charCode != 0)
		{
			str = static_cast<wchar_t>(charCode);
		}
	}

	if (str.empty())
	{
		// finally, map OEM key codes
		switch (virtualKeyCode)
		{
#if(_WIN32_WINNT >= 0x0500)
#endif /* _WIN32_WINNT >= 0x0500 */
#if(_WIN32_WINNT >= 0x0604)
#endif /* _WIN32_WINNT >= 0x0604 */
		case VK_OEM_NEC_EQUAL: str = L"oem_nec_equal"; break;
		//case VK_OEM_FJ_JISHO: str = L"oem_fj_jisho"; break;
		case VK_OEM_FJ_MASSHOU: str = L"oem_fj_masshou"; break;
		case VK_OEM_FJ_TOUROKU: str = L"oem_fj_touroku"; break;
		case VK_OEM_FJ_LOYA: str = L"oem_fj_loya"; break;
		case VK_OEM_FJ_ROYA: str = L"oem_fj_roya"; break;
#if(_WIN32_WINNT >= 0x0500)
#endif /* _WIN32_WINNT >= 0x0500 */
		case VK_OEM_1: str = L"oem_1"; break;
		case VK_OEM_PLUS: str = L"oem_plus"; break;
		case VK_OEM_COMMA: str = L"oem_comma"; break;
		case VK_OEM_MINUS: str = L"oem_minus"; break;
		case VK_OEM_PERIOD: str = L"oem_period"; break;
		case VK_OEM_2: str = L"oem_2"; break;
		case VK_OEM_3: str = L"oem_3"; break;
#if(_WIN32_WINNT >= 0x0604)
#endif /* _WIN32_WINNT >= 0x0604 */
		case VK_OEM_4: str = L"oem_4"; break;
		case VK_OEM_5: str = L"oem_5"; break;
		case VK_OEM_6: str = L"oem_6"; break;
		case VK_OEM_7: str = L"oem_7"; break;
		case VK_OEM_8: str = L"oem_8"; break;
		case VK_OEM_AX: str = L"oem_ax"; break;
		case VK_OEM_102: str = L"oem_102"; break;
#if(WINVER >= 0x0400)
#endif /* WINVER >= 0x0400 */
#if(_WIN32_WINNT >= 0x0500)
#endif /* _WIN32_WINNT >= 0x0500 */
		case VK_OEM_RESET: str = L"oem_reset"; break;
		case VK_OEM_JUMP: str = L"oem_jump"; break;
		case VK_OEM_PA1: str = L"oem_pa1"; break;
		case VK_OEM_PA2: str = L"oem_pa2"; break;
		case VK_OEM_PA3: str = L"oem_pa3"; break;
		case VK_OEM_WSCTRL: str = L"oem_wsctrl"; break;
		case VK_OEM_CUSEL: str = L"oem_cusel"; break;
		case VK_OEM_ATTN: str = L"oem_attn"; break;
		case VK_OEM_FINISH: str = L"oem_finish"; break;
		case VK_OEM_COPY: str = L"oem_copy"; break;
		case VK_OEM_AUTO: str = L"oem_auto"; break;
		case VK_OEM_ENLW: str = L"oem_enlw"; break;
		case VK_OEM_BACKTAB: str = L"oem_backtab"; break;
		case VK_OEM_CLEAR: str = L"oem_clear"; break;
		}
	}

	if (str.empty())
	{
		// fallback for what is likely a totally invalid key code
		str = L"#" + std::to_wstring(virtualKeyCode);
	}

	if (modShift) str = L"Shift + " + str;
	if (modAlt) str = L"Alt + " + str;
	if (modCtrl) str = L"Ctrl + " + str;

	return std::move(str);
}
