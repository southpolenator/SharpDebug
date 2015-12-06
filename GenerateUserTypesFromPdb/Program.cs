using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    class Program
    {
        private static void OpenPdb(string path, out IDiaDataSource dia, out IDiaSession session)
        {
            dia = new DiaSource();
            dia.loadDataFromPdb(path);
            dia.openSession(out session);
        }

        private static void DumpSymbol(IDiaSymbol symbol)
        {
            Type type = typeof(IDiaSymbol);

            foreach (var property in type.GetProperties())
            {
                Console.WriteLine("{0} = {1}", property.Name, property.GetValue(symbol));
            }
        }

        static void Main(string[] args)
        {
            var error = Console.Error;
            var output = Console.Out;

            if (args.Length < 2)
            {
                error.WriteLine("Not enough arguments, expected 2:");
                error.WriteLine("  [pdbPath]          Path to PDB file");
                error.WriteLine("  type1,type2,...    Types to be exported");
                return;
            }

            string pdbPath = args[0];
            string[] typeNames = args[1].Split(",".ToCharArray());
            bool singleLineProperty = true;

            string moduleName = Path.GetFileNameWithoutExtension(pdbPath).ToLower();
            Dictionary<string, IDiaSymbol> symbols = new Dictionary<string, IDiaSymbol>();
            IDiaDataSource dia;
            IDiaSession session;

            OpenPdb(pdbPath, out dia, out session);
            foreach (var typeName in typeNames)
            {
                IDiaSymbol symbol = session.globalScope.GetChild(typeName, SymTagEnum.SymTagUDT);

                if (symbol == null)
                {
                    error.WriteLine("Symbol not found: {0}", typeName);
                }
                else
                {
                    symbols.Add(typeName, symbol);
                }
            }

            //foreach (var symbol in session.globalScope.GetChildren(SymTagEnum.SymTagData))
            //{
            //    if ((DataKind)symbol.dataKind == DataKind.DataIsGlobal)
            //    {
            //        Console.WriteLine("{0}", symbol.name);
            //    }
            //}

            //foreach (var symbol in session.globalScope.GetChildren(SymTagEnum.SymTagUDT))
            //{
            //    if (symbol.GetBaseClasses().Where(c => c.name == "XDES").Any())
            //    {
            //        Console.WriteLine(symbol.name);
            //    }
            //}

            //return;

            foreach (var symbolEntry in symbols)
            {
                var symbol = symbolEntry.Value;
                var fields = symbol.GetChildren(SymTagEnum.SymTagData).ToArray();
                bool hasStatic = false, hasNonStatic = false;
                string baseType = GetBaseTypeString(error, symbol, symbols.Keys);

                output.WriteLine(@"[UserType(ModuleName = ""{0}"", TypeName = ""{1}""]", moduleName, symbol.name);
                output.WriteLine(@"class {0} : {1}", symbol.name, baseType);
                output.WriteLine(@"{");
                foreach (var field in fields)
                {
                    bool isStatic = (DataKind)field.dataKind == DataKind.DataIsStaticMember;
                    string fieldTypeString = GetTypeString(field.type, symbols.Keys, field.length);

                    output.WriteLine("    private {0}UserMember<{1}> _{2}", isStatic ? "static " : "", fieldTypeString, field.name);
                    hasStatic = hasStatic || isStatic;
                    hasNonStatic = hasNonStatic || !isStatic;
                }

                if (hasStatic)
                {
                    output.WriteLine();
                    output.WriteLine("    static {0}()", symbol.name);
                    output.WriteLine("    {");

                    foreach (var field in fields)
                    {
                        bool isStatic = (DataKind)field.dataKind == DataKind.DataIsStaticMember;
                        string fieldTypeString = GetTypeString(field.type, symbols.Keys, field.length);

                        if (isStatic)
                        {
                            string castingTypeString = GetCastingType(fieldTypeString);

                            if (castingTypeString.StartsWith("BasicType<"))
                            {
                                output.WriteLine("        _{0} = UserMember.Create(() => {1}.GetValue(Process.GetGlobal(\"{2}!{3}::{0}\")));", field.name, castingTypeString, moduleName, symbol.name);
                            }
                            else if (string.IsNullOrEmpty(castingTypeString))
                            {
                                output.WriteLine("        _{0} = UserMember.Create(() => Process.GetGlobal(\"{1}!{2}::{0}\"));", field.name, moduleName, symbol.name);
                            }
                            else
                            {
                                output.WriteLine("        _{0} = UserMember.Create(() => ({1})Process.GetGlobal(\"{2}!{3}::{0}\"));", field.name, castingTypeString, moduleName, symbol.name);
                            }
                        }
                    }

                    output.WriteLine("    }");
                }

                if (hasNonStatic)
                {
                    output.WriteLine();
                    output.WriteLine("    public {0}(Variable variable)", symbol.name);
                    output.WriteLine("        : base(variable)");
                    output.WriteLine("    {");

                    foreach (var field in fields)
                    {
                        bool isStatic = (DataKind)field.dataKind == DataKind.DataIsStaticMember;
                        string fieldTypeString = GetTypeString(field.type, symbols.Keys, field.length);

                        if (!isStatic)
                        {
                            string castingTypeString = GetCastingType(fieldTypeString);

                            if (castingTypeString.StartsWith("BasicType<"))
                            {
                                output.WriteLine("        _{0} = UserMember.Create(() => {1}.GetValue(variable.GetField(\"{0}\")));", field.name, castingTypeString);
                            }
                            else if (string.IsNullOrEmpty(castingTypeString))
                            {
                                output.WriteLine("        _{0} = UserMember.Create(() => variable.GetField(\"{0}\"));", field.name);
                            }
                            else
                            {
                                output.WriteLine("        _{0} = UserMember.Create(() => ({1})variable.GetField(\"{0}\"));", field.name, castingTypeString);
                            }
                        }
                    }

                    output.WriteLine("    }");
                }

                bool firstField = true;
                foreach (var field in fields)
                {
                    bool isStatic = (DataKind)field.dataKind == DataKind.DataIsStaticMember;
                    string fieldTypeString = GetTypeString(field.type, symbols.Keys, field.length);

                    if (singleLineProperty)
                    {
                        if (firstField)
                        {
                            output.WriteLine();
                            firstField = false;
                        }

                        output.WriteLine("    public {0}{1} {2} {{ get {{ return _{2}; }} }}", isStatic ? "static " : "", fieldTypeString, field.name);
                    }
                    else
                    {
                        output.WriteLine();
                        output.WriteLine("    public {0}{1} {2}", isStatic ? "static " : "", fieldTypeString, field.name);
                        output.WriteLine("    {");
                        output.WriteLine("        get");
                        output.WriteLine("        {");
                        output.WriteLine("            return _{0}.Value;", field.name);
                        output.WriteLine("        }");
                        output.WriteLine("    }");
                    }
                }

                output.WriteLine(@"}");
                output.WriteLine();
            }
        }

        private static string GetCastingType(string typeString)
        {
            if (typeString.EndsWith("?"))
                return "BasicType<" + typeString.Substring(0, typeString.Length - 1) + ">";
            if (typeString == "Variable")
                return "";
            return typeString;
        }

        private static string GetBaseTypeString(TextWriter error, IDiaSymbol type, ICollection<string> exportedTypes)
        {
            var baseClasses = type.GetBaseClasses().ToArray();

            if (baseClasses.Length > 1)
            {
                //throw new Exception(string.Format("Multiple inheritance is not supported. Type {0} is inherited from {1}", type.name, string.Join(", ", baseClasses.Select(c => c.name))));
                error.WriteLine(string.Format("Multiple inheritance is not supported, defaulting to 'UserType' as base class. Type {0} is inherited from {1}", type.name, string.Join(", ", baseClasses.Select(c => c.name))));
                return "UserType";
            }

            if (baseClasses.Length == 1)
            {
                type = baseClasses[0];

                if (exportedTypes.Contains(type.name))
                {
                    return type.name;
                }

                return GetBaseTypeString(error, type, exportedTypes);
            }

            return "UserType";
        }

        private static string GetTypeString(IDiaSymbol type, ICollection<string> exportedTypes, ulong bitLength = 0)
        {
            switch ((SymTagEnum)type.symTag)
            {
                case SymTagEnum.SymTagBaseType:
                    if (bitLength == 1)
                        return "bool";
                    switch ((BasicType)type.baseType)
                    {
                        case BasicType.btBit:
                        case BasicType.btBool:
                            return "bool";
                        case BasicType.btChar:
                        case BasicType.btWChar:
                            return "char";
                        case BasicType.btBSTR:
                            return "string";
                        case BasicType.btVoid:
                            return "void";
                        case BasicType.btFloat:
                            return type.length <= 4 ? "float" : "double";
                        case BasicType.btInt:
                        case BasicType.btLong:
                            switch (type.length)
                            {
                                case 0:
                                    return "void";
                                case 1:
                                    return "sbyte";
                                case 2:
                                    return "short";
                                case 4:
                                    return "int";
                                case 8:
                                    return "long";
                                default:
                                    throw new Exception("Unexpected type length " + type.length);
                            }

                        case BasicType.btUInt:
                        case BasicType.btULong:
                            switch (type.length)
                            {
                                case 0:
                                    return "void";
                                case 1:
                                    return "byte";
                                case 2:
                                    return "ushort";
                                case 4:
                                    return "uint";
                                case 8:
                                    return "ulong";
                                default:
                                    throw new Exception("Unexpected type length " + type.length);
                            }

                        case BasicType.btHresult:
                            return "Hresult";
                        default:
                            throw new Exception("Unexpected basic type " + (BasicType)type.baseType);
                    }

                case SymTagEnum.SymTagPointerType:
                    {
                        IDiaSymbol pointerType = type.type;

                        switch ((SymTagEnum)pointerType.symTag)
                        {
                            case SymTagEnum.SymTagBaseType:
                            case SymTagEnum.SymTagEnum:
                                {
                                    string innerType = GetTypeString(pointerType, exportedTypes);

                                    if (innerType == "void")
                                        return "NakedPointer";
                                    if (innerType == "char")
                                        return "string";
                                    return innerType + "?";
                                }

                            case SymTagEnum.SymTagUDT:
                                return GetTypeString(pointerType, exportedTypes);
                            default:
                                return "CodePointer<" + GetTypeString(pointerType, exportedTypes) + ">";
                        }
                    }

                case SymTagEnum.SymTagUDT:
                case SymTagEnum.SymTagEnum:
                    {
                        string typeName = type.name;

                        return exportedTypes.Contains(typeName) ? typeName : "Variable";
                    }

                case SymTagEnum.SymTagArrayType:
                    return "CodeArray<" + GetTypeString(type.type, exportedTypes) + ">";

                case SymTagEnum.SymTagFunctionType:
                    return "CodeFunction";

                default:
                    throw new Exception("Unexpected type tag " + (SymTagEnum)type.symTag);
            }
        }
    }
}
