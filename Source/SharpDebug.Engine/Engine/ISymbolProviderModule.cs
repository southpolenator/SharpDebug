﻿using System;
using System.Collections.Generic;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Debugging symbol provider for a module.
    /// </summary>
    public interface ISymbolProviderModule
    {
        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        ulong GetGlobalVariableAddress(string globalVariableName);

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        uint GetGlobalVariableTypeId(string globalVariableName);

        /// <summary>
        /// Gets the code type tag of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        CodeTypeTag GetTypeTag(uint typeId);

        /// <summary>
        /// Gets the type's built-in type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        BuiltinType GetTypeBuiltinType(uint typeId);

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        uint GetTypeSize(uint typeId);

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        uint GetTypeId(string typeName);

        /// <summary>
        /// Tries to get the type identifier.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeId">The type identifier.</param>
        bool TryGetTypeId(string typeName, out uint typeId);

        /// <summary>
        /// Gets the template arguments. This is optional to be implemented in symbol module provider. If it is not implemented, <see cref="NativeCodeType.GetTemplateArguments"/> will do the job.
        /// <para>For given type: MyType&lt;Arg1, 2, Arg3&lt;5&gt;&gt;</para>
        /// <para>It will return: <code>new object[] { CodeType.Create("Arg1", Module), 2, CodeType.Create("Arg3&lt;5&gt;", Module) }</code></para>
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        object[] GetTemplateArguments(uint typeId);

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        string GetTypeName(uint typeId);

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        uint GetTypeElementTypeId(uint typeId);

        /// <summary>
        /// Gets the type pointer to type of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <returns>Type id to pointer type, or <c>int.MaxValue</c> if it doesn't exist and fake should be used.</returns>
        uint GetTypePointerToTypeId(uint typeId);

        /// <summary>
        /// Gets the names of all fields of the specified type. It searches all inherited classes too.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        string[] GetTypeAllFieldNames(uint typeId);

        /// <summary>
        /// Gets the field type id and offset of the specified type. It searches all inherited classes too.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Tuple of filed type id and its offset. In case of field being part of virtual inheritance class, returns Tuple of virtual inheritance class type id and negative of its index entry in the virtual table.</returns>
        Tuple<uint, int> GetTypeAllFieldTypeAndOffset(uint typeId, string fieldName);

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        string[] GetTypeFieldNames(uint typeId);

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        Tuple<uint, int> GetTypeFieldTypeAndOffset(uint typeId, string fieldName);

        /// <summary>
        /// Gets the names of static fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        string[] GetTypeStaticFieldNames(uint typeId);

        /// <summary>
        /// Gets the static field type id and address of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        Tuple<uint, ulong> GetTypeStaticFieldTypeAndAddress(uint typeId, string fieldName);

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        Tuple<uint, int> GetTypeBaseClass(uint typeId, string className);

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        void GetSourceFileNameAndLine(uint address, out string sourceFileName, out uint sourceFileLine, out ulong displacement);

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        void GetFunctionNameAndDisplacement(uint address, out string functionName, out ulong displacement);

        /// <summary>
        /// Determines whether the specified process address is function type public symbol.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>
        ///   <c>true</c> if the specified process address is function type public symbol; otherwise, <c>false</c>.
        /// </returns>
        bool IsFunctionAddressPublicSymbol(uint address);

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="relativeAddress">The relative address.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        VariableCollection GetFrameLocals(StackFrame frame, uint relativeAddress, bool arguments);

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        ulong ReadSimpleData(CodeType codeType, ulong address);

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        string GetEnumName(uint enumTypeId, ulong enumValue);

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(uint typeId);

        /// <summary>
        /// Gets the symbol name by address.
        /// </summary>
        /// <param name="address">The address within the module.</param>
        Tuple<string, ulong> GetSymbolNameByAddress(uint address);

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="vtableAddress">The vtable address within the module.</param>
        Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(uint vtableAddress);

        /// <summary>
        /// Gets the virtual base class start address.
        /// </summary>
        /// <param name="objectTypeId">Object type identifier.</param>
        /// <param name="objectAddress">Object address.</param>
        /// <param name="virtualTypeId">Virtual class type identifier.</param>
        /// <returns>Address of the object which code type is virtual class.</returns>
        ulong GetVirtualClassBaseAddress(uint objectTypeId, ulong objectAddress, uint virtualTypeId);

        #region CodeGen needed functionality
        /// <summary>
        /// Gets all available types from the module.
        /// </summary>
        /// <returns>Enumeration of type identifiers.</returns>
        IEnumerable<uint> GetAllTypes();

        /// <summary>
        /// Gets the name and value of all enumeration values.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <returns>Enumeration of tuples of name and value for all enumeration values.</returns>
        IEnumerable<Tuple<string, string>> GetEnumValues(uint enumTypeId);

        /// <summary>
        /// Determines whether the specified type has virtual table of functions.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        bool HasTypeVTable(uint typeId);

        /// <summary>
        /// Gets the global scope type id.
        /// </summary>
        uint GetGlobalScope();

        /// <summary>
        /// Gets path to the symbols file or <c>null</c> if we don't have symbols.
        /// </summary>
        string GetSymbolsPath();
        #endregion
    }
}
