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

    switch (cmd.command)
    {
    case CmdLineArgs::Command::List:
        List(paths, modes);
        break;
    default:
        std::cerr << "Command not implemented" << std::endl;
        return 1;
    }

    //// https://stackoverflow.com/a/62038912/552373
    //HRESULT hr = S_OK;
    //UINT32 NumPathArrayElements = 0;
    //UINT32 NumModeInfoArrayElements = 0;
    ////LONG error = GetDisplayConfigBufferSizes((QDC_ALL_PATHS | QDC_VIRTUAL_MODE_AWARE), &NumPathArrayElements, &NumModeInfoArrayElements);
    //hr = GetDisplayConfigBufferSizes((QDC_ALL_PATHS), &NumPathArrayElements, &NumModeInfoArrayElements);
    //std::vector<DISPLAYCONFIG_PATH_INFO> PathInfoArray2(NumPathArrayElements);
    //std::vector<DISPLAYCONFIG_MODE_INFO> ModeInfoArray2(NumModeInfoArrayElements);
    ////error = QueryDisplayConfig((QDC_ALL_PATHS | QDC_VIRTUAL_MODE_AWARE), &NumPathArrayElements, &PathInfoArray2[0], &NumModeInfoArrayElements, &ModeInfoArray2[0], NULL);
    //hr = QueryDisplayConfig((QDC_ALL_PATHS), &NumPathArrayElements, &PathInfoArray2[0], &NumModeInfoArrayElements, &ModeInfoArray2[0], NULL);

    //struct displaySourcePair
    //{
    //    std::wstring displayName;
    //    UINT32 displayId;
    //};

    //std::vector<displaySourcePair> ocupiedDisplays;

    //if (hr == S_OK)
    //{

    //    DISPLAYCONFIG_SOURCE_DEVICE_NAME SourceName = {};
    //    SourceName.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
    //    SourceName.header.size = sizeof(SourceName);

    //    DISPLAYCONFIG_TARGET_PREFERRED_MODE PreferedMode = {};
    //    PreferedMode.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE;
    //    PreferedMode.header.size = sizeof(PreferedMode);


    //    int newId = 0;


    //    for (UINT32 i = 0; i < NumPathArrayElements; i++)
    //    {
    //        std::cout << "Path " << i << ": ";

    //        bool match = false;
    //        SourceName.header.adapterId = PathInfoArray2[i].sourceInfo.adapterId;
    //        SourceName.header.id = PathInfoArray2[i].sourceInfo.id;

    //        PreferedMode.header.adapterId = PathInfoArray2[i].targetInfo.adapterId;
    //        PreferedMode.header.id = PathInfoArray2[i].targetInfo.id;

    //        hr = HRESULT_FROM_WIN32(DisplayConfigGetDeviceInfo(&SourceName.header));
    //        hr = HRESULT_FROM_WIN32(DisplayConfigGetDeviceInfo(&PreferedMode.header));

    //        if (hr == S_OK)
    //        {
    //            std::wcout << SourceName.viewGdiDeviceName << L" ";

    //            if ((PathInfoArray2[i].flags & DISPLAYCONFIG_PATH_ACTIVE) == true)
    //            {
    //                std::cout << "active ";

    //                std::wstring str = std::wstring(SourceName.viewGdiDeviceName);
    //                displaySourcePair tmpStruct;
    //                tmpStruct.displayId = PreferedMode.header.id;
    //                tmpStruct.displayName = str;
    //                ocupiedDisplays.push_back(tmpStruct);

    //                //if (str == LR"(\\.\DISPLAY3)")
    //                //{
    //                //    std::cout << "switch-off ";
    //                //    PathInfoArray2[i].flags &= ~DISPLAYCONFIG_PATH_ACTIVE;
    //                //}
    //            }

    //            for (int k = 0; k < ocupiedDisplays.size(); k++)
    //            {
    //                std::wstring str = std::wstring(SourceName.viewGdiDeviceName);
    //                if (ocupiedDisplays[k].displayName == str || ocupiedDisplays[k].displayId == PreferedMode.header.id)
    //                {
    //                    match = true;
    //                }
    //            }

    //            if (match == false && PathInfoArray2[i].targetInfo.targetAvailable == 1)
    //            {
    //                std::cout << "switch-on ";

    //                PathInfoArray2[i].flags |= DISPLAYCONFIG_PATH_ACTIVE;
    //                std::wstring str = std::wstring(SourceName.viewGdiDeviceName);
    //                displaySourcePair tmpStruct;
    //                tmpStruct.displayId = PreferedMode.header.id;
    //                tmpStruct.displayName = str;
    //                ocupiedDisplays.push_back(tmpStruct);
    //            }

    //            if (PathInfoArray2[i].targetInfo.targetAvailable == 1)
    //            {
    //                std::cout << "newSrcId ";
    //                PathInfoArray2[i].sourceInfo.id = newId;
    //                newId++;
    //            }

    //            if (PathInfoArray2[i].targetInfo.id != PreferedMode.header.id)
    //            {
    //                PathInfoArray2[i].targetInfo.id = PreferedMode.header.id;
    //            }

    //            PathInfoArray2[i].sourceInfo.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
    //            PathInfoArray2[i].targetInfo.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
    //        }

    //        std::cout << "\n";
    //    }

    //    //hr = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_VALIDATE | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES | SDC_VIRTUAL_MODE_AWARE));
    //    //hr = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES | SDC_VIRTUAL_MODE_AWARE));
    //    long sdc1 = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_VALIDATE | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES));
    //    std::cout << "SetDisplayConfig SDC_VALIDATE " << sdc1 << "\n";
    //    long sdc2 = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES));
    //    std::cout << "SetDisplayConfig SDC_APPLY " << sdc2 << "\n";
    //}

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
