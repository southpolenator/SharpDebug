using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    [Flags]
    enum UserTypeGenerationFlags
    {
        SingleLineProperty,
        GenerateFieldComment,
    }

    class UserType
    {
        public UserType(IDiaSymbol symbol, string moduleName)
        {
            Symbol = symbol;
            ModuleName = moduleName;
            InnerTypes = new List<UserType>();
            Usings = new HashSet<string>(new string[] { "CsScripts" });
        }

        public IDiaSymbol Symbol { get; private set; }

        public string ModuleName { get; private set; }

        public string Namespace { get; set; }

        public UserType DeclaredInType { get; private set; }

        public List<UserType> InnerTypes { get; private set; }

        public HashSet<string> Usings { get; private set; }

        public string ClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return Symbol.name.Substring(DeclaredInType.Symbol.name.Length + 2);
                }

                return Symbol.name;
            }
        }

        public string FullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.FullClassName, ClassName);
                }

                if (Namespace != null)
                {
                    return string.Format("{0}.{1}", Namespace, ClassName);
                }

                return ClassName;
            }
        }

        public void WriteCode(IndentedWriter output, TextWriter error, Dictionary<string, UserType> exportedTypes, UserTypeGenerationFlags options, int indentation = 0)
        {
            var symbol = Symbol;
            var moduleName = ModuleName;
            var fields = symbol.GetChildren(SymTagEnum.SymTagData).ToArray();
            bool hasStatic = false, hasNonStatic = false;
            string baseType = GetBaseTypeString(error, symbol, exportedTypes);

            if (DeclaredInType == null)
            {
                if (Usings.Count > 0)
                {
                    foreach (var u in Usings.OrderBy(s => s))
                    {
                        output.WriteLine(indentation, "using {0};", u);
                    }

                    output.WriteLine();
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    output.WriteLine(indentation, "namespace {0}", Namespace);
                    output.WriteLine(indentation, "{{");
                    indentation++;
                }
            }

            output.WriteLine(indentation, @"[UserType(ModuleName = ""{0}"", TypeName = ""{1}"")]", moduleName, symbol.name);
            output.WriteLine(indentation, @"public partial class {0} : {1}", ClassName, baseType);
            output.WriteLine(indentation, @"{{");
            foreach (var field in fields)
            {
                bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember;
                string fieldTypeString = GetTypeString(field.type, exportedTypes, field.length);

                if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldComment))
                    output.WriteLine(indentation + 1, "// {0} {1};", GetOriginalTypeString(field.type), field.name);
                output.WriteLine(indentation + 1, "private {0}UserMember<{1}> _{2};", isStatic ? "static " : "", fieldTypeString, field.name);
                if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldComment))
                    output.WriteLine();
                hasStatic = hasStatic || isStatic;
                hasNonStatic = hasNonStatic || !isStatic;
            }

            if (hasStatic)
            {
                output.WriteLine();
                output.WriteLine(indentation + 1, "static {0}()", ClassName);
                output.WriteLine(indentation + 1, "{{");

                foreach (var field in fields)
                {
                    bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember;
                    string fieldTypeString = GetTypeString(field.type, exportedTypes, field.length);

                    if (isStatic)
                    {
                        string castingTypeString = GetCastingType(fieldTypeString);

                        if (castingTypeString.StartsWith("BasicType<"))
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => {1}.GetValue(Process.Current.GetGlobal(\"{2}!{3}::{0}\")));", field.name, castingTypeString, moduleName, symbol.name);
                        }
                        else if (string.IsNullOrEmpty(castingTypeString))
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => Process.Current.GetGlobal(\"{1}!{2}::{0}\"));", field.name, moduleName, symbol.name);
                        }
                        else if (castingTypeString == "NakedPointer" || castingTypeString == "CodeFunction" || castingTypeString.StartsWith("CodeArray") || castingTypeString.StartsWith("CodePointer"))
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => new {1}(Process.Current.GetGlobal(\"{2}!{3}::{0}\")));", field.name, castingTypeString, moduleName, symbol.name);
                        }
                        else
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => ({1})Process.Current.GetGlobal(\"{2}!{3}::{0}\"));", field.name, castingTypeString, moduleName, symbol.name);
                        }
                    }
                }

                output.WriteLine(indentation + 1, "}}");
            }

            if (hasNonStatic)
            {
                output.WriteLine();
                output.WriteLine(indentation + 1, "public {0}(Variable variable)", ClassName);
                output.WriteLine(indentation + 2, ": base(variable)");
                output.WriteLine(indentation + 1, "{{");

                foreach (var field in fields)
                {
                    bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember;
                    string fieldTypeString = GetTypeString(field.type, exportedTypes, field.length);

                    if (!isStatic)
                    {
                        string castingTypeString = GetCastingType(fieldTypeString);

                        if (castingTypeString.StartsWith("BasicType<"))
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => {1}.GetValue(variable.GetField(\"{0}\")));", field.name, castingTypeString);
                        }
                        else if (string.IsNullOrEmpty(castingTypeString))
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => variable.GetField(\"{0}\"));", field.name);
                        }
                        else if (castingTypeString == "NakedPointer" || castingTypeString == "CodeFunction" || castingTypeString.StartsWith("CodeArray") || castingTypeString.StartsWith("CodePointer"))
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => new {1}(variable.GetField(\"{0}\")));", field.name, castingTypeString);
                        }
                        else
                        {
                            output.WriteLine(indentation + 2, "_{0} = UserMember.Create(() => ({1})variable.GetField(\"{0}\"));", field.name, castingTypeString);
                        }
                    }
                }

                output.WriteLine(indentation + 1, "}}");
            }

            bool firstField = true;
            foreach (var field in fields)
            {
                bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember;
                string fieldTypeString = GetTypeString(field.type, exportedTypes, field.length);

                if (options.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                {
                    if (firstField)
                    {
                        output.WriteLine();
                        firstField = false;
                    }

                    output.WriteLine(indentation + 1, "public {0}{1} {2} {{ get {{ return _{2}.Value; }} }}", isStatic ? "static " : "", fieldTypeString, field.name);
                }
                else
                {
                    output.WriteLine();
                    output.WriteLine(indentation + 1, "public {0}{1} {2}", isStatic ? "static " : "", fieldTypeString, field.name);
                    output.WriteLine(indentation + 1, "{{");
                    output.WriteLine(indentation + 2, "get");
                    output.WriteLine(indentation + 2, "{{");
                    output.WriteLine(indentation + 3, "return _{0}.Value;", field.name);
                    output.WriteLine(indentation + 2, "}}");
                    output.WriteLine(indentation + 1, "}}");
                }
            }

            foreach (var innerType in InnerTypes)
            {
                output.WriteLine();
                innerType.WriteCode(output, error, exportedTypes, options, indentation + 1);
            }

            output.WriteLine(indentation, @"}}");

            if (DeclaredInType == null && !string.IsNullOrEmpty(Namespace))
            {
                indentation--;
                output.WriteLine(indentation, "}}");
            }
        }

        public void SetDeclaredInType(UserType declaredInType)
        {
            DeclaredInType = declaredInType;
            declaredInType.InnerTypes.Add(this);
            foreach (var u in Usings)
            {
                declaredInType.Usings.Add(u);
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

        private static string GetBaseTypeString(TextWriter error, IDiaSymbol type, Dictionary<string, UserType> exportedTypes)
        {
            var baseClasses = type.GetBaseClasses().ToArray();

            if (baseClasses.Length > 1)
            {
                //throw new Exception(string.Format("Multiple inheritance is not supported. Type {0} is inherited from {1}", type.name, string.Join(", ", baseClasses.Select(c => c.name))));
                error.WriteLine(string.Format("Multiple inheritance is not supported, defaulting to 'UserType' as base class. Type {0} is inherited from:\n  {1}", type.name, string.Join("\n  ", baseClasses.Select(c => c.name))));
                return "UserType";
            }

            if (baseClasses.Length == 1)
            {
                type = baseClasses[0];

                UserType userType;

                if (exportedTypes.TryGetValue(type.name, out userType))
                {
                    return userType.FullClassName;
                }

                return GetBaseTypeString(error, type, exportedTypes);
            }

            return "UserType";
        }

        private static string GetTypeString(IDiaSymbol type, Dictionary<string, UserType> exportedTypes, ulong bitLength = 0)
        {
            switch ((SymTagEnum)type.symTag)
            {
                case SymTagEnum.SymTagBaseType:
                    if (bitLength == 1)
                        return "bool";
                    switch ((BasicType)type.baseType)
                    {
                        case BasicType.Bit:
                        case BasicType.Bool:
                            return "bool";
                        case BasicType.Char:
                        case BasicType.WChar:
                            return "char";
                        case BasicType.BSTR:
                            return "string";
                        case BasicType.Void:
                            return "void";
                        case BasicType.Float:
                            return type.length <= 4 ? "float" : "double";
                        case BasicType.Int:
                        case BasicType.Long:
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

                        case BasicType.UInt:
                        case BasicType.ULong:
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

                        case BasicType.Hresult:
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
                        UserType userType;

                        if (exportedTypes.TryGetValue(typeName, out userType))
                        {
                            return userType.FullClassName;
                        }

                        if ((SymTagEnum)type.symTag == SymTagEnum.SymTagEnum)
                        {
                            return "uint";
                        }

                        return "Variable";
                    }

                case SymTagEnum.SymTagArrayType:
                    return "CodeArray<" + GetTypeString(type.type, exportedTypes) + ">";

                case SymTagEnum.SymTagFunctionType:
                    return "CodeFunction";

                default:
                    throw new Exception("Unexpected type tag " + (SymTagEnum)type.symTag);
            }
        }

        private static string GetOriginalTypeString(IDiaSymbol type)
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

                        return GetOriginalTypeString(pointerType) + "*";
                    }

                case SymTagEnum.SymTagUDT:
                case SymTagEnum.SymTagEnum:
                    {
                        return type.name;
                    }

                case SymTagEnum.SymTagFunctionType:
                case SymTagEnum.SymTagArrayType:
                    return GetOriginalTypeString(type.type) + "[]";

                default:
                    throw new Exception("Unexpected type tag " + (SymTagEnum)type.symTag);
            }
        }
    }
}
