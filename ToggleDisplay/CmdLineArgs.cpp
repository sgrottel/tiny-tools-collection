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

#include "yaclap.hpp"

#include <algorithm>
#include <iostream>

bool CmdLineArgs::Parse(int argc, const wchar_t* argv[])
{
    command = Command::Unknown;
    id.clear();

    yaclap::Parser<wchar_t> parser(L"ToggleDisplay.exe", L"Toggle Display Utility");

    yaclap::Argument<wchar_t> idArgument(L"id", L"An identifier for the display to select for the operation");

    yaclap::Command<wchar_t> listCmd({ L"LIST", yaclap::Alias<wchar_t>::StringCompare::CaseInsensitive }, L"to list all displays");

    yaclap::Command<wchar_t> toggleCmd({ L"TOGGLE", yaclap::Alias<wchar_t>::StringCompare::CaseInsensitive }, L"to toggle a display (enable if disabled, disable if enabled)");
    toggleCmd.Add(idArgument);

    yaclap::Command<wchar_t> enableCmd({ L"ENABLE", yaclap::Alias<wchar_t>::StringCompare::CaseInsensitive }, L"to enable a display");
    enableCmd.Add(idArgument);

    yaclap::Command<wchar_t> disableCmd({ L"DISABLE", yaclap::Alias<wchar_t>::StringCompare::CaseInsensitive }, L"to disable a display");
    disableCmd.Add(idArgument);

    parser.Add(listCmd)
        .Add(toggleCmd)
        .Add(enableCmd)
        .Add(disableCmd);

    yaclap::Parser<wchar_t>::Result res = parser.Parse(argc, argv);

    if (res.HasCommand(listCmd)) { command = Command::List; }
    else if (res.HasCommand(toggleCmd)) { command = Command::Toggle; }
    else if (res.HasCommand(enableCmd)) { command = Command::Enable; }
    else if (res.HasCommand(disableCmd)) { command = Command::Disable; }
    else {
        res.SetError(L"You must specify a command");
    }

    auto const& idVal = res.GetArgument(idArgument);
    if (idVal.HasValue()) {
        id = idVal;
    }

    parser.PrintErrorAndHelpIfNeeded(res);
    return res.IsSuccess() && !res.ShouldShowHelp();
}
