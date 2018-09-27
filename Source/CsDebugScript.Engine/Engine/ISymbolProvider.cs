using System;
using System.Collections.Generic;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Debugging symbol provider.
    /// </summary>
    public interface ISymbolProvider
    {
        /// <summary>
        /// Gets the <see cref="ISymbolProviderModule"/> interface for the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        ISymbolProviderModule GetSymbolProviderModule(Module module);

        /// <summary>
        /// Gets the code type tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        CodeTypeTag GetTypeTag(Module module, uint typeId);

        /// <summary>
        /// Gets the type's built-in type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        BuiltinType GetTypeBuiltinType(Module module, uint typeId);

        /// <summary>
        /// Gets the names of all fields of the specified type (including all base classes).
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        string[] GetTypeAllFieldNames(Module module, uint typeId);

        /// <summary>
        /// Gets the field type id and offset of the specified type (from the list of all fields, including all base classes).
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        Tuple<uint, int> GetTypeAllFieldTypeAndOffset(Module module, uint typeId, string fieldName);

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        string[] GetTypeFieldNames(Module module, uint typeId);

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName);

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        Tuple<uint, int> GetTypeBaseClass(Module module, uint typeId, string className);

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(Module module, uint typeId);

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        string GetTypeName(Module module, uint typeId);

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        string GetEnumName(Module module, uint enumTypeId, ulong enumValue);

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        uint GetTypeElementTypeId(Module module, uint typeId);

        /// <summary>
        /// Gets the type pointer to type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        uint GetTypePointerToTypeId(Module module, uint typeId);

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        uint GetTypeSize(Module module, uint typeId);

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        ulong GetGlobalVariableAddress(Module module, string globalVariableName);

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        uint GetGlobalVariableTypeId(Module module, string globalVariableName);

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        void GetStackFrameSourceFileNameAndLine(StackFrame stackFrame, out string sourceFileName, out uint sourceFileLine, out ulong displacement);

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        void GetStackFrameFunctionName(StackFrame stackFrame, out string functionName, out ulong displacement);

        /// <summary>
        /// Gets the source file name and line for the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        void GetProcessAddressSourceFileNameAndLine(Process process, ulong address, out string sourceFileName, out uint sourceFileLine, out ulong displacement);

        /// <summary>
        /// Gets the name of the function for the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        void GetProcessAddressFunctionName(Process process, ulong address, out string functionName, out ulong displacement);

        /// <summary>
        /// Determines whether the specified process address is function type public symbol.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <returns>
        ///   <c>true</c> if the specified process address is function type public symbol; otherwise, <c>false</c>.
        /// </returns>
        bool IsFunctionAddressPublicSymbol(Process process, ulong address);

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        VariableCollection GetFrameLocals(StackFrame stackFrame, bool arguments);

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeName">Name of the type.</param>
        uint GetTypeId(Module module, string typeName);

        /// <summary>
        /// Gets the template arguments.
        /// <para>For given type: MyType&lt;Arg1, 2, Arg3&lt;5&gt;&gt;</para>
        /// <para>It will return: <code>new object[] { CodeType.Create("Arg1", Module), 2, CodeType.Create("Arg3&lt;5&gt;", Module) }</code></para>
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        object[] GetTemplateArguments(Module module, uint typeId);

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        ulong ReadSimpleData(CodeType codeType, ulong address);

        /// <summary>
        /// Gets the symbol name by address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        Tuple<string, ulong> GetSymbolNameByAddress(Process process, ulong address);

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="vtableAddress">The vtable address.</param>
        Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(Process process, ulong vtableAddress);

        /// <summary>
        /// Gets the virtual base class start address.
        /// </summary>
        /// <param name="originalCodeType">Code type of the object.</param>
        /// <param name="objectAddress">Object address.</param>
        /// <param name="virtualCodeType">Virtual class code type.</param>
        /// <returns>Address of the object which code type is virtual class.</returns>
        ulong GetVirtualClassBaseAddress(CodeType originalCodeType, ulong objectAddress, CodeType virtualCodeType);

        /// <summary>
        /// Gets path to the symbols file or <c>null</c> if we don't have symbols.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>Paths to the symbols file.</returns>
        string GetModuleSymbolsPath(Module module);
    }
}
