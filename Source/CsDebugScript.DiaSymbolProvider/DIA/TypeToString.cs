using System;
using System.Text;

namespace DIA
{
    /// <summary>
    /// Exposes converted from IDiaSymbol type to code text
    /// </summary>
    public static class TypeToString
    {
        /// <summary>
        /// C/C++ code type generator
        /// </summary>
        public static class CppType
        {
            /// <summary>
            /// Gets the C/C++ code of the specified type.
            /// </summary>
            /// <param name="type">The type.</param>
            public static string GetTypeString(IDiaSymbol type)
            {
                switch (type.symTag)
                {
                    case SymTagEnum.BaseType:
                        switch (type.baseType)
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
                            case BasicType.NoType:
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

                            case BasicType.Char16:
                                return "char16_t";

                            case BasicType.Char32:
                                return "char32_t";

                            default:
                                throw new Exception("Unexpected basic type " + type.baseType);
                        }

                    case SymTagEnum.PointerType:
                        {
                            IDiaSymbol pointerType = type.type;

                            return GetTypeString(pointerType) + "*";
                        }

                    case SymTagEnum.BaseClass:
                    case SymTagEnum.UDT:
                    case SymTagEnum.Enum:
                    case SymTagEnum.VTable:
                    case SymTagEnum.VTableShape:
                        return type.name ?? "";

                    case SymTagEnum.FunctionType:
                        {
                            StringBuilder sb = new StringBuilder();
                            var arguments = type.GetChildren(SymTagEnum.FunctionArgType);
                            bool first = true;

                            sb.Append(GetTypeString(type.type));
                            sb.Append("(");
                            foreach (var argument in arguments)
                            {
                                if (first)
                                    first = false;
                                else
                                    sb.Append(",");
                                sb.Append(GetTypeString(argument.type));
                            }
                            sb.Append(")");
                            return sb.ToString();
                        }

                    case SymTagEnum.ArrayType:
                        return GetTypeString(type.type) + "[]";

                    default:
                        throw new Exception("Unexpected type tag " + type.symTag);
                }
            }
        }

        /// <summary>
        /// Gets the code of the specified type in original language.
        /// </summary>
        /// <param name="type">The type.</param>
        public static string GetTypeString(IDiaSymbol type)
        {
            switch (type.language)
            {
                case CV_CFL_LANG.CV_CFL_C:
                case CV_CFL_LANG.CV_CFL_CXX:
                    return CppType.GetTypeString(type);
                default:
                    throw new Exception("Unsupported language");
            }
        }
    }
}
