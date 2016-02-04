using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using CsScripts;
using System;
using System.IO;

namespace CsScriptManaged.SymbolProviders
{
    /// <summary>
    /// Symbol provider that is being implemented over DIA library.
    /// </summary>
    internal class DiaSymbolProvider : ISymbolProvider
    {
        /// <summary>
        /// The modules cache
        /// </summary>
        private DictionaryCache<Tuple<string, ulong>, ISymbolProviderModule> modules = new DictionaryCache<Tuple<string, ulong>, ISymbolProviderModule>(LoadModule);

        /// <summary>
        /// Loads the module from PDB file.
        /// </summary>
        /// <param name="pdbAndModuleAddress">The tuple of PDB path and module address.</param>
        private static ISymbolProviderModule LoadModule(Tuple<string, ulong> pdbAndModuleAddress)
        {
            string pdb = pdbAndModuleAddress.Item1;
            ulong moduleAddress = pdbAndModuleAddress.Item2;

            if (string.IsNullOrEmpty(pdb) || Path.GetExtension(pdb).ToLower() != ".pdb")
            {
                return new DbgEngSymbolProvider();
            }

            return new DiaModule(pdb, moduleAddress);
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(Module module, string globalVariableName)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetGlobalVariableAddress(module, globalVariableName);
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(Module module, string globalVariableName)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetGlobalVariableTypeId(module, globalVariableName);
        }

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(Module module, uint typeId)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeElementTypeId(module, typeId);
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(Module module, uint typeId)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeAllFieldNames(module, typeId);
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeAllFieldTypeAndOffset(module, typeId, fieldName);
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeName(module, typeId);
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(Module module, uint typeId)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeSize(module, typeId);
        }

        /// <summary>
        /// Gets the symbol tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public SymTag GetTypeTag(Module module, uint typeId)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeTag(module, typeId);
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(Module module, string typeName)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeId(module, typeName);
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
            ulong distance;
            Module module;
            ISymbolProviderModule diaModule = GetDiaModule(stackFrame.Process, stackFrame.InstructionOffset, out distance, out module);

            diaModule.GetSourceFileNameAndLine(stackFrame.Process, stackFrame.InstructionOffset, (uint)distance, out sourceFileName, out sourceFileLine, out displacement);
        }

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetStackFrameFunctionName(StackFrame stackFrame, out string functionName, out ulong displacement)
        {
            ulong distance;
            Module module;
            ISymbolProviderModule diaModule = GetDiaModule(stackFrame.Process, stackFrame.InstructionOffset, out distance, out module);

            diaModule.GetFunctionNameAndDisplacement(stackFrame.Process, stackFrame.InstructionOffset, (uint)distance, out functionName, out displacement);
            if (!functionName.Contains("!"))
            {
                functionName = module.Name + "!" + functionName;
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
            ulong distance;
            Module module;
            ISymbolProviderModule diaModule = GetDiaModule(process, address, out distance, out module);

            diaModule.GetSourceFileNameAndLine(process, address, (uint)distance, out sourceFileName, out sourceFileLine, out displacement);
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
            ulong distance;
            Module module;
            ISymbolProviderModule diaModule = GetDiaModule(process, address, out distance, out module);

            diaModule.GetFunctionNameAndDisplacement(process, address, (uint)distance, out functionName, out displacement);
            functionName = module.Name + "!" + functionName;
        }

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        public VariableCollection GetFrameLocals(StackFrame stackFrame, bool arguments)
        {
            ulong distance;
            Module module;
            ISymbolProviderModule diaModule = GetDiaModule(stackFrame.Process, stackFrame.InstructionOffset, out distance, out module);

            return diaModule.GetFrameLocals(stackFrame, module, (uint)distance, arguments);
        }

        /// <summary>
        /// Gets the DIA module.
        /// </summary>
        /// <param name="module">The module.</param>
        private ISymbolProviderModule GetDiaModule(Module module)
        {
            return modules[Tuple.Create(module.SymbolFileName, module.Offset)];
        }

        /// <summary>
        /// Gets the DIA module.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="distance">The distance.</param>
        private ISymbolProviderModule GetDiaModule(Process process, ulong instructionOffset, out ulong distance, out Module module)
        {
            module = null;
            distance = ulong.MaxValue;
            foreach (var m in process.Modules)
            {
                if (instructionOffset > m.Offset && distance > instructionOffset - m.Offset)
                {
                    module = m;
                    distance = instructionOffset - m.Offset;
                }
            }

            return GetDiaModule(module);
        }

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        public ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            ISymbolProviderModule diaModule = GetDiaModule(codeType.Module);

            return diaModule.ReadSimpleData(codeType, address);
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeFieldNames(module, typeId);
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeFieldTypeAndOffset(module, typeId, fieldName);
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(Module module, uint typeId, string className)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetTypeBaseClass(module, typeId, className);
        }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(Module module, uint enumTypeId, ulong enumValue)
        {
            ISymbolProviderModule diaModule = GetDiaModule(module);

            return diaModule.GetEnumName(module, enumTypeId, enumValue);
        }
    }
}
