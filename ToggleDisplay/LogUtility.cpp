// ToggleDisplay
// Copyright 2024, SGrottel
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
#include "LogUtility.h"

#define VC_EXTRALEAN
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <shlobj_core.h>

#include "SimpleLog/SimpleLog.hpp"

#include <sstream>

namespace
{
    std::string W2A(std::wstring const& w)
    {
        std::string a(w.size(), '?');
        for (size_t i = 0; i < w.size(); ++i)
        {
            if (w[i] < 128) a[i] = static_cast<char>(w[i]);
        }
        return std::move(a);
    }

    // copied from <d3dkmdt.h> because including does not work, because ... Microsoft says: no.
    typedef enum _D3DKMDT_VIDEO_SIGNAL_STANDARD
    {
        D3DKMDT_VSS_UNINITIALIZED = 0,

        // VESA standards
        D3DKMDT_VSS_VESA_DMT = 1,    // See VESA Display Monitor Timings specification
        D3DKMDT_VSS_VESA_GTF = 2,    // See VESA Generalized Timing Formula standard
        D3DKMDT_VSS_VESA_CVT = 3,    // See VESA Coordinated Video Timings standard

        // De-facto standards
        D3DKMDT_VSS_IBM = 4,
        D3DKMDT_VSS_APPLE = 5,

        // Legacy STV standards                 W  x H{i|p} @ (  VR        /      HR       )
        D3DKMDT_VSS_NTSC_M = 6,    //  720 x 525i   @ (59.94  [Hz] /  15,734.27[Hz])
        D3DKMDT_VSS_NTSC_J = 7,    //  720 x 525i   @ (59.94  [Hz] /  15,734.27[Hz])
        D3DKMDT_VSS_NTSC_443 = 8,    //  720 x 525i   @ (59.94  [Hz] /  15,734.27[Hz])
        D3DKMDT_VSS_PAL_B = 9,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_B1 = 10,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_G = 11,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_H = 12,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_I = 13,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_D = 14,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_N = 15,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_NC = 16,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_B = 17,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_D = 18,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_G = 19,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_H = 20,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_K = 21,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_K1 = 22,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_L = 23,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_SECAM_L1 = 24,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])

        // CEA/EIA standards
        D3DKMDT_VSS_EIA_861 = 25,
        D3DKMDT_VSS_EIA_861A = 26,
        D3DKMDT_VSS_EIA_861B = 27,

        // More legacy STV standards
        D3DKMDT_VSS_PAL_K = 28,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_K1 = 29,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_L = 30,    //  720 x 625i   @ (50     [Hz] /  15,625   [Hz])
        D3DKMDT_VSS_PAL_M = 31,    //  720 x 525i   @ (59.94  [Hz] /  15,734   [Hz])

        D3DKMDT_VSS_OTHER = 255
    }
    D3DKMDT_VIDEO_SIGNAL_STANDARD;

    template<class Stream>
    Stream& operator<<(Stream& s, DISPLAYCONFIG_RATIONAL const& r)
    {
        s << r.Numerator << "/" << r.Denominator;
        return s;
    }

    template<class Stream>
    Stream& operator<<(Stream& s, LUID const& id)
    {
        s << id.HighPart << ", " << id.LowPart;
        return s;
    }

    void LogMode(sgrottel::ISimpleLog& log, DisplayConfig::ModeInfo const* mode)
    {
        std::stringstream str;
        str << "Mode ";
        if (mode != nullptr)
        {
            switch (mode->infoType)
            {
            case DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE: str << "SOURCE "; break;
            case DISPLAYCONFIG_MODE_INFO_TYPE_TARGET: str << "TARGET "; break;
            case DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE: str << "DesktopImage "; break;
            default: str << "unknown "; break;
            }

            str << mode->id << " Adapter(" << mode->adapterId << ")";

            switch (mode->infoType)
            {
            case DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE:
                str << "\n\tsize: " << mode->sourceMode.width
                    << ", " << mode->sourceMode.height
                    << "\n\tformat: ";
                switch (mode->sourceMode.pixelFormat)
                {
                case DISPLAYCONFIG_PIXELFORMAT_8BPP: str << "8BPP"; break;
                case DISPLAYCONFIG_PIXELFORMAT_16BPP: str << "16BPP"; break;
                case DISPLAYCONFIG_PIXELFORMAT_24BPP: str << "24BPP"; break;
                case DISPLAYCONFIG_PIXELFORMAT_32BPP: str << "32BPP"; break;
                case DISPLAYCONFIG_PIXELFORMAT_NONGDI: str << "NONGDI"; break;
                default: str << "UNKNOWN"; break;
                }
                str << "\n\tpos: " << mode->sourceMode.position.x
                    << ", " << mode->sourceMode.position.y;
                break;
            case DISPLAYCONFIG_MODE_INFO_TYPE_TARGET:
                str << "\n\tpxRt: " << mode->targetMode.targetVideoSignalInfo.pixelRate
                    << "\n\thSync: " << mode->targetMode.targetVideoSignalInfo.hSyncFreq
                    << "\n\tvSync: " << mode->targetMode.targetVideoSignalInfo.vSyncFreq
                    << "\n\taSize: (" << mode->targetMode.targetVideoSignalInfo.activeSize.cx << ", " << mode->targetMode.targetVideoSignalInfo.activeSize.cy << ")"
                    << "\n\ttSize: (" << mode->targetMode.targetVideoSignalInfo.totalSize.cx << ", " << mode->targetMode.targetVideoSignalInfo.totalSize.cy << ")";
                str << "\n\tvStd: ";
                switch (mode->targetMode.targetVideoSignalInfo.AdditionalSignalInfo.videoStandard) {
                case D3DKMDT_VSS_UNINITIALIZED: str << "UNINITIALIZED"; break;
                case D3DKMDT_VSS_VESA_DMT: str << "VESA_DMT"; break;
                case D3DKMDT_VSS_VESA_GTF: str << "VESA_GTF"; break;
                case D3DKMDT_VSS_VESA_CVT: str << "VESA_CVT"; break;
                case D3DKMDT_VSS_IBM: str << "IBM"; break;
                case D3DKMDT_VSS_APPLE: str << "APPLE"; break;
                case D3DKMDT_VSS_NTSC_M: str << "NTSC_M"; break;
                case D3DKMDT_VSS_NTSC_J: str << "NTSC_J"; break;
                case D3DKMDT_VSS_NTSC_443: str << "NTSC_443"; break;
                case D3DKMDT_VSS_PAL_B: str << "PAL_B"; break;
                case D3DKMDT_VSS_PAL_B1: str << "PAL_B1"; break;
                case D3DKMDT_VSS_PAL_G: str << "PAL_G"; break;
                case D3DKMDT_VSS_PAL_H: str << "PAL_H"; break;
                case D3DKMDT_VSS_PAL_I: str << "PAL_I"; break;
                case D3DKMDT_VSS_PAL_D: str << "PAL_D"; break;
                case D3DKMDT_VSS_PAL_N: str << "PAL_N"; break;
                case D3DKMDT_VSS_PAL_NC: str << "PAL_NC"; break;
                case D3DKMDT_VSS_SECAM_B: str << "SECAM_B"; break;
                case D3DKMDT_VSS_SECAM_D: str << "SECAM_D"; break;
                case D3DKMDT_VSS_SECAM_G: str << "SECAM_G"; break;
                case D3DKMDT_VSS_SECAM_H: str << "SECAM_H"; break;
                case D3DKMDT_VSS_SECAM_K: str << "SECAM_K"; break;
                case D3DKMDT_VSS_SECAM_K1: str << "SECAM_K1"; break;
                case D3DKMDT_VSS_SECAM_L: str << "SECAM_L"; break;
                case D3DKMDT_VSS_SECAM_L1: str << "SECAM_L1"; break;
                case D3DKMDT_VSS_EIA_861: str << "EIA_861"; break;
                case D3DKMDT_VSS_EIA_861A: str << "EIA_861A"; break;
                case D3DKMDT_VSS_EIA_861B: str << "EIA_861B"; break;
                case D3DKMDT_VSS_PAL_K: str << "PAL_K"; break;
                case D3DKMDT_VSS_PAL_K1: str << "PAL_K1"; break;
                case D3DKMDT_VSS_PAL_L: str << "PAL_L"; break;
                case D3DKMDT_VSS_PAL_M: str << "PAL_M"; break;
                case D3DKMDT_VSS_OTHER: str << "OTHER"; break;
                default: str << "UNKNOWN"; break;
                }
                str << ", vSyncDiv: " << mode->targetMode.targetVideoSignalInfo.AdditionalSignalInfo.vSyncFreqDivider;
                str << "\n\tscanline: ";
                switch (mode->targetMode.targetVideoSignalInfo.scanLineOrdering)
                {
                case DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED: str << "UNSPECIFIED"; break;
                case DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE: str << "PROGRESSIVE"; break;
                case DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED: str << "INTERLACED"; break;
                case DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST: str << "INTERLACED_LOWERFIELDFIRST"; break;
                default: str << "UNKNOWN"; break;
                }
                break;
            case DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE:
                str << "DesktopImage " << mode->id << " Adapter(" << mode->adapterId << ")";
                str << "\n\tsrcSize: " << mode->desktopImageInfo.PathSourceSize.x << ", " << mode->desktopImageInfo.PathSourceSize.y;
                str << "\n\timage: " << mode->desktopImageInfo.DesktopImageRegion.left
                    << ", " << mode->desktopImageInfo.DesktopImageRegion.top
                    << ", " << mode->desktopImageInfo.DesktopImageRegion.right
                    << ", " << mode->desktopImageInfo.DesktopImageRegion.bottom;
                str << "\n\tclip: " << mode->desktopImageInfo.DesktopImageClip.left
                    << ", " << mode->desktopImageInfo.DesktopImageClip.top
                    << ", " << mode->desktopImageInfo.DesktopImageClip.right
                    << ", " << mode->desktopImageInfo.DesktopImageClip.bottom;
                break;
            default:
                str << "unknown";
                break;
            }
        }
        else
        {
            str << "nullptr";
        }
        log.Detail(str.str().c_str());
    }

}

void LogPath(sgrottel::ISimpleLog& log, DisplayConfig::PathInfo const* path)
{
    std::stringstream str;
    str << "Path ";
    if (path != nullptr)
    {
        std::string deviceName = W2A(DisplayConfig::GetGdiDeviceName(*path));
        str << "\n\tsrc: " << deviceName << " :: " << path->sourceInfo.id << " Adapter(" << path->sourceInfo.adapterId << ")";
        if ((path->flags & DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) == DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE)
        {
            str << "\n\t\tsrc_ClnGrpId:" << path->sourceInfo.cloneGroupId;
            str << "\n\t\tsrc_srcModeInfoIdx:" << path->sourceInfo.sourceModeInfoIdx;
        }
        else
        {
            str << "\n\t\tsrc_ModeInfoIdx:" << path->sourceInfo.modeInfoIdx;
        }
        str << "\n\t\tsrc_flags:";
        if ((path->sourceInfo.statusFlags & DISPLAYCONFIG_SOURCE_IN_USE) == DISPLAYCONFIG_SOURCE_IN_USE) str << " IN_USE";

        DisplayConfig::TargetDeviceName targetDeviceName = DisplayConfig::GetTargetDeviceName(*path);
        str << "\n\ttar: " << W2A(targetDeviceName.name) << " (" << W2A(targetDeviceName.path) << ") :: " << path->targetInfo.id << " Adapter(" << path->targetInfo.adapterId << ")";
        uint32_t preferedModeId = DisplayConfig::GetTargetPreferedModeId(*path);
        str << "\n\t\ttar_prefMode: " << preferedModeId;
        if ((path->flags & DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) == DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE)
        {
            str << "\n\t\ttar_dsktpModeInfoIdx:" << path->targetInfo.desktopModeInfoIdx;
            str << "\n\t\ttar_tarModeInfoIdx:" << path->targetInfo.targetModeInfoIdx;
        }
        else
        {
            str << "\n\t\ttar_modeInfoIdx:" << path->targetInfo.modeInfoIdx;
        }

        str << "\n\t\ttar_outTech: ";
        switch (path->targetInfo.outputTechnology)
        {
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER: str << "OTHER"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15: str << "HD15"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO: str << "SVIDEO"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO: str << "COMPOSITE_VIDEO"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO: str << "COMPONENT_VIDEO"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI: str << "DVI"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI: str << "HDMI"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS: str << "LVDS"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN: str << "D_JPN"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI: str << "SDI"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL: str << "DISPLAYPORT_EXTERNAL"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED: str << "DISPLAYPORT_EMBEDDED"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL: str << "UDI_EXTERNAL"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED: str << "UDI_EMBEDDED"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE: str << "SDTVDONGLE"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST: str << "MIRACAST"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_WIRED: str << "INDIRECT_WIRED"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_VIRTUAL: str << "INDIRECT_VIRTUAL"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_USB_TUNNEL: str << "DISPLAYPORT_USB_TUNNEL"; break;
        case DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL: str << "INTERNAL"; break;
        default: str << "UNKNOWN"; break;
        }
        str << "\n\t\ttar_rot: ";
        switch (path->targetInfo.rotation)
        {
        case DISPLAYCONFIG_ROTATION_IDENTITY: str << "IDENTITY"; break;
        case DISPLAYCONFIG_ROTATION_ROTATE90: str << "ROTATE90"; break;
        case DISPLAYCONFIG_ROTATION_ROTATE180: str << "ROTATE180"; break;
        case DISPLAYCONFIG_ROTATION_ROTATE270: str << "ROTATE270"; break;
        default: str << "UNKNOWN"; break;
        }
        str << "\n\t\ttar_scale: ";
        switch (path->targetInfo.scaling)
        {
        case DISPLAYCONFIG_SCALING_IDENTITY: str << "IDENTITY"; break;
        case DISPLAYCONFIG_SCALING_CENTERED: str << "CENTERED"; break;
        case DISPLAYCONFIG_SCALING_STRETCHED: str << "STRETCHED"; break;
        case DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX: str << "ASPECTRATIOCENTEREDMAX"; break;
        case DISPLAYCONFIG_SCALING_CUSTOM: str << "CUSTOM"; break;
        case DISPLAYCONFIG_SCALING_PREFERRED: str << "PREFERRED"; break;
        default: str << "UNKNOWN"; break;
        }
        str << "\n\t\ttar_rate: " << path->targetInfo.refreshRate;
        str << "\n\t\ttar_scanline: ";
        switch (path->targetInfo.scanLineOrdering)
        {
        case DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED: str << "UNSPECIFIED"; break;
        case DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE: str << "PROGRESSIVE"; break;
        case DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED: str << "INTERLACED"; break;
        case DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST: str << "INTERLACED_LOWERFIELDFIRST"; break;
        default: str << "UNKNOWN"; break;
        }
        str << "\n\t\ttar_available: " << (path->targetInfo.targetAvailable == TRUE) ? "TRUE" : "FALSE";
        str << "\n\t\ttar_flags:";
        if ((path->targetInfo.statusFlags & DISPLAYCONFIG_TARGET_IN_USE) == DISPLAYCONFIG_TARGET_IN_USE) str << " IN_USE";
        if ((path->targetInfo.statusFlags & DISPLAYCONFIG_TARGET_FORCIBLE) == DISPLAYCONFIG_TARGET_FORCIBLE) str << " FORCIBLE";
        if ((path->targetInfo.statusFlags & DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_BOOT) == DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_BOOT) str << " FORCED_AVAILABILITY_BOOT";
        if ((path->targetInfo.statusFlags & DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_PATH) == DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_PATH) str << " FORCED_AVAILABILITY_PATH";
        if ((path->targetInfo.statusFlags & DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_SYSTEM) == DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_SYSTEM) str << " FORCED_AVAILABILITY_SYSTEM";
        if ((path->targetInfo.statusFlags & DISPLAYCONFIG_TARGET_IS_HMD) == DISPLAYCONFIG_TARGET_IS_HMD) str << " IS_HMD";

        str << "\n\tflags:";
        if ((path->flags & DISPLAYCONFIG_PATH_ACTIVE) == DISPLAYCONFIG_PATH_ACTIVE) str << " ACTIVE";
        if ((path->flags & DISPLAYCONFIG_PATH_PREFERRED_UNSCALED) == DISPLAYCONFIG_PATH_PREFERRED_UNSCALED) str << " PREFERRED_UNSCALED";
        if ((path->flags & DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) == DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE) str << " SUPPORT_VIRTUAL_MODE";
    }
    else
    {
        str << "nullptr";
    }
    log.Detail(str.str().c_str());
}

void LogPaths(sgrottel::ISimpleLog& log, DisplayConfig::PathsVector const& paths)
{
    for (auto const& path : paths)
    {
        LogPath(log, &path);
    }
}

void LogModes(sgrottel::ISimpleLog& log, DisplayConfig::ModesVector const& modes)
{
    for (auto const& mode : modes)
    {
        LogMode(log, &mode);
    }
}
