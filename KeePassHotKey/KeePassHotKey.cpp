//
// KeePassHotKey
// Main application entry point
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

#include "Common.h"
#include "Config.h"

void reportException(std::string const& msgUtf8) {
	_tstringstream text;
	text << _T("Error: ") << fromUtf8<TCHAR>(msgUtf8.c_str());

	MessageBox(NULL,
		text.str().c_str(),
		k_caption,
		MB_ICONERROR | MB_OK | MB_APPLMODAL);
}

int APIENTRY _tWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow) {
	Config config;
	try {
		config.init(lpCmdLine);
		if (!config.continueProgram()) return 0;


		MessageBox(NULL, _T("Hello World"), k_caption, MB_OK | MB_APPLMODAL);

	}
	catch (std::exception const& ex) {
		reportException(ex.what());
	}
	catch (...) {
		reportException("Unexpected Exception");
	}

	return 0;
}