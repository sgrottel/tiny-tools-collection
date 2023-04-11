// ToggleDisplay
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
#include "CmdLineArgs.h"

#include <algorithm>
#include <iostream>

bool CmdLineArgs::Parse(int argc, const wchar_t* argv[])
{
    command = Command::Unknown;
    id.clear();

    if (argc <= 1)
    {
        return false;
    }

    std::wstring cmdStr{argv[1]};
    std::transform(cmdStr.begin(), cmdStr.end(), cmdStr.begin(), std::toupper);

    if (cmdStr == L"LIST")
    {
        command = Command::List;
    }
    else if (cmdStr == L"TOGGLE")
    {
        command = Command::Toggle;
    }
    else if (cmdStr == L"ENABLE")
    {
        command = Command::Enable;
    }
    else if (cmdStr == L"DISABLE")
    {
        command = Command::Disable;
    }

    if (argc > 2)
    {
        id = argv[2];
    }

    return command != Command::Unknown;
}

void CmdLineArgs::PrintHelp()
{
    std::cerr << "You must specify a command:\n"
        << "    LIST    -- to list all displays\n"
        << "    TOGGLE  -- to toggle a display (enable if disabled, disable if enabled)\n"
        << "    ENABLE  -- to enable a display\n"
        << "    DISABLE -- to disable a display\n";
}
