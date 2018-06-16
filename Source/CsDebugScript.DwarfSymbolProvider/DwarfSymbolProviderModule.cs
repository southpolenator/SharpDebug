using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// DWARF symbol provider for specific module.
    /// </summary>
    /// <seealso cref="CsDebugScript.Engine.ISymbolProviderModule" />
    internal class DwarfSymbolProviderModule : ISymbolProviderModule
    {
        /// <summary>
        /// The enumerator of all symbols in all compilation units.
        /// </summary>
        private IEnumerator<DwarfSymbol> symbolEnumerator;

        /// <summary>
        /// Flag indicating if all symbols were enumerated.
        /// </summary>
        private volatile bool allSymbolsEnumerated = false;

        /// <summary>
        /// Mapping from type name to type.
        /// </summary>
        private ConcurrentDictionary<string, DwarfSymbol> typeNameToType = new ConcurrentDictionary<string, DwarfSymbol>();

        /// <summary>
        /// Mapping from type offset to its pointer type.
        /// </summary>
        private ConcurrentDictionary<int, DwarfSymbol> typeOffsetToPointerType = new ConcurrentDictionary<int, DwarfSymbol>();

        /// <summary>
        /// Mapping from global variable name to global variable symbol.
        /// </summary>
        private ConcurrentDictionary<string, DwarfSymbol> globalVariables = new ConcurrentDictionary<string, DwarfSymbol>();

        /// <summary>
        /// The list of function symbols.
        /// </summary>
        private List<DwarfSymbol> functionsCache = new List<DwarfSymbol>();

        /// <summary>
        /// The list of function addresses.
        /// </summary>
        private List<ulong> functionAddressesCache;

        /// <summary>
        /// The list of line information.
        /// </summary>
        private SimpleCache<List<DwarfLineInformation>> lineInformationCache;

        /// <summary>
        /// The list of line information addresses.
        /// </summary>
        private SimpleCache<List<uint>> lineInformationAddressesCache;

        /// <summary>
        /// The list of frame description entries
        /// </summary>
        private SimpleCache<List<DwarfFrameDescriptionEntry>> frameDescriptionEntries;

        /// <summary>
        /// The frame description entry finder
        /// </summary>
        private SimpleCache<MemoryRegionFinder> frameDescriptionEntryFinder;

        /// <summary>
        /// The list of frame description entries from exception handling frames stream.
        /// </summary>
        private SimpleCache<List<DwarfFrameDescriptionEntry>> frameDescriptionEntriesFromExceptionHandlingStream;

        /// <summary>
        /// The frame description entry finder from exception handling frames stream.
        /// </summary>
        private SimpleCache<MemoryRegionFinder> frameDescriptionEntryFinderFromExceptionHandlingStream;

        /// <summary>
        /// Dictionary of type to virtual table size.
        /// </summary>
        private DictionaryCache<DwarfSymbol, int> virtualTableSizes;

        /// <summary>
        /// The code segment offset (read from the image)
        /// </summary>
        private ulong codeSegmentOffset;

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
        /// The next unnamed type id
        /// </summary>
        private int nextUnnamedTypeId = 0;

        /// <summary>
        /// The list of public symbols sorted by Address.
        /// </summary>
        private List<PublicSymbol> publicSymbols;

        /// <summary>
        /// The list of public symbols addresses sorted.
        /// </summary>
        private List<ulong> publicSymbolsAddresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="DwarfSymbolProviderModule" /> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="compilationUnits">The compilation units.</param>
        /// <param name="programs">The line number programs.</param>
        /// <param name="commonInformationEntries">The common information entries.</param>
        /// <param name="publicSymbols">The public symbols.</param>
        /// <param name="codeSegmentOffset">The code segment offset.</param>
        /// <param name="is64bit">if set to <c>true</c> image is 64 bit.</param>
        public DwarfSymbolProviderModule(Module module, DwarfCompilationUnit[] compilationUnits, DwarfLineNumberProgram[] programs, DwarfCommonInformationEntry[] commonInformationEntries, IReadOnlyList<PublicSymbol> publicSymbols, ulong codeSegmentOffset, bool is64bit)
        {
            Module = module;
            Is64bit = is64bit;
            symbolEnumerator = compilationUnits.SelectMany(cu => cu.Symbols).GetEnumerator();
            this.publicSymbols = publicSymbols.OrderBy(s => s.Address).ToList();
            publicSymbolsAddresses = this.publicSymbols.Select(s => s.Address).ToList();
            lineInformationCache = SimpleCache.Create(() =>
            {
                List<DwarfLineInformation> result = programs.SelectMany(p => p.Files).SelectMany(f => f.Lines).ToList();

                result.Sort((l1, l2) => (int)l1.Address - (int)l2.Address);
                return result;
            });
            lineInformationAddressesCache = SimpleCache.Create(() => lineInformationCache.Value.Select(l => l.Address).ToList());
            frameDescriptionEntries = SimpleCache.Create(() =>
            {
                List<DwarfFrameDescriptionEntry> result = commonInformationEntries
                    .Where(e => !(e is DwarfExceptionHandlingCommonInformationEntry))
                    .SelectMany(e => e.FrameDescriptionEntries)
                    .ToList();

                result.Sort((f1, f2) => (int)f1.InitialLocation - (int)f2.InitialLocation);
                return result;
            });
            frameDescriptionEntryFinder = SimpleCache.Create(() => new MemoryRegionFinder(frameDescriptionEntries.Value.Select(f => new MemoryRegion { BaseAddress = f.InitialLocation, RegionSize = f.AddressRange }).ToList()));
            frameDescriptionEntriesFromExceptionHandlingStream = SimpleCache.Create(() =>
            {
                List<DwarfFrameDescriptionEntry> result = commonInformationEntries
                    .Where(e => e is DwarfExceptionHandlingCommonInformationEntry)
                    .SelectMany(e => e.FrameDescriptionEntries)
                    .ToList();

                result.Sort((f1, f2) => (int)f1.InitialLocation - (int)f2.InitialLocation);
                return result;
            });
            frameDescriptionEntryFinderFromExceptionHandlingStream = SimpleCache.Create(() => new MemoryRegionFinder(frameDescriptionEntriesFromExceptionHandlingStream.Value.Select(f => new MemoryRegion { BaseAddress = f.InitialLocation, RegionSize = f.AddressRange }).ToList()));
            virtualTableSizes = new DictionaryCache<DwarfSymbol, int>(GetVirtualTableSize);
            this.codeSegmentOffset = codeSegmentOffset;
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Flag indicating if image is 64 bit.
        /// </summary>
        internal bool Is64bit { get; private set; }

        /// <summary>
        /// Gets the name of the enumeration value.
        /// </summary>
        /// <param name="enumTypeId">The enumeration type identifier.</param>
        /// <param name="enumValue">The enumeration value.</param>
        public string GetEnumName(uint enumTypeId, ulong enumValue)
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
        /// <param name="relativeAddress">The relative address.</param>
        /// <param name="arguments">if set to <c>true</c> only arguments will be returned.</param>
        public VariableCollection GetFrameLocals(StackFrame frame, uint relativeAddress, bool arguments)
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
                            CodeType codeType = Module.TypesById[parameterTypeId];
                            ulong address = ResolveAddress(Module.Process, function, parameter, frame.FrameContext);
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
                                CodeType codeType = Module.TypesById[parameterTypeId];
                                ulong address = ResolveAddress(Module.Process, function, data, frame.FrameContext);
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
                        }
                    }
                }
            }

            return new VariableCollection(variables.ToArray());
        }

        /// <summary>
        /// Gets the name of the function for the specified stack frame.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetFunctionNameAndDisplacement(uint address, out string functionName, out ulong displacement)
        {
            DwarfSymbol function = FindFunction(address, out displacement);

            functionName = function?.FullName;
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(string globalVariableName)
        {
            DwarfSymbol globalVariable = FindGlobalVariable(globalVariableName);
            Location location = DecodeLocation(globalVariable.Attributes[DwarfAttribute.Location]);

            return location.Address + Module.Address;
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(string globalVariableName)
        {
            DwarfSymbol globalVariable = FindGlobalVariable(globalVariableName);

            return GetTypeId(GetType(globalVariable));
        }

        /// <summary>
        /// Gets the runtime code type and offset to original code type.
        /// </summary>
        /// <param name="vtableAddress">The vtable address within the module.</param>
        public Tuple<CodeType, int> GetRuntimeCodeTypeAndOffset(uint vtableAddress)
        {
            // Try to find original code type that is using this vtable by inspecting first vtable function entry
            CodeType originalCodeType = null;
            bool originalCodeTypeNoVirtuality = true;

            try
            {
                // Try to find what is the first function at virtual table address
                MemoryBuffer memoryBuffer = Debugger.ReadMemory(Module.Process, vtableAddress + Module.Address, Module.PointerSize);
                ulong firstFunctionAddress = UserType.ReadPointer(memoryBuffer, 0, (int)Module.Process.GetPointerSize());
                ulong displacement;
                DwarfSymbol firstFunction = FindFunction(firstFunctionAddress - Module.Address, out displacement);

                if (displacement == 0)
                {
                    // If function symbol doesn't point to specification, resolve it
                    if (firstFunction.Attributes.ContainsKey(DwarfAttribute.Specification))
                    {
                        firstFunction = firstFunction.Attributes[DwarfAttribute.Specification].Reference;
                    }

                    // Resolve code type that is implicit argument to first function
                    DwarfSymbol implicitArgument = firstFunction.Attributes[DwarfAttribute.ObjectPointer].Reference;
                    DwarfSymbol abstractOrigin = implicitArgument.Attributes.ContainsKey(DwarfAttribute.AbstractOrigin) ? implicitArgument.Attributes[DwarfAttribute.AbstractOrigin].Reference : implicitArgument;
                    DwarfSymbol type = GetType(abstractOrigin);

                    if (type.Tag == DwarfTag.PointerType)
                    {
                        type = GetType(type);
                    }

                    // Find class that has defined found function for the first time.
                    if (firstFunction.Attributes.ContainsKey(DwarfAttribute.VtableElemLocation))
                    {
                        Location location = DecodeLocationStatic(firstFunction.Attributes[DwarfAttribute.VtableElemLocation]);
                        int desiredVirtualFunction;

                        originalCodeTypeNoVirtuality = false;
                        if (location.Type == LocationType.AbsoluteAddress)
                        {
                            desiredVirtualFunction = (int)location.Address;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        while (desiredVirtualFunction > 0)
                        {
                            // Search through base classes until you find base class that is using exact virtual table entry that we need.
                            var baseClasses = type.Children.Where(c => c.Tag == DwarfTag.Inheritance).Select(c => Tuple.Create(GetType(c), DecodeDataMemberLocation(c))).OrderBy(t => t.Item2).ToArray();
                            int totalVirtualFunctions = 0;
                            bool found = false;

                            for (int i = 0; i < baseClasses.Length; i++)
                            {
                                int virtualTableSize = virtualTableSizes[baseClasses[i].Item1];

                                if (virtualTableSize + totalVirtualFunctions > desiredVirtualFunction)
                                {
                                    type = baseClasses[i].Item1;
                                    desiredVirtualFunction -= totalVirtualFunctions;
                                    found = true;
                                    break;
                                }
                                totalVirtualFunctions += virtualTableSize;
                            }

                            if (!found)
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                    originalCodeType = Module.TypesById[GetTypeId(type)];
                }
            }
            catch
            {
                // We have failed to get original code type
            }

            // Try to locate address in public symbols and see if it is virtual table
            PublicSymbol publicSymbol;
            int publicSymbolIndex = publicSymbolsAddresses.BinarySearch(vtableAddress);

            if (publicSymbolIndex < 0)
            {
                publicSymbolIndex = ~publicSymbolIndex;
            }
            if (publicSymbolIndex >= publicSymbolsAddresses.Count)
            {
                publicSymbolIndex = publicSymbolsAddresses.Count - 1;
            }
            if (publicSymbolsAddresses[publicSymbolIndex] > vtableAddress && publicSymbolIndex > 0)
            {
                publicSymbolIndex--;
            }

            publicSymbol = publicSymbols[publicSymbolIndex];
            string publicSymbolName = null;
            if (publicSymbol.DemangledName.StartsWith("vtable for "))
            {
                publicSymbolName = publicSymbol.DemangledName.Substring(11);
            }
            else if (publicSymbol.DemangledName.StartsWith("VTT for "))
            {
                publicSymbolName = publicSymbol.DemangledName.Substring(8);
            }

            if (!string.IsNullOrEmpty(publicSymbolName))
            {
                DwarfSymbol symbol;

                if (typeNameToType.TryGetValue(publicSymbolName, out symbol))
                {
                    CodeType codeType = Module.TypesById[GetTypeId(symbol)];

                    // Using original code type has priority if first function had virtuality set (not defaulted to 0).
                    if (originalCodeType != null && !originalCodeTypeNoVirtuality)
                    {
                        // Maybe runtime code type doesn't inherit original code type - we found original code type incorrectly,
                        // so if getting base class throws, we can fall back to reading offset from the vtable.
                        try
                        {
                            if (codeType != originalCodeType)
                            {
                                return codeType.BaseClasses[originalCodeType.Name];
                            }

                            return Tuple.Create(codeType, 0);
                        }
                        catch
                        {
                        }
                    }

                    // If we failed to get original code type, try looking at class start offset
                    // Layout of vtable:
                    // -2: object start offset
                    // -1: RTTI definition
                    // 0: first function address
                    MemoryBuffer memoryBuffer = Debugger.ReadMemory(Module.Process, vtableAddress + Module.Address - 2 * Module.PointerSize, Module.PointerSize);
                    long offset;

                    if (Module.PointerSize == 8)
                    {
                        offset = UserType.ReadLong(memoryBuffer, 0);
                    }
                    else if (Module.PointerSize == 4)
                    {
                        offset = UserType.ReadInt(memoryBuffer, 0);
                    }
                    else
                    {
                        // Unexpected pointer size
                        throw new NotImplementedException();
                    }

                    if (offset > 0)
                    {
                        // If we haven't tested original code type because its first function didn't have virtuality set, we will now do the test.
                        if (originalCodeType != null && originalCodeTypeNoVirtuality)
                        {
                            if (codeType != originalCodeType)
                            {
                                return codeType.BaseClasses[originalCodeType.Name];
                            }

                            return Tuple.Create(codeType, 0);
                        }

                        // Something went wrong. Either we are not on the begining of the vtable, or compiler generated vtable differently.
                        throw new NotImplementedException();
                    }

                    return Tuple.Create(codeType, (int)-offset);
                }
            }

            // We failed to find it in public symbols, just return found original code type
            return Tuple.Create(originalCodeType, 0);
        }

        /// <summary>
        /// Gets the virtual table size for the specified type.
        /// </summary>
        /// <param name="type">Symbol that represents type.</param>
        /// <returns>Virtual table size in number of entries.</returns>
        private int GetVirtualTableSize(DwarfSymbol type)
        {
            int childrenSum = 0;
            int maxFunctionEntry = -1;

            foreach (DwarfSymbol child in type.Children)
            {
                if (child.Tag == DwarfTag.Inheritance)
                {
                    childrenSum += GetVirtualTableSize(child);
                }
                else if (child.Tag == DwarfTag.Subprogram)
                {
                    DwarfVirtuality virtuality = (DwarfVirtuality)child.GetConstantAttribute(DwarfAttribute.Virtuality, (ulong)DwarfVirtuality.None);

                    if (virtuality != DwarfVirtuality.None)
                    {
                        maxFunctionEntry = Math.Max(maxFunctionEntry, 0);
                        if (child.Attributes.ContainsKey(DwarfAttribute.VtableElemLocation))
                        {
                            Location location = DecodeLocationStatic(child.Attributes[DwarfAttribute.VtableElemLocation]);

                            if (location.Type == LocationType.AbsoluteAddress)
                            {
                                maxFunctionEntry = Math.Max(maxFunctionEntry, (int)location.Address);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
            }

            return Math.Max(childrenSum, maxFunctionEntry + 1);
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
            int index = lineInformationAddressesCache.Value.BinarySearch(address);

            if (index < 0)
            {
                index = ~index;
            }
            if (index >= lineInformationCache.Value.Count)
            {
                index = lineInformationCache.Value.Count - 1;
            }
            if (lineInformationAddressesCache.Value[index] > address && index > 0)
            {
                index--;
            }

            if (lineInformationAddressesCache.Value[index] > address)
            {
                throw new KeyNotFoundException();
            }

            sourceFileName = lineInformationCache.Value[index].File.Path;
            sourceFileLine = lineInformationCache.Value[index].Line;
            displacement = address - lineInformationAddressesCache.Value[index];
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(uint typeId)
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
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(uint typeId, string fieldName)
        {
            DwarfSymbol type = GetType(typeId);

            if (type.Tag == DwarfTag.PointerType)
            {
                type = GetType(type);
            }

            var value = GetTypeAllFieldTypeAndOffset(type, fieldName);

            if (value != null)
            {
                return Tuple.Create(GetTypeId(value.Item1), value.Item2);
            }

            throw new Exception("Field name not found");
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="type">Type symbol.</param>
        /// <param name="fieldName">Name of the field.</param>
        private Tuple<DwarfSymbol, int> GetTypeAllFieldTypeAndOffset(DwarfSymbol type, string fieldName)
        {
            if (type.Children != null)
            {
                foreach (DwarfSymbol child in type.Children)
                {
                    if (child.Tag == DwarfTag.Member && child.Name == fieldName)
                    {
                        DwarfSymbol fieldType = GetType(child);
                        int offset = DecodeDataMemberLocation(child);

                        return Tuple.Create(fieldType, offset);
                    }
                    else if (child.Tag == DwarfTag.Inheritance)
                    {
                        var result = GetTypeAllFieldTypeAndOffset(GetType(child), fieldName);

                        if (result != null)
                        {
                            DwarfVirtuality virtuality = (DwarfVirtuality)child.GetConstantAttribute(DwarfAttribute.Virtuality, (ulong)DwarfVirtuality.None);

                            if (virtuality != DwarfVirtuality.None)
                            {
                                return Tuple.Create(GetType(child), int.MinValue);
                            }
                            else
                            {
                                int offset = DecodeDataMemberLocation(child);

                                return Tuple.Create(result.Item1, offset + result.Item2);
                            }
                        }
                    }
                    else if (child.Tag == DwarfTag.Member && string.IsNullOrEmpty(child.Name))
                    {
                        // We want to add unnamed unions
                        DwarfSymbol unionType = GetType(child);

                        if (unionType.Tag == DwarfTag.UnionType)
                        {
                            var result = GetTypeAllFieldTypeAndOffset(GetType(child), fieldName);

                            if (result != null)
                            {
                                int offset = DecodeDataMemberLocation(child);

                                return Tuple.Create(result.Item1, offset + result.Item2);
                            }
                        }
                    }
                }
            }

            return null;
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
            DwarfSymbol objectType = GetType(objectTypeId);
            DwarfSymbol virtualType = GetType(virtualTypeId);

            return GetVirtualClassBaseAddress(objectType, objectAddress, virtualType);
        }

        /// <summary>
        /// Gets the virtual base class start address.
        /// </summary>
        /// <param name="objectType">Object type symbol.</param>
        /// <param name="objectAddress">Object address.</param>
        /// <param name="virtualType">Virtual class type symbol.</param>
        /// <returns>Address of the object which code type is virtual class.</returns>
        private ulong GetVirtualClassBaseAddress(DwarfSymbol objectType, ulong objectAddress, DwarfSymbol virtualType)
        {
            if (objectType.Children != null)
            {
                foreach (DwarfSymbol child in objectType.Children)
                {
                    if (child.Tag == DwarfTag.Inheritance)
                    {
                        ulong newAddress = DecodeDataMemberLocation(child, objectAddress);
                        DwarfSymbol childType = GetType(child);

                        if (childType == virtualType)
                        {
                            return newAddress;
                        }

                        newAddress = GetVirtualClassBaseAddress(childType, newAddress, virtualType);
                        if (newAddress != 0)
                        {
                            return newAddress;
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the type of the type basic.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public BuiltinType GetTypeBuiltinType(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            return GetTypeBasicType(type);
        }

        /// <summary>
        /// Gets the basic type of the specified type.
        /// </summary>
        private BuiltinType GetTypeBasicType(DwarfSymbol type)
        {
            ulong size = type.GetConstantAttribute(DwarfAttribute.ByteSize);

            while (true)
            {
                switch (type.Tag)
                {
                    case DwarfTag.BaseType:
                        switch (type.Name)
                        {
                            case "double":
                            case "float":
                            case "long double":
                                switch (size)
                                {
                                    default:
                                    case 4:
                                        return BuiltinType.Float32;
                                    case 8:
                                        return BuiltinType.Float64;
                                    case 10:
                                    case 12:
                                    case 16:
                                        return BuiltinType.Float80;
                                }
                            case "short":
                            case "short int":
                            case "int":
                            case "long int":
                            case "long long int":
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
                            case "unsigned short":
                            case "short unsigned int":
                            case "unsigned int":
                            case "long unsigned int":
                            case "unsigned char":
                            case "long long unsigned int":
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
                            case "wchar_t":
                            case "signed char":
                            case "char":
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
                            case "bool":
                                return BuiltinType.Bool;
                            case "void":
                                return BuiltinType.Void;
                        }
                        throw new Exception($"Unknown BasicType name: {type.Name}");
                    case DwarfTag.EnumerationType:
                        type = GetType(type);
                        if (type == null)
                        {
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
                        }
                        continue;
                    default:
                        return BuiltinType.NoType;
                }
            }
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            return GetTypeName(type);
        }

        /// <summary>
        /// Gets the type pointer to type identifier.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypePointerToTypeId(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);
            DwarfSymbol pointerType;

            if (typeOffsetToPointerType.TryGetValue(type.Offset, out pointerType))
            {
                return GetTypeId(pointerType);
            }

            pointerType = ContinueSymbolSearch((symbol) =>
            {
                return symbol.Tag == DwarfTag.PointerType && GetType(symbol) == type;
            });

            if (pointerType != null)
            {
                return GetTypeId(pointerType);
            }

            throw new Exception("There is no pointer type to the specified type ID");
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            if (!type.Attributes.ContainsKey(DwarfAttribute.ByteSize))
            {
                if (type.Tag == DwarfTag.SubroutineType)
                {
                    return Module.PointerSize;
                }
                else if (type.Tag == DwarfTag.ArrayType)
                {
                    DwarfSymbol subrangetype = type.Children.FirstOrDefault();

                    if (subrangetype.Tag == DwarfTag.SubrangeType)
                    {
                        ulong count = subrangetype.GetConstantAttribute(DwarfAttribute.Count);

                        if (count == 0)
                        {
                            count = subrangetype.GetConstantAttribute(DwarfAttribute.UpperBound) + 1;
                        }
                        return (uint)count * GetTypeSize(GetTypeId(GetType(type)));
                    }
                }
                else
                {
                    // TODO: throw new NotImplementedException();
                    return 0;
                }
            }

            return (uint)type.Attributes[DwarfAttribute.ByteSize].Constant;
        }

        /// <summary>
        /// Gets the code type tag of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public CodeTypeTag GetTypeTag(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            switch (type.Tag)
            {
                case DwarfTag.ReferenceType:
                case DwarfTag.PointerType:
                    return CodeTypeTag.Pointer;
                case DwarfTag.SubroutineType:
                    return CodeTypeTag.Function;
                case DwarfTag.StructureType:
                    return CodeTypeTag.Structure;
                case DwarfTag.ClassType:
                    return CodeTypeTag.Class;
                case DwarfTag.UnionType:
                    return CodeTypeTag.Union;
                case DwarfTag.BaseType:
                    return CodeTypeTag.BuiltinType;
                case DwarfTag.EnumerationType:
                    return CodeTypeTag.Enum;
                case DwarfTag.ArrayType:
                    return CodeTypeTag.Array;
                default:
                    return CodeTypeTag.Unsupported;
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
        /// <param name="address">The address within the module.</param>
        public Tuple<string, ulong> GetSymbolNameByAddress(uint address)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(uint typeId, string className)
        {
            DwarfSymbol type = GetType(typeId);

            if (CodeType.TypeNameMatches(type.Name, className) || CodeType.TypeNameMatches(type.FullName, className))
            {
                return Tuple.Create(typeId, 0);
            }

            if (type.Children != null)
            {
                foreach (DwarfSymbol child in type.Children)
                {
                    if (child.Tag == DwarfTag.Inheritance)
                    {
                        DwarfSymbol baseClass = GetType(child);

                        if (baseClass.Name == className || baseClass.FullName == className)
                        {
                            DwarfVirtuality virtuality = (DwarfVirtuality)child.GetConstantAttribute(DwarfAttribute.Virtuality, (ulong)DwarfVirtuality.None);

                            if (virtuality != DwarfVirtuality.None)
                            {
                                return Tuple.Create(GetTypeId(baseClass), int.MinValue);
                            }
                            else
                            {
                                int offset = DecodeDataMemberLocation(child);

                                return Tuple.Create(GetTypeId(baseClass), offset);
                            }
                        }
                    }
                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type's direct base classes type and offset.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public Dictionary<string, Tuple<uint, int>> GetTypeDirectBaseClasses(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);
            Dictionary<string, Tuple<uint, int>> result = new Dictionary<string, Tuple<uint, int>>();

            if (type.Children != null)
            {
                foreach (DwarfSymbol child in type.Children)
                {
                    if (child.Tag == DwarfTag.Inheritance)
                    {
                        DwarfSymbol baseClass = GetType(child);

                        if (!string.IsNullOrEmpty(baseClass.Name))
                        {
                            DwarfVirtuality virtuality = (DwarfVirtuality)child.GetConstantAttribute(DwarfAttribute.Virtuality, (ulong)DwarfVirtuality.None);
                            int offset = virtuality == DwarfVirtuality.None ? DecodeDataMemberLocation(child) : int.MinValue;

                            result.Add(baseClass.FullName, Tuple.Create(GetTypeId(baseClass), offset));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(uint typeId)
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
                            if (!child.Attributes.ContainsKey(DwarfAttribute.Artifical) && (!child.Attributes.ContainsKey(DwarfAttribute.External) || !child.Attributes[DwarfAttribute.External].Flag))
                            {
                                names.Add(child.Name);
                            }
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
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(uint typeId, string fieldName)
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
                            int offset = DecodeDataMemberLocation(child);

                            return Tuple.Create(fieldTypeId, typeOffset + offset);
                        }
                        else if (child.Tag == DwarfTag.Member && string.IsNullOrEmpty(child.Name))
                        {
                            // We want to add unnamed unions
                            DwarfSymbol unionType = GetType(child);

                            if (unionType.Tag == DwarfTag.UnionType)
                            {
                                int offset = DecodeDataMemberLocation(child);

                                baseClasses.Enqueue(Tuple.Create(unionType, typeOffset + offset));
                            }
                        }
                    }
                }
            }

            throw new Exception("Field name not found");
        }

        /// <summary>
        /// Gets the type element type identifier.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);

            return GetTypeId(GetType(type));
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(string typeName)
        {
            if (typeName.StartsWith("const "))
            {
                typeName = typeName.Substring(6);
            }
            if (typeName.EndsWith(" const*"))
            {
                typeName = typeName.Substring(0, typeName.Length - 7) + "*";
            }
            if (typeName == "unsigned long" && Is64bit)
            {
                typeName = "long long unsigned int";
            }

            DwarfSymbol type;

            if (typeNameToType.TryGetValue(typeName, out type))
            {
                return GetTypeId(type);
            }

            type = ContinueSymbolSearch((symbol) =>
                {
                    return GetTypeName(symbol) == typeName;
                });

            if (type != null)
            {
                return GetTypeId(type);
            }

            throw new Exception("Type name not found");
        }

        /// <summary>
        /// Gets all available types from the module.
        /// </summary>
        /// <returns>Enumeration of type identifiers.</returns>
        public IEnumerable<uint> GetAllTypes()
        {
            if (!allSymbolsEnumerated)
            {
                ContinueSymbolSearch(s => false);
            }

            foreach (DwarfSymbol type in typeNameToType.Values)
            {
                if (type.Tag == DwarfTag.ClassType || type.Tag == DwarfTag.StructureType || type.Tag == DwarfTag.EnumerationType
                    || type.Tag == DwarfTag.InterfaceType || type.Tag == DwarfTag.UnionType)
                {
                    yield return GetTypeId(type);
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
            DwarfSymbol enumType = GetType(enumTypeId);

            foreach (DwarfSymbol child in enumType.Children)
            {
                if (child.Tag == DwarfTag.Enumerator)
                {
                    string svalue = child.GetConstantAttribute(DwarfAttribute.ConstValue).ToString();

                    yield return Tuple.Create(child.Name, svalue);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified type has virtual table of functions.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        public bool HasTypeVTable(uint typeId)
        {
            DwarfSymbol type = GetType(typeId);
            Queue<DwarfSymbol> baseClasses = new Queue<DwarfSymbol>();

            baseClasses.Enqueue(type);
            while (baseClasses.Count > 0)
            {
                type = baseClasses.Dequeue();
                if (type.Children != null)
                {
                    foreach (DwarfSymbol symbol in type.Children)
                    {
                        if (symbol.Tag == DwarfTag.Inheritance)
                        {
                            baseClasses.Enqueue(GetType(symbol));
                        }
                        else if (symbol.Tag == DwarfTag.Subprogram && symbol.Attributes.ContainsKey(DwarfAttribute.Virtuality))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the global scope.
        /// </summary>
        public uint GetGlobalScope()
        {
            // Symbol that has all info about globals (global variables)
            // TODO: throw new NotImplementedException();
            return 0;
        }

        /// <summary>
        /// Continues the symbol search with populating caches.
        /// </summary>
        /// <param name="predicate">The found symbol predicate.</param>
        /// <returns>Symbol that satisfied the predicate.</returns>
        private DwarfSymbol ContinueSymbolSearch(Func<DwarfSymbol, bool> predicate)
        {
            if (!allSymbolsEnumerated)
            {
                lock (symbolEnumerator)
                {
                    if (!allSymbolsEnumerated)
                    {
                        while (symbolEnumerator.MoveNext())
                        {
                            DwarfSymbol symbol = symbolEnumerator.Current;

                            // If it is type, add it to collection
                            string typeName = GetTypeName(symbol);

                            if (!string.IsNullOrEmpty(typeName))
                            {
                                typeNameToType.TryAdd(typeName, symbol);
                                typeNameToType.TryAdd(CleanSymbolNameNumbers(typeName), symbol);
                                typeNameToType.TryAdd(GetTypeNameNicePrint(symbol, true), symbol);
                                typeNameToType.TryAdd(GetTypeNameNicePrint(symbol, false), symbol);
                            }

                            // If it is pointer type, add it to collection
                            if (symbol.Tag == DwarfTag.PointerType)
                            {
                                DwarfSymbol type = GetType(symbol);

                                if (type != null)
                                {
                                    typeOffsetToPointerType.TryAdd(type.Offset, symbol);
                                }
                            }

                            // If it is function, add it to cache
                            if (symbol.Tag == DwarfTag.Subprogram)
                            {
                                DwarfAttributeValue addressValue;

                                if (symbol.Attributes.TryGetValue(DwarfAttribute.LowPc, out addressValue))
                                {
                                    functionsCache.Add(symbol);
                                }
                            }

                            // If it is global variable, add it to collection

                            if (symbol.Tag == DwarfTag.Variable)
                            {
                                DwarfAttributeValue locationAttributeValue;
                                string fullName = symbol.FullName;

                                if (!string.IsNullOrEmpty(fullName) && symbol.Attributes.TryGetValue(DwarfAttribute.Location, out locationAttributeValue))
                                {
                                    Location location = DecodeLocation(locationAttributeValue);

                                    if (location.Type == LocationType.AbsoluteAddress)
                                    {
                                        globalVariables.TryAdd(fullName, symbol);
                                    }
                                }
                            }
                            else if (symbol.Tag == DwarfTag.Member && symbol.Attributes.ContainsKey(DwarfAttribute.External)
                                && symbol.Attributes[DwarfAttribute.External].Flag)
                            {
                                string fullName = symbol.FullName;

                                if (!string.IsNullOrEmpty(fullName))
                                {
                                    globalVariables.TryAdd(fullName, symbol);
                                }
                            }

                            // Check predicate if we should stop search
                            if (predicate(symbol))
                            {
                                return symbol;
                            }
                        }

                        functionsCache.Sort((f1, f2) => (int)f1.GetConstantAttribute(DwarfAttribute.LowPc) - (int)f2.GetConstantAttribute(DwarfAttribute.LowPc));
                        functionAddressesCache = functionsCache.Select(f => f.GetConstantAttribute(DwarfAttribute.LowPc)).ToList();
                        allSymbolsEnumerated = true;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the type name using nice print.
        /// </summary>
        /// <param name="symbol">The symbol of the type.</param>
        /// <param name="gccPrint">if set to <c>true</c> GCC print compatibility will be used.</param>
        private string GetTypeNameNicePrint(DwarfSymbol symbol, bool gccPrint)
        {
            try
            {
                if (symbol.Tag == DwarfTag.Typedef)
                {
                    return GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint);
                }
                if (symbol.Tag == DwarfTag.PointerType)
                {
                    return GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint) + " *";
                }
                if (symbol.Tag == DwarfTag.ReferenceType)
                {
                    return GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint) + " &";
                }
                if (symbol.Tag == DwarfTag.ArrayType)
                {
                    DwarfSymbol subrangetype = symbol.Children.FirstOrDefault();

                    if (subrangetype.Tag == DwarfTag.SubrangeType)
                    {
                        ulong count = subrangetype.GetConstantAttribute(DwarfAttribute.Count);

                        if (count == 0)
                        {
                            count = subrangetype.GetConstantAttribute(DwarfAttribute.UpperBound) + 1;
                        }
                        return GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint) + $"[{count}]";
                    }
                    return GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint) + "[]";
                }
                if (symbol.Tag == DwarfTag.ConstType)
                {
                    return "const " + GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint);
                }
                if (symbol.Tag == DwarfTag.VolatileType)
                {
                    return "volatile " + GetTypeNameNicePrint(symbol.Attributes[DwarfAttribute.Type].Reference, gccPrint);
                }
            }
            catch
            {
                return GetTypeName(symbol);
            }

            StringBuilder sb = new StringBuilder();
            Stack<DwarfSymbol> symbols = new Stack<DwarfSymbol>();

            while (symbol != null && symbol.Tag != DwarfTag.CompileUnit && !string.IsNullOrEmpty(symbol.Name))
            {
                symbols.Push(symbol);
                symbol = symbol.Parent;
            }

            while (symbols.Count > 0)
            {
                symbol = symbols.Pop();
                if (sb.Length > 0)
                {
                    sb.Append("::");
                }

                string symbolName = symbol.Name;
                int position = sb.Length;

                if (symbol.Children != null && symbolName.Contains("<"))
                {
                    try
                    {
                        DwarfSymbol[] parameters = symbol.Children.Where(c => c.Tag == DwarfTag.TemplateTypeParameter || c.Tag == DwarfTag.TemplateValueParameter).ToArray();

                        sb.Append(symbolName.Substring(0, symbolName.IndexOf('<')));
                        sb.Append("<");
                        foreach (DwarfSymbol parameter in parameters)
                        {
                            if (sb[sb.Length - 1] != '<')
                            {
                                sb.Append(", ");
                            }
                            switch (parameter.Tag)
                            {
                                case DwarfTag.TemplateTypeParameter:
                                    {
                                        DwarfSymbol type = parameter.Attributes[DwarfAttribute.Type].Reference;

                                        sb.Append(GetTypeNameNicePrint(type, gccPrint));
                                    }
                                    break;
                                case DwarfTag.TemplateValueParameter:
                                    {
                                        DwarfSymbol type = parameter.Attributes[DwarfAttribute.Type].Reference;
                                        ulong value = parameter.GetConstantAttribute(DwarfAttribute.ConstValue);
                                        string stringValue = null;

                                        if (!parameter.Attributes.ContainsKey(DwarfAttribute.ConstValue))
                                        {
                                            throw new NotImplementedException();
                                        }
                                        switch (type.Tag)
                                        {
                                            case DwarfTag.BaseType:
                                                switch (GetTypeBasicType(type))
                                                {
                                                    case BuiltinType.Bool:
                                                        stringValue = value != 0 ? "true" : "false";
                                                        break;
                                                    case BuiltinType.Int8:
                                                        stringValue = ((sbyte)value).ToString();
                                                        break;
                                                    case BuiltinType.Int16:
                                                        stringValue = ((short)value).ToString();
                                                        break;
                                                    case BuiltinType.Int32:
                                                        stringValue = ((int)value).ToString();
                                                        break;
                                                    case BuiltinType.Int64:
                                                    case BuiltinType.Int128:
                                                        stringValue = ((long)value).ToString();
                                                        break;
                                                    case BuiltinType.UInt8:
                                                    case BuiltinType.UInt16:
                                                    case BuiltinType.UInt32:
                                                    case BuiltinType.UInt64:
                                                    case BuiltinType.UInt128:
                                                        stringValue = value.ToString();
                                                        break;
                                                    default:
                                                        stringValue = value.ToString();
                                                        break;
                                                }
                                                break;
                                            case DwarfTag.EnumerationType:
                                                {
                                                    DwarfSymbol enumValue = type.Children.FirstOrDefault(c => c.GetConstantAttribute(DwarfAttribute.ConstValue) == value);

                                                    if (enumValue != null && !gccPrint)
                                                    {
                                                        stringValue = $"{GetTypeNameNicePrint(type, gccPrint)}::{enumValue.Name}";
                                                    }
                                                    else
                                                    {
                                                        stringValue = $"({GetTypeNameNicePrint(type, gccPrint)}){value}";
                                                    }
                                                }
                                                break;
                                            default:
                                                throw new NotImplementedException();
                                        }
                                        sb.Append(stringValue);
                                    }
                                    break;
                            }
                        }
                        if (sb[sb.Length - 1] == '>')
                        {
                            sb.Append(" >");
                        }
                        else
                        {
                            sb.Append(">");
                        }
                    }
                    catch
                    {
                        sb.Length = position;
                    }
                }

                if (sb.Length == position)
                {
                    sb.Append(symbolName);
                }
            }

            return sb.Length > 0 ? sb.ToString() : GetTypeName(symbol);
        }

        /// <summary>
        /// Cleans numbers from the symbol name (removed trailing type).
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        private string CleanSymbolNameNumbers(string typeName)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < typeName.Length; i++)
            {
                if (char.IsDigit(typeName[i]) && (i == 0 || (!char.IsLetterOrDigit(typeName[i - 1]) && typeName[i - 1] != '_')))
                {
                    int numberStart = i;

                    while (i < typeName.Length && char.IsDigit(typeName[i]))
                    {
                        sb.Append(typeName[i]);
                        i++;
                    }

                    int numberEnd = i;

                    while (i < typeName.Length && (typeName[i] == 'u' || typeName[i] == 'U' || typeName[i] == 'l' || typeName[i] == 'L'))
                    {
                        i++;
                    }
                    i--;
                }
                else
                {
                    sb.Append(typeName[i]);
                }
            }

            return sb.ToString();
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
            /// Gets the canonical frame address location.
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
        /// Decodes <see cref="DwarfAttribute.DataMemberLocation"/> attribute from the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol that should have <see cref="DwarfAttribute.DataMemberLocation"/> attribute.</param>
        private int DecodeDataMemberLocation(DwarfSymbol symbol)
        {
            DwarfAttributeValue value;

            if (!symbol.Attributes.TryGetValue(DwarfAttribute.DataMemberLocation, out value))
            {
                return 0;
            }

            Location location = DecodeLocationStatic(value, isDataMemberLocation: true);

            if (location.Type == LocationType.AbsoluteAddress)
            {
                return (int)location.Address;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes <see cref="DwarfAttribute.DataMemberLocation"/> attribute from the specified symbol.
        /// </summary>
        /// <param name="symbol">Symbol that should have <see cref="DwarfAttribute.DataMemberLocation"/> attribute.</param>
        /// <param name="objectAddress">Address of the object from which decoding expression should start.</param>
        private ulong DecodeDataMemberLocation(DwarfSymbol symbol, ulong objectAddress)
        {
            DwarfAttributeValue value;

            if (!symbol.Attributes.TryGetValue(DwarfAttribute.DataMemberLocation, out value))
            {
                return 0;
            }

            Location location = DecodeLocationStatic(value, isDataMemberLocation: true, objectAddress: objectAddress);

            if (location.Type == LocationType.AbsoluteAddress)
            {
                return location.Address;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Decodes location from the specified attribute value.
        /// </summary>
        /// <param name="value">The location attribute value.</param>
        /// <param name="frameContext">Optional frame context used for resolving register values.</param>
        /// <param name="frameBase">The frame base location.</param>
        /// <param name="isDataMemberLocation">Flag if we are decoding location for <see cref="DwarfAttribute.DataMemberLocation"/></param>
        /// <param name="objectAddress">Address of the object that contains this data member. It is used only when <paramref name="isDataMemberLocation"/> is set to <c>true</c>.</param>
        private Location DecodeLocationStatic(DwarfAttributeValue value, ThreadContext frameContext = null, Location? frameBase = null, bool isDataMemberLocation = false, ulong objectAddress = 0)
        {
            if (value.Type == DwarfAttributeValueType.Constant)
            {
                if (!isDataMemberLocation)
                {
                    return Location.Absolute(value.Constant);
                }
                else
                {
                    return Location.Absolute(value.Constant + objectAddress);
                }
            }

            if (value.Type != DwarfAttributeValueType.ExpressionLocation && value.Type != DwarfAttributeValueType.Block)
            {
                return Location.Invalid;
            }

            using (DwarfMemoryReader reader = new DwarfMemoryReader(value.ExpressionLocation))
            {
                Stack<Location> stack = new Stack<Location>();

                if (isDataMemberLocation)
                {
                    stack.Push(Location.Absolute(objectAddress));
                }

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
                        case DwarfOperation.lit0:
                        case DwarfOperation.lit1:
                        case DwarfOperation.lit2:
                        case DwarfOperation.lit3:
                        case DwarfOperation.lit4:
                        case DwarfOperation.lit5:
                        case DwarfOperation.lit6:
                        case DwarfOperation.lit7:
                        case DwarfOperation.lit8:
                        case DwarfOperation.lit9:
                        case DwarfOperation.lit10:
                        case DwarfOperation.lit11:
                        case DwarfOperation.lit12:
                        case DwarfOperation.lit13:
                        case DwarfOperation.lit14:
                        case DwarfOperation.lit15:
                        case DwarfOperation.lit16:
                        case DwarfOperation.lit17:
                        case DwarfOperation.lit18:
                        case DwarfOperation.lit19:
                        case DwarfOperation.lit20:
                        case DwarfOperation.lit21:
                        case DwarfOperation.lit22:
                        case DwarfOperation.lit23:
                        case DwarfOperation.lit24:
                        case DwarfOperation.lit25:
                        case DwarfOperation.lit26:
                        case DwarfOperation.lit27:
                        case DwarfOperation.lit28:
                        case DwarfOperation.lit29:
                        case DwarfOperation.lit30:
                        case DwarfOperation.lit31:
                            stack.Push(Location.Absolute((ulong)(operation - DwarfOperation.lit0)));
                            break;
                        case DwarfOperation.breg0:
                        case DwarfOperation.breg1:
                        case DwarfOperation.breg2:
                        case DwarfOperation.breg3:
                        case DwarfOperation.breg4:
                        case DwarfOperation.breg5:
                        case DwarfOperation.breg6:
                        case DwarfOperation.breg7:
                        case DwarfOperation.breg8:
                        case DwarfOperation.breg9:
                        case DwarfOperation.breg10:
                        case DwarfOperation.breg11:
                        case DwarfOperation.breg12:
                        case DwarfOperation.breg13:
                        case DwarfOperation.breg14:
                        case DwarfOperation.breg15:
                        case DwarfOperation.breg16:
                        case DwarfOperation.breg17:
                        case DwarfOperation.breg18:
                        case DwarfOperation.breg19:
                        case DwarfOperation.breg20:
                        case DwarfOperation.breg21:
                        case DwarfOperation.breg22:
                        case DwarfOperation.breg23:
                        case DwarfOperation.breg24:
                        case DwarfOperation.breg25:
                        case DwarfOperation.breg26:
                        case DwarfOperation.breg27:
                        case DwarfOperation.breg28:
                        case DwarfOperation.breg29:
                        case DwarfOperation.breg30:
                        case DwarfOperation.breg31:
                            stack.Push(Location.RegisterRelative(operation - DwarfOperation.breg0, (int)reader.SLEB128()));
                            break;
                        case DwarfOperation.bregx:
                            stack.Push(Location.RegisterRelative((int)reader.LEB128(), (int)reader.SLEB128()));
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
                        case DwarfOperation.deref:
                            {
                                Location location = stack.Pop();

                                if (frameContext == null && location.Type != LocationType.AbsoluteAddress)
                                {
                                    return Location.Invalid;
                                }

                                ulong address = ResolveLocation(location, frameContext);
                                MemoryBuffer buffer = Debugger.ReadMemory(Module.Process, address, Module.PointerSize);

                                address = UserType.ReadPointer(buffer, 0, (int)Module.PointerSize);
                                stack.Push(Location.Absolute(address));
                            }
                            break;
                        case DwarfOperation.Duplicate:
                            stack.Push(stack.Peek());
                            break;
                        case DwarfOperation.Minus:
                            {
                                Location b = stack.Pop();
                                Location a = stack.Pop();
                                Location c = Location.Absolute(a.Address - b.Address);

                                stack.Push(c);
                            }
                            break;
                        case DwarfOperation.Plus:
                            {
                                Location b = stack.Pop();
                                Location a = stack.Pop();
                                Location c = Location.Absolute(a.Address + b.Address);

                                stack.Push(c);
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
        /// Checks whether function has special unwinding, or default (fast) unwinding should be used.
        /// </summary>
        /// <param name="functionStart">Address of the function start.</param>
        /// <returns><c>true</c> if fast unwinding should be used; <c>false</c> otherwise.</returns>
        private bool IsFastUnwind(ulong functionStart)
        {
            // if prologue is
            //   55     pushl %ebp
            //   89 e5  movl %esp, %ebp
            //  or
            //   55        pushq %rbp
            //   48 89 e5  movq %rsp, %rbp
            // We should pull in the ABI architecture default unwind plan and return that
            byte[] i386_push_mov = new byte[] { 0x55, 0x89, 0xe5 };
            byte[] x86_64_push_mov = new byte[] { 0x55, 0x48, 0x89, 0xe5 };
            MemoryBuffer buffer = Debugger.ReadMemory(Module.Process, functionStart, (uint)Math.Max(i386_push_mov.Length, x86_64_push_mov.Length));
            bool same = true;

            for (int i = 0; i < i386_push_mov.Length && same; i++)
            {
                same = i386_push_mov[i] == buffer.Bytes[i];
            }

            if (!same)
            {
                same = true;
                for (int i = 0; i < x86_64_push_mov.Length && same; i++)
                {
                    same = x86_64_push_mov[i] == buffer.Bytes[i];
                }
            }

            return same;
        }

        /// <summary>
        /// Decodes location from the specified attribute value. Also applies code segment offset for absolute address.
        /// </summary>
        /// <param name="value">The location attribute value.</param>
        /// <param name="frameContext">Optional frame context used for resolving register values.</param>
        /// <param name="frameBase">The frame base location.</param>
        private Location DecodeLocation(DwarfAttributeValue value, ThreadContext frameContext = null, Location? frameBase = null)
        {
            Location location = DecodeLocationStatic(value, frameContext, frameBase);

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
        /// <param name="frameContext">The frame context used for resolving register values.</param>
        private Location ResolveFrameBaseLocation(DwarfAttributeValue frameBaseAttribute, ThreadContext frameContext)
        {
            Location frameBase = DecodeLocation(frameBaseAttribute, frameContext);

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
        /// <param name="frameContext">The frame context used for resolving register values.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="currentAddress">The current address.</param>
        /// <param name="cfaLocation">The canonical frame address location.</param>
        /// <param name="stopBeforeRestore">if set to <c>true</c> stops before restore instruction.</param>
        private void ProcessCanonicalFrameAddressInstructions(DwarfCommonInformationEntry entry, ThreadContext frameContext, byte[] instructions, ref ulong currentAddress, ref Location cfaLocation, bool stopBeforeRestore = false)
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
                                    cfaLocation = DecodeLocationStatic(new DwarfAttributeValue()
                                    {
                                        Type = DwarfAttributeValueType.ExpressionLocation,
                                        Value = data.ReadBlock(data.LEB128()),
                                    }, frameContext);
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
                                    dummyLocation = DecodeLocationStatic(new DwarfAttributeValue()
                                    {
                                        Type = DwarfAttributeValueType.Block,
                                        Value = data.ReadBlock(data.LEB128()),
                                    }, frameContext);
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
        /// <param name="frameContext">The frame context used for resolving register values.</param>
        /// <param name="searchAddress">Search address where IP is located.</param>
        private Location ResolveCanonicalFrameAddress(DwarfAttributeValue startAddressValue, DwarfAttributeValue endAddressValue, ThreadContext frameContext, ulong searchAddress = ulong.MaxValue)
        {
            ulong startAddress = startAddressValue.Address + codeSegmentOffset;
            ulong endAddress = endAddressValue.Type == DwarfAttributeValueType.Address ? endAddressValue.Address + codeSegmentOffset : startAddress + endAddressValue.Constant;
            Location ebp = Location.RegisterRelative(Is64bit ? 6 : 5, Is64bit ? 16 : 8);
            ulong location = searchAddress == ulong.MaxValue ? startAddress : searchAddress;

            try
            {
                if (!IsFastUnwind(startAddressValue.Address + Module.Address))
                {
                    DwarfFrameDescriptionEntry description = null;
                    int index = frameDescriptionEntryFinderFromExceptionHandlingStream.Value.Find(location);

                    if (index >= 0)
                    {
                        description = frameDescriptionEntriesFromExceptionHandlingStream.Value[index];
                    }

                    if (description == null)
                    {
                        index = frameDescriptionEntryFinder.Value.Find(location);
                        if (index >= 0)
                        {
                            description = frameDescriptionEntries.Value[index];
                        }
                    }

                    if (description != null)
                    {
                        Location result = Location.CanonicalFrameAddress;

                        ProcessCanonicalFrameAddressInstructions(description.CommonInformationEntry, frameContext, description.CommonInformationEntry.InitialInstructions, ref location, ref result);
                        ProcessCanonicalFrameAddressInstructions(description.CommonInformationEntry, frameContext, description.Instructions, ref location, ref result, stopBeforeRestore: true);
                        return result;
                    }
                }
            }
            catch
            {
            }

            return ebp;
        }

        /// <summary>
        /// Resolves location into memory address based on specified frame context.
        /// </summary>
        /// <param name="location">Location to be resolved.</param>
        /// <param name="frameContext">The frame context used for resolving register values.</param>
        /// <returns>Resolved memory address.</returns>
        private ulong ResolveLocation(Location location, ThreadContext frameContext)
        {
            if (location.Type == LocationType.AbsoluteAddress)
            {
                return location.Address;
            }
            else if (location.Type == LocationType.RegisterRelative)
            {
                ulong address;

                if (Is64bit)
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
        /// Gets function canonical frame address. Used for unwinding.
        /// </summary>
        /// <param name="process">Process being debugged.</param>
        /// <param name="instructionPointer">Instruction pointer location.</param>
        /// <param name="frameContext">Frame context for resolving canonical frame address.</param>
        /// <returns>Resolved canonical frame address.</returns>
        public ulong GetFunctionCanonicalFrameAddress(Process process, ulong instructionPointer, ThreadContext frameContext)
        {
            ulong functionDisplacement;
            DwarfSymbol function = FindFunction(instructionPointer - Module.Address, out functionDisplacement);

            if (function == null)
            {
                return 0;
            }

            Location canonicalFrameAddress = ResolveCanonicalFrameAddress(function.Attributes[DwarfAttribute.LowPc], function.Attributes[DwarfAttribute.HighPc], frameContext, instructionPointer);

            return ResolveLocation(canonicalFrameAddress, frameContext);
        }

        /// <summary>
        /// Resolves the symbol address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="function">The function.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="frameContext">The frame context.</param>
        private ulong ResolveAddress(Process process, DwarfSymbol function, DwarfSymbol symbol, ThreadContext frameContext)
        {
            Location frameBase = ResolveFrameBaseLocation(function.Attributes[DwarfAttribute.FrameBase], frameContext);
            Location canonicalFrameAddress = ResolveCanonicalFrameAddress(function.Attributes[DwarfAttribute.LowPc], function.Attributes[DwarfAttribute.HighPc], frameContext);
            Location location = DecodeLocation(symbol.Attributes[DwarfAttribute.Location], frameContext, frameBase);

            if (location.Type == LocationType.RegisterRelative && location.Register == Location.CanonicalFrameAddress.Register)
            {
                return ResolveLocation(canonicalFrameAddress, frameContext) + (ulong)location.Offset;
            }

            return ResolveLocation(location, frameContext);
        }

        /// <summary>
        /// Finds the function at the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="displacement">The displacement.</param>
        private DwarfSymbol FindFunction(ulong address, out ulong displacement)
        {
            DwarfSymbol function;

            if (!allSymbolsEnumerated)
            {
                function = ContinueSymbolSearch(symbol =>
                {
                    if (symbol.Tag == DwarfTag.Subprogram)
                    {
                        DwarfAttributeValue startAddressValue, endAddressValue;

                        if (symbol.Attributes.TryGetValue(DwarfAttribute.LowPc, out startAddressValue) && symbol.Attributes.TryGetValue(DwarfAttribute.HighPc, out endAddressValue))
                        {
                            ulong startAddress = startAddressValue.Address;
                            ulong endAddress = endAddressValue.Type == DwarfAttributeValueType.Constant ? startAddress + endAddressValue.Constant : endAddressValue.Address;

                            return startAddress <= address && address < endAddress;
                        }
                    }

                    return false;
                });

                if (function != null)
                {
                    displacement = address - function.GetConstantAttribute(DwarfAttribute.LowPc);
                    return function;
                }
            }

            int index = functionAddressesCache.BinarySearch(address);

            if (index < 0)
            {
                index = ~index;
            }
            if (index >= functionsCache.Count)
            {
                index = functionsCache.Count - 1;
            }
            if (index < 0)
            {
                displacement = address;
                return null;
            }
            if (functionAddressesCache[index] > address && index > 0)
            {
                index--;
            }

            if (functionAddressesCache[index] > address)
            {
                displacement = address;
                return null;
            }

            displacement = address - functionAddressesCache[index];
            return functionsCache[index];
        }

        /// <summary>
        /// Finds the global variable.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        private DwarfSymbol FindGlobalVariable(string globalVariableName)
        {
            DwarfSymbol globalVariable;

            if (globalVariables.TryGetValue(globalVariableName, out globalVariable))
            {
                return globalVariable;
            }

            globalVariable = ContinueSymbolSearch(symbol =>
            {
                if (symbol.Tag == DwarfTag.Variable && symbol.FullName == globalVariableName)
                {
                    Location location = DecodeLocation(symbol.Attributes[DwarfAttribute.Location]);

                    if (location.Type == LocationType.AbsoluteAddress)
                    {
                        return true;
                    }
                }
                else if (symbol.Tag == DwarfTag.Member && symbol.Attributes.ContainsKey(DwarfAttribute.External)
                    && symbol.Attributes[DwarfAttribute.External].Flag && symbol.FullName == globalVariableName)
                {
                    return true;
                }

                return false;
            });

            if (globalVariable != null)
            {
                return globalVariable;
            }

            throw new Exception($"Unable to find global variable: {globalVariableName}");
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
        private static DwarfSymbol GetType(DwarfSymbol symbol)
        {
            DwarfAttributeValue typeAttributeValue;

            if (!symbol.Attributes.TryGetValue(DwarfAttribute.Type, out typeAttributeValue))
            {
                return null;
            }

            DwarfSymbol type = typeAttributeValue.Reference;

            while (type.Tag == DwarfTag.Typedef || type.Tag == DwarfTag.ConstType || type.Tag == DwarfTag.VolatileType)
            {
                if (!type.Attributes.TryGetValue(DwarfAttribute.Type, out typeAttributeValue))
                {
                    return null;
                }

                type = typeAttributeValue.Reference;
            }
            return type;
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private string GetTypeName(DwarfSymbol type)
        {
            if (type == null)
            {
                return "void";
            }

            if (type.Tag == DwarfTag.PointerType)
            {
                return GetTypeName(GetType(type)) + "*";
            }
            if (type.Tag == DwarfTag.ReferenceType)
            {
                return GetTypeName(GetType(type)) + "&";
            }
            if (type.Tag == DwarfTag.ArrayType)
            {
                DwarfSymbol subrangetype = type.Children.FirstOrDefault();

                if (subrangetype.Tag == DwarfTag.SubrangeType)
                {
                    ulong count = subrangetype.GetConstantAttribute(DwarfAttribute.Count);

                    if (count == 0)
                    {
                        count = subrangetype.GetConstantAttribute(DwarfAttribute.UpperBound) + 1;
                    }
                    return $"{GetTypeName(GetType(type))}[{count}]";
                }
                return $"{GetTypeName(GetType(type))}[]";
            }

            switch (type.Tag)
            {
                case DwarfTag.ClassType:
                case DwarfTag.StructureType:
                case DwarfTag.BaseType:
                case DwarfTag.EnumerationType:
                case DwarfTag.InterfaceType:
                case DwarfTag.SubroutineType:
                case DwarfTag.UnionType:
                    return type.FullName ?? GetUnnamedType();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the unnamed type name.
        /// </summary>
        /// <returns>Generated name for unnamed type.</returns>
        private string GetUnnamedType()
        {
            int unnamedTypeId = System.Threading.Interlocked.Increment(ref nextUnnamedTypeId);

            return $"__unnamed_type_{unnamedTypeId}";
        }
    }
}
