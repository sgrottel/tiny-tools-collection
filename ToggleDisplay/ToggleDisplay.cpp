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
#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include "CmdLineArgs.h"
#include "DisplayConfig.h"

#include <iostream>
#include <cassert>

void List(DisplayConfig::PathsVector const& paths, DisplayConfig::ModesVector const& modes);

int wmain(int argc, const wchar_t* argv[])
{
    DisplayConfig::PathsVector paths;
    DisplayConfig::ModesVector modes;
    CmdLineArgs cmd;

    if (!cmd.Parse(argc, argv))
    {
        cmd.PrintHelp();
        return 1;
    }

    DisplayConfig::ReturnCode res;
    res = DisplayConfig::Query(DisplayConfig::QueryScope::AllPaths, paths, modes);
    if (res != DisplayConfig::ReturnCode::Success)
    {
        std::cerr << "Failed to query display config: " << DisplayConfig::to_string(res) << std::endl;
        return 1;
    }
    DisplayConfig::FilterPaths(paths);

    DisplayConfig::PathInfo* selected = DisplayConfig::FindPath(paths, cmd.id);

    switch (cmd.command)
    {
    case CmdLineArgs::Command::List:
        List(paths, modes);
        break;
    case CmdLineArgs::Command::Toggle:
        if (selected == nullptr)
        {
            std::cerr << "You must specify a display, either by name, target name, or target path." << std::endl;
            return 1;
        }
        if (DisplayConfig::IsEnabled(*selected))
        {
            std::cout << "Selected display is enabled... disabling\n";
            DisplayConfig::SetDisabled(*selected);
        }
        else
        {
            std::cout << "Selected display is disabled... enabling\n";
            DisplayConfig::SetEnabled(*selected);
        }
        res = DisplayConfig::Apply(paths);
        if (res != DisplayConfig::ReturnCode::Success)
        {
            std::cerr << "Failed to apply changed display config: " << DisplayConfig::to_string(res) << std::endl;
            return 1;
        }

        break;
    case CmdLineArgs::Command::Enable:
        if (selected == nullptr)
        {
            std::cerr << "You must specify a display, either by name, target name, or target path." << std::endl;
            return 1;
        }
        if (DisplayConfig::IsEnabled(*selected))
        {
            std::cout << "Selected display is already enabled" << std::endl;
            return 0;
        }
        DisplayConfig::SetEnabled(*selected);
        std::cout << "Enabling display" << std::endl;
        res = DisplayConfig::Apply(paths);
        if (res != DisplayConfig::ReturnCode::Success)
        {
            std::cerr << "Failed to apply changed display config: " << DisplayConfig::to_string(res) << std::endl;
            return 1;
        }

        break;
    case CmdLineArgs::Command::Disable:
        if (selected == nullptr)
        {
            std::cerr << "You must specify a display, either by name, target name, or target path." << std::endl;
            return 1;
        }
        if (!DisplayConfig::IsEnabled(*selected))
        {
            std::cout << "Selected display is already disabled" << std::endl;
            return 0;
        }
        DisplayConfig::SetDisabled(*selected);
        std::cout << "Disabling display" << std::endl;
        res = DisplayConfig::Apply(paths);
        if (res != DisplayConfig::ReturnCode::Success)
        {
            std::cerr << "Failed to apply changed display config: " << DisplayConfig::to_string(res) << std::endl;
            return 1;
        }

        break;
    default:
        std::cerr << "Command not implemented" << std::endl;
        return 1;
    }

    return 0;
}

void List(DisplayConfig::PathsVector const& paths, DisplayConfig::ModesVector const& modes)
{
    for (DisplayConfig::PathInfo const& path : paths)
    {
        std::wstring deviceName = DisplayConfig::GetGdiDeviceName(path);
        DisplayConfig::TargetDeviceName targetDeviceName = DisplayConfig::GetTargetDeviceName(path);
        uint32_t preferedModeId = DisplayConfig::GetTargetPreferedModeId(path);

        std::wcout
            << deviceName << L" -> " << targetDeviceName.name << L" (" << targetDeviceName.path << L")"
            << (DisplayConfig::IsEnabled(path) ? L"  [enabled]" : L"  [disabled]")
            << L"\n";

        uint32_t srcModeIdx = path.sourceInfo.modeInfoIdx;
        if (srcModeIdx < modes.size())
        {
            assert(modes[srcModeIdx].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE);
            auto const& srcMode = modes[srcModeIdx].sourceMode;
            std::wcout << L"    (w: " << srcMode.width << L"; h: " << srcMode.height << L"; x: " << srcMode.position.x << L"; y: " << srcMode.position.y << L")\n";
        }
    }
}
