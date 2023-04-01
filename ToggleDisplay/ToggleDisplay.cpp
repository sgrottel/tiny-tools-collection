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

#include <iostream>
#include <string>
#include <algorithm>
#include <iomanip>
#include <vector>


int main(int argc, const char* argv[])
{
    //if (argc <= 1)
    //{
    //    std::cerr << "You must specify a command:\n"
    //        << "    LIST    -- to list all displays\n"
    //        << "    TOGGLE  -- to toggle a display (enable if disabled, disable if enabled)\n"
    //        << "    ENABLE  -- to enable a display\n"
    //        << "    DISABLE -- to disable a display\n";
    //    return 1;
    //}

    //std::string cmd{argv[1]};
    //std::transform(cmd.begin(), cmd.end(), cmd.begin(), std::toupper);

    //std::string id;
    //if (argc > 2)
    //{
    //    id = argv[2];
    //}

    //if (cmd == "LIST")
    //{
    //    Monitors monitors;
    //    std::cout << "Monitors:\n";
    //    for (auto const& mon : monitors)
    //    {
    //        std::cout << mon.GetName() << "\n"
    //            << "    " << (mon.IsEnabled() ? "enabled" : "disabled") << "\n";
    //        if (mon.IsPrimay())
    //            std::cout << "    primary\n";
    //        if (mon.AreBoundsValid())
    //            std::cout << "    [" << mon.GetX() << "; " << mon.GetY() << "; " << mon.GetWidth() << "; " << mon.GetHeight() << "]\n";
    //    }
    //    std::cout << std::endl;

    //    monitors.Disable(monitors[2]);
    //    monitors.Enable(monitors[2]);

    //}
    //else
    //{
    //    std::cerr << "Unknown command: " << cmd << "\n";
    //    return 1;
    //}


    using namespace std;


    vector<DISPLAYCONFIG_PATH_INFO> paths;
    vector<DISPLAYCONFIG_MODE_INFO> modes;
    UINT32 flags =
        QDC_ONLY_ACTIVE_PATHS |
        //QDC_ALL_PATHS |
        QDC_VIRTUAL_MODE_AWARE;
    LONG result = ERROR_SUCCESS;

    do
    {
        // Determine how many path and mode structures to allocate
        UINT32 pathCount, modeCount;
        result = GetDisplayConfigBufferSizes(flags, &pathCount, &modeCount);

        if (result != ERROR_SUCCESS)
        {
            return HRESULT_FROM_WIN32(result);
        }

        // Allocate the path and mode arrays
        paths.resize(pathCount);
        modes.resize(modeCount);

        // Get all active paths and their modes
        result = QueryDisplayConfig(flags, &pathCount, paths.data(), &modeCount, modes.data(), nullptr);

        // The function may have returned fewer paths/modes than estimated
        paths.resize(pathCount);
        modes.resize(modeCount);

        // It's possible that between the call to GetDisplayConfigBufferSizes and QueryDisplayConfig
        // that the display state changed, so loop on the case of ERROR_INSUFFICIENT_BUFFER.
    } while (result == ERROR_INSUFFICIENT_BUFFER);

    if (result != ERROR_SUCCESS)
    {
        return HRESULT_FROM_WIN32(result);
    }

    //for (auto& p : paths) {
    //    if (p.targetInfo.outputTechnology == DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI)
    //    {
    //        p.flags &= ~DISPLAYCONFIG_PATH_ACTIVE;
    //    }
    //}

    LONG sdcRes = -1;
    //sdcRes = SetDisplayConfig(paths.size(), paths.data(), 0, nullptr, SDC_VALIDATE | SDC_TOPOLOGY_EXTEND/* | SDC_ALLOW_PATH_ORDER_CHANGES*/);

    //sdcRes = SetDisplayConfig(0, NULL, 0, NULL, (SDC_VALIDATE | SDC_TOPOLOGY_EXTEND));

    switch (sdcRes)
    {
    case ERROR_SUCCESS: std::cout << "ERROR_SUCCESS" << "\n"; break;
    case ERROR_INVALID_PARAMETER: std::cout << "ERROR_INVALID_PARAMETER" << "\n"; break;
    case ERROR_NOT_SUPPORTED: std::cout << "ERROR_NOT_SUPPORTED" << "\n"; break;
    case ERROR_ACCESS_DENIED: std::cout << "ERROR_ACCESS_DENIED" << "\n"; break;
    case ERROR_GEN_FAILURE: std::cout << "ERROR_GEN_FAILURE" << "\n"; break;
    case ERROR_BAD_CONFIGURATION: std::cout << "ERROR_BAD_CONFIGURATION" << "\n"; break;
    default: std::cout << "ERROR Undocumented value!\n"; break;
    }

    //// For each active path
    //for (auto& path : paths)
    //{
    //    // Find the target (monitor) friendly name
    //    DISPLAYCONFIG_TARGET_DEVICE_NAME targetName = {};
    //    targetName.header.adapterId = path.targetInfo.adapterId;
    //    targetName.header.id = path.targetInfo.id;
    //    targetName.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
    //    targetName.header.size = sizeof(targetName);
    //    result = DisplayConfigGetDeviceInfo(&targetName.header);

    //    if (result != ERROR_SUCCESS)
    //    {
    //        return HRESULT_FROM_WIN32(result);
    //    }

    //    // Find the adapter device name
    //    DISPLAYCONFIG_ADAPTER_NAME adapterName = {};
    //    adapterName.header.adapterId = path.targetInfo.adapterId;
    //    adapterName.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME;
    //    adapterName.header.size = sizeof(adapterName);

    //    result = DisplayConfigGetDeviceInfo(&adapterName.header);

    //    if (result != ERROR_SUCCESS)
    //    {
    //        return HRESULT_FROM_WIN32(result);
    //    }

    //    wcout
    //        << L"Path:\n"
    //        << L"  Monitor: " << (targetName.flags.friendlyNameFromEdid ? targetName.monitorFriendlyDeviceName : L"Unknown") << L"\n"
    //        << L"           " << targetName.monitorDevicePath << L"\n"
    //        << L" adapter: "  << adapterName.adapterDevicePath << L"\n"
    //        << L" on target:  " << path.targetInfo.id << L"\n";
    //}


    // https://stackoverflow.com/a/62038912/552373
    HRESULT hr = S_OK;
    UINT32 NumPathArrayElements = 0;
    UINT32 NumModeInfoArrayElements = 0;
    //LONG error = GetDisplayConfigBufferSizes((QDC_ALL_PATHS | QDC_VIRTUAL_MODE_AWARE), &NumPathArrayElements, &NumModeInfoArrayElements);
    hr = GetDisplayConfigBufferSizes((QDC_ALL_PATHS), &NumPathArrayElements, &NumModeInfoArrayElements);
    std::vector<DISPLAYCONFIG_PATH_INFO> PathInfoArray2(NumPathArrayElements);
    std::vector<DISPLAYCONFIG_MODE_INFO> ModeInfoArray2(NumModeInfoArrayElements);
    //error = QueryDisplayConfig((QDC_ALL_PATHS | QDC_VIRTUAL_MODE_AWARE), &NumPathArrayElements, &PathInfoArray2[0], &NumModeInfoArrayElements, &ModeInfoArray2[0], NULL);
    hr = QueryDisplayConfig((QDC_ALL_PATHS), &NumPathArrayElements, &PathInfoArray2[0], &NumModeInfoArrayElements, &ModeInfoArray2[0], NULL);

    struct displaySourcePair
    {
        std::wstring displayName;
        UINT32 displayId;
    };

    std::vector<displaySourcePair> ocupiedDisplays;

    if (hr == S_OK)
    {

        DISPLAYCONFIG_SOURCE_DEVICE_NAME SourceName = {};
        SourceName.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
        SourceName.header.size = sizeof(SourceName);

        DISPLAYCONFIG_TARGET_PREFERRED_MODE PreferedMode = {};
        PreferedMode.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE;
        PreferedMode.header.size = sizeof(PreferedMode);


        int newId = 0;


        for (UINT32 i = 0; i < NumPathArrayElements; i++)
        {
            std::cout << "Path " << i << ": ";

            bool match = false;
            SourceName.header.adapterId = PathInfoArray2[i].sourceInfo.adapterId;
            SourceName.header.id = PathInfoArray2[i].sourceInfo.id;

            PreferedMode.header.adapterId = PathInfoArray2[i].targetInfo.adapterId;
            PreferedMode.header.id = PathInfoArray2[i].targetInfo.id;

            hr = HRESULT_FROM_WIN32(DisplayConfigGetDeviceInfo(&SourceName.header));
            hr = HRESULT_FROM_WIN32(DisplayConfigGetDeviceInfo(&PreferedMode.header));

            if (hr == S_OK)
            {
                std::wcout << SourceName.viewGdiDeviceName << L" ";

                if ((PathInfoArray2[i].flags & DISPLAYCONFIG_PATH_ACTIVE) == true)
                {
                    std::cout << "active ";

                    std::wstring str = std::wstring(SourceName.viewGdiDeviceName);
                    displaySourcePair tmpStruct;
                    tmpStruct.displayId = PreferedMode.header.id;
                    tmpStruct.displayName = str;
                    ocupiedDisplays.push_back(tmpStruct);

                    //if (str == LR"(\\.\DISPLAY3)")
                    //{
                    //    std::cout << "switch-off ";
                    //    PathInfoArray2[i].flags &= ~DISPLAYCONFIG_PATH_ACTIVE;
                    //}
                }

                for (int k = 0; k < ocupiedDisplays.size(); k++)
                {
                    std::wstring str = std::wstring(SourceName.viewGdiDeviceName);
                    if (ocupiedDisplays[k].displayName == str || ocupiedDisplays[k].displayId == PreferedMode.header.id)
                    {
                        match = true;
                    }
                }

                if (match == false && PathInfoArray2[i].targetInfo.targetAvailable == 1)
                {
                    std::cout << "switch-on ";

                    PathInfoArray2[i].flags |= DISPLAYCONFIG_PATH_ACTIVE;
                    std::wstring str = std::wstring(SourceName.viewGdiDeviceName);
                    displaySourcePair tmpStruct;
                    tmpStruct.displayId = PreferedMode.header.id;
                    tmpStruct.displayName = str;
                    ocupiedDisplays.push_back(tmpStruct);
                }

                if (PathInfoArray2[i].targetInfo.targetAvailable == 1)
                {
                    std::cout << "newSrcId ";
                    PathInfoArray2[i].sourceInfo.id = newId;
                    newId++;
                }

                if (PathInfoArray2[i].targetInfo.id != PreferedMode.header.id)
                {
                    PathInfoArray2[i].targetInfo.id = PreferedMode.header.id;
                }

                PathInfoArray2[i].sourceInfo.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                PathInfoArray2[i].targetInfo.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
            }

            std::cout << "\n";
        }

        //hr = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_VALIDATE | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES | SDC_VIRTUAL_MODE_AWARE));
        //hr = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES | SDC_VIRTUAL_MODE_AWARE));
        long sdc1 = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_VALIDATE | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES));
        std::cout << "SetDisplayConfig SDC_VALIDATE " << sdc1 << "\n";
        long sdc2 = SetDisplayConfig(NumPathArrayElements, &PathInfoArray2[0], 0, NULL, (SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES));
        std::cout << "SetDisplayConfig SDC_APPLY " << sdc2 << "\n";
    }

    return 0;
}
