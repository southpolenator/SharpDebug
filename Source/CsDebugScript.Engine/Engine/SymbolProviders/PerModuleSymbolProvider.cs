using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;

namespace CsDebugScript.Engine.SymbolProviders
{
    /// <summary>
    /// Symbol provider that is being implemented over DIA library.
    /// </summary>
    public abstract class PerModuleSymbolProvider : ISymbolProvider
    {
        /// <summary>
        /// The modules cache
        /// </summary>
        private DictionaryCache<Module, ISymbolProviderModule> modules;

        /// <summary>
        /// The cache of runtime code type and offset
        /// </summary>
        private DictionaryCache<Tuple<Process, ulong>, Tuple<CodeType, int>> runtimeCodeTypeAndOffsetCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerModuleSymbolProvider" /> class.
        /// </summary>
        /// <param name="fallbackSymbolProvider">The fall-back symbol provider.</param>
        public PerModuleSymbolProvider(ISymbolProvider fallbackSymbolProvider = null)
        {
            FallbackSymbolProvider = fallbackSymbolProvider ?? Context.SymbolProvider;
            if (FallbackSymbolProvider == null)
            {
                FallbackSymbolProvider = Context.Debugger?.GetDefaultSymbolProvider();
            }
            modules = new DictionaryCache<Module, ISymbolProviderModule>(LoadModule);
            runtimeCodeTypeAndOffsetCache = new DictionaryCache<Tuple<Process, ulong>, Tuple<CodeType, int>>(GetRuntimeCodeTypeAndOffset);
        }

        /// <summary>
        /// Gets the fall-back symbol provider (one that we forward calls to if our symbol provider cannot handle call).
        /// </summary>
        public ISymbolProvider FallbackSymbolProvider { get; private set; }

        /// <summary>
        /// Loads symbol provider module from the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>Interface for symbol provider module</returns>
        public abstract ISymbolProviderModule LoadModule(Module module);

        /// <summary>
        /// Gets the <see cref="ISymbolProviderModule" /> interface for the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        public ISymbolProviderModule GetSymbolProviderModule(Module module)
        {
            if (module == null)
            {
                return null;
            }

            if (module.SymbolProvider == null)
            {
                module.SymbolProvider = modules[module];
            }

            if (module.SymbolProvider == null && FallbackSymbolProvider != null)
            {
                module.SymbolProvider = FallbackSymbolProvider.GetSymbolProviderModule(module);
            }

            return module.SymbolProvider;
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(Module module, string globalVariableName)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetGlobalVariableAddress(module, globalVariableName);
            }

            return symbolProviderModule.GetGlobalVariableAddress(module, globalVariableName);
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(Module module, string globalVariableName)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetGlobalVariableTypeId(module, globalVariableName);
            }

            return symbolProviderModule.GetGlobalVariableTypeId(module, globalVariableName);
        }

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeElementTypeId(module, typeId);
            }

            return symbolProviderModule.GetTypeElementTypeId(module, typeId);
        }

        /// <summary>
        /// Gets the type pointer to type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypePointerToTypeId(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypePointerToTypeId(module, typeId);
            }

            return symbolProviderModule.GetTypePointerToTypeId(module, typeId);
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeAllFieldNames(module, typeId);
            }

            return symbolProviderModule.GetTypeAllFieldNames(module, typeId);
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeAllFieldTypeAndOffset(module, typeId, fieldName);
            }

            return symbolProviderModule.GetTypeAllFieldTypeAndOffset(module, typeId, fieldName);
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeName(module, typeId);
            }

            return symbolProviderModule.GetTypeName(module, typeId);
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeSize(module, typeId);
            }

            return symbolProviderModule.GetTypeSize(module, typeId);
        }

        /// <summary>
        /// Gets the code type tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public CodeTypeTag GetTypeTag(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeTag(module, typeId);
            }

            return symbolProviderModule.GetTypeTag(module, typeId);
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(Module module, string typeName)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeId(module, typeName);
            }

            return symbolProviderModule.GetTypeId(module, typeName);
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
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(stackFrame.Process, stackFrame.InstructionOffset, out distance, out module);

            if (symbolProviderModule == null)
            {
                FallbackSymbolProvider.GetStackFrameSourceFileNameAndLine(stackFrame, out sourceFileName, out sourceFileLine, out displacement);
                return;
            }

            symbolProviderModule.GetSourceFileNameAndLine(stackFrame.Process, stackFrame.InstructionOffset, (uint)distance, out sourceFileName, out sourceFileLine, out displacement);
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
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(stackFrame.Process, stackFrame.InstructionOffset, out distance, out module);

            if (symbolProviderModule == null)
            {
                FallbackSymbolProvider.GetStackFrameFunctionName(stackFrame, out functionName, out displacement);
                return;
            }

            symbolProviderModule.GetFunctionNameAndDisplacement(stackFrame.Process, stackFrame.InstructionOffset, (uint)distance, out functionName, out displacement);
            if (string.IsNullOrEmpty(functionName))
            {
                functionName = $"{module.Name}!???";
            }
            else if (!functionName.Contains("!"))
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
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(process, address, out distance, out module);

            if (symbolProviderModule == null)
            {
                FallbackSymbolProvider.GetProcessAddressSourceFileNameAndLine(process, address, out sourceFileName, out sourceFileLine, out displacement);
                return;
            }

            symbolProviderModule.GetSourceFileNameAndLine(process, address, (uint)distance, out sourceFileName, out sourceFileLine, out displacement);
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
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(process, address, out distance, out module);

            if (symbolProviderModule == null)
            {
                FallbackSymbolProvider.GetProcessAddressFunctionName(process, address, out functionName, out displacement);
                return;
            }

            symbolProviderModule.GetFunctionNameAndDisplacement(process, address, (uint)distance, out functionName, out displacement);
            functionName = module.Name + "!" + functionName;
        }

        /// <summary>
        /// Determines whether the specified process address is function type public symbol.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <returns>
        ///   <c>true</c> if the specified process address is function type public symbol; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFunctionAddressPublicSymbol(Process process, ulong address)
        {
            ulong distance;
            Module module;
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(process, address, out distance, out module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.IsFunctionAddressPublicSymbol(process, address);
            }

            return symbolProviderModule.IsFunctionAddressPublicSymbol(process, address, (uint)distance);
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
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(stackFrame.Process, stackFrame.InstructionOffset, out distance, out module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetFrameLocals(stackFrame, arguments);
            }

            return symbolProviderModule.GetFrameLocals(stackFrame, module, (uint)distance, arguments);
        }

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        public ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(codeType.Module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.ReadSimpleData(codeType, address);
            }

            return symbolProviderModule.ReadSimpleData(codeType, address);
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeFieldNames(module, typeId);
            }

            return symbolProviderModule.GetTypeFieldNames(module, typeId);
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeFieldTypeAndOffset(module, typeId, fieldName);
            }

            return symbolProviderModule.GetTypeFieldTypeAndOffset(module, typeId, fieldName);
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(Module module, uint typeId, string className)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeBaseClass(module, typeId, className);
            }

            return symbolProviderModule.GetTypeBaseClass(module, typeId, className);
        }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(Module module, uint enumTypeId, ulong enumValue)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetEnumName(module, enumTypeId, enumValue);
            }

            return symbolProviderModule.GetEnumName(module, enumTypeId, enumValue);
        }

        /// <summary>
        /// Gets the type's built-in type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public BuiltinType GetTypeBuiltinType(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeBuiltinType(module, typeId);
            }

            return symbolProviderModule.GetTypeBuiltinType(module, typeId);
        }

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(Module module, uint typeId)
        {
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetTypeDirectBaseClasses(module, typeId);
            }

            return symbolProviderModule.GetTypeDirectBaseClasses(module, typeId);
        }

        /// <summary>
        /// Gets the symbol name by address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        public Tuple<string, ulong> GetSymbolNameByAddress(Process process, ulong address)
        {
            ulong distance;
            Module module;
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(process, address, out distance, out module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetSymbolNameByAddress(process, address);
            }

            var result = symbolProviderModule.GetSymbolNameByAddress(process, address, (uint)distance);

            return new Tuple<string, ulong>(module.Name + "!" + result.Item1, result.Item2);
        }

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="vtableAddress">The vtable address.</param>
        public Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(Process process, ulong vtableAddress)
        {
            return runtimeCodeTypeAndOffsetCache[Tuple.Create(process, vtableAddress)];
        }

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="tuple">The tuple containing process and vtable address.</param>
        private Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(Tuple<Process, ulong> tuple)
        {
            Process process = tuple.Item1;
            ulong vtableAddress = tuple.Item2;
            ulong distance;
            Module module;
            ISymbolProviderModule symbolProviderModule = GetSymbolProviderModule(process, vtableAddress, out distance, out module);

            if (symbolProviderModule == null)
            {
                return FallbackSymbolProvider.GetRuntimeCodeTypeAndOffset(tuple.Item1, tuple.Item2);
            }

            return symbolProviderModule?.GetRuntimeCodeTypeAndOffset(process, vtableAddress, (uint)distance);
        }

        /// <summary>
        /// Gets the symbol provider module interface.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="instructionOffset">The instruction offset.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="module">The module.</param>
        private ISymbolProviderModule GetSymbolProviderModule(Process process, ulong instructionOffset, out ulong distance, out Module module)
        {
            module = process.GetModuleByInnerAddress(instructionOffset);
            distance = module != null ? instructionOffset - module.Address : ulong.MaxValue;
            return GetSymbolProviderModule(module);
        }
    }
}
