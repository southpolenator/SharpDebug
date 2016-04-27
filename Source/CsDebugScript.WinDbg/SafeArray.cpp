#include "stdafx.h"
#include "SafeArray.h"

SafeArray::SafeArray()
	: m_pointer(nullptr)
{
}

SafeArray::~SafeArray()
{
	Release();
}

HRESULT SafeArray::Release()
{
	HRESULT result = S_OK;

	if (m_pointer)
	{
		result = SafeArrayDestroy(m_pointer);
		m_pointer = nullptr;
	}

	return result;
}

void SafeArray::CreateVector(
	_In_ VARTYPE vt,
	_In_ LONG lowerBound,
	_In_ ULONG count)
{
	Release();
	m_pointer = SafeArrayCreateVector(vt, lowerBound, count);
}

HRESULT SafeArray::PutElement(
	_In_ LONG index,
	_In_ void* value)
{
	return SafeArrayPutElement(m_pointer, &index, value);
}

SafeArray::operator SAFEARRAY*() const
{
	return m_pointer;
}
