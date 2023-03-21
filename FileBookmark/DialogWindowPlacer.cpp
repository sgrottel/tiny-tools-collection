// FileBookmark
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
#include "DialogWindowPlacer.h"

#include <vector>
#include <chrono>
#include <algorithm>

using filebookmark::DialogWindowPlacer;

namespace
{
	struct WindowCollectorContext {
		DWORD processId;
		std::vector<HWND> wnds;
	};

	BOOL CALLBACK CollectProcessWindows(_In_ HWND hWnd, _In_ LPARAM lParam)
	{
		WindowCollectorContext* context = reinterpret_cast<WindowCollectorContext*>(lParam);
		DWORD processId = 0;
		DWORD threadId = ::GetWindowThreadProcessId(hWnd, &processId);

		if (IsWindowVisible(hWnd) && (processId == context->processId))
		{
			context->wnds.push_back(hWnd);
		}
		return TRUE;
	}

	struct MonitorCollectorContext {
		std::vector<RECT> rects;
	};

	BOOL CALLBACK CollectMonitors(HMONITOR monitor, HDC deviceContext, LPRECT rect, LPARAM lParam)
	{
		MonitorCollectorContext* context = reinterpret_cast<MonitorCollectorContext*>(lParam);
		if (rect != nullptr)
		{
			context->rects.push_back(*rect);
		}
		return TRUE;
	}

}

DialogWindowPlacer::DialogWindowPlacer()
{
	MonitorCollectorContext monitors;
	EnumDisplayMonitors(nullptr, nullptr, &CollectMonitors, reinterpret_cast<LPARAM>(&monitors));

	m_targetArea.left = m_targetArea.right = m_targetArea.bottom = m_targetArea.top = 0;

	POINT pos{ 0, 0 };
	GetCursorPos(&pos);

	for (RECT const& mon : monitors.rects)
	{
		if (mon.left < pos.x && pos.x < mon.right && mon.top < pos.y && pos.y < mon.bottom)
		{
			m_targetArea = mon;
		}
	}

	WindowCollectorContext context;
	context.processId = GetCurrentProcessId();
	EnumWindows(&CollectProcessWindows, reinterpret_cast<LPARAM>(&context));

	m_run = true;
	m_worker = std::move(std::thread(
		[this, windows = context.wnds]()
		{
			WindowCollectorContext context;
			context.processId = GetCurrentProcessId();
			typedef std::chrono::high_resolution_clock clock;

			auto start = clock::now();
			while (m_run && ((clock::now() - start) < std::chrono::seconds(10)))
			{
				std::this_thread::sleep_for(std::chrono::milliseconds(10));
				EnumWindows(&CollectProcessWindows, reinterpret_cast<LPARAM>(&context));

				for (HWND w : context.wnds)
				{
					if (std::find(windows.begin(), windows.end(), w) != windows.end())
						continue;

					PlaceWindow(w);
					m_run = false;
				}
			}
		}));
}

DialogWindowPlacer::~DialogWindowPlacer()
{
	m_run = false;
	if (m_worker.joinable())
	{
		m_worker.join();
	}
}

void DialogWindowPlacer::PlaceWindow(HWND wnd)
{
	RECT r;
	if (!GetWindowRect(wnd, &r)) return;

	int left = m_targetArea.left + ((m_targetArea.right - m_targetArea.left) - (r.right - r.left)) / 2;
	int top = m_targetArea.top + ((m_targetArea.bottom - m_targetArea.top) - (r.bottom - r.top)) / 2;

	SetWindowPos(wnd, nullptr, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
}
