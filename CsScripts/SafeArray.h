#pragma once

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

protected:
	SAFEARRAY* m_pointer;
};
