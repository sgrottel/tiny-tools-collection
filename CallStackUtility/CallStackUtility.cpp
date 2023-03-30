//
// CallStackUtility.cpp
// Open Source via MIT License
//
#include "CallStackUtility.h"

#include <Windows.h>
#include <DbgHelp.h>
#include <WinBase.h>

#include <cassert>
#include <iostream>
#include <algorithm>

namespace
{

	/// Function type defintions directly copied from the corresponding Microsoft headers
	///
	/// This is the reason for the inconsistent argument style. This style is kept for the wrapper
	/// functions below.
	typedef USHORT(WINAPI* CaptureStackBackTraceType)(
		__in ULONG FramesToSkip,
		__in ULONG FramesToCapture,
		__out PVOID* BackTrace,
		__out_opt PULONG BackTraceHash);
	typedef DWORD(__stdcall* SymSetOptionsType)(
		_In_ DWORD SymOptions);
	typedef BOOL(__stdcall* SymInitializeType)(
		_In_ HANDLE hProcess,
		_In_opt_ PCSTR UserSearchPath,
		_In_ BOOL fInvadeProcess);
	typedef BOOL(__stdcall* SymFromAddrType)(
		_In_ HANDLE hProcess,
		_In_ DWORD64 Address,
		_Out_opt_ PDWORD64 Displacement,
		_Inout_ PSYMBOL_INFO Symbol);
	typedef BOOL(__stdcall* SymGetLineFromAddr64Type)(
		_In_ HANDLE hProcess,
		_In_ DWORD64 qwAddr,
		_Out_ PDWORD pdwDisplacement,
		_Out_ PIMAGEHLP_LINE64 Line64);
	typedef BOOL(__stdcall* SymGetModuleInfo64Type)(
		_In_ HANDLE hProcess,
		_In_ DWORD64 qwAddr,
		_Out_ PIMAGEHLP_MODULE64 ModuleInfo);
	typedef BOOL(__stdcall* SymRefreshModuleListType)(
		HANDLE hProcess);

	std::unique_ptr<callstackutility::CallStack::Api> api;
	std::mutex apiLock;
	callstackutility::CallStack refStack;

}

/// Utility class managing the library access
class callstackutility::CallStack::Api
{
public:

	Api()
	{
		hKernel32 = ::LoadLibraryW(L"kernel32.dll");
		m_CaptureStackBackTrace = (CaptureStackBackTraceType)
			(::GetProcAddress(hKernel32, "RtlCaptureStackBackTrace"));

		hDbgHelp = ::LoadLibraryW(L"Dbghelp.dll");
		m_SymSetOptions = (SymSetOptionsType)
			(::GetProcAddress(hDbgHelp, "SymSetOptions"));
		m_SymInitialize = (SymInitializeType)
			(::GetProcAddress(hDbgHelp, "SymInitialize"));
		m_SymFromAddr = (SymFromAddrType)
			(::GetProcAddress(hDbgHelp, "SymFromAddr"));
		m_SymGetLineFromAddr64 = (SymGetLineFromAddr64Type)
			(::GetProcAddress(hDbgHelp, "SymGetLineFromAddr64"));
		m_SymGetModuleInfo64 = (SymGetModuleInfo64Type)
			(::GetProcAddress(hDbgHelp, "SymGetModuleInfo64"));
		m_SymRefreshModuleList = (SymRefreshModuleListType)
			(::GetProcAddress(hDbgHelp, "SymRefreshModuleList"));

		if (valid())
		{
			HANDLE process = ::GetCurrentProcess();
			m_SymSetOptions(SYMOPT_DEFERRED_LOADS | SYMOPT_LOAD_LINES);
			m_SymInitialize(process, NULL, TRUE);
			// pseudo handle `process` does not need to be closed
		}
	}

	~Api()
	{
		m_CaptureStackBackTrace = nullptr;
		m_SymSetOptions = nullptr;
		m_SymInitialize = nullptr;
		m_SymFromAddr = nullptr;
		m_SymGetLineFromAddr64 = nullptr;
		m_SymGetModuleInfo64 = nullptr;
		m_SymRefreshModuleList = nullptr;

		::FreeLibrary(hDbgHelp);
		hDbgHelp = nullptr;
		::FreeLibrary(hKernel32); // trying to unload the kernel is a bit useless, but for consistency
		hKernel32 = nullptr;
	}

		// Copy deleted, move possible
	Api(Api const& src) = delete;
	Api& operator=(Api const& src) = delete;

	Api(Api&& src)
	{
		operator=(std::move(src));
	}

	Api& operator=(Api&& src)
	{
		m_CaptureStackBackTrace = src.m_CaptureStackBackTrace;
		m_SymSetOptions = src.m_SymSetOptions;
		m_SymInitialize = src.m_SymInitialize;
		m_SymFromAddr = src.m_SymFromAddr;
		m_SymGetLineFromAddr64 = src.m_SymGetLineFromAddr64;
		m_SymGetModuleInfo64 = src.m_SymGetModuleInfo64;
		m_SymRefreshModuleList = src.m_SymRefreshModuleList;
		hDbgHelp = src.hDbgHelp;
		hKernel32 = src.hKernel32;

		src.m_CaptureStackBackTrace = nullptr;
		src.m_SymSetOptions = nullptr;
		src.m_SymInitialize = nullptr;
		src.m_SymFromAddr = nullptr;
		src.m_SymGetLineFromAddr64 = nullptr;
		src.m_SymGetModuleInfo64 = nullptr;
		src.m_SymRefreshModuleList = nullptr;
		src.hDbgHelp = nullptr;
		src.hKernel32 = nullptr;

		return *this;
	}

	/// \return true if all functions were loaded successfully
	inline bool valid() const
	{
		return m_CaptureStackBackTrace != nullptr
			&& m_SymSetOptions != nullptr
			&& m_SymInitialize != nullptr
			&& m_SymFromAddr != nullptr
			&& m_SymGetLineFromAddr64 != nullptr
			&& m_SymGetModuleInfo64 != nullptr
			&& m_SymRefreshModuleList != nullptr;
	}

	/// Captures a stack back trace by walking up the stack and recording the information for each frame.
	/// https://msdn.microsoft.com/library/windows/desktop/bb204633(v=vs.85).aspx
	inline USHORT CaptureStackBackTrace(
		__in ULONG FramesToSkip,
		__in ULONG FramesToCapture,
		__out PVOID* BackTrace,
		__out_opt PULONG BackTraceHash) const
	{
		return m_CaptureStackBackTrace(FramesToSkip, FramesToCapture, BackTrace, BackTraceHash);
	}

	/// Retrieves symbol information for the specified address
	/// https://docs.microsoft.com/windows/desktop/api/dbghelp/nf-dbghelp-symfromaddr
	/// All DbgHelp functions, such as this one, are single threaded. Therefore, calls from more than
	/// one thread to this function will likely result in unexpected behavior or memory corruption.
	/// To avoid this, you must synchronize all concurrent calls from more than one thread to this
	/// function.
	inline BOOL SymFromAddr(
		_In_ HANDLE hProcess,
		_In_ DWORD64 Address,
		_Out_opt_ PDWORD64 Displacement,
		_Inout_ PSYMBOL_INFO Symbol) const
	{
		return m_SymFromAddr(hProcess, Address, Displacement, Symbol);
	}

	/// Locates line information for the specified module and file name.
	/// https://docs.microsoft.com/windows/desktop/api/dbghelp/nf-dbghelp-symgetfilelineoffsets64
	/// All DbgHelp functions, such as this one, are single threaded. Therefore, calls from more than
	/// one thread to this function will likely result in unexpected behavior or memory corruption.
	/// To avoid this, you must synchronize all concurrent calls from more than one thread to this
	/// function.
	inline BOOL SymGetLineFromAddr64(
		_In_ HANDLE hProcess,
		_In_ DWORD64 qwAddr,
		_Out_ PDWORD pdwDisplacement,
		_Out_ PIMAGEHLP_LINE64 Line64) const
	{
		return m_SymGetLineFromAddr64(hProcess, qwAddr, pdwDisplacement, Line64);
	}

	/// Retrieves the module information of the specified module.
	/// https://docs.microsoft.com/windows/desktop/api/dbghelp/nf-dbghelp-symgetmoduleinfo
	/// All DbgHelp functions, such as this one, are single threaded. Therefore, calls from more than
	/// one thread to this function will likely result in unexpected behavior or memory corruption.
	/// To avoid this, you must synchronize all concurrent calls from more than one thread to this
	/// function.
	inline BOOL SymGetModuleInfo64(
		_In_ HANDLE hProcess,
		_In_ DWORD64 qwAddr,
		_Out_ PIMAGEHLP_MODULE64 ModuleInfo) const
	{
		return m_SymGetModuleInfo64(hProcess, qwAddr, ModuleInfo);
	}

	/// Refreshes the module list for the process.
	/// https://docs.microsoft.com/windows/desktop/api/dbghelp/nf-dbghelp-symrefreshmodulelist
	/// All DbgHelp functions, such as this one, are single threaded. Therefore, calls from more than
	/// one thread to this function will likely result in unexpected behavior or memory corruption.
	/// To avoid this, you must synchronize all concurrent calls from more than one thread to this
	/// function.
	inline BOOL SymRefreshModuleList(HANDLE hProcess) const
	{
		return m_SymRefreshModuleList(hProcess);
	}

private:
	// library handles
	HMODULE hKernel32;
	HMODULE hDbgHelp;

	// function pointers
	CaptureStackBackTraceType m_CaptureStackBackTrace;
	SymSetOptionsType m_SymSetOptions;
	SymInitializeType m_SymInitialize;
	SymFromAddrType m_SymFromAddr;
	SymGetLineFromAddr64Type m_SymGetLineFromAddr64;
	SymGetModuleInfo64Type m_SymGetModuleInfo64;
	SymRefreshModuleListType m_SymRefreshModuleList;

};
	//callstackutility::CallStack createCallStack(
	//	CallStackUtilityLibraries const& api,
	//	callstackutility::CallStack const& refStack)
	//{


	//	// pseudo handle `process` does not need to be closed
	//	return callstackutility::CallStack{ frames };
	//}


callstackutility::CallStack callstackutility::CallStack::Capture()
{
	using namespace std::chrono_literals;
	std::unique_lock<std::mutex> lock{ apiLock };

	// validate api
	if (!api)
	{
		// API needs to initialize
		api = std::make_unique<callstackutility::CallStack::Api>();
		if (!api) return CallStack{};

		// Lazy try to set up the reference stack
		assert(refStack.size() == 0);

		// shared temporary state during initialization
		struct InitHelper
		{
			std::mutex m_mutex;
			std::condition_variable m_cv;
			::callstackutility::CallStack m_callStack;
		};
		std::shared_ptr<InitHelper> init = std::make_shared<InitHelper>();

		std::thread initWorker{ [init]() {
			// load the API anew to be independent from the main thread
			auto api = std::make_unique<callstackutility::CallStack::Api>();
			auto cs = callstackutility::CallStack::Load(*api);
			{
				std::lock_guard<std::mutex> lock(init->m_mutex);
				init->m_callStack = cs;
			}
			init->m_cv.notify_all();
		} };

		if (init->m_cv.wait_for(lock, 500ms, [init]() {
			std::lock_guard<std::mutex> lock(init->m_mutex);
			return init->m_callStack.size() > 0;
			}))
		{
			if (initWorker.joinable()) initWorker.join();
			refStack = init->m_callStack;
			// continue with normal call stack query below ...
		}
		else
		{
			lock.unlock();
			// the initialization took too long (hung?)
			// Assume stack information is not available
			{
				// try to actively kill the other thread
				auto handle = initWorker.native_handle();
				initWorker.detach();
				if (handle != INVALID_HANDLE_VALUE)
				{
					::TerminateThread(handle, 12);
				}
			}
			return CallStack{};
		}

	}
	// break if api is still not avaliable
	if (!api) return CallStack{};

	// If no reference stack exists, then the whole functionality might be compromised
	if (refStack.size() == 0) return CallStack{};

	CallStack cs = Load(*api);

	// remove top frames shared with reference stack
	size_t commonHead = 0;
	for (; commonHead < std::min<size_t>(cs.m_frames.size(), refStack.size()); ++commonHead)
	{
		if (cs.m_frames[commonHead].m_address != refStack[commonHead].m_address) break;
	}
	if (commonHead > 0)
	{
		if (commonHead + 1 < cs.m_frames.size())
		{
			commonHead++; // to also skip `CallStack::Capture` which is -just- not part of the refStack
		}
		cs.m_frames.erase(cs.m_frames.begin(), cs.m_frames.begin() + commonHead);
	}

	return cs;
}

callstackutility::CallStack callstackutility::CallStack::Load(Api const& api)
{
	// All calls to the api are not thread-safe. Therefore access to the api must be exclusive,
	// e.g. by calling under a lock, or by using an api instance not shared with other threads.
	// In case one `m_threadLock` is used.
	// Case two is used during initialization of the reference stack.
	assert(api.valid());

	HANDLE process = ::GetCurrentProcess();
	api.SymRefreshModuleList(process);

	// Quote from Microsoft Documentation:
	//  Windows Server 2003 and Windows XP:
	//  The sum of the FramesToSkip and FramesToCapture parameters must be less than 63.
	const int callstackMaxSize = 62;
	void* callstack[callstackMaxSize];
	unsigned short callstackSize = api.CaptureStackBackTrace(0, callstackMaxSize, callstack, NULL);

	std::vector<callstackutility::CallStack::Frame> frames;
	if (callstackSize > 0)
	{
		SYMBOL_INFO* symbol = (SYMBOL_INFO*)::calloc(sizeof(SYMBOL_INFO) + 256 * sizeof(char), 1);
		if (symbol == nullptr) throw std::runtime_error("allocation failed");
		symbol->MaxNameLen = 255;
		symbol->SizeOfStruct = sizeof(SYMBOL_INFO);

		frames.reserve(callstackSize);
		callstackutility::CallStack::Frame frame;
		for (int i = 0; i < callstackSize; ++i)
		{
			frame.m_address = (uint64_t)callstack[i];

			DWORD64 addr = (DWORD64)(callstack[i]);
			DWORD64 disp = 0;

			if (api.SymFromAddr(process, addr, &disp, symbol) != 0)
			{
				frame.m_function = symbol->Name;
				frame.m_symAddress = symbol->Address;
				frame.m_imageBaseAddress = symbol->ModBase;

				IMAGEHLP_MODULE64 modinfo;
				modinfo.SizeOfStruct = sizeof(IMAGEHLP_MODULE64);
				if (api.SymGetModuleInfo64(process, symbol->ModBase, &modinfo) != 0)
				{
					frame.m_imageName = modinfo.ImageName;
				}

				IMAGEHLP_LINE64 line;
				line.SizeOfStruct = sizeof(IMAGEHLP_LINE64);
				if (api.SymGetLineFromAddr64(process, addr, (PDWORD)&disp, &line) != 0)
				{
					frame.m_file = line.FileName;
					frame.m_line = line.LineNumber;
				}
			}

			frames.push_back(frame);
		}
		::free(symbol);
	}

	return CallStack{ frames };
}

callstackutility::CallStack::CallStack(std::vector<Frame> frames)
	: m_frames{ frames }
{}
