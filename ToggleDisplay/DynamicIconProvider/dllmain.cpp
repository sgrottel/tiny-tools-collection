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

    struct bgra {
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

        for (size_t y = 0; y < height; ++y)
        {
            size_t sy = (y * (size.cy - 1)) / (height - 1);
            for (size_t x = 0; x < width; ++x)
            {
                size_t sx = (x * (size.cx - 1)) / (width - 1);

                data[x + y * width] = bmp[sx + sy * size.cx];
            }
        }

        DeleteObject(dc);
        ReleaseDC(NULL, hdcScreen);
    }

    struct Area {
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

}

extern "C" int __declspec(dllexport) openhere_generateicon(int width, int height, unsigned char* bgradata)
{
    LoadBackground(width, height, reinterpret_cast<bgra*>(bgradata));
    const Area drawArea = SelectIconDrawArea(width, height);

    for (int y = drawArea.y1; y < drawArea.y2; ++y)
        for (int x = drawArea.x1; x < drawArea.x2; ++x)
        {
            unsigned char* c = bgradata + (x + (height - 1 - y) * width) * 4;
            c[2] = 255;
        }

    //for (int y = 0; y < height; ++y)
    //{
    //    float fy = static_cast<float>(y) / static_cast<float>(height - 1);
    //    for (int x = 0; x < width; ++x)
    //    {
    //        float fx = static_cast<float>(x) / static_cast<float>(width - 1);

    //        unsigned char* c = bgradata + (x + y * width) * 4;

    //        float dx = (fx - 0.5f) * 2.0f;
    //        float dy = (fy - 0.5f) * 2.0f;
    //        float dr = dx * dx + dy * dy;

    //        float a = (1.0f - dr) * 5.0f;
    //        if (a < 0.0f) a = 0.0;
    //        if (a > 1.0f) a = 1.0f;

    //        c[0] = static_cast<unsigned char>(a * 255.0f);
    //        c[1] = static_cast<unsigned char>(a * fy * 255.0f);
    //        c[2] = static_cast<unsigned char>(a * fx * 255.0f);
    //        c[3] = static_cast<unsigned char>(a * 255.0f);

    //    }
    //}

    return TRUE;
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
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

