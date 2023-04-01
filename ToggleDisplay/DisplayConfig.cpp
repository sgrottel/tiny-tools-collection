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
#include "DisplayConfig.h"

#include <algorithm>
#include <unordered_set>

DisplayConfig::ReturnCode DisplayConfig::Query(QueryScope scope, PathsVector& outPaths, ModesVector& outModes, bool virtualAware)
{
    UINT32 flags = 0;
    switch (scope)
    {
    case QueryScope::AllPaths:
        flags |= QDC_ALL_PATHS;
        break;
    case QueryScope::OnlyActivePaths:
        flags |= QDC_ONLY_ACTIVE_PATHS;
        break;
    default:
        return ReturnCode::InvalidParameter;
    }
    if (virtualAware)
    {
        flags |= QDC_VIRTUAL_MODE_AWARE;
    }

    LONG result = -1;

    UINT32 pathCount = 0, modeCount = 0;
    do
    {
        // Determine how many path and mode structures to allocate
        result = GetDisplayConfigBufferSizes(flags, &pathCount, &modeCount);
        if (result != ERROR_SUCCESS)
        {
            return MapReturnCode(result);
        }
        // Allocate the path and mode arrays
        outPaths.resize(pathCount);
        outModes.resize(modeCount);

        // Get all active paths and their modes
        result = QueryDisplayConfig(flags, &pathCount, outPaths.data(), &modeCount, outModes.data(), nullptr);

        // It's possible that between the call to GetDisplayConfigBufferSizes and QueryDisplayConfig
        // that the display state changed, so loop on the case of ERROR_INSUFFICIENT_BUFFER.
    } while (result == ERROR_INSUFFICIENT_BUFFER);

    // The function may have returned fewer paths/modes than estimated
    outPaths.resize(pathCount);
    outModes.resize(modeCount);

    return MapReturnCode(result);
}

DisplayConfig::ReturnCode DisplayConfig::Apply(PathsVector& paths)
{
    // clear modeInfoIdx fields, as the API will be requested to newly align the mode info data
    for (PathInfo& path : paths)
    {
        path.sourceInfo.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
        path.targetInfo.modeInfoIdx = DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
    }

    // validate
    long result = SetDisplayConfig(static_cast<uint32_t>(paths.size()), paths.data(), 0, nullptr, SDC_VALIDATE | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES);
    if (result != ERROR_SUCCESS)
    {
        return MapReturnCode(result);
    }

    result = SetDisplayConfig(static_cast<uint32_t>(paths.size()), paths.data(), 0, nullptr, SDC_APPLY | SDC_TOPOLOGY_SUPPLIED | SDC_ALLOW_PATH_ORDER_CHANGES);
    return MapReturnCode(result);
}

void DisplayConfig::FilterPaths(PathsVector& paths)
{
    // Remove all paths without available target
    paths.erase(std::remove_if(paths.begin(), paths.end(), [](PathInfo const& p) { return !p.targetInfo.targetAvailable; }), paths.end());

    // All enabled paths first
    std::sort(paths.begin(), paths.end(),
        [](PathInfo const& a, PathInfo const& b)
        {
            bool ae = IsEnabled(a);
            bool be = IsEnabled(b);
            if (ae == be)
            {
                return a.sourceInfo.id < b.sourceInfo.id;
            }
            return ae > be;
        }
    );

    // Remove all alternative disabled paths where there are enabled paths
    std::unordered_set<std::wstring> enabledDisplays;
    std::unordered_set<std::wstring> enabledTargetDevices;
    for(PathInfo const& p : paths)
    {
        if (!IsEnabled(p)) continue;
        std::wstring srcName = GetGdiDeviceName(p);
        enabledDisplays.insert(srcName);
        DisplayConfig::TargetDeviceName tarName = GetTargetDeviceName(p);
        enabledTargetDevices.insert(tarName.path);
    }
    paths.erase(std::remove_if(paths.begin(), paths.end(),
        [&enabledDisplays, &enabledTargetDevices](PathInfo const& p)
        {
            if (IsEnabled(p)) return false;

            std::wstring srcName = GetGdiDeviceName(p);
            if (enabledDisplays.find(srcName) != enabledDisplays.end()) return true;

            // these could be interesting for cloning. I don't care for now
            DisplayConfig::TargetDeviceName tarName = GetTargetDeviceName(p);
            if (enabledTargetDevices.find(tarName.path) != enabledTargetDevices.end()) return true;

            return false;
        }
    ), paths.end());

    // If multiple disabled paths only differ in source 'id' (EVERYTHING else identical), then only keep the one with the smallest id
    std::unordered_set<PathInfo const*> toDelete;
    auto end = paths.end();
    for (auto it = paths.begin(); it != end; ++it)
    {
        PathInfo a = *it; // copy!
        a.sourceInfo.id = 0;
        if (IsEnabled(*it)) continue;
        for (auto it2 = paths.begin(); it2 != it; ++it2)
        {
            PathInfo b = *it2; // copy!
            b.sourceInfo.id = 0;
            if (memcmp(&a, &b, sizeof(PathInfo)) == 0)
            {
                toDelete.insert(&(*it));
                break;
            }
        }
    }
    if (toDelete.size() > 0) {
        paths.erase(std::remove_if(paths.begin(), paths.end(),
            [&toDelete](PathInfo const& p)
            {
                if (IsEnabled(p)) return false;
                return toDelete.find(&p) != toDelete.end();
            }
        ), paths.end());
    }

}

DisplayConfig::PathInfo* DisplayConfig::FindPath(PathsVector& paths, std::wstring const& id)
{
    std::wstring needle{id};
    std::transform(needle.begin(), needle.end(), needle.begin(), std::tolower);

    for (PathInfo& path : paths)
    {
        std::wstring srcName = GetGdiDeviceName(path);
        std::transform(srcName.begin(), srcName.end(), srcName.begin(), std::tolower);
        if (needle == srcName) return &path;

        DisplayConfig::TargetDeviceName tarName = GetTargetDeviceName(path);
        std::transform(tarName.name.begin(), tarName.name.end(), tarName.name.begin(), std::tolower);
        if (needle == tarName.name) return &path;
        std::transform(tarName.path.begin(), tarName.path.end(), tarName.path.begin(), std::tolower);
        if (needle == tarName.path) return &path;
    }

    return nullptr;
}

std::wstring DisplayConfig::GetGdiDeviceName(PathInfo const& path)
{
    DISPLAYCONFIG_SOURCE_DEVICE_NAME sourceName = {};
    sourceName.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
    sourceName.header.size = sizeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME);
    sourceName.header.adapterId = path.sourceInfo.adapterId;
    sourceName.header.id = path.sourceInfo.id;
    auto ret = MapReturnCode(DisplayConfigGetDeviceInfo(&sourceName.header));
    return (ret == ReturnCode::Success)
        ? std::wstring{sourceName.viewGdiDeviceName}
        : std::wstring{};
}

DisplayConfig::TargetDeviceName DisplayConfig::GetTargetDeviceName(PathInfo const& path)
{
    DISPLAYCONFIG_TARGET_DEVICE_NAME targetName = {};
    targetName.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
    targetName.header.size = sizeof(DISPLAYCONFIG_TARGET_DEVICE_NAME);
    targetName.header.adapterId = path.targetInfo.adapterId;
    targetName.header.id = path.targetInfo.id;
    auto ret = MapReturnCode(DisplayConfigGetDeviceInfo(&targetName.header));
    TargetDeviceName name{};
    if (ret == ReturnCode::Success)
    {
        name.name = targetName.monitorFriendlyDeviceName;
        name.path = targetName.monitorDevicePath;

        if (name.name.empty())
        {
            if (path.targetInfo.outputTechnology == DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL)
            {
                name.name = L"<internal>";
            }
        }
    }
    return name;
}

uint32_t DisplayConfig::GetTargetPreferedModeId(PathInfo const& path)
{
    DISPLAYCONFIG_TARGET_PREFERRED_MODE preferedMode = {};
    preferedMode.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE;
    preferedMode.header.size = sizeof(DISPLAYCONFIG_TARGET_PREFERRED_MODE);
    preferedMode.header.adapterId = path.targetInfo.adapterId;
    preferedMode.header.id = path.targetInfo.id;
    auto ret = MapReturnCode(DisplayConfigGetDeviceInfo(&preferedMode.header));
    return (ret == ReturnCode::Success)
        ? preferedMode.header.id
        : 0xffffffff;
}

bool DisplayConfig::IsEnabled(PathInfo const& path)
{
    return (path.flags & DISPLAYCONFIG_PATH_ACTIVE) != 0;
}

void DisplayConfig::SetEnabled(PathInfo& path)
{
    path.flags |= DISPLAYCONFIG_PATH_ACTIVE;
}

void DisplayConfig::SetDisabled(PathInfo& path)
{
    path.flags &= ~DISPLAYCONFIG_PATH_ACTIVE;
}

std::string DisplayConfig::to_string(ReturnCode code)
{
    switch (code)
    {
    case ReturnCode::Success: return "ERROR_SUCCESS";
    case ReturnCode::InvalidParameter: return "ERROR_INVALID_PARAMETER";
    case ReturnCode::NotSupported: return "ERROR_NOT_SUPPORTED";
    case ReturnCode::AccessDenied: return "ERROR_ACCESS_DENIED";
    case ReturnCode::GenFailure: return "ERROR_GEN_FAILURE";
    case ReturnCode::BadConfiguration: return "ERROR_BAD_CONFIGURATION";
    case ReturnCode::InsufficientBuffer: return "ERROR_INSUFFICIENT_BUFFER";
    }
    return "Unknown";
}

DisplayConfig::ReturnCode DisplayConfig::MapReturnCode(long code)
{
    switch (code)
    {
    case ERROR_SUCCESS: return ReturnCode::Success;
    case ERROR_INVALID_PARAMETER: return ReturnCode::InvalidParameter;
    case ERROR_NOT_SUPPORTED: return ReturnCode::NotSupported;
    case ERROR_ACCESS_DENIED: return ReturnCode::AccessDenied;
    case ERROR_GEN_FAILURE: return ReturnCode::GenFailure;
    case ERROR_BAD_CONFIGURATION: return ReturnCode::BadConfiguration;
    case ERROR_INSUFFICIENT_BUFFER: return ReturnCode::InsufficientBuffer;
    }
    return ReturnCode::Unknown;
}
