# Easier coding in interactive mode

// TODO: Write about REPL, variable, etc.

Interactive mode helps you achive more with less coding. While in interactive mode, you will have access to these global objects:
#### Modules
Dynamic object that allows you to find Module by name as if it is a member:
```cs
writeln(Modules.ConsoleApp1.Address);
```
#### Globals
Dynamic object that allows you to find global variable by name as if it is a member:
```cs
writeln(Globals._errno);
```
#### Arguents/Locals
Dynamic object that allows you to find argument/local variable by name as if it is a member:
```cs
writeln(Arguments.argc);
```

#### Dump()
Extension method that allows you to visualize different types on output.
If variable that needs to be visualized is simple type, it will be just printed to the console using `Console.WriteLine`.
Drawing objects will be visualized using drawing visualizer. See more about [drawings](Drawings.md).
If variable implements IConsoleVisualizer, it will be outputted to console using its method.
For everything else, variable is considered as of complex type and it will be visualized using result visualizer (table tree).

#### Processes
```cs
Processes => Process.All;
```

#### Threads
```cs
Threads => Threads.All;
```

#### Frames
```cs
Frames => StackTrace.Current.Frames
```

## Tutorial scripts rewritten in interactive mode

### Accessing local variables
```cs
writeln(Arguments.argc);
dynamic data = Locals.this.m_data; // Note that this is local variable on current stack frame
data.Dump();
// When resulting expression is not void and line doesn't have ';', it is the same as calling .Dump() method on it:
data
```
