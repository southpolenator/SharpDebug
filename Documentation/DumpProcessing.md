## Creating a new project that uses debugger engine
* Create a new .NET project (you can use Console Application)
* Add NuGet package SharpDebug
* Add initialization code:
```cs
using SharpDebug;

DebuggerInitialization.OpenDumpFile("path_to_dump_file", "symbol_path;srv*");
// After this line, you can execute any code that can be executed in the script
```

## Creating a new project that uses scripting and UI
* Create a new .NET project (you can use Console Application)
* Add NuGet package SharpDebug
* Add NuGet package SharpDebug.UI
* Add initialization code:
```cs
SharpDebug.DebuggerInitialization.OpenDumpFile("path_to_dump_file", "symbol_path;srv*");
SharpDebug.UI.InteractiveWindow.ShowModalWindow();
```
Instead of opening interactive window, you can execute scripts:
```cs
SharpDebug.ScriptExecution.Execute("path_to_script");
```
Or execute interactive commands with
```cs
var interactiveExecution = new SharpDebug.InteractiveExecution();
interactiveExecution.Interpret("<C# code>");
```

## Sample project
Please take a look at [SharpDebug.Engine.Test](../Tests/SharpDebug.Engine.Test/Program.cs). It shows how to:
* Open a dump
* Execute some C# code against it
* Execute C# script
