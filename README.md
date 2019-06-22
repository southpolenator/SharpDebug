# `SharpDebug`: C# debugging automation tool
Set of .NET libraries that provide access to different debugging tools.
SharpDebug.Engine provides framework for writting .NET code against different debuggers/dump processing.
Extension libraries provide access to dbgeng.dll for processing Windows dumps, ability to read Linux core dumps, WinDbg/VisualStudio extension with interactive scripting support.
Debugging both native and managed code is supported (currently, managed code is supported only for dbgeng.dll, WinDbg and Visual Studio extensions).

### Latest status
[![Build status](https://ci.appveyor.com/api/projects/status/d2j4lxglq0tl1x1i/branch/next?svg=true)](https://ci.appveyor.com/project/southpolenator/sharpdebug/branch/next)
[![Code coverage](https://img.shields.io/codecov/c/github/southpolenator/SharpDebug.svg)](https://codecov.io/github/southpolenator/SharpDebug)
[![Nuget version](https://img.shields.io/nuget/v/sharpdebug.engine.svg?style=flat)](https://www.nuget.org/packages/sharpdebug.engine/)
[![GitHub release](https://img.shields.io/github/release/southpolenator/sharpdebug.svg?style=flat)](https://github.com/southpolenator/SharpDebug/releases/latest)

Latest version of Visual Studio extension is uploaded to [Open VSIX Gallery](http://vsixgallery.com/).
If you want newer build than what is available in Releases page or as nuget package, you can click on [Latest build](https://ci.appveyor.com/project/southpolenator/sharpdebug/branch/next), select Configuration and click on Artifacts. You can also use private nuget feed from [AppVeyor CI builds](https://ci.appveyor.com/nuget/sharpdebug-raewbx34ffcq).

# Debugger extensions:
* [Visual Studio Extension](Documentation/VisualStudioExtension.md)
* [WinDbg Extension](Documentation/WinDbgExtension.md)

# Quick start for using engine as standalone application for dump processing
* Create a new .NET project (you can use Console Application)
* Add NuGet package [SharpDebug](https://www.nuget.org/packages/SharpDebug)
* Start using it:

```cs
using SharpDebug;

DebuggerInitialization.OpenDump("path_to_dump_file", "symbol_path;srv*");
// After this line, you can execute any code that can be executed in a script. For example:
foreach (Module module in Process.Current.Modules)
    Console.WriteLine(module.Name);
```

Take a look at [Tutorials](Documentation/Tutorials.md) and [Automate dump processing](Documentation/DumpProcessing.md). It will come in handy :)

# Building the project
Prerequisites:
1. [.NET core 2.0](https://www.microsoft.com/net/download/core)
2. [Visual Studio Community 2017](https://www.visualstudio.com/downloads/) (only for building WinDbg extension, VisualStudio extension)

Take a look at [instructions](Documentation/Build.md).

# Supporting the project
If you like the project, use it, "star" it, share ideas on how else it can be used, file issues, send pull requests, etc...
