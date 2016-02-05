using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using CsScripts;
using DbgEngManaged;
using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsScriptManaged.SymbolProviders
{
    /// <summary>
    /// Symbol provider that is being implemented over DbgEng.dll.
    /// </summary>
    internal class DbgEngSymbolProvider : ISymbolProvider, ISymbolProviderModule
    {
        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(Module module, string globalVariableName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                string name = module.Name + "!" + globalVariableName;

                return Context.Symbols.GetOffsetByNameWide(name);
            }
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(Module module, string globalVariableName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                string name = module.Name + "!" + globalVariableName.Replace("::", ".");
                uint typeId;
                ulong moduleId;

                Context.Symbols.GetSymbolTypeIdWide(name, out typeId, out moduleId);
                return typeId;
            }
        }

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                var typedData = GlobalCache.TypedData[Tuple.Create(module.Id, typeId, module.Process.PEB)];
                typedData.Data = module.Process.PEB;
                var result = Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.GetDereference,
                    InData = typedData,
                }).OutData.TypeId;

                return result;
            }
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                List<string> fields = new List<string>();
                uint nameSize;

                try
                {
                    for (uint fieldIndex = 0; ; fieldIndex++)
                    {
                        StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                        Context.Symbols.GetFieldName(module.Id, typeId, fieldIndex, sb, (uint)sb.Capacity, out nameSize);
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
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                try
                {
                    uint fieldTypeId, fieldOffset;

                    Context.Symbols.GetFieldTypeAndOffsetWide(module.Id, typeId, fieldName, out fieldTypeId, out fieldOffset);
                    return Tuple.Create(fieldTypeId, (int)fieldOffset);
                }
                catch (Exception)
                {
                    return Tuple.Create<uint, int>(0, -1);
                }
            }
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                uint nameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                Context.Symbols.GetTypeName(module.Id, typeId, sb, (uint)sb.Capacity, out nameSize);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return Context.Symbols.GetTypeSize(module.Id, typeId);
            }
        }

        /// <summary>
        /// Gets the symbol tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public SymTag GetTypeTag(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return GlobalCache.TypedData[Tuple.Create(module.Id, typeId, module.Process.PEB)].Tag;
            }
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(Module module, string typeName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return Context.Symbols.GetTypeIdWide(module.Id, module.Name + "!" + typeName);
            }
        }

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetStackFrameSourceFileNameAndLine(StackFrame stackFrame, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
        {
            using (StackFrameSwitcher switcher = new StackFrameSwitcher(stackFrame))
            {
                uint fileNameLength;
                StringBuilder sb = new StringBuilder(Constants.MaxFileName);

                Context.Symbols.GetLineByOffset(stackFrame.InstructionOffset, out sourceFileLine, sb, (uint)sb.Capacity, out fileNameLength, out displacement);
                sourceFileName = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetStackFrameFunctionName(StackFrame stackFrame, out string functionName, out ulong displacement)
        {
            using (StackFrameSwitcher switcher = new StackFrameSwitcher(stackFrame))
            {
                uint functionNameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                Context.Symbols.GetNameByOffset(stackFrame.InstructionOffset, sb, (uint)sb.Capacity, out functionNameSize, out displacement);
                functionName = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the source file name and line for the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetProcessAddressSourceFileNameAndLine(Process process, ulong address, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(process))
            {
                uint fileNameLength;
                StringBuilder sb = new StringBuilder(Constants.MaxFileName);

                Context.Symbols.GetLineByOffset(address, out sourceFileLine, sb, (uint)sb.Capacity, out fileNameLength, out displacement);
                sourceFileName = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the name of the function for the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetProcessAddressFunctionName(Process process, ulong address, out string functionName, out ulong displacement)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(process))
            {
                uint functionNameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                Context.Symbols.GetNameByOffset(address, sb, (uint)sb.Capacity, out functionNameSize, out displacement);
                functionName = sb.ToString();
            }
        }

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        public VariableCollection GetFrameLocals(StackFrame stackFrame, bool arguments)
        {
            DebugScopeGroup scopeGroup = arguments ? DebugScopeGroup.Arguments : DebugScopeGroup.Locals;

            using (StackFrameSwitcher switcher = new StackFrameSwitcher(stackFrame))
            {
                IDebugSymbolGroup2 symbolGroup;
                Context.Symbols.GetScopeSymbolGroup2((uint)scopeGroup, null, out symbolGroup);
                uint localsCount = symbolGroup.GetNumberSymbols();
                Variable[] variables = new Variable[localsCount];
                for (uint i = 0; i < localsCount; i++)
                {
                    StringBuilder name = new StringBuilder(Constants.MaxSymbolName);
                    uint nameSize;

                    symbolGroup.GetSymbolName(i, name, (uint)name.Capacity, out nameSize);
                    var entry = symbolGroup.GetSymbolEntryInformation(i);
                    var module = stackFrame.Process.ModulesById[entry.ModuleBase];
                    var codeType = module.TypesById[entry.TypeId];
                    var address = entry.Offset;

                    variables[i] = Variable.CreateNoCast(codeType, address, name.ToString());
                }

                return new VariableCollection(variables);
            }
        }

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="processAddress">The process address.</param>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetSourceFileNameAndLine(Process process, ulong processAddress, uint address, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
        {
            GetProcessAddressSourceFileNameAndLine(process, processAddress, out sourceFileName, out sourceFileLine, out displacement);
        }

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="processAddress">The process address.</param>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetFunctionNameAndDisplacement(Process process, ulong processAddress, uint address, out string functionName, out ulong displacement)
        {
            GetProcessAddressFunctionName(process, processAddress, out functionName, out displacement);
        }

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="module">The module.</param>
        /// <param name="relativeAddress">The relative address.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        public VariableCollection GetFrameLocals(StackFrame frame, Module module, uint relativeAddress, bool arguments)
        {
            return GetFrameLocals(frame, arguments);
        }

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        public ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            Module module = codeType.Module;

            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return GlobalCache.TypedData[Tuple.Create(module.Id, codeType.TypeId, address)].Data;
            }
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(Module module, uint typeId, string className)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(Module module, uint enumTypeId, ulong enumValue)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                uint enumNameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                Context.Symbols.GetConstantNameWide(module.Offset, enumTypeId, enumValue, sb, (uint)sb.Capacity, out enumNameSize);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the type of the basic type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public BasicType GetTypeBasicType(Module module, uint typeId)
        {
            // TODO: This is currently unsupported
            return BasicType.NoType;
        }

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(Module module, uint typeId)
        {
            throw new Exception("This is not supported using DbgEng.dll. Please use DIA symbol provider.");
        }
    }
}
