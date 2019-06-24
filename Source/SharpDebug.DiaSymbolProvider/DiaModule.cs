using DIA;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpDebug.Engine.SymbolProviders
{
    /// <summary>
    /// DIA library module representation
    /// </summary>
    internal class DiaModule : ISymbolProviderModule
    {
        /// <summary>
        /// The DIA data source
        /// </summary>
        private IDiaDataSource dia;

        /// <summary>
        /// The DIA session
        /// </summary>
        private IDiaSession session;

        /// <summary>
        /// The global scope symbol
        /// </summary>
        private IDiaSymbol globalScope;

        /// <summary>
        /// The cache of type all fields
        /// </summary>
        private DictionaryCache<uint, List<Tuple<string, uint, int>>> typeAllFields;

        /// <summary>
        /// The cache of type fields
        /// </summary>
        private DictionaryCache<uint, List<Tuple<string, uint, int>>> typeFields;

        /// <summary>
        /// The cache of type static fields
        /// </summary>
        private DictionaryCache<uint, List<Tuple<string, uint, ulong>>> typeStaticFields;

        /// <summary>
        /// The basic types
        /// </summary>
        private SimpleCache<Dictionary<string, IDiaSymbol>> basicTypes;

        /// <summary>
        /// The cache of enumeration types to {enumeration value, name}
        /// </summary>
        private DictionaryCache<uint, Dictionary<ulong, string>> enumTypeNames;

        /// <summary>
        /// The cache of symbol names by address
        /// </summary>
        private DictionaryCache<uint, Tuple<string, ulong>> symbolNamesByAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiaModule"/> class.
        /// </summary>
        /// <param name="pdbPath">The PDB path.</param>
        /// <param name="module">The module.</param>
        public DiaModule(string pdbPath, Module module)
        {
            IDiaSession diaSession;

            dia = DiaLoader.CreateDiaSource();
            dia.loadDataFromPdb(pdbPath);
            dia.openSession(out diaSession);
            Initialize(diaSession, module);
            PdbPath = pdbPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiaModule"/> class.
        /// </summary>
        /// <param name="diaSession">The DIA session.</param>
        /// <param name="module">The module.</param>
        public DiaModule(IDiaSession diaSession, Module module)
        {
            Initialize(diaSession, module);
            PdbPath = module.SymbolFileName;
        }

        /// <summary>
        /// Initializes this instance of the <see cref="DiaModule"/> class.
        /// </summary>
        /// <param name="diaSession">The DIA session.</param>
        /// <param name="module">The module.</param>
        private void Initialize(IDiaSession diaSession, Module module)
        {
            Module = module;
            session = diaSession;
            globalScope = session.globalScope;
            typeAllFields = new DictionaryCache<uint, List<Tuple<string, uint, int>>>(GetTypeAllFields);
            typeFields = new DictionaryCache<uint, List<Tuple<string, uint, int>>>(GetTypeFields);
            typeStaticFields = new DictionaryCache<uint, List<Tuple<string, uint, ulong>>>(GetTypeStaticFields);
            basicTypes = SimpleCache.Create(() =>
            {
                var types = new Dictionary<string, IDiaSymbol>();
                var basicTypes = globalScope.GetChildren(SymTagEnum.BaseType);

                foreach (var type in basicTypes)
                {
                    try
                    {
                        string typeString = TypeToString.GetTypeString(type);

                        if (!types.ContainsKey(typeString))
                        {
                            types.Add(typeString, type);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                return types;
            });
            symbolNamesByAddress = new DictionaryCache<uint, Tuple<string, ulong>>((distance) =>
            {
                IDiaSymbol symbol;
                int displacement;
                string name;

                session.findSymbolByRVAEx(distance, SymTagEnum.Null, out symbol, out displacement);
                name = symbol.get_undecoratedNameEx(UndecoratedNameOptions.NameOnly | UndecoratedNameOptions.NoEscu);
                return Tuple.Create(name, (ulong)displacement);
            });

            session.loadAddress = module.Address;
            enumTypeNames = new DictionaryCache<uint, Dictionary<ulong, string>>(GetEnumName);
        }

        /// <summary>
        /// Gets path to the symbols file.
        /// </summary>
        public string PdbPath { get; private set; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        internal Module Module { get; private set; }

        /// <summary>
        /// Gets the basic types.
        /// </summary>
        private Dictionary<string, IDiaSymbol> BasicTypes
        {
            get
            {
                return basicTypes.Value;
            }
        }

        /// <summary>
        /// Gets path to the symbols file or <c>null</c> if we don't have symbols.
        /// </summary>
        public string GetSymbolsPath()
        {
            return PdbPath;
        }

        /// <summary>
        /// Gets all fields from the type (including base classes).
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private List<Tuple<string, uint, int>> GetTypeAllFields(uint typeId)
        {
            var type = GetTypeFromId(typeId);
            List<Tuple<string, uint, int>> fields = new List<Tuple<string, uint, int>>();

            GetTypeAllFields(type, fields);

            return fields;
        }

        /// <summary>
        /// Gets all fields from the type (including base classes).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="typeFields">The type fields.</param>
        /// <param name="offset">The offset.</param>
        private void GetTypeAllFields(IDiaSymbol type, List<Tuple<string, uint, int>> typeFields, int offset = 0)
        {
            // Get all fields from base classes
            var bases = type.GetBaseClasses();

            foreach (var b in bases)
            {
                int newOffset = b.virtualBaseClass || offset == int.MinValue ? int.MinValue : offset + b.offset;

                GetTypeAllFields(b, typeFields, newOffset);
            }

            // Get type fields
            var fields = type.GetChildren(SymTagEnum.Data);
            foreach (var field in fields)
            {
                if (field.dataKind == DataKind.StaticMember)
                {
                    continue;
                }

                int newOffset = offset == int.MinValue ? int.MinValue : offset + field.offset;
                uint typeId = offset == int.MinValue ? type.typeId : field.typeId;

                typeFields.Add(Tuple.Create(field.name, typeId, newOffset));
            }
        }

        /// <summary>
        /// Gets the type fields.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private List<Tuple<string, uint, int>> GetTypeFields(uint typeId)
        {
            var type = GetTypeFromId(typeId);
            List<Tuple<string, uint, int>> typeFields = new List<Tuple<string, uint, int>>();
            var fields = type.GetChildren(SymTagEnum.Data);

            foreach (var field in fields)
            {
                if (field.dataKind == DataKind.StaticMember)
                {
                    continue;
                }

                typeFields.Add(Tuple.Create(field.name, field.typeId, field.offset));
            }

            return typeFields;
        }

        /// <summary>
        /// Gets the type static fields.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private List<Tuple<string, uint, ulong>> GetTypeStaticFields(uint typeId)
        {
            var type = GetTypeFromId(typeId);
            List<Tuple<string, uint, ulong>> typeFields = new List<Tuple<string, uint, ulong>>();
            var fields = type.GetChildren(SymTagEnum.Data);

            foreach (var field in fields)
            {
                if (field.dataKind != DataKind.StaticMember && field.dataKind != DataKind.Constant)
                {
                    continue;
                }

                typeFields.Add(Tuple.Create(field.name, field.typeId, field.relativeVirtualAddress + Module.Offset));
            }

            return typeFields;
        }

        /// <summary>
        /// Gets the type symbol from type identifier.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private IDiaSymbol GetTypeFromId(uint typeId)
        {
            return session.symbolById(typeId);
        }

        /// <summary>
        /// Gets the code type tag of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public CodeTypeTag GetTypeTag(uint typeId)
        {
            return FixStaFailure(() =>
            {
                SymTagEnum symTag = GetTypeFromId(typeId).symTag;

                return symTag.ToCodeTypeTag();
            });
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(uint typeId)
        {
            return FixStaFailure(() =>
            {
                return (uint)GetTypeFromId(typeId).length;
            });
        }

        /// <summary>
        /// Tries to get the type identifier.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeId">The type identifier.</param>
        public bool TryGetTypeId(string typeName, out uint typeId)
        {
            uint tid = 0;

            bool result = FixStaFailure(() =>
            {
                IDiaSymbol type;

                if (typeName == "unsigned __int64")
                    typeName = "unsigned long long";
                else if (typeName == "__int64")
                    typeName = "long long";
                else if (typeName == "long")
                    typeName = "int";
                else if (typeName == "unsigned long")
                    typeName = "unsigned int";
                else if (typeName == "signed char")
                    typeName = "char";

                if (basicTypes.Cached)
                {
                    if (!BasicTypes.TryGetValue(typeName, out type))
                        type = GetTypeFromGlobalSpace(typeName);
                }
                else
                {
                    type = GetTypeFromGlobalSpace(typeName);
                    if (type == null)
                        type = BasicTypes[typeName];
                }

                if (type != null)
                {
                    tid = GetTypeId(type);
                    return true;
                }

                tid = 0;
                return false;
            });

            typeId = tid;
            return result;
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(string typeName)
        {
            uint typeId;

            if (!TryGetTypeId(typeName, out typeId))
                throw new Exception($"Type name not found: {typeName}");
            return typeId;
        }

        /// <summary>
        /// Gets the template arguments. This is optional to be implemented in symbol module provider. If it is not implemented, <see cref="NativeCodeType.GetTemplateArguments"/> will do the job.
        /// <para>For given type: MyType&lt;Arg1, 2, Arg3&lt;5&gt;&gt;</para>
        /// <para>It will return: <code>new object[] { CodeType.Create("Arg1", Module), 2, CodeType.Create("Arg3&lt;5&gt;", Module) }</code></para>
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public object[] GetTemplateArguments(uint typeId)
        {
            return null;
        }

        /// <summary>
        /// Gets the type from global space.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        private IDiaSymbol GetTypeFromGlobalSpace(string typeName)
        {
            IDiaSymbol type = globalScope.GetChild(typeName, SymTagEnum.UDT);

            if (type == null)
            {
                type = globalScope.GetChild(typeName, SymTagEnum.Enum);
            }

            if (type == null)
            {
                type = globalScope.GetChild(typeName);
            }

            return type;
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="type">The type.</param>
        private uint GetTypeId(IDiaSymbol type)
        {
            return type.symIndexId;
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(uint typeId)
        {
            return FixStaFailure(() =>
            {
                return TypeToString.GetTypeString(GetTypeFromId(typeId));
            });
        }

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(uint typeId)
        {
            return FixStaFailure(() =>
            {
                return GetTypeFromId(typeId).typeId;
            });
        }

        /// <summary>
        /// Gets the type pointer to type of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <returns>Type id to pointer type, or <c>int.MaxValue</c> if it doesn't exist and fake should be used.</returns>
        public uint GetTypePointerToTypeId(uint typeId)
        {
            return FixStaFailure(() =>
            {
                var symbol = GetTypeFromId(typeId);
                var pointer = symbol.objectPointerType;

                if (pointer != null)
                    return pointer.symIndexId;
                if (TryGetTypeId(symbol.name + "*", out typeId))
                    return typeId;
                return (uint)int.MaxValue;
            });
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(uint typeId)
        {
            return FixStaFailure(() =>
            {
                return GetTypeAllFieldNames(GetTypeFromId(typeId));
            });
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private string[] GetTypeAllFieldNames(IDiaSymbol type)
        {
            if (type.symTag == SymTagEnum.PointerType)
            {
                type = type.type;
            }

            var fields = typeAllFields[type.symIndexId];

            return fields.Select(t => t.Item1).ToArray();
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(uint typeId, string fieldName)
        {
            return FixStaFailure(() =>
            {
                return GetTypeFieldTypeAndOffset(GetTypeFromId(typeId), fieldName);
            });
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldName">Name of the field.</param>
        private Tuple<uint, int> GetTypeFieldTypeAndOffset(IDiaSymbol type, string fieldName)
        {
            if (type.symTag == SymTagEnum.PointerType)
            {
                type = type.type;
            }

            var fields = typeAllFields[type.symIndexId];

            foreach (var field in fields)
            {
                if (field.Item1 != fieldName)
                {
                    continue;
                }

                return Tuple.Create(field.Item2, field.Item3);
            }

            throw new Exception("Field not found");
        }

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        /// <exception cref="System.Exception">Address not found</exception>
        /// <exception cref="Exception">Address not found</exception>
        public void GetSourceFileNameAndLine(uint address, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
        {
            string fileName = null;
            uint fileLine = 0;
            ulong disp = 0;

            FixStaFailure(() =>
            {
                IDiaSymbol function = session.findSymbolByRVA(address, SymTagEnum.Function);
                IDiaEnumLineNumbers lineNumbers = session.findLinesByRVA(address, (uint)function.length);

                foreach (IDiaLineNumber lineNumber in lineNumbers.Enum())
                {
                    if (address >= lineNumber.relativeVirtualAddress)
                    {
                        fileName = lineNumber.sourceFile.fileName;
                        fileLine = lineNumber.lineNumber;
                        disp = address - lineNumber.relativeVirtualAddress;
                        return;
                    }
                }

                throw new Exception("Address not found");
            });
            sourceFileName = fileName;
            sourceFileLine = fileLine;
            displacement = disp;
        }

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetFunctionNameAndDisplacement(uint address, out string functionName, out ulong displacement)
        {
            int innerDisplacement;
            IDiaSymbol function;

            session.findSymbolByRVAEx(address, SymTagEnum.Function, out function, out innerDisplacement);
            displacement = (ulong)innerDisplacement;
            functionName = function.name;
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
            int innerDisplacement;
            IDiaSymbol function;

            session.findSymbolByRVAEx(address, SymTagEnum.Null, out function, out innerDisplacement);
            return function != null && function.symTag == SymTagEnum.PublicSymbol;
        }

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="relativeAddress">The relative address.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        public VariableCollection GetFrameLocals(StackFrame frame, uint relativeAddress, bool arguments)
        {
            return FixStaFailure(() =>
            {
                IDiaSymbol function;
                int displacement;
                List<Variable> variables = new List<Variable>();

                session.findSymbolByRVAEx(relativeAddress, SymTagEnum.Function, out function, out displacement);
                GetFrameLocals(function, relativeAddress, variables, frame, Module, arguments);
                if (!arguments)
                {
                    IDiaSymbol block;

                    // Locate locals using block.
                    session.findSymbolByRVAEx(relativeAddress, SymTagEnum.Block, out block, out displacement);

                    if (block != null)
                    {
                        // Traverse blocks till we reach function.
                        while (block.symTag != SymTagEnum.Function)
                        {
                            GetFrameLocals(block, uint.MaxValue, variables, frame, Module, arguments);
                            block = block.lexicalParent;
                        }
                    }
                }

                return new VariableCollection(variables.ToArray());
            });
        }

        /// <summary>
        /// Gets the stack frame locals.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="relativeAddress">The relative address or uint.MaxValue if only first children are desired.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="frame">The frame.</param>
        /// <param name="module">The module.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        private static void GetFrameLocals(IDiaSymbol block, uint relativeAddress, List<Variable> variables, StackFrame frame, Module module, bool arguments)
        {
            IEnumerable<IDiaSymbol> symbols;

            if (relativeAddress != uint.MaxValue)
            {
                IDiaEnumSymbols symbolsEnum = block.findChildrenExByRVA(SymTagEnum.Null, null, 0, relativeAddress);

                symbols = symbolsEnum.Enum();
            }
            else
            {
                symbols = block.GetChildren(SymTagEnum.Data);
            }

            string previousName = null;
            uint previousCountLiveRanges = 0;
            uint previousOffsetInUdt = 0;

            foreach (var symbol in symbols)
            {
                SymTagEnum tag = symbol.symTag;

                if (tag == SymTagEnum.Data)
                {
                    DataKind symbolDataKind = symbol.dataKind;

                    if ((arguments && symbolDataKind != DataKind.Param) || symbol.locationType == LocationType.Null)
                    {
                        continue;
                    }
                }
                else if (tag != SymTagEnum.FunctionArgType || !arguments)
                {
                    continue;
                }

                try
                {
                    CodeType codeType = module.TypesById[symbol.typeId];
                    string variableName = symbol.name;
                    uint countLiveRanges = symbol.countLiveRanges;
                    uint offsetInUdt = symbol.offsetInUdt;
                    ulong address = ResolveAddress(module.Process, symbol, frame.FrameContext);
                    Variable variable;
                    bool hasData = symbol.locationType == LocationType.Enregistered;

                    if (codeType.IsPointer && hasData)
                    {
                        variable = Variable.CreatePointerNoCast(codeType, address, variableName, variableName);
                    }
                    else
                    {
                        variable = Variable.CreateNoCast(codeType, address, variableName, variableName);
                    }
                    if (!codeType.IsPointer && hasData)
                    {
                        variable.Data = address;
                    }

                    if (previousName != variableName || previousOffsetInUdt != offsetInUdt || previousCountLiveRanges != countLiveRanges)
                    {
                        variables.Add(variable);
                    }
                    previousName = variableName;
                    previousCountLiveRanges = countLiveRanges;
                    previousOffsetInUdt = offsetInUdt;
                }
                catch
                {
                    // TODO: Surface error to the user ...somehow...
                }
            }
        }

        /// <summary>
        /// Resolves the symbol address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="frameContext">The frame context.</param>
        private static ulong ResolveAddress(Process process, IDiaSymbol symbol, ThreadContext frameContext)
        {
            ulong address;

            switch (symbol.locationType)
            {
                case LocationType.RegRel:
                case LocationType.Enregistered:
                    switch (symbol.registerId)
                    {
                        case CV_HREG_e.CV_AMD64_ESP:
                        case CV_HREG_e.CV_AMD64_RSP:
                            address = frameContext.StackPointer;
                            break;
                        case CV_HREG_e.CV_AMD64_RIP: //case CV_HREG_e.CV_REG_EIP:
                            address = frameContext.InstructionPointer;
                            break;
                        case CV_HREG_e.CV_AMD64_RBP:
                        case CV_HREG_e.CV_AMD64_EBP:
                            address = frameContext.FramePointer;
                            break;
                        case CV_HREG_e.CV_ALLREG_VFRAME:
                            if (process.ArchitectureType == ArchitectureType.Amd64)
                            {
                                address = frameContext.StackPointer;
                            }
                            else
                            {
                                address = frameContext.FramePointer;
                            }
                            break;
                        default:
                            {
                                IRegistersAccess registersAccess = frameContext.Registers as IRegistersAccess;

                                if (registersAccess == null)
                                {
                                    throw new Exception("Unknown register id" + symbol.registerId);
                                }
                                address = registersAccess.GetRegisterValue(symbol.registerId);
                            }
                            break;
                    }

                    if (symbol.locationType == LocationType.RegRel)
                    {
                        address += (ulong)symbol.offset;
                    }
                    return address;

                case LocationType.Static:
                    return symbol.virtualAddress;

                default:
                    throw new Exception("Unknown location type " + symbol.locationType);
            }
        }

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        public ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            return Debugger.ReadSimpleData(codeType, address);
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(string globalVariableName)
        {
            return FixStaFailure(() =>
            {
                var globalVariable = GetGlobalVariable(globalVariableName);

                return globalVariable.relativeVirtualAddress + Module.Offset;
            });
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(string globalVariableName)
        {
            return FixStaFailure(() =>
            {
                var globalVariable = GetGlobalVariable(globalVariableName);

                return GetTypeId(globalVariable.type);
            });
        }

        /// <summary>
        /// Gets the global variable.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        private IDiaSymbol GetGlobalVariable(string globalVariableName)
        {
            var globalVariable = globalScope.GetChild(globalVariableName);

            if (globalVariable == null)
            {
                var spaces = globalVariableName.Split(new string[] { "::" }, StringSplitOptions.None);

                if (spaces.Length > 0)
                {
                    var type = globalScope.GetChild(string.Join("::", spaces.Take(spaces.Length - 1)));

                    if (type != null)
                    {
                        globalVariable = type.GetChild(spaces.Last(), SymTagEnum.Data);
                    }
                }
            }

            if (globalVariable == null || globalVariable.relativeVirtualAddress == 0)
            {
                throw new Exception("Global variable not found " + globalVariableName);
            }

            return globalVariable;
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(uint typeId)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.symTag == SymTagEnum.PointerType)
                {
                    type = type.type;
                }

                var fields = typeFields[type.symIndexId];

                return fields.Select(t => t.Item1).ToArray();
            });
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(uint typeId, string fieldName)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.symTag == SymTagEnum.PointerType)
                {
                    type = type.type;
                }

                var fields = typeFields[type.symIndexId];

                foreach (var field in fields)
                {
                    if (field.Item1 != fieldName)
                    {
                        continue;
                    }

                    return Tuple.Create(field.Item2, field.Item3);
                }

                throw new Exception("Field not found");
            });
        }

        /// <summary>
        /// Gets the names of static fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeStaticFieldNames(uint typeId)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.symTag == SymTagEnum.PointerType)
                {
                    type = type.type;
                }

                var fields = typeStaticFields[type.symIndexId];

                return fields.Select(t => t.Item1).ToArray();
            });
        }

        /// <summary>
        /// Gets the static field type id and address of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, ulong> GetTypeStaticFieldTypeAndAddress(uint typeId, string fieldName)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.symTag == SymTagEnum.PointerType)
                {
                    type = type.type;
                }

                var fields = typeStaticFields[type.symIndexId];

                foreach (var field in fields)
                {
                    if (field.Item1 != fieldName)
                    {
                        continue;
                    }

                    return Tuple.Create(field.Item2, field.Item3);
                }

                throw new Exception("Field not found");
            });
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
            return FixStaFailure(() =>
            {
                IDiaSymbol objectType = GetTypeFromId(objectTypeId);
                IDiaSymbol virtualType = GetTypeFromId(virtualTypeId);

                return GetVirtualClassBaseAddress(objectType, objectAddress, virtualType.name);
            });
        }

        /// <summary>
        /// Gets the virtual base class start address.
        /// </summary>
        /// <param name="objectType">Object type as <see cref="IDiaSymbol"/>.</param>
        /// <param name="objectAddress">Object address.</param>
        /// <param name="virtualTypeName">Virtual class type name.</param>
        /// <returns>Address of the object which code type is virtual class.</returns>
        private ulong GetVirtualClassBaseAddress(IDiaSymbol objectType, ulong objectAddress, string virtualTypeName)
        {
            var baseClasses = objectType.GetBaseClasses();

            foreach (IDiaSymbol b in baseClasses)
            {
                int offset;

                if (b.virtualBaseClass)
                {
                    ulong vttAddress = objectAddress + (ulong)b.virtualBasePointerOffset;
                    ulong vbAddress = Module.Process.ReadPointer(vttAddress);
                    offset = Module.Process.ReadInt(vbAddress + b.virtualBaseDispIndex * 4);
                }
                else
                {
                    offset = b.offset;
                }

                ulong newAddress = objectAddress + (ulong)offset;

                if (CodeType.TypeNameMatches(b.name, virtualTypeName))
                {
                    return newAddress;
                }

                newAddress = GetVirtualClassBaseAddress(b, newAddress, virtualTypeName);
                if (newAddress != 0)
                {
                    return newAddress;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(uint typeId, string className)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.symTag == SymTagEnum.PointerType)
                {
                    type = type.type;
                }

                if (CodeType.TypeNameMatches(type.name, className))
                {
                    return Tuple.Create(type.symIndexId, 0);
                }

                Stack<Tuple<IDiaSymbol, int>> classes = new Stack<Tuple<IDiaSymbol, int>>();

                classes.Push(Tuple.Create(type, 0));

                while (classes.Count > 0)
                {
                    var tuple = classes.Pop();
                    var bases = tuple.Item1.GetBaseClasses();

                    foreach (var b in bases.Reverse())
                    {
                        int offset = tuple.Item2 + b.offset;

                        if (b.virtualBaseClass || tuple.Item2 == int.MinValue)
                        {
                            offset = int.MinValue;
                        }

                        if (CodeType.TypeNameMatches(b.name, className))
                        {
                            return Tuple.Create(GetTypeId(b.name), offset);
                        }

                        classes.Push(Tuple.Create(b, offset));
                    }
                }

                throw new Exception("Base class not found");
            });
        }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(uint enumTypeId, ulong enumValue)
        {
            return FixStaFailure(() =>
            {
                return enumTypeNames[enumTypeId][enumValue];
            });
        }

        /// <summary>
        /// Gets the name of all enumeration values.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        private Dictionary<ulong, string> GetEnumName(uint enumTypeId)
        {
            var type = GetTypeFromId(enumTypeId);

            if (type.symTag != SymTagEnum.Enum)
            {
                throw new Exception("type must be enum");
            }

            var children = type.GetChildren().ToArray();
            var result = new Dictionary<ulong, string>();

            foreach (var child in children)
            {
                ulong value = Convert.ToUInt64(child.value);

                if (!result.ContainsKey(value))
                {
                    result.Add(value, child.name);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the type's built-in type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public BuiltinType GetTypeBuiltinType(uint typeId)
        {
            return FixStaFailure(() =>
            {
                BasicType basicType = GetTypeFromId(typeId).baseType;
                uint size = GetTypeSize(typeId);

                switch (basicType)
                {
                    case BasicType.Bool:
                        return BuiltinType.Bool;
                    case BasicType.Char16:
                    case BasicType.Char32:
                    case BasicType.WChar:
                    case BasicType.Char:
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
                    case BasicType.Int:
                    case BasicType.Long:
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
                    case BasicType.UInt:
                    case BasicType.ULong:
                    case BasicType.Hresult:
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
                    case BasicType.Float:
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
                    case BasicType.Void:
                        return BuiltinType.Void;
                    default:
                        return BuiltinType.NoType;
                }
            });
        }

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(uint typeId)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.symTag == SymTagEnum.PointerType)
                {
                    type = type.type;
                }

                var bases = type.GetBaseClasses();
                var result = new Dictionary<string, Tuple<uint, int>>();

                foreach (var b in bases.Reverse())
                {
                    int offset = b.virtualBaseClass ? int.MinValue : b.offset;

                    result.Add(b.name, Tuple.Create(GetTypeId(b.name), offset));
                }

                return result;
            });
        }

        /// <summary>
        /// Gets the symbol name by address.
        /// </summary>
        /// <param name="address">The address within the module.</param>
        public Tuple<string, ulong> GetSymbolNameByAddress(uint address)
        {
            return FixStaFailure(() =>
            {
                return symbolNamesByAddress[address];
            });
        }

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="vtableAddress">The vtable address within the module.</param>
        public Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(uint vtableAddress)
        {
            return FixStaFailure(() =>
            {
                IDiaSymbol symbol;
                int displacement;
                string fullyUndecoratedName, partiallyUndecoratedName;

                session.findSymbolByRVAEx(vtableAddress, SymTagEnum.PublicSymbol, out symbol, out displacement);
                fullyUndecoratedName = symbol.get_undecoratedNameEx(UndecoratedNameOptions.NameOnly | UndecoratedNameOptions.NoEscu) ?? symbol.name;
                partiallyUndecoratedName = symbol.get_undecoratedNameEx(UndecoratedNameOptions.NoEscu) ?? symbol.name;

                // Fully undecorated name should be in form: "DerivedClass::`vftable'" or  "DerivedClass::`vbtable'"
                const string vftableString = "::`vftable'";
                const string vbtableString = "::`vbtable'";
                string vtableString = vftableString;

                if (string.IsNullOrEmpty(fullyUndecoratedName) || !fullyUndecoratedName.EndsWith(vftableString))
                {
                    vtableString = vbtableString;
                    if (string.IsNullOrEmpty(fullyUndecoratedName) || !fullyUndecoratedName.EndsWith(vbtableString))
                    {
                        // Pointer is not vtable.
                        return null;
                    }
                }

                string codeTypeName = fullyUndecoratedName.Substring(0, fullyUndecoratedName.Length - vtableString.Length);
                CodeType codeType = CodeType.Create(codeTypeName, Module);

                // Partially undecorated name should be in form: "const DerivedClass::`vftable'{for `BaseClass'}"
                string partiallyUndecoratedNameStart = string.Format("const {0}{1}{{for `", codeTypeName, vtableString);

                if (!partiallyUndecoratedName.StartsWith(partiallyUndecoratedNameStart))
                {
                    // Single Inheritace
                    return Tuple.Create(codeType, 0);
                }

                string innerCodeTypeName = partiallyUndecoratedName.Substring(partiallyUndecoratedNameStart.Length, partiallyUndecoratedName.Length - 2 - partiallyUndecoratedNameStart.Length);

                var baseClassWithVTable = codeType.BaseClasses[innerCodeTypeName];

                return Tuple.Create(codeType, baseClassWithVTable.Item2);
            });
        }

        /// <summary>
        /// Releases the COM objects.
        /// </summary>
        internal void ReleaseComObjects()
        {
            foreach (var basicType in basicTypes.Value.Values)
                Marshal.FinalReleaseComObject(basicType);
            Marshal.FinalReleaseComObject(globalScope);
            Marshal.FinalReleaseComObject(session);
            Marshal.FinalReleaseComObject(dia);
        }

        /// <summary>
        /// Gets all available types from the module.
        /// </summary>
        /// <returns>Enumeration of type identifiers.</returns>
        public IEnumerable<uint> GetAllTypes()
        {
            // Get all types defined in the symbol
            List<IDiaSymbol> diaGlobalTypes = session.globalScope.GetChildren(SymTagEnum.UDT).ToList();
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.Enum));

            // Remove duplicate symbols by searching for the by name
            HashSet<string> usedNames = new HashSet<string>();

            foreach (IDiaSymbol s in diaGlobalTypes)
            {
                string name = s.name;

                if (!usedNames.Contains(name))
                {
                    IDiaSymbol ss = session.globalScope.GetChild(name, s.symTag);

                    if (ss != null)
                    {
                        yield return GetTypeId(ss);
                    }
                    else
                    {
                        yield return GetTypeId(s);
                    }

                    usedNames.Add(name);
                }
            }
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
            var type = GetTypeFromId(enumTypeId);

            if (type.symTag == SymTagEnum.Enum)
            {
                foreach (var enumValue in type.GetChildren(SymTagEnum.Null))
                {
                    yield return Tuple.Create(enumValue.name, enumValue.value.ToString());
                }
            }
        }

        /// <summary>
        /// Determines whether the specified type has virtual table of functions.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public bool HasTypeVTable(uint typeId)
        {
            return FixStaFailure(() =>
            {
                var type = GetTypeFromId(typeId);

                if (type.GetChildren(SymTagEnum.VTable).Any())
                {
                    return true;
                }
                var bases = type.GetBaseClasses();

                foreach (var baseClass in bases)
                {
                    if (baseClass.offset == 0 && HasTypeVTable(GetTypeId(baseClass)))
                    {
                        return true;
                    }
                }
                return false;
            });
        }

        /// <summary>
        /// Gets the global scope.
        /// </summary>
        public uint GetGlobalScope()
        {
            return FixStaFailure(() =>
            {
                return GetTypeId(session.globalScope);
            });
        }

        /// <summary>
        /// Helper function that tries to fix problems with executing COM functions in STA thread.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="action">Action that should be evaluated.</param>
        private static T FixStaFailure<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (InvalidCastException ex)
            {
                if (!ex.Message.Contains("COM"))
                    throw;

                try
                {
                    return System.Threading.Tasks.Task.Run(action).Result;
                }
                catch
                {
                    throw new AggregateException(ex);
                }
            }
        }

        /// <summary>
        /// Helper function that tries to fix problems with executing COM functions in STA thread.
        /// </summary>
        /// <param name="action">Action that should be evaluated.</param>
        private static void FixStaFailure(Action action)
        {
            try
            {
                action();
            }
            catch (InvalidCastException ex)
            {
                if (!ex.Message.Contains("COM"))
                    throw;

                try
                {
                    System.Threading.Tasks.Task.Run(action).Wait();
                }
                catch
                {
                    throw new AggregateException(ex);
                }
            }
        }
    }
}
