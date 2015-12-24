using CsScripts;
using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CsScriptManaged.SymbolProviders
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
        /// The cache of type all fields
        /// </summary>
        private DictionaryCache<uint, List<Tuple<string, uint, int>>> typeAllFields;

        /// <summary>
        /// The cache of type all fields
        /// </summary>
        private DictionaryCache<uint, List<Tuple<string, uint, int>>> typeFields;

        /// <summary>
        /// The basic types
        /// </summary>
        private SimpleCache<Dictionary<string, IDiaSymbol>> basicTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiaModule"/> class.
        /// </summary>
        /// <param name="pdbPath">The PDB path.</param>
        public DiaModule(string pdbPath)
        {
            dia = new DiaSource();
            dia.loadDataFromPdb(pdbPath);
            dia.openSession(out session);
            typeAllFields = new DictionaryCache<uint, List<Tuple<string, uint, int>>>(GetTypeAllFields);
            typeFields = new DictionaryCache<uint, List<Tuple<string, uint, int>>>(GetTypeFields);
            basicTypes = SimpleCache.Create(() =>
            {
                var types = new Dictionary<string, IDiaSymbol>();
                var basicTypes = session.globalScope.GetChildren(SymTagEnum.SymTagBaseType);

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
        }

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
                GetTypeAllFields(b, typeFields, offset + b.offset);
            }

            // Get type fields
            var fields = type.GetChildren(SymTagEnum.SymTagData);
            foreach (var field in fields)
            {
                if ((DataKind)field.dataKind == DataKind.StaticMember)
                    continue;

                typeFields.Add(Tuple.Create(field.name, field.typeId, offset + field.offset));
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
            var fields = type.GetChildren(SymTagEnum.SymTagData);

            foreach (var field in fields)
            {
                if ((DataKind)field.dataKind == DataKind.StaticMember)
                    continue;

                typeFields.Add(Tuple.Create(field.name, field.typeId, field.offset));
            }

            return typeFields;
        }

        /// <summary>
        /// Gets the type symbol from type identifier.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private IDiaSymbol GetTypeFromId(uint typeId)
        {
            IDiaSymbol type;

            session.symbolById(typeId, out type);
            return type;
        }

        /// <summary>
        /// Gets the symbol tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public SymTag GetTypeTag(Module module, uint typeId)
        {
            return (SymTag)GetTypeFromId(typeId).symTag;
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(Module module, uint typeId)
        {
            return (uint)GetTypeFromId(typeId).length;
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeName">Name of the type.</param>
        public uint GetTypeId(Module module, string typeName)
        {
            IDiaSymbol type;

            if (!BasicTypes.TryGetValue(typeName, out type))
            {
                type = session.globalScope.GetChild(typeName);
            }

            return GetTypeId(type);
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
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            return TypeToString.GetTypeString(GetTypeFromId(typeId));
        }

        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(Module module, uint typeId)
        {
            return GetTypeFromId(typeId).typeId;
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeAllFieldNames(Module module, uint typeId)
        {
            return GetTypeAllFieldNames(GetTypeFromId(typeId));
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private string[] GetTypeAllFieldNames(IDiaSymbol type)
        {
            if ((SymTagEnum)type.symTag == SymTagEnum.SymTagPointerType)
                type = type.type;

            var fields = typeAllFields[type.symIndexId];

            return fields.Select(t => t.Item1).ToArray();
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeAllFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            return GetTypeFieldTypeAndOffset(GetTypeFromId(typeId), fieldName);
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldName">Name of the field.</param>
        private Tuple<uint, int> GetTypeFieldTypeAndOffset(IDiaSymbol type, string fieldName)
        {
            if ((SymTagEnum)type.symTag == SymTagEnum.SymTagPointerType)
                type = type.type;

            var fields = typeAllFields[type.symIndexId];

            foreach (var field in fields)
            {
                if (field.Item1 != fieldName)
                    continue;

                return Tuple.Create(field.Item2, field.Item3);
            }

            throw new Exception("Field not found");
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
        /// <exception cref="System.Exception">Address not found</exception>
        /// <exception cref="Exception">Address not found</exception>
        public void GetSourceFileNameAndLine(Process process, ulong processAddress, uint address, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
        {
            IDiaEnumLineNumbers lineNumbers;
            IDiaSymbol function;

            session.findSymbolByRVA(address, SymTagEnum.SymTagFunction, out function);
            session.findLinesByRVA(address, (uint)function.length, out lineNumbers);
            foreach (IDiaLineNumber lineNumber in lineNumbers)
            {
                if (address == lineNumber.relativeVirtualAddress)
                {
                    sourceFileName = lineNumber.sourceFile.fileName;
                    sourceFileLine = lineNumber.lineNumber;
                    displacement = 0;
                    return;
                }
            }

            throw new Exception("Address not found");
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
            int innerDisplacement;
            IDiaSymbol function;

            session.findSymbolByRVAEx(address, SymTagEnum.SymTagFunction, out function, out innerDisplacement);
            displacement = (ulong)innerDisplacement;
            functionName = function.name;
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
            IDiaSymbol function;
            int displacement;
            List<Variable> variables = new List<Variable>();

            session.findSymbolByRVAEx(relativeAddress, SymTagEnum.SymTagFunction, out function, out displacement);
            foreach (var symbol in function.GetChildren(SymTagEnum.SymTagData))
            {
                if (arguments && (DataKind)symbol.dataKind != DataKind.Param)
                {
                    continue;
                }

                CodeType codeType = module.TypesById[symbol.typeId];
                ulong address = ResolveAddress(symbol, frame.FrameContext);

                variables.Add(Variable.CreateNoCast(codeType, address, symbol.name));
            }

            return new VariableCollection(variables.ToArray());
        }

        /// <summary>
        /// Resolves the symbol address.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="frameContext">The frame context.</param>
        private static ulong ResolveAddress(IDiaSymbol symbol, ThreadContext frameContext)
        {
            ulong address;

            switch ((LocationType)symbol.locationType)
            {
                case LocationType.RegRel:
                    switch ((CV_HREG_e)symbol.registerId)
                    {
                        case CV_HREG_e.CV_AMD64_ESP:
                        case CV_HREG_e.CV_AMD64_RSP:
                            address = frameContext.StackPointer;
                            break;
                        case CV_HREG_e.CV_AMD64_RIP:
                            address = frameContext.InstructionPointer;
                            break;
                        case CV_HREG_e.CV_AMD64_RBP:
                            address = frameContext.FramePointer;
                            break;
                        default:
                            throw new Exception("Unknown register id" + (CV_HREG_e)symbol.registerId);
                    }

                    address += (ulong)symbol.offset;
                    return address;
                default:
                    throw new Exception("Unknown location type " + (LocationType)symbol.locationType);
            }
        }

        /// <summary>
        /// Reads the simple data (1 to 8 bytes) for specified type and address to read from.
        /// </summary>
        /// <param name="codeType">Type of the code.</param>
        /// <param name="address">The address.</param>
        public ulong ReadSimpleData(CodeType codeType, ulong address)
        {
            uint read, bufferSize = codeType.Size;
            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);

            try
            {
                // TODO: This doesn't work with bit fields
                Context.DataSpaces.ReadVirtual(address, buffer, bufferSize, out read);
                switch (bufferSize)
                {
                    case 1:
                        return Marshal.ReadByte(buffer);
                    case 2:
                        return (ushort)Marshal.ReadInt16(buffer);
                    case 4:
                        return (uint)Marshal.ReadInt32(buffer);
                    case 8:
                        return (ulong)Marshal.ReadInt64(buffer);
                    default:
                        throw new Exception("Unexpected data size " + bufferSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Gets the global variable address.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public ulong GetGlobalVariableAddress(Module module, string globalVariableName)
        {
            var globalVariable = GetGlobalVariable(globalVariableName);

            return globalVariable.relativeVirtualAddress + module.Offset;
        }

        /// <summary>
        /// Gets the global variable type identifier.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="globalVariableName">Name of the global variable.</param>
        public uint GetGlobalVariableTypeId(Module module, string globalVariableName)
        {
            var globalVariable = GetGlobalVariable(globalVariableName);

            return GetTypeId(globalVariable.type);
        }

        /// <summary>
        /// Gets the global variable.
        /// </summary>
        /// <param name="globalVariableName">Name of the global variable.</param>
        private IDiaSymbol GetGlobalVariable(string globalVariableName)
        {
            var globalVariable = session.globalScope.GetChild(globalVariableName);

            if (globalVariable == null)
            {
                var spaces = globalVariableName.Split(new string[] { "::" }, StringSplitOptions.None);

                if (spaces.Length > 0)
                {
                    var type = session.globalScope.GetChild(string.Join("::", spaces.Take(spaces.Length - 1)));

                    if (type != null)
                        globalVariable = type.GetChild(spaces.Last(), SymTagEnum.SymTagData);
                }
            }

            if (globalVariable == null)
                throw new Exception("Global variable not found " + globalVariableName);

            return globalVariable;
        }

        /// <summary>
        /// Gets the names of fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            var type = GetTypeFromId(typeId);

            if ((SymTagEnum)type.symTag == SymTagEnum.SymTagPointerType)
                type = type.type;

            var fields = typeFields[type.symIndexId];

            return fields.Select(t => t.Item1).ToArray();
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            var type = GetTypeFromId(typeId);

            if ((SymTagEnum)type.symTag == SymTagEnum.SymTagPointerType)
                type = type.type;

            var fields = typeFields[type.symIndexId];

            foreach (var field in fields)
            {
                if (field.Item1 != fieldName)
                    continue;

                return Tuple.Create(field.Item2, field.Item3);
            }

            throw new Exception("Field not found");
        }

        /// <summary>
        /// Gets the type's base class type and offset.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="className">Name of the class.</param>
        public Tuple<uint, int> GetTypeBaseClass(Module module, uint typeId, string className)
        {
            var type = GetTypeFromId(typeId);

            if ((SymTagEnum)type.symTag == SymTagEnum.SymTagPointerType)
                type = type.type;

            if (type.name == className)
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

                    if (b.name == className)
                    {
                        return Tuple.Create(b.symIndexId, offset);
                    }

                    classes.Push(Tuple.Create(b, offset));
                }
            }

            throw new Exception("Base class not found");
        }
    }
}
