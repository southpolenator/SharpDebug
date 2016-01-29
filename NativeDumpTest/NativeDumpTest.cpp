// NativeDumpTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <string>
#include <vector>
#include <list>

using namespace std;

class MyTestClass
{
public:
	wstring string1;
	list<wstring> strings;
	wstring stringArray[100];
	vector<string> ansiStrings;

	static int staticVariable;
} globalVariable;

int MyTestClass::staticVariable = 1212121212;

void BreakPointFunction()
{
}

int main(int argc, char** argv)
{
	MyTestClass * p = &globalVariable;
	MyTestClass ** q = &p;

	p->string1 = L"qwerty";
	p->strings.push_back(L"Foo");
	p->strings.push_back(L"Bar");
	p->ansiStrings.push_back("AnsiFoo");
	p->ansiStrings.push_back("AnsiBar");

	BreakPointFunction();

    return 0;
}
