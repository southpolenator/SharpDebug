## Automatic code generation
Before reading how to use code generation tool, you should know more about [user types](UserTypes.md).

### Using code generation tool with command line arguments
Here is the list of all command line arguments:
```
-s, --symbols                                  Path to symbols file which will be used to generate the code
-t, --types                                    List of types to be exported. If omitted all types will be exported.
--no-type-info-comment                         (Default: false) Generate filed type info comment
--multi-line-properties                        (Default: false) Generate properties as multi line
--use-direct-class-access                      (Default: false) Generated code that will use class members directly
                                               (not using GetField, but GetClassField).
--force-user-types-to-new-instead-of-casting   (Default: false) Force using new during type casting instead of
                                               direct casting
--cache-user-type-fields                       (Default: false) Caches result of getting user type field when
                                               exporting user type
--cache-static-user-type-fields                (Default: false) Caches result of getting static user type field when
                                               exporting user type
--lazy-cache-user-type-fields                  (Default: false) Cache result of getting user type field inside
                                               UserMember when exporting user type
--generate-physical-mapping-of-user-types      (Default: false) Generate physical access to fields in exported user
                                               types (instead of symbolic/by name)
--generate-assembly-with-il                    (Default: false) Generate assembly by emitting IL instead of
                                               compiling C# code.
--generated-assembly-name                      (Default: ) Name of the assembly that will be generated next to
                                               sources in output folder
--generated-props-file-name                    (Default: ) Name of the props file that will be generated next to
                                               sources in output folder. It can be later included into project that
                                               will be compiled
-x, --xml-config                               Path to xml file with configuration
--use-dwarf                                    (Default: false) Use DWARF symbol provider
--use-pdb-reader                               (Default: false) Use PDB reader symbol provider
--help                                         Display this help screen.
--version                                      Display version information.
```

### Using code generation tool with XML configuration file
You can specify path to the XML configuration file using `--xml-config` argument.
Here is an example XML configuration file:
```xml
<XmlConfig>
  <Types>
    <Type Name="MyTestClass" />
  </Types>
  <Modules>
    <Module Namespace="MyModule" PdbPath="myPdb.pdb"/>
  </Modules>
  <Transformations>
    <Transformation OriginalType="std::any"
                    NewType="CsDebugScript.CommonUserTypes.NativeTypes.std.any" />
    <Transformation OriginalType="std::array&lt;${T},${Length}&gt;"
                    NewType="CsDebugScript.CommonUserTypes.NativeTypes.std.array&lt;${T}&gt;" />
    <Transformation OriginalType="std::basic_string&lt;char,${char_traits},${allocator}&gt;"
                    NewType="CsDebugScript.CommonUserTypes.NativeTypes.std.@string" />
    <Transformation OriginalType="std::vector&lt;${T},${allocator}&gt;"
                    NewType="CsDebugScript.CommonUserTypes.NativeTypes.std.vector&lt;${T}&gt;" />
  </Transformations>
  <UseDirectClassAccess>true</UseDirectClassAccess>
  <DontSaveGeneratedCodeFiles>true</DontSaveGeneratedCodeFiles>
  <GeneratePhysicalMappingOfUserTypes>true</GeneratePhysicalMappingOfUserTypes>
</XmlConfig>
```
You can see more about all available XML fields in [source code](../Source/CsDebugScript.CodeGen/XmlConfig.cs).

#### Transformations
Transformations are being used for user types defined somewhere else (like [common user types](CommonUserTypes.md)). They allow mapping from symbol type to existing user type during code generation. For example, if user defines transformation for std::vector class like in example above, all members that are of type std::vector will be exported as CsDebubScript.commonUserTypes.NativeTypes.std.vector.

#### Generating physical mapping of user types
In order to fully benefit performance wise from code generation, you want to use this option. This will generate user types that read whole memory associated with the type (size of the type is written in symbol file) and later using types and offsets available from symbol file it will read directly from [MemoryBuffer](../Source/CsDebugScript.Engine/Engine/Utility/MemoryBuffer.cs). You can find all Read* functions in [UserType](../Source/CsDebugScript.Engine/UserType.cs) class.

#### Using direct class access
Some symbol providers (DbgEng symbol provider) doesn't support getting base classes, class fields, but only all fields defined in the type. Modern symbol providers support direct class access and should be used by default.

#### Different ways to generate assembly
There are two assembly generators:
1. Roslyn
2. IL emitting

Roslyn allows usage of `IncludedFiles` and `ReferencedAssemblies`.
IL emitting is the fastest assembly generation since it doesn't do two pass generation (source code and then compiling it), but it doesn't support added user code (`InlcudedFiles`).

#### Enhancing generated user types with custom code
All user types are generated as partial classes allowing user to add custom code to make user types easier to use.

Let's take a look at following C++ structure:
```cpp
template <typename ValueType>
struct MyList
{
    ValueType value;
    MyList* next = nullptr;
};
```
Code generation will generate something similar to this class:
```cs
[UserType(TypeName = "MyList<>")]
public partial class MyList<T> : UserType
{
    partial void PartialInitialize();

    public MyList(Variable variable)
        : base(variable)
    {
        PartialInitialize();
    }
    public T value => GetField("value").CastAs<T>();
    public MyList next => GetField("next").CastAs<MyList>();
}
```
This will allow user to add code for enumerating list:
```cs
public partial class MyList<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator()
    {
        return Enumerate().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Enumerate().GetEnumerator();
    }

    private IEnumerable<T> Enumerate()
    {
        yield return value;
        for (MyList p = next; p != null; p = p.next)
            yield return p.value;
    }
}
```
User custom code + physical mapping of user types will produce user types that are way more faster than [common user type](CommonUserTypes.md) currently available.
