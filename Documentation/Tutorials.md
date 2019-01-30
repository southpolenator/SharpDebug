# Tutorial scripts

### Enumerating processes being debugged
```cs
foreach (Process process in Process.All)
    Console.WriteLine($"{process.Id}:{process.SystemId} {process.ExecutableName}");
Console.WriteLine($"Current process: {Process.Current.SystemId}");
```

### Enumeratung all threads in current process being debugged
```cs
foreach (Thread thread in Thread.All)
    Console.WriteLine($"{thread.Id}:{thread.SystemId}");
Console.WriteLine($"Current thread: {Thread.Current.Id}:{Thread.Current.SystemId}");
```

### Enumerating all modules in current process being debugged
```cs
foreach (Module module in Module.All)
    Console.WriteLine($"0x{module.Offset:X} {module.Name}");
```

### List all functions on a call stack of the current thread
```cs
foreach (StackFrame frame in Thread.Current.StackTrace.Frames)
    Console.WriteLine(frame.FunctionName);
```

### Mappings of current process/thread shortcuts
```cs
Thread.All => Process.Current.Threads;
Module.All => Process.Current.Modules;
Thread.Current => Process.Current.CurrentThread;
StackTrace.Current => Thread.Current.StackTrace;
StackFrame.Current => StackTrace.Current.CurrentFrame;
```

### Accessing local variables in scripts
```cs
VariableCollection locals = StackTrace.Current.Frames[0].Locals;
foreach (Variable l in locals)
    Console.WriteLine(l.GetName());
dynamic myVar = locals["myVar"];
Console.WriteLine(myVar);
```

### Accessing global variables
```cs
Variable globalVariable = Process.Current.GetGlobal("mymodule!globalVariable");
Variable staticClassVariable = Process.Current.GetGlobal("mymodule!Class::staticVariable");
```
You can also access through Modules in [interactive mode](InteractiveMode.md):
```cs
dynamic globalVariable = Modules.mymodule.globalVariable;
dynamic staticClassVariable = Modules.mymodule.GetVariable("Class::staticVariable");
```

### Accessing variable fields
```cs
Variable variable = Process.Current.GetGlobal("mymodule!globalVariable");
Variable field = variable.GetField("field");
```
Or if you use dynamic:
```cs
dynamic variable = Process.Current.GetGlobal("mymodule!globalVariable");
dynamic field = variable.field;
```

### Getting variable type
```cs
CodeType type = myVariable.GetCodeType();
```

### Casting variable to built-in type
```cs
Variable variable = Process.Current.GetGlobal("mymodule!globalVariable");
int intValue = (int)variable;
ulong ulongValue = (ulong)variable;
```

### Casting variable to user type
```cs
Variable variable = Process.Current.GetGlobal("mymodule!globalVariable");
std.wstring s = variable.CastAs<std.wstring>();
```

### Casting variable to code type given by string
```cs
Variable p = Process.Current.GetGlobal("mymodule!voidPointerVariable");
Console.WriteLine(p); // This will print address of void* pointer
Variable s = p.CastAs("wchar_t*");
Console.WriteLine(s); // This will print string value pointed by s
```
Getting array element of variable array
```cs
Variable a = Process.Current.GetGlobal("mymodule!intArrayVariable");
int i = (int)a.GetArrayElement(0);
```
Or by using dynamic:
```cs
dynamic a = Modules.mymodule.intArrayVariable;
int i = (int)a[0];
```

## Debugging CLR applications
Most of the things will work automatically the same way as for native applications.
When debugger (for example DbgEng.dll) doesn't know how to get CLR stack trace correctly, you can find ClrThread and get ClrStackTrace.
There are also CLR specific classes that help with debugging CLR applications. Debugging CLR applications is done using [ClrMD](https://github.com/Microsoft/clrmd).

**Note:** _ClrMD at this moment supports Windows PDBs only. If you want to debug CLR application that has Portable PDBs, you can use [Pdb2Pdb](https://github.com/dotnet/symreader-converter) tool to convert from Portable to Windows PDBs. Another option is to add `<DebugType>Full</DebugType>` to your CLR project file._

### Enumerating CLR runtimes in the process
```cs
foreach (Runtime runtime in Process.Current.ClrRuntimes)
    Console.WriteLine(runtime.Version);
```

### Getting All user AppDomains in the runtime
```cs
foreach (AppDomain appDomain in runtime.AppDomains)
    Console.WriteLine($"{appDomain.Id}: {appDomain.Name}");
```

### Getting system and shared AppDomains
```cs
Console.WriteLine($"{runtime.SystemDomain.Id}: {runtime.SystemDomain.Name}");
Console.WriteLine($"{runtime.SharedDomain.Id}: {runtime.SharedDomain.Name}");
```

### Getting all modules loaded in the AppDomain
```cs
foreach (Module module in appDomain.Modules)
    Console.WriteLine($"0x{module.Address:X} {module.Name}");
```

### Getting the heap and all objects in it
```cs
Heap heap = runtime.Heap;
foreach (Variable variable in heap.EnumerateObects())
    Console.WriteLine($"({variable.GetCodeType()}) {variable.GetPointerAddress()}");
```

### Getting generation sizes from the heap
```cs
for (int generation = 0; generation <= 3; generation++)
    Console.WriteLine("{generation}: {heap.GetSizeByGeneration(generation)}");
```

### Getting all managed threads from the runtime
```cs
foreach (ClrThread thread in runtime.Threads)
    Console.WriteLine("{0}", thread.Id);
```

### Getting managed call stack from the thread
```cs
ClrThread thread = Thread.Current.FindClrThread();
if (thread != null)
    foreach (StackFrame frame in thread.ClrCallStack.Frames)
        Console.WriteLine(frame.FunctionNameWithoutModule);
```

### Getting last thrown exception in the thread
```cs
ClrThread thread = Thread.Current.FindClrThread();
ClrException exception = thread?.LastThrownException;
Console.WriteLine(exception?.Message);
```

### Enumerating stack objects
```cs
ClrThread thread = Thread.Current.FindClrThread();
foreach (Variable variable in thread.EnumerateStackObjects())
    Console.WriteLine("({variable.GetCodeType()}) {variable.GetPointerAddress()}");
```

### Getting arguments and local variables from function on the call stack
It works the same way as for native debugging with an addition that if debugger doesn't recognize CLR call stack correctly, you can use ClrThread.ClrStackTrace to get correct one.
```cs
Thread thread = Thread.Current;
foreach (StackFrame frame in thread.StackTrace.Frames)
{
    Console.WriteLine(frame.FunctionName);
    foreach (Variable arg in frame.Arguments)
        Console.WriteLine("argument '{arg.GetName()}': ({arg.GetCodeType()}) {arg.GetPointerAddress()}");
    foreach (Variable local in frame.Locals)
        Console.WriteLine("argument '{local.GetName()}': ({local.GetCodeType()}) {local.GetPointerAddress()}");
}
```

### Getting static fields from the type
You can use the same mechanisms used in native debugging to read CLR static variables. If your application has multiple AppDomains, you can get static variable per AppDomain.
```cs
Module myModule = Modules.All.Single(m => m.Name = "MyModule");
CodeType codeType = CodeType.Create("MyType", myModule);
Variable sf = codeType.GetStaticField("MyStaticField"); // This will use current AppDomain for CLR applications
Variable sf2 = codeType.GetClrStaticField("MyStaticField", Process.Current.ClrRuntimes[0].AppDomains[1]);
Variable sf3 = myModule.GetVariable("MyType.MyStaticField");
Variable sf4 = Process.Current.GetGlobal("MyModule!MyType.MyStaticField");
```

### Working with Variables returned from CLR application
You will walk through field, values the same way you do as with native Variables:
```cs
// Getting variable fields
Variable variable = Process.Current.GetGlobal("mymodule!MyType.singleton");
Variable field = variable.GetField("field");
// CodeType
CodeType type = myVariable.GetCodeType();
// Arrays
Variable a = Process.Current.GetGlobal("mymodule!MyType.intArrayVariable");
int i = (int)a.GetArrayElement(0);
// Strings
Variable s = Process.Current.GetGlobal("mymodule!MyType.stringVariable");
ClrString str = new ClrString(s);
Console.WriteLine(str.Text);
// Exceptions
Variable e = Process.Current.GetGlobal("mymodule!MyType.exceptionVariable");
ClrException exception = new ClrException(e);
Console.WriteLine(exception.Message);
```

## Advanced scripting
Previous examples show that scripts don't have namespace and class definition inside. Here you will learn what else can be used in scripts.

### How to use `using` in scripts
`using` C# statement must be located at the top of the script. It is supposed to be used as in any other C# file but its scope is for entire script file:
```cs
using System;
using System.Linq;
```

### Importing files into scripts
Having multiple script files is preferable as script files should share common code. For example, having this helper script file, helper.csx:
```cs
using System;
 
void HelpMe(string text)
{
    Console.WriteLine(text);
}
```
we can have sample script that is referencing that helper file, script.csx:
```cs
#load "helper.csx"
 
Console.Error.WriteLine("This is sample error");
HelpMe("It works!");
```

### Referencing assemblies in scripts
By now, you have huge collection of common code and compiling scripts is not that fast any more, you should create .NET library (dll) and just reference it from the script:
```cs
#r "CsDebugScript.CommonUserTypes.dll"
using std = CsDebugScript.CommonUserTypes.NativeTypes.std;
 
var variable = Process.Current.GetGlobal("mymodule!globalVariable");
var s = variable.CastAs<std.wstring>();
```
Please note that [CommonUserTypes](CommonUserTypes.md) library is already automatically added as a reference to all scripts.
