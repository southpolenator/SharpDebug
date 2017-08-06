using CsDebugScript.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using CsDebugScript.Engine.Native;
using Dia2Lib;
using CsDebugScript.Engine.Utility;
using System.Collections.Concurrent;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// DWARF symbol provider for specific module.
    /// </summary>
    /// <seealso cref="CsDebugScript.Engine.ISymbolProviderModule" />
    internal class DwarfSymbolProviderModule : ISymbolProviderModule
    {
        /// <summary>
        /// The compilation units
        /// </summary>
        private DwarfCompilationUnit[] compilationUnits;

        /// <summary>
        /// The line number programs
        /// </summary>
        private DwarfLineNumberProgram[] programs;

        /// <summary>
        /// The common information entries
        /// </summary>
        private DwarfCommonInformationEntry[] commonInformationEntries;

        /// <summary>
        /// The code segment offset (read from the image)
        /// </summary>
        private ulong codeSegmentOffset;

        /// <summary>
        /// Flag indicating if image is 64 bit.
        /// </summary>
        private bool is64bit;

        /// <summary>
        /// The cache of global variables
        /// </summary>
        private DictionaryCache<string, DwarfSymbol> globalVariables;

        /// <summary>
        /// Mapping from type ID to type.
        /// </summary>
        private ConcurrentDictionary<uint, DwarfSymbol> typeIdToType = new ConcurrentDictionary<uint, DwarfSymbol>();

        /// <summary>
        /// Mapping from The type offset to type ID.
        /// </summary>
        private ConcurrentDictionary<int, uint> typeOffsetToTypeId = new ConcurrentDictionary<int, uint>();

        /// <summary>
        /// The next available type ID.
        /// </summary>
        private uint nextTypeId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DwarfSymbolProviderModule"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="compilationUnits">The compilation units.</param>
        /// <param name="programs">The line number programs.</param>
        /// <param name="commonInformationEntries">The common information entries.</param>
        /// <param name="codeSegmentOffset">The code segment offset.</param>
        /// <param name="is64bit">if set to <c>true</c> image is 64 bit.</param>
        public DwarfSymbolProviderModule(Module module, DwarfCompilationUnit[] compilationUnits, DwarfLineNumberProgram[] programs, DwarfCommonInformationEntry[] commonInformationEntries, ulong codeSegmentOffset, bool is64bit)
        {
            Module = module;
            this.compilationUnits = compilationUnits;
            this.programs = programs;
            this.commonInformationEntries = commonInformationEntries;
            this.codeSegmentOffset = codeSegmentOffset;
            this.is64bit = is64bit;
            globalVariables = new DictionaryCache<string, DwarfSymbol>(FindGlobalVariable);
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(Module module, uint enumTypeId, ulong enumValue)
        {
            DwarfSymbol enumType = GetType(enumTypeId);

            foreach (DwarfSymbol child in enumType.Children)
            {
                if (child.Tag == DwarfTag.Enumerator && child.GetConstantAttribute(DwarfAttribute.ConstValue) == enumValue)
                {
                    return child.Name;
                }
            }

            return null;
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
            ulong displacement;
            DwarfSymbol function = FindFunction(relativeAddress, out displacement);
            List<Variable> variables = new List<Variable>();

            if (function.Children != null)
            {
                if (arguments)
                {
                    foreach (DwarfSymbol parameter in function.Children)
                    {
                        if (parameter.Tag == DwarfTag.FormalParameter && !string.IsNullOrEmpty(parameter.Name))
                        {
                            DwarfSymbol parameterType = GetType(parameter);
                            uint parameterTypeId = GetTypeId(parameterType);
                            CodeType codeType = module.TypesById[parameterTypeId];
                            ulong address = ResolveAddress(module.Process, function, parameter, frame.FrameContext);
                            string variableName = parameter.Name;

                            variables.Add(Variable.CreateNoCast(codeType, address, variableName, variableName));
                        }
                    }
                }
                else
                {
                    Queue<List<DwarfSymbol>> childrenQueue = new Queue<List<DwarfSymbol>>();

                    childrenQueue.Enqueue(function.Children);
                    while (childrenQueue.Count > 0)
                    {
                        List<DwarfSymbol> children = childrenQueue.Dequeue();

                        foreach (DwarfSymbol data in children)
                        {
                            if (data.Tag == DwarfTag.Variable && !string.IsNullOrEmpty(data.Name))
                            {
                                DwarfSymbol parameterType = GetType(data);
                                uint parameterTypeId = GetTypeId(parameterType);
                                CodeType codeType = module.TypesById[parameterTypeId];
                                ulong address = ResolveAddress(module.Process, function, data, frame.FrameContext);
                                string variableName = data.Name;

                                variables.Add(Variable.CreateNoCast(codeType, address, variableName, variableName));
                            }
                            else if (data.Tag == DwarfTag.LexicalBlock)
                            {
                                ulong startAddress = data.Attributes[DwarfAttribute.LowPc].Address;
                                DwarfAttributeValue endAddressValue = data.Attributes[DwarfAttribute.HighPc];
                                ulong endAddress = endAddressValue.Type == DwarfAttributeValueType.Constant ? startAddress + endAddressValue.Constant : endAddressValue.Address;

                                if (startAddress <= relativeAddress && relativeAddress < endAddress && data.Children != null)
                                {
                                    childrenQueue.Enqueue(data.Children);
                                }
                            }
                            else
                            {
                                // TODO:
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
            }

            return new VariableCollection(variables.ToArray());
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
            DwarfSymbol function = FindFunction(address, out displacement);

            functionName = function?.FullName;
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(Module module, string globalVariableName)
        {
            DwarfSymbol globalVariable = globalVariables[globalVariableName];
            Location location = DecodeLocation(globalVariable.Attributes[DwarfAttribute.Location]);

            return location.Address + module.Address;
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(Module module, string globalVariableName)
        {
            DwarfSymbol globalVariable = globalVariables[globalVariableName];

            return GetTypeId(GetType(globalVariable));
        }

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="vtableAddress">The vtable address.</param>
        /// <param name="distance">The distance within the module.</param>
        public Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(Process process, ulong vtableAddress, uint distance)
        {
            // TODO: Something better must be done. This is not always correct.
            MemoryBuffer memoryBuffer =  Debugger.ReadMemory(process, vtableAddress, process.GetPointerSize());
            ulong firstFunctionAddress = UserType.ReadPointer(memoryBuffer, 0, (int)process.GetPointerSize());
            ulong displacement;
            DwarfSymbol firstFunction = FindFunction(firstFunctionAddress - Module.Address, out displacement);
            DwarfSymbol implicitArgument = firstFunction.Attributes[DwarfAttribute.ObjectPointer].Reference;
            DwarfSymbol abstractOrigin = implicitArgument.Attributes.ContainsKey(DwarfAttribute.AbstractOrigin) ? implicitArgument.Attributes[DwarfAttribute.AbstractOrigin].Reference : implicitArgument;
            DwarfSymbol type = GetType(abstractOrigin);

            if (type.Tag == DwarfTag.PointerType)
            {
                type = GetType(type);
            }

            CodeType codeType = Module.TypesById[GetTypeId(type)];

            return Tuple.Create(codeType, 0);
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
            // TODO: Not linear search
            ulong minDistance = ulong.MaxValue;
            DwarfLineInformation bestMatch = null;

            foreach (DwarfLineNumberProgram program in programs)
            {
                foreach (DwarfFileInformation file in program.Files)
                {
                    foreach (DwarfLineInformation line in file.Lines)
                    {
                        ulong distance = address - line.Address;

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            bestMatch = line;
                        }
                    }
                }
            }

            if (bestMatch == null || minDistance > 0x10000)
            {
                throw new KeyNotFoundException();
            }

            sourceFileName = bestMatch.File.Path;
            sourceFileLine = bestMatch.Line;
            displacement = minDistance;
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(Module module, uint typeId)
        {
            DwarfSymbol type = GetType(typeId);
            List<string> names = new List<string>();

            if (type.Tag == DwarfTag.PointerType)
            {
                type = GetType(type);
            }

            Queue<DwarfSymbol> baseClasses = new Queue<DwarfSymbol>();

            baseClasses.Enqueue(type);
            while (baseClasses.Count > 0)
            {
                type = baseClasses.Dequeue();
                if (type.Children != null)
                {
                    foreach (DwarfSymbol child in type.Children)
                    {
                        if (child.Tag == DwarfTag.Member && !string.IsNullOrEmpty(child.Name))
                        {
                            if (!child.Attributes.ContainsKey(DwarfAttribute.External) || !child.Attributes[DwarfAttribute.External].Flag)
                            {
                                names.Add(child.Name);
                            }
                        }
                        else if (child.Tag == DwarfTag.Inheritance)
                        {
                            baseClasses.Enqueue(GetType(child));
                        }
                        else if (child.Tag == DwarfTag.Member && string.IsNullOrEmpty(child.Name))
                        {
                            // We want to add unnamed unions
                            DwarfSymbol unionType = GetType(child);

                            if (unionType.Tag == DwarfTag.UnionType)
                            {
                                baseClasses.Enqueue(unionType);
                            }
                        }
                    }
                }
            }

            return names.ToArray();
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            DwarfSymbol type = GetType(typeId);

            if (type.Tag == DwarfTag.PointerType)
            {
                type = GetType(type);
            }

            Queue<Tuple<DwarfSymbol, int>> baseClasses = new Queue<Tuple<DwarfSymbol, int>>();

            baseClasses.Enqueue(Tuple.Create(type, 0));
            while (baseClasses.Count > 0)
            {
                Tuple<DwarfSymbol, int> typeAndOffset = baseClasses.Dequeue();
                int typeOffset = typeAndOffset.Item2;

                type = typeAndOffset.Item1;
                if (type.Children != null)
                {
                    foreach (DwarfSymbol child in type.Children)
                    {
                        if (child.Tag == DwarfTag.Member && child.Name == fieldName)
                        {
                            uint fieldTypeId = GetTypeId(GetType(child));
                            int offset = (int)child.GetConstantAttribute(DwarfAttribute.DataMemberLocation);

                            return Tuple.Create(fieldTypeId, typeOffset + offset);
                        }
                        else if (child.Tag == DwarfTag.Inheritance)
                        {
                            int offset = (int)child.GetConstantAttribute(DwarfAttribute.DataMemberLocation);

                            baseClasses.Enqueue(Tuple.Create(GetType(child), typeOffset + offset));
                        }
                        else if (child.Tag == DwarfTag.Member && string.IsNullOrEmpty(child.Name))
                        {
                            // We want to add unnamed unions
                            DwarfSymbol unionType = GetType(child);

                            if (unionType.Tag == DwarfTag.UnionType)
                            {
                                int offset = (int)child.GetConstantAttribute(DwarfAttribute.DataMemberLocation);

                                baseClasses.Enqueue(Tuple.Create(unionType, typeOffset + offset));
                            }
                        }
                    }
                }
            }

            throw new Exception("Field name not found");
        }

        /// <summary>
        /// Gets the type of the type basic.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public BasicType GetTypeBasicType(Module module, uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            switch (type.Tag)
            {
                case DwarfTag.BaseType:
                    switch (type.Name)
                    {
                        case "double":
                        case "float":
                        case "long double":
                            return BasicType.Float;
                        case "int":
                            return BasicType.Int;
                        case "unsigned int":
                            return BasicType.UInt;
                        case "long long unsigned int":
                            return BasicType.ULong;
                        case "wchar_t":
                            return BasicType.WChar;
                        case "char":
                            return BasicType.Char;
                    }
                    throw new NotImplementedException();
                default:
                    return BasicType.NoType;
            }
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            if (type.Tag == DwarfTag.PointerType)
            {
                return GetTypeName(module, GetTypeId(GetType(type))) + "*";
            }
            if (type.Tag == DwarfTag.ArrayType)
            {
                return GetTypeName(module, GetTypeId(GetType(type))) + "[]";
            }

            return type.FullName;
        }

        /// <summary>
        /// Gets the type pointer to type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypePointerToTypeId(Module module, uint typeId)
        {
            // TODO: Don't do linear search
            DwarfSymbol type = GetType(typeId);

            foreach (DwarfCompilationUnit compilationUnit in compilationUnits)
            {
                foreach (DwarfSymbol symbol in compilationUnit.Symbols)
                {
                    if (symbol.Tag == DwarfTag.PointerType && GetType(symbol) == type)
                    {
                        return GetTypeId(symbol);
                    }
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(Module module, uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            if (!type.Attributes.ContainsKey(DwarfAttribute.ByteSize))
            {
                if (type.Tag == DwarfTag.SubroutineType)
                {
                    return module.Process.GetPointerSize();
                }
                else if (type.Tag == DwarfTag.ArrayType)
                {
                    DwarfSymbol subrangetype = type.Children.FirstOrDefault();

                    if (subrangetype.Tag == DwarfTag.SubrangeType)
                    {
                        return (uint)(subrangetype.GetConstantAttribute(DwarfAttribute.UpperBound) + 1) * GetTypeSize(module, GetTypeId(GetType(type)));
                    }
                }

                throw new NotImplementedException();
            }

            return (uint)type.Attributes[DwarfAttribute.ByteSize].Constant;
        }

        /// <summary>
        /// Gets the symbol tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public SymTag GetTypeTag(Module module, uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            switch (type.Tag)
            {
                case DwarfTag.PointerType:
                    return SymTag.PointerType;
                case DwarfTag.SubroutineType:
                    return SymTag.FunctionType;
                case DwarfTag.StructureType:
                case DwarfTag.ClassType:
                    return SymTag.UDT;
                case DwarfTag.BaseType:
                    return SymTag.BaseType;
                case DwarfTag.EnumerationType:
                    return SymTag.Enum;
                case DwarfTag.ArrayType:
                    return SymTag.ArrayType;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the specified process address is function type public symbol.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="processAddress">The process address.</param>
        /// <param name="address">The address.</param>
        /// <returns>
        ///   <c>true</c> if the specified process address is function type public symbol; otherwise, <c>false</c>.
        /// </returns>
        public bool IsFunctionAddressPublicSymbol(Process process, ulong processAddress, uint address)
        {
            // TODO:
            return false;
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
        /// Gets the symbol name by address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="distance">The distance within the module.</param>
        public Tuple<string, ulong> GetSymbolNameByAddress(Process process, ulong address, uint distance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(Module module, uint typeId, string className)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(Module module, uint typeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type element type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(Module module, uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            return GetTypeId(GetType(type));
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(Module module, string typeName)
        {
            foreach (DwarfCompilationUnit compilationUnit in compilationUnits)
            {
                foreach (DwarfSymbol symbol in compilationUnit.Symbols)
                {
                    if (symbol.FullName == typeName)
                    {
                        return GetTypeId(symbol);
                    }
                }
            }

            throw new Exception("Type name not found");
        }

        /// <summary>
        /// Object location type
        /// </summary>
        private enum LocationType
        {
            /// <summary>
            /// Location couldn't be resolved.
            /// </summary>
            Invalid,

            /// <summary>
            /// Value is inside the register.
            /// </summary>
            InRegister,

            /// <summary>
            /// Value is at address written in register summed with offset.
            /// </summary>
            RegisterRelative,

            /// <summary>
            /// Value is at absolute address.
            /// </summary>
            AbsoluteAddress,
        }

        /// <summary>
        /// Resolved object location.
        /// </summary>
        private struct Location
        {
            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            public LocationType Type { get; set; }

            /// <summary>
            /// Gets or sets the absolute address.
            /// </summary>
            public ulong Address { get; set; }

            /// <summary>
            /// Gets or sets the register.
            /// </summary>
            public int Register { get; set; }

            /// <summary>
            /// Gets or sets the offset.
            /// </summary>
            public int Offset { get; set; }

            /// <summary>
            /// Gets the invalid object location.
            /// </summary>
            public static Location Invalid
            {
                get
                {
                    return new Location
                    {
                        Type = LocationType.Invalid,
                    };
                }
            }

            /// <summary>
            /// Gets the canonical frame address locaiton.
            /// </summary>
            public static Location CanonicalFrameAddress
            {
                get
                {
                    return new Location
                    {
                        Type = LocationType.RegisterRelative,
                        Register = -1,
                    };
                }
            }

            /// <summary>
            /// Makes absolute address location.
            /// </summary>
            /// <param name="address">The address.</param>
            public static Location Absolute(ulong address)
            {
                return new Location
                {
                    Type = LocationType.AbsoluteAddress,
                    Address = address,
                };
            }

            /// <summary>
            /// Makes in register location.
            /// </summary>
            /// <param name="register">The register.</param>
            public static Location InRegister(int register)
            {
                return new Location
                {
                    Type = LocationType.InRegister,
                    Register = register,
                };
            }

            /// <summary>
            /// Makes register relative location.
            /// </summary>
            /// <param name="register">The register.</param>
            /// <param name="offset">The offset.</param>
            public static Location RegisterRelative(int register, int offset = 0)
            {
                return new Location
                {
                    Type = LocationType.RegisterRelative,
                    Register = register,
                    Offset = offset,
                };
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                switch (Type)
                {
                    case LocationType.Invalid:
                        return "Invalid";
                    case LocationType.AbsoluteAddress:
                        return $"0x{Address:X}";
                    case LocationType.RegisterRelative:
                        return $"*(reg{Register}) + {Offset}";
                    case LocationType.InRegister:
                        return $"reg{Register}";
                }

                return "Unknown";
            }
        }

        /// <summary>
        /// Decodes location from the specified attribute value.
        /// </summary>
        /// <param name="value">The location attribute value.</param>
        /// <param name="frameBase">The frame base location.</param>
        private static Location DecodeLocationStatic(DwarfAttributeValue value, Location? frameBase = null)
        {
            if (value.Type == DwarfAttributeValueType.Constant)
            {
                return Location.Absolute(value.Constant);
            }

            if (value.Type != DwarfAttributeValueType.ExpressionLocation && value.Type != DwarfAttributeValueType.Block)
            {
                return Location.Invalid;
            }

            using (DwarfMemoryReader reader = new DwarfMemoryReader(value.ExpressionLocation))
            {
                Stack<Location> stack = new Stack<Location>();

                // TODO:
                //if (at == DW_AT_data_member_location)
                //    stack[stackDepth++] = mkAbs(0);

                while (!reader.IsEnd)
                {
                    DwarfOperation operation = (DwarfOperation)reader.ReadByte();

                    if (operation == DwarfOperation.None)
                    {
                        break;
                    }

                    switch (operation)
                    {
                        case DwarfOperation.reg0:
                        case DwarfOperation.reg1:
                        case DwarfOperation.reg2:
                        case DwarfOperation.reg3:
                        case DwarfOperation.reg4:
                        case DwarfOperation.reg5:
                        case DwarfOperation.reg6:
                        case DwarfOperation.reg7:
                        case DwarfOperation.reg8:
                        case DwarfOperation.reg9:
                        case DwarfOperation.reg10:
                        case DwarfOperation.reg11:
                        case DwarfOperation.reg12:
                        case DwarfOperation.reg13:
                        case DwarfOperation.reg14:
                        case DwarfOperation.reg15:
                        case DwarfOperation.reg16:
                        case DwarfOperation.reg17:
                        case DwarfOperation.reg18:
                        case DwarfOperation.reg19:
                        case DwarfOperation.reg20:
                        case DwarfOperation.reg21:
                        case DwarfOperation.reg22:
                        case DwarfOperation.reg23:
                        case DwarfOperation.reg24:
                        case DwarfOperation.reg25:
                        case DwarfOperation.reg26:
                        case DwarfOperation.reg27:
                        case DwarfOperation.reg28:
                        case DwarfOperation.reg29:
                        case DwarfOperation.reg30:
                        case DwarfOperation.reg31:
                            stack.Push(Location.InRegister(operation - DwarfOperation.reg0));
                            break;
                        case DwarfOperation.const1u:
                            stack.Push(Location.Absolute(reader.ReadByte()));
                            break;
                        case DwarfOperation.const1s:
                            stack.Push(Location.Absolute((ulong)(sbyte)reader.ReadByte()));
                            break;
                        case DwarfOperation.const2u:
                            stack.Push(Location.Absolute(reader.ReadUshort()));
                            break;
                        case DwarfOperation.const2s:
                            stack.Push(Location.Absolute((ulong)(short)reader.ReadUshort()));
                            break;
                        case DwarfOperation.const4u:
                            stack.Push(Location.Absolute(reader.ReadUint()));
                            break;
                        case DwarfOperation.const4s:
                            stack.Push(Location.Absolute((ulong)(int)reader.ReadUint()));
                            break;
                        case DwarfOperation.constu:
                            stack.Push(Location.Absolute(reader.LEB128()));
                            break;
                        case DwarfOperation.consts:
                            stack.Push(Location.Absolute(reader.SLEB128()));
                            break;
                        case DwarfOperation.plus_uconst:
                            if (stack.Peek().Type == LocationType.InRegister)
                            {
                                return Location.Invalid;
                            }
                            else
                            {
                                Location location = stack.Pop();
                                location.Offset += (int)reader.LEB128();
                                stack.Push(location);
                            }
                            break;
                        case DwarfOperation.addr:
                            stack.Push(Location.Absolute(reader.ReadUint()));
                            break;
                        case DwarfOperation.call_frame_cfa:
                            stack.Push(Location.CanonicalFrameAddress);
                            break;
                        case DwarfOperation.FrameBaseRegister:
                            if (!frameBase.HasValue)
                            {
                                return Location.Invalid;
                            }
                            if (frameBase.Value.Type == LocationType.InRegister)
                            {
                                stack.Push(Location.RegisterRelative(frameBase.Value.Register, (int)reader.SLEB128()));
                            }
                            else if (frameBase.Value.Type == LocationType.RegisterRelative)
                            {
                                stack.Push(Location.RegisterRelative(frameBase.Value.Register, frameBase.Value.Offset + (int)reader.SLEB128()));
                            }
                            else
                            {
                                return Location.Invalid;
                            }
                            break;
                        default:
                            throw new Exception($"Unsupported DwarfOperation: {operation}");
                    }
                }
                while (stack.Count > 1)
                {
                    stack.Pop();
                }
                return stack.Pop();
            }
        }

        /// <summary>
        /// Decodes location from the specified attribute value. Also applies code segment offset for absolute address.
        /// </summary>
        /// <param name="value">The location attribute value.</param>
        /// <param name="frameBase">The frame base location.</param>
        private Location DecodeLocation(DwarfAttributeValue value, Location? frameBase = null)
        {
            Location location = DecodeLocationStatic(value, frameBase);

            if (location.Type == LocationType.AbsoluteAddress && location.Address > codeSegmentOffset)
            {
                location.Address -= codeSegmentOffset;
            }
            return location;
        }

        /// <summary>
        /// Resolves the frame base location.
        /// </summary>
        /// <param name="frameBaseAttribute">The frame base attribute value.</param>
        private Location ResolveFrameBaseLocation(DwarfAttributeValue frameBaseAttribute)
        {
            Location frameBase = DecodeLocation(frameBaseAttribute);

            if (frameBase.Type == LocationType.AbsoluteAddress)
            {
                // TODO:
                //if (frameBase.is_abs()) // pointer into location list in .debug_loc? assume CFA
                //    frameBase = findBestFBLoc(img, cu, frameBase.off);
                throw new Exception("Unsupported absolute address for frame base");
            }

            return frameBase;
        }

        /// <summary>
        /// Processes the canonical frame address instructions.
        /// </summary>
        /// <param name="entry">The common information entry.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="currentAddress">The current address.</param>
        /// <param name="cfaLocation">The canonical frame address location.</param>
        /// <param name="stopBeforeRestore">if set to <c>true</c> stops before restore instruction.</param>
        private void ProcessCanonicalFrameAddressInstructions(DwarfCommonInformationEntry entry, byte[] instructions, ref ulong currentAddress, ref Location cfaLocation, bool stopBeforeRestore = false)
        {
            using (DwarfMemoryReader data = new DwarfMemoryReader(instructions))
            {
                while (!data.IsEnd)
                {
                    byte instruction = data.ReadByte();
                    Location dummyLocation = new Location();

                    switch ((DwarfCanonicalFrameAddressInstruction)(instruction & 0xc0))
                    {
                        case DwarfCanonicalFrameAddressInstruction.advance_loc:
                            currentAddress += (instruction & 0x3fUL) * entry.CodeAlignmentFactor;
                            break;
                        case DwarfCanonicalFrameAddressInstruction.offset:
                            dummyLocation.Register = instruction & 0x3f;
                            dummyLocation.Offset = (int)(data.LEB128() * entry.DataAlignmentFactor);
                            break;
                        case DwarfCanonicalFrameAddressInstruction.restore:
                            if (stopBeforeRestore)
                            {
                                return;
                            }
                            dummyLocation.Register = instruction & 0x3f;
                            break;
                        case DwarfCanonicalFrameAddressInstruction.extended:
                            switch ((DwarfCanonicalFrameAddressInstruction)instruction)
                            {
                                case DwarfCanonicalFrameAddressInstruction.set_loc:
                                    currentAddress = data.ReadUlong(entry.AddressSize) + entry.SegmentSelectorSize;
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.advance_loc1:
                                    currentAddress = data.ReadByte();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.advance_loc2:
                                    currentAddress = data.ReadUshort();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.advance_loc4:
                                    currentAddress = data.ReadUint();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.def_cfa:
                                    cfaLocation.Register = (int)data.LEB128();
                                    cfaLocation.Offset = (int)data.LEB128();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.def_cfa_sf:
                                    cfaLocation.Register = (int)data.LEB128();
                                    cfaLocation.Offset = (int)data.SLEB128() * (int)entry.DataAlignmentFactor;
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.def_cfa_register:
                                    cfaLocation.Register = (int)data.LEB128();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.def_cfa_offset:
                                    cfaLocation.Offset = (int)data.LEB128();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.def_cfa_offset_sf:
                                    cfaLocation.Offset = (int)data.SLEB128() * (int)entry.DataAlignmentFactor;
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.def_cfa_expression:
                                    cfaLocation = DecodeLocation(new DwarfAttributeValue()
                                    {
                                        Type = DwarfAttributeValueType.ExpressionLocation,
                                        Value = data.ReadBlock(data.LEB128()),
                                    });
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.undefined:
                                case DwarfCanonicalFrameAddressInstruction.same_value:
                                    dummyLocation.Register = (int)data.LEB128();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.val_offset:
                                case DwarfCanonicalFrameAddressInstruction.offset_extended:
                                    dummyLocation.Register = (int)data.LEB128();
                                    dummyLocation.Offset = (int)data.LEB128() * (int)entry.DataAlignmentFactor;
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.val_offset_sf:
                                case DwarfCanonicalFrameAddressInstruction.offset_extended_sf:
                                    dummyLocation.Register = (int)data.LEB128();
                                    dummyLocation.Offset = (int)data.SLEB128() * (int)entry.DataAlignmentFactor;
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.register:
                                    dummyLocation.Register = (int)data.LEB128();
                                    dummyLocation.Register = (int)data.LEB128();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.expression:
                                case DwarfCanonicalFrameAddressInstruction.val_expression:
                                    dummyLocation.Register = (int)data.LEB128();
                                    dummyLocation = DecodeLocation(new DwarfAttributeValue()
                                    {
                                        Type = DwarfAttributeValueType.Block,
                                        Value = data.ReadBlock(data.LEB128()),
                                    });
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.restore_extended:
                                    if (stopBeforeRestore)
                                    {
                                        return;
                                    }
                                    dummyLocation.Register = (int)data.LEB128();
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.restore_state:
                                    if (stopBeforeRestore)
                                    {
                                        return;
                                    }
                                    break;
                                case DwarfCanonicalFrameAddressInstruction.remember_state:
                                case DwarfCanonicalFrameAddressInstruction.nop:
                                    break;
                                default:
                                    throw new Exception($"Unknown DwarfCanonicalFrameAddressInstruction: {(DwarfCanonicalFrameAddressInstruction)instruction}");
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the canonical frame address location.
        /// </summary>
        /// <param name="startAddressValue">The function start address value.</param>
        /// <param name="endAddressValue">The function end address value.</param>
        private Location ResolveCanonicalFrameAddress(DwarfAttributeValue startAddressValue, DwarfAttributeValue endAddressValue)
        {
            ulong startAddress = startAddressValue.Address + codeSegmentOffset;
            ulong endAddress = endAddressValue.Type == DwarfAttributeValueType.Address ? endAddressValue.Address + codeSegmentOffset : startAddress + endAddressValue.Constant;
            Location ebp = Location.RegisterRelative(is64bit ? 6 : 5, is64bit ? 16 : 8);

            // TODO: Don't do linear search
            foreach (DwarfCommonInformationEntry entry in commonInformationEntries)
            {
                foreach (DwarfFrameDescriptionEntry description in entry.FrameDescriptionEntries)
                {
                    if (description.InitialLocation <= startAddress && description.InitialLocation + description.AddressRange >= endAddress)
                    {
                        Location result = Location.CanonicalFrameAddress;
                        ulong location = startAddress;

                        ProcessCanonicalFrameAddressInstructions(description.CommonInformationEntry, description.CommonInformationEntry.InitialInstructions, ref location, ref result);
                        ProcessCanonicalFrameAddressInstructions(description.CommonInformationEntry, description.Instructions, ref location, ref result, stopBeforeRestore: true);
                        return result;
                    }
                }
            }

            return ebp;
        }

        /// <summary>
        /// Resolves the symbol address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="frameContext">The frame context.</param>
        private ulong ResolveAddress(Process process, DwarfSymbol function, DwarfSymbol symbol, ThreadContext frameContext)
        {
            Location frameBase = ResolveFrameBaseLocation(function.Attributes[DwarfAttribute.FrameBase]);
            Location canonicalFrameAddress = ResolveCanonicalFrameAddress(function.Attributes[DwarfAttribute.LowPc], function.Attributes[DwarfAttribute.HighPc]);
            Location location = DecodeLocation(symbol.Attributes[DwarfAttribute.Location], frameBase);

            if (location.Type == LocationType.AbsoluteAddress)
            {
                return location.Address;
            }
            else if (location.Type == LocationType.RegisterRelative)
            {
                if (location.Register == Location.CanonicalFrameAddress.Register)
                {
                    location.Register = canonicalFrameAddress.Register;
                    location.Offset += canonicalFrameAddress.Offset;
                }

                ulong address;

                if (is64bit)
                {
                    switch (location.Register)
                    {
                        case 6: // RBP
                            address = frameContext.FramePointer;
                            break;
                        case 7: // RSP
                            address = frameContext.StackPointer;
                            break;
                        default:
                            throw new Exception($"Unsupported register: {location.Register}");
                    }
                }
                else
                {
                    switch (location.Register)
                    {
                        case 4: // ESP
                            address = frameContext.StackPointer;
                            break;
                        case 5: // EBP
                            address = frameContext.FramePointer;
                            break;
                        case 8: // EIP
                            address = frameContext.InstructionPointer;
                            break;
                        default:
                            throw new Exception($"Unsupported register: {location.Register}");
                    }
                }

                return address + (ulong)location.Offset;
            }

            throw new Exception("Unsupported location");
        }

        /// <summary>
        /// Finds the function at the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="displacement">The displacement.</param>
        private DwarfSymbol FindFunction(ulong address, out ulong displacement)
        {
            // TODO: Not linear search
            ulong minDistance = ulong.MaxValue;
            DwarfSymbol bestMatch = null;

            foreach (DwarfCompilationUnit compilationUnit in compilationUnits)
            {
                foreach (DwarfSymbol symbol in compilationUnit.Symbols)
                {
                    if (symbol.Tag == DwarfTag.Subprogram)
                    {
                        DwarfAttributeValue addressValue;

                        if (symbol.Attributes.TryGetValue(DwarfAttribute.LowPc, out addressValue))
                        {
                            ulong functionAddress = addressValue.Address;

                            if (functionAddress < address)
                            {
                                ulong distance = address - functionAddress;

                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    bestMatch = symbol;
                                }
                            }
                        }
                    }
                }
            }

            if (bestMatch == null)
            {
                displacement = address;
                return null;
            }

            displacement = minDistance;
            return bestMatch;
        }

        /// <summary>
        /// Finds the global variable.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        private DwarfSymbol FindGlobalVariable(string globalVariableName)
        {
            // TODO: Not linear search
            foreach (DwarfCompilationUnit compilationUnit in compilationUnits)
            {
                foreach (DwarfSymbol symbol in compilationUnit.Symbols)
                {
                    if (symbol.Tag == DwarfTag.Variable && symbol.FullName == globalVariableName)
                    {
                        Location location = DecodeLocation(symbol.Attributes[DwarfAttribute.Location]);

                        if (location.Type == LocationType.AbsoluteAddress)
                        {
                            return symbol;
                        }
                    }
                    else if (symbol.Tag == DwarfTag.Member && symbol.Attributes.ContainsKey(DwarfAttribute.External)
                        && symbol.Attributes[DwarfAttribute.External].Flag && symbol.FullName == globalVariableName)
                    {
                        return symbol;
                    }
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type ID.
        /// </summary>
        /// <param name="type">The type.</param>
        private uint GetTypeId(DwarfSymbol type)
        {
            uint typeId;

            if (typeOffsetToTypeId.TryGetValue(type.Offset, out typeId))
            {
                return typeId;
            }

            lock (typeOffsetToTypeId)
            {
                typeId = nextTypeId++;
                typeIdToType.TryAdd(typeId, type);
                typeOffsetToTypeId.TryAdd(type.Offset, typeId);
                return typeId;
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="typeId">The type ID.</param>
        private DwarfSymbol GetType(uint typeId)
        {
            DwarfSymbol type;

            typeIdToType.TryGetValue(typeId, out type);
            return type;
        }

        /// <summary>
        /// Gets the type associated with the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        private DwarfSymbol GetType(DwarfSymbol symbol)
        {
            DwarfSymbol type = symbol.Attributes[DwarfAttribute.Type].Reference;

            while (type.Tag == DwarfTag.Typedef || type.Tag == DwarfTag.ConstType)
            {
                type = type.Attributes[DwarfAttribute.Type].Reference;
            }
            return type;
        }
    }
}
