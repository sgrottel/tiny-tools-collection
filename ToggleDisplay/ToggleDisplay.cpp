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
#include <shlobj_core.h>

#include "CmdLineArgs.h"
#include "DisplayConfig.h"

#include "SimpleLog/SimpleLog.hpp"
#include "LogUtility.h"

#include <iostream>
#include <cassert>

void List(DisplayConfig::PathsVector const& paths, DisplayConfig::ModesVector const& modes, sgrottel::ISimpleLog& log);

int wmain(int argc, const wchar_t* argv[])
{
    using sgrottel::SimpleLog;

    DisplayConfig::PathsVector paths;
    DisplayConfig::ModesVector modes;
    CmdLineArgs cmd;

    // echoing log with all default settings
    sgrottel::EchoingSimpleLog log;

    if (!cmd.Parse(argc, argv))
    {
        cmd.PrintHelp(log);
        return 1;
    }

    DisplayConfig::ReturnCode res;
    res = DisplayConfig::Query(DisplayConfig::QueryScope::AllPaths, paths, modes);
    if (res != DisplayConfig::ReturnCode::Success)
    {
        SimpleLog::Error(log, "Failed to query display config: %s", DisplayConfig::to_string(res).c_str());
        return 1;
    }
    log.Write(sgrottel::EchoingSimpleLog::FlagDontEcho, "Query Result Paths:");
    LogPaths(log, paths);
    log.Write(sgrottel::EchoingSimpleLog::FlagDontEcho, "Query Result Modes:");
    LogModes(log, modes);

    DisplayConfig::FilterPaths(paths);
    log.Write(sgrottel::EchoingSimpleLog::FlagDontEcho, "Filtered Paths:");
    LogPaths(log, paths);

    DisplayConfig::PathInfo* selected = DisplayConfig::FindPath(paths, cmd.id);
    log.Write(sgrottel::EchoingSimpleLog::FlagDontEcho, "Selected Path:");
    LogPath(log, selected);

    log.Write(sgrottel::EchoingSimpleLog::FlagDontEcho, ("Command = " + std::to_string(static_cast<int>(cmd.command))).c_str());
    switch (cmd.command)
    {
    case CmdLineArgs::Command::List:
        List(paths, modes, log);
        break;
    case CmdLineArgs::Command::Toggle:
        if (selected == nullptr)
        {
            SimpleLog::Error(log, "You must specify a display, either by name, target name, or target path.");
            return 1;
        }
        if (DisplayConfig::IsEnabled(*selected))
        {
            log.Write("Selected display is enabled... disabling\n");
            DisplayConfig::SetDisabled(*selected);
        }
        else
        {
            log.Write("Selected display is disabled... enabling\n");
            DisplayConfig::SetEnabled(*selected);
        }
        res = DisplayConfig::Apply(paths);
        if (res != DisplayConfig::ReturnCode::Success)
        {
            SimpleLog::Error(log, "Failed to apply changed display config: %s", DisplayConfig::to_string(res).c_str());
            return 1;
        }

        break;
    case CmdLineArgs::Command::Enable:
        if (selected == nullptr)
        {
            SimpleLog::Error(log, "You must specify a display, either by name, target name, or target path.");
            return 1;
        }
        if (DisplayConfig::IsEnabled(*selected))
        {
            log.Write("Selected display is already enabled");
            return 0;
        }
        DisplayConfig::SetEnabled(*selected);
        log.Write("Enabling display");
        res = DisplayConfig::Apply(paths);
        if (res != DisplayConfig::ReturnCode::Success)
        {
            SimpleLog::Error(log, "Failed to apply changed display config: %s", DisplayConfig::to_string(res).c_str());
            return 1;
        }

        break;
    case CmdLineArgs::Command::Disable:
        if (selected == nullptr)
        {
            SimpleLog::Error(log, "You must specify a display, either by name, target name, or target path.");
            return 1;
        }
        if (!DisplayConfig::IsEnabled(*selected))
        {
            log.Write("Selected display is already disabled");
            return 0;
        }
        DisplayConfig::SetDisabled(*selected);
        log.Write("Disabling display");
        res = DisplayConfig::Apply(paths);
        if (res != DisplayConfig::ReturnCode::Success)
        {
            SimpleLog::Error(log, "Failed to apply changed display config: %s", DisplayConfig::to_string(res).c_str());
            return 1;
        }

        break;
    default:
        SimpleLog::Error(log, "Command not implemented");
        return 1;
    }

    return 0;
}

void List(DisplayConfig::PathsVector const& paths, DisplayConfig::ModesVector const& modes, sgrottel::ISimpleLog& log)
{
    using sgrottel::SimpleLog;
    for (DisplayConfig::PathInfo const& path : paths)
    {
        std::wstring deviceName = DisplayConfig::GetGdiDeviceName(path);
        DisplayConfig::TargetDeviceName targetDeviceName = DisplayConfig::GetTargetDeviceName(path);
        uint32_t preferedModeId = DisplayConfig::GetTargetPreferedModeId(path);

        SimpleLog::Write(log, L"%s -> %s (%s)  [%s]", deviceName.c_str(), targetDeviceName.name.c_str(), targetDeviceName.path.c_str(),
            (DisplayConfig::IsEnabled(path) ? L"enabled" : L"disabled"));
        uint32_t srcModeIdx = path.sourceInfo.modeInfoIdx;
        if (srcModeIdx < modes.size())
        {
            assert(modes[srcModeIdx].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE);
            auto const& srcMode = modes[srcModeIdx].sourceMode;
            SimpleLog::Write(log, L"    (w: %u; h: %d; x: %d; y: %d)", srcMode.width, srcMode.height, srcMode.position.x, srcMode.position.y);
        }
    }
}
