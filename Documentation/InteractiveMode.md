# Easier coding in interactive mode
Before reading about interactive mode, don't forget to look at [tutorials](Tutorials.md).

Interactive mode is implemented as [REPL](https://en.wikipedia.org/wiki/REPL). It allows you to store results in variable and reuse it later:
```
C#> int n = 5;
C#> Console.WriteLine(n);
5
C#> for (int i = 0; i < n; i++)
        Console.WriteLine(i);
0
1
2
3
4
C#>
```
Interactive mode helps you achive more with less coding, so it defines helper functions and global objects:

### Helper functions

#### Console functions
```cs
write() => Console.Write()
writeln() => Console.WriteLine()
```

#### Drawing functions
```cs
void DrawImage(dynamic width, dynamic height, dynamic data, ChannelType[] channels, dynamic stride = null);
```
Where `width`, `height` and `stride` can be any kind of number that can be casted to int. It can also be Variable that holds number type.
`channels` can be any value from `CsDebugScript.Drawing.Channels` predefined static fields or you can create a new one.
`data` can be ulong value of the pointer, pointer to a simple type, or pointer to void. If pixel channel type cannot be deduced, you can use generics function:
```cs
void DrawImage<T>(dynamic width, dynamic height, dynamic data, ChannelType[] channels, dynamic stride = null);
```
There are also predefined functions for well known channels:
```cs
void DrawRgbImage(dynamic width, dynamic height, dynamic data, dynamic stride = null);
void DrawRgbaImage(dynamic width, dynamic height, dynamic data, dynamic stride = null);
void DrawBgrImage(dynamic width, dynamic height, dynamic data, dynamic stride = null);
void DrawBgraImage(dynamic width, dynamic height, dynamic data, dynamic stride = null);
void DrawCmykImage(dynamic width, dynamic height, dynamic data, dynamic stride = null);
void DrawGrayscaleImage(dynamic width, dynamic height, dynamic data, dynamic stride = null);
```
Similarly, they all have their generics versions.

### Helper global objects
While in interactive mode, you will have access to these global objects:

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
writeln(Locals.someArray.Length);
```

#### Dump()
Extension method that allows you to visualize different types on output.
If variable that needs to be visualized is simple type, it will be just printed to the console using `Console.WriteLine`.
Drawing objects will be visualized using drawing visualizer. See more about [drawings](Drawings.md).
If variable implements IConsoleVisualizer, it will be outputted to console using its method.
For everything else, variable is considered as of complex type and it will be visualized using result visualizer (table tree).
```cs
dynamic image = Locals.this.m_cvImage; // Note that 'this' is local variable on current stack frame
// When resulting expression is not void and line doesn't have ';', it is the same as calling .Dump() method on it:
image
// It is equivalent to:
image.Dump();
// And if it is 'cv::Mat' or 'cvMat' it will be nicely visualized as an image :)
```

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

## Automatic casting
In interactive mode, all variables will be automatically casted to predefined [user types](UserTypes.md).
In a function that has local variable:
```cpp
std::vector<std::string> strings;
```
In script can be used as:
```cs
dynamic strings = Locals.strings;
writeln(strings.Length);
for (int i = 0; i < strings.Length; i++)
    writeln($"{i+1}. {strings[i].Text}");
// On the other hand you can just dump it to the screen and benefit from auto casting by visualizing result:
Locals.strings.Dump();
```
See more about predefined [common user types](CommonUserTypes.md).
