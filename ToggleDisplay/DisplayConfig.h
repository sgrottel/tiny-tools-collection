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
#pragma once
#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include <string>
#include <vector>

class DisplayConfig
{
public:
    typedef DISPLAYCONFIG_PATH_INFO PathInfo;
    typedef DISPLAYCONFIG_MODE_INFO ModeInfo;
    typedef std::vector<DISPLAYCONFIG_PATH_INFO> PathsVector;
    typedef std::vector<DISPLAYCONFIG_MODE_INFO> ModesVector;

    enum class QueryScope {
        AllPaths,
        OnlyActivePaths
    };

    enum class ReturnCode {
        Unknown,
        Success, // The function succeeded.
        InvalidParameter, // The combination of parameters and flags that are specified is invalid.
        NotSupported, // The system is not running a graphics driver that was written according to the Windows Display Driver Model(WDDM).The function is only supported on a system with a WDDM driver running.
        AccessDenied, // The caller does not have access to the console session.This error occurs if the calling process does not have access to the current desktop or is running on a remote session.
        GenFailure, // An unspecified error occurred.
        InsufficientBuffer, // The supplied path and mode buffer are too small.
        BadConfiguration // The function could not find a workable solution for the source and target modes that the caller did not specify.
    };

    struct TargetDeviceName {
        std::wstring name;
        std::wstring path;
    };

    // QueryDisplayConfig
    // Allocates the required buffers inside the provided vectors and queries the system API
    // The values of `outPaths` and `outModes` is only defined when the function returns `Success`
    static ReturnCode Query(QueryScope scope, PathsVector& outPaths, ModesVector& outModes, bool virtualAware = false);

    // Filter and sort `paths`
    //
    // All enabled paths first
    // Remove all paths without available target
    // Remove all alternative disabled paths where there are enabled paths
    // If multiple disabled paths only differ in source 'id' (EVERYTHING else identical), then only keep the one with the smallest id
    //
    // The filtering logic is based on code by `PuFF1k`:
    // https://stackoverflow.com/a/62038912/552373
    static void FilterPaths(PathsVector& paths);

    static std::wstring GetGdiDeviceName(PathInfo const& path);
    static TargetDeviceName GetTargetDeviceName(PathInfo const& path);
    static uint32_t GetTargetPreferedModeId(PathInfo const& path);
    static bool IsEnabled(PathInfo const& path);

    static std::string to_string(ReturnCode code);

private:

    static ReturnCode MapReturnCode(long code);

};
