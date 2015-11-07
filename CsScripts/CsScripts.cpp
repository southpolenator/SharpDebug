// CsScripts.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "CsScripts.h"

CSSCRIPTS_API HRESULT DebugExtensionInitialize(
	_Out_ PULONG Version,
	_Out_ PULONG Flags)
{
	if (Version != nullptr)
		*Version = DEBUG_EXTENSION_VERSION(0, 1);
	if (Flags != nullptr)
		*Flags = 0;
	return S_OK;
}
