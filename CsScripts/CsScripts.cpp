// CsScripts.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "CsScripts.h"
#include "SafeArray.h"
#include <iostream>
#include <sstream>
#include <vector>


// CLR headers
#include <metahost.h>
#include <CorError.h>

#pragma comment(lib, "mscoree.lib")

// Import mscorlib.tlb (Microsoft Common Language Runtime Class Library).
#import "mscorlib.tlb" raw_interfaces_only				\
    high_property_prefixes("_get","_put","_putref")		\
    rename("ReportEvent", "InteropServices_ReportEvent")


#import "CsScriptManaged.tlb" raw_interfaces_only

// Debugging engine headers
#define KDEXT_64BIT 
#include <wdbgexts.h>
#include <Dbgeng.h>

using namespace std;
using namespace mscorlib;

// Checks HRESULT expression and throws ComException if fails.
//
#define CHECKCOM(expr) \
    { \
        HRESULT _temp_hr = (expr); \
        if (FAILED(_temp_hr)) \
        { \
            WriteComException(_temp_hr, #expr); \
            return _temp_hr; \
        } \
    }

void WriteComException(HRESULT hr, const char* expression)
{
    CAutoComPtr<IErrorInfo> errorInfo;

    cout << "COM Exception!!!" << endl;
    cout << "HRESULT: " << hex << showbase << hr << noshowbase << dec << endl;
    cout << "Expression: " << expression << endl;
    if (SUCCEEDED(GetErrorInfo(0, &errorInfo)) && errorInfo != nullptr)
    {
        BSTR description = nullptr;

        errorInfo->GetDescription(&description);
        if (description != nullptr)
        {
            wcout << "Description: " << description << endl;
        }

        CAutoComPtr<_Exception> exception;

        if (SUCCEEDED(errorInfo->QueryInterface(IID_PPV_ARGS(&exception))))
        {
        	BSTR toString = nullptr, stackTrace = nullptr;

        	exception->get_ToString(&toString);
        	if (toString != nullptr)
        	{
        		wcout << "Exception.ToString(): " << toString << endl;
        	}
        }
    }
}

class HostControl : public IHostControl
{
public:
	HostControl()
		: counter(1)
	{
	}

	virtual HRESULT STDMETHODCALLTYPE QueryInterface(
		REFIID riid,
		void ** ppvObject)
	{
		if (riid == __uuidof(IHostControl))
		{
			AddRef();
			*ppvObject = this;
			return S_OK;
		}

		return E_NOINTERFACE;
	}

	virtual ULONG STDMETHODCALLTYPE AddRef()
	{
		return ++counter;
	}

	virtual ULONG STDMETHODCALLTYPE Release()
	{
		ULONG result = --counter;

		if (result == 0)
			delete this;
		return result;
	}

	HRESULT STDMETHODCALLTYPE GetHostManager(
		_In_ REFIID riid,
		_Outptr_result_maybenull_ void **ppv)
	{
		return E_NOINTERFACE;
	}

	HRESULT STDMETHODCALLTYPE SetAppDomainManager(
		_In_ DWORD dwAppDomainID,
		_In_ IUnknown *pUnkAppDomainManager)
	{
		return pUnkAppDomainManager->QueryInterface(__uuidof(CsScriptManaged::IExecutor), (PVOID*)&m_appDomainManager);
	}

	CsScriptManaged::IExecutor* GetAppDomainManager() const
	{
		return m_appDomainManager;
	}

private:
	CAutoComPtr<CsScriptManaged::IExecutor> m_appDomainManager;
	ULONG counter;
};


wstring GetCurrentDllDirectory()
{
    wchar_t dllpath[8000];
    HMODULE hm = NULL;

    if (!GetModuleHandleExA(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
        GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
        (LPCSTR)&GetCurrentDllDirectory,
        &hm))
    {
        int ret = GetLastError();
        fprintf(stderr, "GetModuleHandle returned %d\n", ret);
    }

    GetModuleFileNameW(hm, dllpath, ARRAYSIZE(dllpath));

    wstring path = dllpath;
    size_t pos = path.find_last_of('\\');

    if (pos != wstring::npos)
        path.resize(pos + 1);
    return path;
}

wstring GetWorkingDirectory()
{
	wchar_t dllpath[8000];

	GetCurrentDirectoryW(ARRAYSIZE(dllpath), dllpath);
	return dllpath;
}

class ClrInitializator
{
public:
	HRESULT Initialize(const wchar_t* csScriptsManaged, IDebugClient* client)
	{
		// We should figure out needed runtime version
		//
		CAutoComPtr<ICLRMetaHostPolicy> pClrHostPolicy;

		CHECKCOM(CLRCreateInstance(
			CLSID_CLRMetaHostPolicy,
			IID_ICLRMetaHostPolicy,
			(LPVOID*)&pClrHostPolicy));

		wstring runtimeVersion;
		wchar_t queriedRuntimeVersion[100] = { 0 };
		DWORD length = sizeof(queriedRuntimeVersion) / sizeof(wchar_t);

		CHECKCOM(pClrHostPolicy->GetRequestedRuntime(
			METAHOST_POLICY_HIGHCOMPAT,
			csScriptsManaged,
			nullptr,
			queriedRuntimeVersion,
			&length,
			nullptr,
			nullptr,
			nullptr,
			IID_PPV_ARGS(&runtimeInfo)));
		runtimeVersion = queriedRuntimeVersion;

		// Set custom memory manager and start CLR
		//
		hostControl = new HostControl();
		CHECKCOM(runtimeInfo->BindAsLegacyV2Runtime());
		//CHECKCOM(runtimeInfo->SetDefaultStartupFlags(clrStartupFlags, nullptr));
		CHECKCOM(runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&clrRuntimeHost)));
		CHECKCOM(clrRuntimeHost->GetCLRControl(&clrControl));
		//CHECKCOM(clrControl->SetAppDomainManagerType(L"CsScriptManaged", L"CsScriptManaged.CustomAppDomainManager"));
		CHECKCOM(clrRuntimeHost->SetHostControl(hostControl));
		CHECKCOM(clrRuntimeHost->Start());

		// Create a new AppDomain that will contain application configuration.
		//
		CAutoComPtr<IUnknown> appDomainSetupThunk;
		CAutoComPtr<IAppDomainSetup> appDomainSetup;
		CAutoComPtr<IUnknown> appDomainThunk;
		CAutoComPtr<_AppDomain> appDomain;

		CHECKCOM(runtimeInfo->GetInterface(CLSID_CorRuntimeHost, IID_PPV_ARGS(&corRuntimeHost)));
		CHECKCOM(corRuntimeHost->CreateDomainSetup(&appDomainSetupThunk));
		CHECKCOM(appDomainSetupThunk->QueryInterface(IID_PPV_ARGS(&appDomainSetup)));
		CHECKCOM(corRuntimeHost->CreateDomainEx(L"MyDomain", appDomainSetup, nullptr, &appDomainThunk));
		CHECKCOM(appDomainThunk->QueryInterface(IID_PPV_ARGS(&appDomain)));

		// Load our assembly
		//
		CAutoComPtr<_Assembly> mscorlibAssembly;
		CAutoComPtr<_Type> reflectionAssemblyType;
		SafeArray loadFromArguments;
		variant_t loadFromResult;
		variant_t arg1(csScriptsManaged);

		loadFromArguments.CreateVector(VT_VARIANT, 0, 1);
		loadFromArguments.PutElement(0, &arg1);

		CHECKCOM(GetAssemblyFromAppDomain(appDomain, L"mscorlib", &mscorlibAssembly));
		CHECKCOM(mscorlibAssembly->GetType_2(bstr_t(L"System.Reflection.Assembly"), &reflectionAssemblyType));
		CHECKCOM(reflectionAssemblyType->InvokeMember_3(bstr_t(L"LoadFrom"), (BindingFlags)(BindingFlags_InvokeMethod | BindingFlags_Public | BindingFlags_Static), nullptr, variant_t(), loadFromArguments, &loadFromResult));

		// Create our extension CLR instance
		//
		CAutoComPtr<_Assembly> assembly = (_Assembly*)(IDispatch*)loadFromResult;
		variant_t variant;

		CHECKCOM(assembly->CreateInstance_2(bstr_t(L"CsScriptManaged.Executor"), true, &variant));
		CHECKCOM(variant.punkVal->QueryInterface(&instance));

		CHECKCOM(instance->InitializeContext(client));
		return S_OK;
	}

	HRESULT ExecuteScript(const wchar_t* scriptPath, const vector<wstring>& arguments)
	{
		// Transfer all arguments to CLR
		//
		SafeArray safeArray;
		bstr_t bstrScriptPath = scriptPath;

		safeArray.CreateVector(VT_BSTR, 0, (ULONG)arguments.size());
		for (size_t i = 0; i < arguments.size(); i++)
		{
			// Intentionally allocating string because SafeArray will automatically dispose of it.
			//
			safeArray.PutElement((LONG)i, SysAllocString(arguments[i].c_str()));
		}

		// Execute script function
		//
		CHECKCOM(instance->ExecuteScript(bstrScriptPath, safeArray));
		return S_OK;
	}

	HRESULT ExecuteScript(const wchar_t* arguments)
	{
		// Execute script function
		//
		bstr_t bstrArguments = arguments;

		CHECKCOM(instance->ExecuteScript_2(bstrArguments));
		return S_OK;
	}

	void Uninitialize()
	{
		clrRuntimeHost->Stop();
		corRuntimeHost = nullptr;
		clrControl = nullptr;
		clrRuntimeHost = nullptr;
		runtimeInfo = nullptr;
		hostControl = nullptr;
		instance = nullptr;
	}

private:
	HRESULT GetAssemblyFromAppDomain(_AppDomain* appDomain, const wchar_t* assemblyName, _Assembly **assembly)
	{
		SAFEARRAY* safearray;
		CComSafeArray<IUnknown*> assemblies;

		CHECKCOM(appDomain->GetAssemblies(&safearray));
		assemblies.Attach(safearray);
		for (int i = 0, n = assemblies.GetCount(); i < n; i++)
		{
			CComPtr<_Assembly> a;

			a = assemblies[i];
			if (a == nullptr)
				continue;
			CComBSTR assemblyFullName;
			CHECKCOM(a->get_FullName(&assemblyFullName));
			if (assemblyFullName != nullptr && _wcsnicmp(assemblyFullName, assemblyName, wcslen(assemblyName)) == 0)
			{
				*assembly = a.Detach();
				return S_OK;
			}
		}

		return E_FAIL;
	}

	CAutoComPtr<ICLRRuntimeInfo> runtimeInfo;
	CAutoComPtr<ICLRRuntimeHost> clrRuntimeHost;
	CAutoComPtr<ICLRControl> clrControl;
	CAutoComPtr<HostControl> hostControl;
	CAutoComPtr<ICorRuntimeHost> corRuntimeHost;
	CAutoComPtr<CsScriptManaged::IExecutor> instance;
} clr;

CSSCRIPTS_API HRESULT DebugExtensionInitialize(
    _Out_ PULONG Version,
    _Out_ PULONG Flags)
{
    wstring currentDirectory = GetCurrentDllDirectory();
    wstring csScriptsManaged = currentDirectory + L"CsScriptManaged.dll";

    // Return parameters
    if (Version != nullptr)
        *Version = DEBUG_EXTENSION_VERSION(0, 1);
    if (Flags != nullptr)
        *Flags = 0;

	// Initialize CRL and CsScriptManaged library
	CAutoComPtr<IDebugClient> debugClient;

	CHECKCOM(DebugCreate(IID_PPV_ARGS(&debugClient)));

	HRESULT hr = clr.Initialize(csScriptsManaged.c_str(), debugClient);

	return hr;
}

CSSCRIPTS_API void CALLBACK DebugExtensionUninitialize()
{
	clr.Uninitialize();
}

HRESULT CALLBACK execute(
	_In_     IDebugClient* Client,
	_In_opt_ PCSTR         Args)
{
	wstringstream ss;

	ss << Args;
	return clr.ExecuteScript(ss.str().c_str());
}
