# User types
Let's take a look at user code:
```cpp
struct my_data
{
    std::vector<std::string> strings;
    double number;
    bool flag;
};

my_data global_data[10000];
```
Exploring this data can be done using Variable class:
```cs
Variable global_data = Process.Current.GetGlobal("global_data");
for (int i = 0; i < global_data.GetArrayLength(); i++)
{
    Variable data = global_data[i];
    double number = (double)data.GetField("number");
    bool flag = (bool)data.GetField("flag");
    std.vector<std.string> strings = new std.vector<std.string>(data.GetField("strings"));

    // Do something with it
}
```
This is ok approach, but there are some problems with it:
1. Extracting my_data structure on one place is ok, but imagine that you need to extract it on multiple places
2. If you make changes to original C++ code and rename some member of my_data structure, your script will stop working and you won't know where are all the problems until you run it.
3. It won't be as fast as reading memory directly.

Here are some steps on how to improve it:
#### Start using [CodeArray](../Source/CsDebugScript.Engine/CodeArray.cs) (get some speed benefits)
```cs
CodeArray global_data = new CodeArray(Process.Current.GetGlobal("global_data"));
foreach (Variable data in global_data)
{
    // Everything else stays the same
}
```
#### Manually define user type (get some type benefits in long term scripts)
```cs
[UserType(TypeName = "my_data")]
class my_data : UserType
{
    public my_data(Variable variable)
        : base(variable)
    {
    }
    public double number => (double)GetField("number");
    public bool flag => (bool)GetField("flag");
    public std.vector<std.string> strings => new std.vector<std.string>(GetField("strings"));
}

CodeArray<my_data> global_data = new CodeArray<my_data>(Process.Current.GetGlobal("global_data"));
foreach (my_data data in global_data)
{
    // Do something directly with data properties instead of extracting fields manually.
}
```
#### Manually define user type using self field that is declared as dynamic (easier to write)
```cs
[UserType(TypeName = "my_data")]
class my_data : DynamicSelfUserType
{
    public my_data(Variable variable)
        : base(variable)
    {
    }
    public double number => self.number;
    public bool flag => self.flag;
    public std.vector<std.string> strings => new std.vector<std.string>(self.strings);
}

// Everything else stays the same
```
#### Use [code generation](CodeGen.md) to export user types from debugging symbols (PDBs, DWARFs, etc.)
One option is to add post build step in your project to automatically generate DLL with C# user types that can be used in scripts. This option helps with maintaining scripts since post build step can also verify that scripts can be still compiled against current code.
Also, it is useful to note that code generation can generate code that will directly read memory instead of going through Variable/CodeType to see where data is stored... This significantly reduces script execution time if you have lots of data to process.
You can read more about [code generation](CodeGen.md).

So, with using code generation, example script would look like this:
```cs
// ModuleGlobals is auto generated static class that will contain all global variables defined in specific module.
foreach (my_data data in ModuleGlobals.global_data)
{
    // Do something with data properties
    double number = data.number;
    bool flag = data.flag;
    std.vector<std.string> strings = data.strings;
    // ...
}
```

## Writing manually user types
There are some special cases when you actually want to write user types manually:
- Code generation doesn't know how to deal with C++ specializations for different number constants or has some other problems with your specific type
- You want to share your user types for different versions of library

In those cases, you want to help engine with work with these user types correctly. That means that you probably want to inherit [UserType](../Source/CsDebugScript.Engine/UserType.cs) class and also you want to add [UserTypeAttribute](../Source/CsDebugScript.Engine/UserTypeAttribute.cs) to new class.

Inheriting from UserType class will help with storing necessary data for you (like remembering Variable, or MemoryBuffer in advanced scenarios). Adding UserTypeAttribute to your class will trigger automatic casting in interactive mode. If you don't care about goddies of UserType and UserTypeAttribute, you should inherit Variable class when implementing your user type.

In order to use casting (like Variable.CastAs<T> function), you will need to implement constructor with one argument that accepts Variable as an argument.

You can continue with reading about [common user types](CommonUserTypes.md) that will contain links to many different manually written user types from common libraries.
