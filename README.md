# WinDbgCs
WinDbg extension for executing C# scripts.
It allows you to automate data querying/processing of both native and managed applications.
It can be also used without WinDbg to open dumps.

### Latest status
[![Build status](https://ci.appveyor.com/api/projects/status/d2j4lxglq0tl1x1i/branch/master?svg=true)](https://ci.appveyor.com/project/southpolenator/windbgcs/branch/master)
[![Code coverage](https://img.shields.io/codecov/c/github/southpolenator/WinDbgCs.svg)](https://codecov.io/github/southpolenator/WinDbgCs)
[![Nuget version](https://img.shields.io/nuget/v/csdebugscript.engine.svg?style=flat)](https://www.nuget.org/packages/csdebugscript.engine/)
[![GitHub release](https://img.shields.io/github/release/southpolenator/windbgcs.svg?style=flat)](https://github.com/southpolenator/WinDbgCs/releases/latest)

# Easy start for using engine
* Create a new .NET project (you can use Console Application)
* Add NuGet package [CsDebugScript.Engine](https://www.nuget.org/packages/CsDebugScript.Engine)
* Start using it:

```
using CsDebugScript;

var debugClient = DbgEngManaged.DebugClient.OpenDumpFile("path_to_dump_file", "symbol_path;srv*");
CsDebugScript.Engine.Context.Initialize(debugClient);
// After this line, you can execute any code that can be executed in the script. For example:
foreach (Module module in Process.Current.Modules)
    Console.WriteLine(module.Name);
```

# Wiki
Take a look a [Wiki page](https://github.com/southpolenator/WinDbgCs/wiki) to get hooked up :)

# Code reference
Now that you are hooked up, cou can take a look at [code reference](http://southpolenator.github.io/WinDbgCsReference/).

# Prerequisites for building the project
0. Visual Studio 2015
1. Windows SDK 10: https://dev.windows.com/en-US/downloads/windows-10-sdk
2. Sandcastle (for documentation reference generation): https://github.com/EWSoftware/SHFB/releases
