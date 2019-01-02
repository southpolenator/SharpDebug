# Tutorial scripts

### Enumerating processes being debugged
```cs
foreach (Process process in Process.All)
    writeln("{0}:{1} {2}", process.Id, process.SystemId, process.ExecutableName);
Console.WriteLine("Current process: {0}", Process.Current.SystemId);
```

### Enumeratung all threads in current process being debugged
```cs
foreach (Thread thread in Thread.All)
    writeln("{0}:{1}", thread.Id, thread.SystemId);
writeln("Current thread: {0}:{1}", Thread.Current.Id, Thread.Current.SystemId);
```

### Enumerating all modules in current process being debugged
```cs
foreach (Module module in Module.All)
    writeln("0x{0:X} {1}", module.Offset, module.Name);
```

### List all functions on a call stack of the current thread
```cs
foreach (StackFrame frame in Thread.Current.StackTrace.Frames)
    writeln(frame.FunctionName);
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
    writeln(l.GetName());
dynamic myVar = locals["myVar"];
writeln(myVar);
```
