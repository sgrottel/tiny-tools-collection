// MicMuteToggle.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include <windows.h>
#include <mmdeviceapi.h>
#include <winrt/base.h>
#include <Functiondiscoverykeys_devpkey.h>
#include <iostream>

namespace
{
	struct CoInitializeScope
	{
		CoInitializeScope()
		{
			::CoInitialize(NULL);
		}
		~CoInitializeScope()
		{
			::CoUninitialize();
		}
	};
}

//
// https://learn.microsoft.com/en-us/windows/win32/coreaudio/endpoint-volume-controls
//
int main()
{
	CoInitializeScope coInited;
	HRESULT hr = S_OK;

	winrt::com_ptr<IMMDeviceEnumerator> pEnumerator;
	hr = CoCreateInstance(
		__uuidof(MMDeviceEnumerator),
		NULL,
		CLSCTX_INPROC_SERVER,
		__uuidof(IMMDeviceEnumerator),
		pEnumerator.put_void());
	if (FAILED(hr))
	{
		std::cerr << "Failed to create IMMDeviceEnumerator" << std::endl;
		return 1;
	}

	winrt::com_ptr<IMMDevice> pDevice;
	hr = pEnumerator->GetDefaultAudioEndpoint(
		eCapture, // mic
		eCommunications, // used for calls
		pDevice.put());
	if (FAILED(hr))
	{
		std::cerr << "Failed to GetDefaultAudioEndpoint" << std::endl;
		return 1;
	}

	winrt::com_ptr<IPropertyStore> pProps;
	hr = pDevice->OpenPropertyStore(STGM_READ, pProps.put());
	if (FAILED(hr))
	{
		std::cerr << "Failed to OpenPropertyStore on pDevice" << std::endl;
		return 1;
	}

	PROPVARIANT varName;
	PropVariantInit(&varName);
	hr = pProps->GetValue(PKEY_Device_FriendlyName, &varName);
	if (FAILED(hr))
	{
		std::cerr << "Failed to PKEY_Device_FriendlyName on pDevice" << std::endl;
		return 1;
	}

	if (varName.vt != VT_EMPTY)
	{
		// Print endpoint friendly name and endpoint ID.
		std::wcout << L"Mic.: " << varName.pwszVal << std::endl;
	}

	PropVariantClear(&varName);

	return 0;
}
