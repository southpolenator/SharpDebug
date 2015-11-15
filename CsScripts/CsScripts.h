// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the CSSCRIPTS_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// CSSCRIPTS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef CSSCRIPTS_EXPORTS
#define CSSCRIPTS_API __declspec(dllexport)
#else
#define CSSCRIPTS_API __declspec(dllimport)
#endif

struct IDebugClient;

extern "C" {

CSSCRIPTS_API HRESULT DebugExtensionInitialize(
	_Out_ PULONG Version,
	_Out_ PULONG Flags);

CSSCRIPTS_API void CALLBACK DebugExtensionUninitialize();

CSSCRIPTS_API HRESULT CALLBACK execute(
	_In_     IDebugClient* client,
	_In_opt_ PCSTR         Args);

CSSCRIPTS_API HRESULT CALLBACK interactive(
	_In_     IDebugClient* client,
	_In_opt_ PCSTR         Args);

};

template<class T>
class CAutoComPtr
{
private:
	CAutoComPtr(CAutoComPtr&);

public:
	CAutoComPtr()
		: pointer(nullptr)
	{
	}

	CAutoComPtr(T* pointer)
		: pointer(pointer)
	{
	}

	~CAutoComPtr()
	{
		if (pointer)
			pointer->Release();
	}

	CAutoComPtr& operator=(T* p)
	{
		if (pointer)
			pointer->Release();
		pointer = p;
		return *this;
	}

	T* operator->() const
	{
		return pointer;
	}

	operator T*() const
	{
		return pointer;
	}

	T** operator&()
	{
		return &pointer;
	}

	bool operator !=(T* p) const
	{
		return pointer != p;
	}

private:
	T* pointer;
};
