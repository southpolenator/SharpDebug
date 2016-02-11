using System;

namespace Dia2Lib
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

                            return GetTypeString(pointerType) + "*";
                        }

                    case SymTagEnum.SymTagBaseClass:
                    case SymTagEnum.SymTagUDT:
                    case SymTagEnum.SymTagEnum:
                        {
                            return type.name;
                        }

                    case SymTagEnum.SymTagFunctionType:
                    case SymTagEnum.SymTagArrayType:
                        return GetTypeString(type.type) + "[]";

                    default:
                        throw new Exception("Unexpected type tag " + (SymTagEnum)type.symTag);
                }
            }
        }

        /// <summary>
        /// Gets the code of the specified type in original language.
        /// </summary>
        /// <param name="type">The type.</param>
        public static string GetTypeString(IDiaSymbol type)
        {
            switch ((CV_CFL_LANG)type.language)
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
