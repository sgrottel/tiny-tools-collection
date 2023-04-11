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

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellscalingapi.h>

#include <vector>

namespace
{

    SIZE GetIconSize(HICON icon)
    {
        ICONINFO ii;
        if (!GetIconInfo(icon, &ii)) return { 0, 0 };
        if (ii.hbmMask) DeleteObject(ii.hbmMask);
        if (!ii.hbmColor) return { 0, 0 };
        BITMAP bmp;
        ZeroMemory(&bmp, sizeof(BITMAP));
        GetObject(ii.hbmColor, sizeof(BITMAP), &bmp);
        DeleteObject(ii.hbmColor);
        return { bmp.bmWidth, bmp.bmHeight };
    }

    HMODULE g_hModule = nullptr;

    struct bgra
    {
        uint8_t b, g, r, a;
    };

    void LoadBackground(int width, int height, bgra* data)
    {
        HICON icon = static_cast<HICON>(LoadImageW(g_hModule, MAKEINTRESOURCEW(1), IMAGE_ICON, width, height, LR_SHARED));
        SIZE size = GetIconSize(icon);

        std::vector<bgra> bmp;
        bmp.resize(size.cx * size.cy);
        for (bgra& c : bmp) c = bgra{ 0, 0, 0, 0 };
        HBITMAP hbmp = CreateBitmap(size.cx, size.cy, 1, 32, bmp.data());

        HDC hdcScreen = GetDC(NULL);
        HDC dc = CreateCompatibleDC(hdcScreen);
        SelectObject(dc, hbmp);

        DrawIconEx(dc, 0, 0, icon, size.cx, size.cy, 0, 0, DI_NORMAL);

        BITMAPINFOHEADER bi{ 0 };
        bi.biSize = sizeof(BITMAPINFOHEADER);
        bi.biWidth = size.cx;
        bi.biHeight = size.cy;
        bi.biPlanes = 1;
        bi.biBitCount = 32;
        bi.biCompression = BI_RGB;
        GetDIBits(dc, hbmp, 0, size.cy, bmp.data(), reinterpret_cast<BITMAPINFO*>(&bi), DIB_RGB_COLORS);

        for (size_t y = 0; y < static_cast<size_t>(height); ++y)
        {
            size_t sy = (y * (size.cy - 1)) / (height - 1);
            for (size_t x = 0; x < static_cast<size_t>(width); ++x)
            {
                size_t sx = (x * (size.cx - 1)) / (width - 1);

                data[x + y * width] = bmp[sx + sy * size.cx];
            }
        }

        DeleteObject(dc);
        ReleaseDC(NULL, hdcScreen);
    }

    struct Area
    {
        int size, x1, y1, x2, y2;
    };

    static constexpr const Area c_IconDrawAreas[] = {
        { 16, 1, 2, 15, 12 },
        { 24, 2, 4, 22, 17 },
        { 32, 3, 5, 29, 23 },
        { 48, 4, 7, 44, 35 },
        { 64, 5, 9, 59, 47 },
        { 256, 20, 36, 236, 188 }
    };
    static constexpr const size_t c_IconDrawAreasCount = sizeof(c_IconDrawAreas) / sizeof(Area);

    Area InterpolateIconDrawArea(Area const& area, int width, int height)
    {
        Area a{ area.size };
        a.x1 = (area.x1 * width) / area.size;
        a.y1 = (area.y1 * height) / area.size;
        a.x2 = (area.x2 * width) / area.size;
        a.y2 = (area.y2 * height) / area.size;

        a.x1++;
        a.y1++;
        a.x2 -= 2;
        a.y2 -= 2;

        return a;
    }

    Area SelectIconDrawArea(int width, int height)
    {
        int s = std::max<int>(width, height);
        for (size_t i = 0; i < c_IconDrawAreasCount - 1; ++i)
        {
            if (s <= c_IconDrawAreas[i].size) return InterpolateIconDrawArea(c_IconDrawAreas[i], width, height);
        }
        return InterpolateIconDrawArea(c_IconDrawAreas[c_IconDrawAreasCount - 1], width, height);
    }

    BOOL CALLBACK collectMonitors(HMONITOR hMon, HDC hDC, LPRECT rect, LPARAM param)
    {
        std::vector<RECT> *monitors = reinterpret_cast<std::vector<RECT> *>(param);
        monitors->push_back(*rect);
        return TRUE;
    }

    void ScaleMonitors(const Area& area, std::vector<RECT>& monitors)
    {
        long minX = 0, maxX = 0, minY = 0, maxY = 0;
        for (RECT& m : monitors)
        {
            if (m.left > m.right) std::swap(m.left, m.right);
            if (m.top > m.bottom) std::swap(m.top, m.bottom);
            minX = std::min<long>(minX, m.left);
            minY = std::min<long>(minY, m.top);
            maxX = std::max<long>(maxX, m.right);
            maxY = std::max<long>(maxY, m.bottom);
        }

        long w = maxX - minX;
        long h = maxY - minY;
        long aw = area.x2 - area.x1;
        long ah = area.y2 - area.y1;

        {
            long p = aw * h / w;
            if (p < ah)
            {
                ah = p;
            }
            else
            {
                aw = ah * w / h;
            }
        }

        long ax = area.x1 + (area.x2 - area.x1 - aw) / 2;
        long ay = area.y1 + (area.y2 - area.y1 - ah) / 2;

        for (RECT& m : monitors)
        {
            m.left = ax + (m.left - minX) * aw / w;
            m.top = ay + (m.top - minY) * ah / h;
            m.right = ax + (m.right - minX) * aw / w;
            m.bottom = ay + (m.bottom - minY) * ah / h;
        }
    }

    void DrawMonitor(const RECT& mon, int width, int height, bgra* data)
    {
        int x, y;
        bgra* c;
        bgra w{ 224, 177, 160, 255 };
        for (x = mon.left; x < mon.right; x++)
        {
            y = mon.top;
            for (y = mon.top; y < mon.bottom; y++)
            {
                c = data + x + (width - 1 - y) * width;
                c->r = 31 + c->r / 2;
                c->g = 47 + c->g / 2;
                c->b = 95 + c->b / 2;
                c->a = 255;
            }
            y = mon.top;
            c = data + x + (width - 1 - y) * width;
            *c = w;
            y = mon.bottom;
            c = data + x + (width - 1 - y) * width;
            *c = w;
        }
        for (y = mon.top; y < mon.bottom; y++)
        {
            x = mon.left;
            c = data + x + (width - 1 - y) * width;
            *c = w;
            x = mon.right;
            c = data + x + (width - 1 - y) * width;
            *c = w;
        }
    }

}

extern "C" int __declspec(dllexport) openhere_generateicon(int width, int height, unsigned char* bgradata)
{
    SetProcessDpiAwareness(PROCESS_PER_MONITOR_DPI_AWARE);

    LoadBackground(width, height, reinterpret_cast<bgra*>(bgradata));
    const Area drawArea = SelectIconDrawArea(width, height);
    std::vector<RECT> monitors;
    EnumDisplayMonitors(nullptr, nullptr, &collectMonitors, reinterpret_cast<LPARAM>(&monitors));
    if (monitors.size() > 0)
    {
        ScaleMonitors(drawArea, monitors);
        for (RECT const& m : monitors)
        {
            DrawMonitor(m, width, height, reinterpret_cast<bgra*>(bgradata));
        }
    }

    return TRUE;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hModule = hModule;
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

