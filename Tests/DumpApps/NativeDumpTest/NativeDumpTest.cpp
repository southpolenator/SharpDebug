// NativeDumpTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <string>
#include <vector>
#include <list>

using namespace std;

enum MyEnum
{
	enumEntry0,
	enumEntry1,
	enumEntry2,
	enumEntry3,
	enumEntry4,
	enumEntry5,
	enumEntry6,
};

class MyTestClass
{
public:
	enum MyEnumInner
	{
		simple0,
		simple1,
		simple2,
		simple3,
		simple4,
	};

	wstring string1;
	list<wstring> strings;
	wstring stringArray[100];
	vector<string> ansiStrings;
	MyEnum enumeration;
	MyEnumInner innerEnumeration;

	static int staticVariable;
} globalVariable;

int MyTestClass::staticVariable = 1212121212;

int main(int argc, char** argv)
{
	MyTestClass * p = &globalVariable;
	MyTestClass ** q = &p;
	MyEnum e = enumEntry3;

	p->string1 = L"qwerty";
	p->strings.push_back(L"Foo");
	p->strings.push_back(L"Bar");
	p->ansiStrings.push_back("AnsiFoo");
	p->ansiStrings.push_back("AnsiBar");

	int testArray[10000];

	for (int i = 0; i < sizeof(testArray) / sizeof(testArray[0]); i++)
		testArray[i] = 0x12121212;

	throw std::bad_exception();

	return 0;
}

struct DoubleTest
{
	double d;
	float f;
	int i;
} doubleTest{ 3.5, 2.5, 5 };
