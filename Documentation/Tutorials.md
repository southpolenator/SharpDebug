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
