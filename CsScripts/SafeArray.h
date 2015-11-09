#pragma once

#include <comutil.h>

// Wrapper class for using SAFEARRAY structure
//
class SafeArray
{
private:
	// Hide copy constructor and assignment operator
	//
	SafeArray(
		_In_ const SafeArray&);

	SafeArray& operator=(
		_In_ const SafeArray&);

public:
	SafeArray();
	~SafeArray();

	// Destroys SAFEARRAY pointer associated with this class
	//
	HRESULT Release();

	// Creates vector within this SAFEARRAY
	//
	void CreateVector(
		_In_ VARTYPE vt,
		_In_ LONG lowerBound,
		_In_ ULONG count);

	// Puts element on specified index of the SAFEARRAY.
	//
	HRESULT PutElement(
		_In_ LONG index,
		_In_ void* value);

	operator SAFEARRAY*() const;

	SAFEARRAY** operator&()
	{
		return &m_pointer;
	}

	LONG size() const
	{
		LONG lower, upper;
		HRESULT hr;

		hr = SafeArrayGetLBound(m_pointer, 1, &lower);
		hr = SafeArrayGetUBound(m_pointer, 1, &upper);
		return upper - lower + 1;
	}

	variant_t operator[](int pos) const
	{
		variant_t variant;
		long index = pos;

		SafeArrayGetElement(m_pointer, &index, &variant);
		return variant;
	}

protected:
	SAFEARRAY* m_pointer;
};
