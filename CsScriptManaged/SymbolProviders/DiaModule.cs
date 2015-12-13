using CsScripts;
using Dia2Lib;
using System;
using System.Collections.Generic;

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
        /// Initializes a new instance of the <see cref="DiaModule"/> class.
        /// </summary>
        /// <param name="pdbPath">The PDB path.</param>
        public DiaModule(string pdbPath)
        {
            dia = new DiaSource();
            dia.loadDataFromPdb(pdbPath);
            dia.openSession(out session);
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
            // TODO: Add support for basic types
            IDiaSymbol type = session.globalScope.GetChild(typeName);

            return type.symIndexId;
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            return GetTypeName(GetTypeFromId(typeId));
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private string GetTypeName(IDiaSymbol type)
        {
            switch ((SymTagEnum)type.symTag)
            {
                case SymTagEnum.SymTagBaseType:
                    switch ((BasicType)type.baseType)
                    {
                        case BasicType.Bit:
                        case BasicType.Bool:
                            return "bool";
                        case BasicType.Char:
                            return "char";
                        case BasicType.WChar:
                            return "wchar_t";
                        case BasicType.BSTR:
                            return "string";
                        case BasicType.Void:
                            return "void";
                        case BasicType.Float:
                            return type.length <= 4 ? "float" : type.length > 9 ? "long double" : "double";
                        case BasicType.Int:
                        case BasicType.Long:
                            switch (type.length)
                            {
                                case 0:
                                    return "void";
                                case 1:
                                    return "char";
                                case 2:
                                    return "short";
                                case 4:
                                    return "int";
                                case 8:
                                    return "long long";
                                default:
                                    throw new Exception("Unexpected type length " + type.length);
                            }

                        case BasicType.UInt:
                        case BasicType.ULong:
                            switch (type.length)
                            {
                                case 0:
                                    return "void";
                                case 1:
                                    return "unsigned char";
                                case 2:
                                    return "unsigned short";
                                case 4:
                                    return "unsigned int";
                                case 8:
                                    return "unsigned long long";
                                default:
                                    throw new Exception("Unexpected type length " + type.length);
                            }

                        case BasicType.Hresult:
                            return "HRESULT";
                        default:
                            throw new Exception("Unexpected basic type " + (BasicType)type.baseType);
                    }

                case SymTagEnum.SymTagPointerType:
                    {
                        IDiaSymbol pointerType = type.type;

                        return GetTypeName(pointerType) + "*";
                    }

                case SymTagEnum.SymTagUDT:
                case SymTagEnum.SymTagEnum:
                    {
                        return type.name;
                    }

                case SymTagEnum.SymTagFunctionType:
                case SymTagEnum.SymTagArrayType:
                    return GetTypeName(type.type) + "[]";

                default:
                    throw new Exception("Unexpected type tag " + (SymTagEnum)type.symTag);
            }
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
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            return GetTypeFieldNames(GetTypeFromId(typeId));
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private string[] GetTypeFieldNames(IDiaSymbol type)
        {
            if ((SymTagEnum)type.symTag == SymTagEnum.SymTagPointerType)
                type = type.type;

            var fields = type.GetChildren(SymTagEnum.SymTagData);
            List<string> fieldNames = new List<string>();

            foreach (var field in fields)
            {
                if ((DataKind)field.dataKind == DataKind.StaticMember)
                    continue;
                fieldNames.Add(field.name);
            }

            return fieldNames.ToArray();
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
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

            var fields = type.GetChildren(SymTagEnum.SymTagData);

            foreach (var field in fields)
            {
                if ((DataKind)field.dataKind == DataKind.StaticMember)
                    continue;
                if (field.name != fieldName)
                    continue;

                return Tuple.Create(field.typeId, field.offset);
            }

            throw new Exception("Field not found");
        }

        /// <summary>
        /// Gets the source file name and line for the specified stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="address">The address.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="sourceFileLine">The source file line.</param>
        /// <param name="displacement">The displacement.</param>
        /// <exception cref="Exception">Address not found</exception>
        public void GetSourceFileNameAndLine(StackFrame stackFrame, uint address, out string sourceFileName, out uint sourceFileLine, out ulong displacement)
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
        /// <param name="stackFrame">The stack frame.</param>
        /// <param name="address">The address.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="displacement">The displacement.</param>
        public void GetFunctionNameAndDisplacement(StackFrame stackFrame, uint address, out string functionName, out ulong displacement)
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
    }
}
