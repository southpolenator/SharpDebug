using CsDebugScript.Engine.Native;
using DbgEng;
using DIA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Symbol provider module that is being implemented over DbgEng.dll.
    /// </summary>
    internal class DbgEngSymbolProviderModule : ISymbolProviderModule
    {
        public DbgEngSymbolProviderModule(DbgEngSymbolProvider dbgEngSymbolProvider, Module module)
        {
            DbgEngSymbolProvider = dbgEngSymbolProvider;
            Module = module;
        }

        /// <summary>
        /// DbgEngDll symbol provider.
        /// </summary>
        public DbgEngSymbolProvider DbgEngSymbolProvider { get; private set; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the DbgEngDll debugger engine.
        /// </summary>
        public DbgEngDll DbgEngDll
        {
            get
            {
                return DbgEngSymbolProvider.DbgEngDll;
            }
        }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(uint enumTypeId, ulong enumValue)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                uint enumNameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                DbgEngDll.Symbols.GetConstantNameWide(Module.Offset, enumTypeId, enumValue, sb, (uint)sb.Capacity, out enumNameSize);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="relativeAddress">The relative address.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        public VariableCollection GetFrameLocals(StackFrame stackFrame, uint relativeAddress, bool arguments)
        {
            DebugScopeGroup scopeGroup = arguments ? DebugScopeGroup.Arguments : DebugScopeGroup.Locals;

            using (StackFrameSwitcher switcher = new StackFrameSwitcher(DbgEngDll.StateCache, stackFrame))
            {
                IDebugSymbolGroup2 symbolGroup;
                DbgEngDll.Symbols.GetScopeSymbolGroup2((uint)scopeGroup, null, out symbolGroup);
                uint localsCount = symbolGroup.GetNumberSymbols();
                Variable[] variables = new Variable[localsCount];
                bool doCleanup = false;

                for (uint i = 0; i < localsCount; i++)
                {
                    try
                    {
                        StringBuilder name = new StringBuilder(Constants.MaxSymbolName);
                        uint nameSize;

                        symbolGroup.GetSymbolName(i, name, (uint)name.Capacity, out nameSize);
                        var entry = symbolGroup.GetSymbolEntryInformation(i);
                        var module = stackFrame.Process.ModulesById[entry.ModuleBase];
                        var codeType = module.TypesById[entry.TypeId];
                        var address = entry.Offset;
                        var variableName = name.ToString();
                        bool hasData = false, pointerRead = false;
                        ulong data = 0;

                        if (address == 0 && entry.Size <= 8)
                        {
                            symbolGroup.GetSymbolValueText(i, name, (uint)name.Capacity, out nameSize);
                            string value = name.ToString();
                            if (value.StartsWith("0x"))
                            {
                                if (value.Length > 10 && value[10] == '`')
                                {
                                    value = value.Substring(0, 10) + value.Substring(11, 8);
                                }
                                value = value.Substring(2);
                                if (codeType.IsPointer)
                                {
                                    address = ulong.Parse(value, System.Globalization.NumberStyles.HexNumber);
                                    pointerRead = true;
                                }
                                else
                                {
                                    hasData = true;
                                    data = ulong.Parse(value, System.Globalization.NumberStyles.HexNumber);
                                }
                            }
                        }

                        if (pointerRead)
                        {
                            variables[i] = Variable.CreatePointerNoCast(codeType, address, variableName, variableName);
                        }
                        else
                        {
                            variables[i] = Variable.CreateNoCast(codeType, address, variableName, variableName);
                        }
                        if (hasData)
                        {
                            variables[i].Data = data;
                        }
                    }
                    catch
                    {
                        // This variable is not available, don't store it in a collection
                        doCleanup = true;
                    }
                }
                if (doCleanup)
                {
                    variables = variables.Where(v => v != null).ToArray();
                }
                return new VariableCollection(variables);
            }
        }

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetFunctionNameAndDisplacement(uint address, out string functionName, out ulong displacement)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                uint functionNameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                DbgEngDll.Symbols.GetNameByOffset(address + Module.Address, sb, (uint)sb.Capacity, out functionNameSize, out displacement);
                functionName = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(string globalVariableName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                string name = Module.Name + "!" + globalVariableName;

                return DbgEngDll.Symbols.GetOffsetByNameWide(name);
            }
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(string globalVariableName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                string name = Module.Name + "!" + globalVariableName.Replace("::", ".");
                uint typeId;
                ulong moduleId;

                DbgEngDll.Symbols.GetSymbolTypeIdWide(name, out typeId, out moduleId);
                return typeId;
            }
        }

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="vtableAddress">The vtable address.</param>
        public Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(uint vtableAddress)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                uint nameSize;
                ulong displacement;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                DbgEngDll.Symbols.GetNameByOffset(vtableAddress + Module.Address, sb, (uint)sb.Capacity, out nameSize, out displacement);

                // Fully undecorated name should be in form: "DerivedClass::`vftable'"
                string fullyUndecoratedName = sb.ToString();
                const string vftableString = "::`vftable'";

                if (string.IsNullOrEmpty(fullyUndecoratedName) || !fullyUndecoratedName.EndsWith(vftableString))
                {
                    // Pointer is not vtable.
                    return null;
                }

                string codeTypeName = fullyUndecoratedName.Substring(0, fullyUndecoratedName.Length - vftableString.Length);
                CodeType codeType = CodeType.Create(Module.Process, codeTypeName);

                // TODO: We need to be able to get partially undecorated name in order to find offset (See DiaModule.cs for more info)
                return Tuple.Create(codeType, 0);
            }
        }

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetSourceFileNameAndLine(uint address, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                uint fileNameLength;
                StringBuilder sb = new StringBuilder(Constants.MaxFileName);

                DbgEngDll.Symbols.GetLineByOffset(address + Module.Address, out sourceFileLine, sb, (uint)sb.Capacity, out fileNameLength, out displacement);
                sourceFileName = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the symbol name by address.
        /// </summary>
        /// <param name="address">The address.</param>
        public Tuple<string, ulong> GetSymbolNameByAddress(uint address)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);
                ulong displacement;
                uint nameSize;

                DbgEngDll.Symbols.GetNameByOffsetWide(address + Module.Address, sb, (uint)sb.Capacity, out nameSize, out displacement);
                return Tuple.Create(sb.ToString(), displacement);
            }
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                List<string> fields = new List<string>();
                uint nameSize;

                try
                {
                    for (uint fieldIndex = 0; ; fieldIndex++)
                    {
                        StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                        DbgEngDll.Symbols.GetFieldName(Module.Address, typeId, fieldIndex, sb, (uint)sb.Capacity, out nameSize);
                        fields.Add(sb.ToString());
                    }
                }
                catch (Exception)
                {
                }

                return fields.ToArray();
            }
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(uint typeId, string fieldName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                try
                {
                    uint fieldTypeId, fieldOffset;

                    DbgEngDll.Symbols.GetFieldTypeAndOffsetWide(Module.Address, typeId, fieldName, out fieldTypeId, out fieldOffset);
                    return Tuple.Create(fieldTypeId, (int)fieldOffset);
                }
                catch (Exception)
                {
                    return Tuple.Create<uint, int>(0, -1);
                }
            }
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(uint typeId, string className)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the virtual base class start address.
        /// </summary>
        /// <param name="objectTypeId">Object type identifier.</param>
        /// <param name="objectAddress">Object address.</param>
        /// <param name="virtualTypeId">Virtual class type identifier.</param>
        /// <returns>Address of the object which code type is virtual class.</returns>
        public ulong GetVirtualClassBaseAddress(uint objectTypeId, ulong objectAddress, uint virtualTypeId)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the type's built-in type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public BuiltinType GetTypeBuiltinType(uint typeId)
        {
            // TODO: Find better way to fetch basic type from DbgEng
            if (GetTypeTag(typeId) == CodeTypeTag.BuiltinType)
            {
                string name = GetTypeName(typeId);
                uint size = GetTypeSize(typeId);

                switch (name)
                {
                    case "float":
                    case "double":
                    case "long double":
                        switch (size)
                        {
                            default:
                            case 4:
                                return BuiltinType.Float32;
                            case 8:
                                return BuiltinType.Float64;
                            case 10:
                                return BuiltinType.Float80;
                        }
                    case "bool":
                        return BuiltinType.Bool;
                    case "char":
                    case "wchar_t":
                        switch (size)
                        {
                            default:
                            case 1:
                                return BuiltinType.Char8;
                            case 2:
                                return BuiltinType.Char16;
                            case 4:
                                return BuiltinType.Char32;
                        }
                    case "short":
                    case "int":
                    case "long":
                    case "long long":
                    case "int64":
                        switch (size)
                        {
                            case 1:
                                return BuiltinType.Int8;
                            case 2:
                                return BuiltinType.Int16;
                            default:
                            case 4:
                                return BuiltinType.Int32;
                            case 8:
                                return BuiltinType.Int64;
                            case 16:
                                return BuiltinType.Int128;
                        }
                    case "unsigned char":
                    case "unsigned short":
                    case "unsigned int":
                    case "unsigned long":
                    case "unsigned long long":
                    case "unsigned int64":
                        switch (size)
                        {
                            case 1:
                                return BuiltinType.UInt8;
                            case 2:
                                return BuiltinType.UInt16;
                            default:
                            case 4:
                                return BuiltinType.UInt32;
                            case 8:
                                return BuiltinType.UInt64;
                            case 16:
                                return BuiltinType.UInt128;
                        }
                    case "void":
                        return BuiltinType.Void;
                    default:
                        return BuiltinType.NoType;
                }
            }

            return BuiltinType.NoType;
        }

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(uint typeId)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                var typedData = DbgEngSymbolProvider.typedData[Tuple.Create(Module.Address, typeId, Module.Process.PebAddress)];
                typedData.Data = Module.Process.PebAddress;
                var result = DbgEngDll.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.GetDereference,
                    InData = typedData,
                }).OutData.TypeId;

                return result;
            }
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(uint typeId)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(uint typeId, string fieldName)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the names of static fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeStaticFieldNames(uint typeId)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the static field type id and address of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, ulong> GetTypeStaticFieldTypeAndAddress(uint typeId, string fieldName)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(string typeName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                return DbgEngDll.Symbols.GetTypeIdWide(Module.Address, Module.Name + "!" + typeName);
            }
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                uint nameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                DbgEngDll.Symbols.GetTypeName(Module.Address, typeId, sb, (uint)sb.Capacity, out nameSize);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the type pointer to type of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypePointerToTypeId(uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                var typedData = DbgEngSymbolProvider.typedData[Tuple.Create(Module.Address, typeId, Module.Process.PebAddress)];
                typedData.Data = Module.Process.PebAddress;
                var result = DbgEngDll.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.GetPointerTo,
                    InData = typedData,
                }).OutData.TypeId;

                return result;
            }
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                return DbgEngDll.Symbols.GetTypeSize(Module.Address, typeId);
            }
        }

        /// <summary>
        /// Gets the code type tag of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public CodeTypeTag GetTypeTag(uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                SymTagEnum symTag = DbgEngSymbolProvider.typedData[Tuple.Create(Module.Address, typeId, Module.Process.PebAddress)].Tag;

                return symTag.ToCodeTypeTag();
            }
        }

        /// <summary>
        /// Determines whether the specified process address is function type public symbol.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>
        ///   <c>true</c> if the specified process address is function type public symbol; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFunctionAddressPublicSymbol(uint address)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, Module.Process))
            {
                ulong moduleAddress;
                uint typeId;

                DbgEngDll.Symbols.GetOffsetTypeId(address + Module.Address, out typeId, out moduleAddress);
                return typeId == 0;
            }
        }

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        public ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            NativeCodeType nativeCodeType = codeType as NativeCodeType;

            if (nativeCodeType == null)
            {
                return Debugger.ReadSimpleData(codeType, address);
            }

            Module module = codeType.Module;

            using (ProcessSwitcher switcher = new ProcessSwitcher(DbgEngDll.StateCache, module.Process))
            {
                return DbgEngSymbolProvider.typedData[Tuple.Create(module.Address, nativeCodeType.TypeId, address)].Data;
            }
        }

        #region CodeGen needed functionality
        /// <summary>
        /// Gets all available types from the module.
        /// </summary>
        /// <returns>Enumeration of type identifiers.</returns>
        public IEnumerable<uint> GetAllTypes()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Gets the name and value of all enumeration values.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <returns>
        /// Enumeration of tuples of name and value for all enumeration values.
        /// </returns>
        public IEnumerable<Tuple<string, string>> GetEnumValues(uint enumTypeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the specified type has virtual table of functions.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public bool HasTypeVTable(uint typeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the global scope.
        /// </summary>
        public uint GetGlobalScope()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
