## Creating a new project that uses debugger engine
* Create a new .NET project (you can use Console Application)
* Add NuGet package CsDebugScript
* Add initialization code:
```cs
using CsDebugScript;

DebuggerInitialization.OpenDumpFile("path_to_dump_file", "symbol_path;srv*");
// After this line, you can execute any code that can be executed in the script
```

## Creating a new project that uses scripting and UI
* Create a new .NET project (you can use Console Application)
* Add NuGet package CsDebugScript
* Add NuGet package CsDebugScript.UI
* Add initialization code:
```cs
CsDebugScript.DebuggerInitialization.OpenDumpFile("path_to_dump_file", "symbol_path;srv*");
CsDebugScript.UI.InteractiveWindow.ShowModalWindow();
```
Instead of opening interactive window, you can execute scripts:
```cs
CsDebugScript.ScriptExecution.Execute("path_to_script");
```
Or execute interactive commands with
```cs
var interactiveExecution = new CsDebugScript.InteractiveExecution();
interactiveExecution.Interpret("<C# code>");
```

## Sample project
Please take a look at [CsDebugScript.Engine.Test](../Tests/CsDebugScript.Engine.Test/Program.cs). It shows how to:
* Open a dump
* Execute some C# code against it
* Execute C# script
