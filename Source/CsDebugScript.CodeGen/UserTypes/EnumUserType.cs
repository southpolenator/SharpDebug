using CsDebugScript.CodeGen.SymbolProviders;
using DIA;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents Enum.
    /// </summary>
    /// <seealso cref="UserType" />
    internal class EnumUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        public EnumUserType(Symbol symbol, string nameSpace)
            : base(symbol, null, nameSpace)
        {
        }

        /// <summary>
        /// Writes the code for this user type to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="error">The error text writer.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <param name="indentation">The current indentation.</param>
        public override void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags generationFlags, int indentation = 0)
        {
            // Check if we need to write namespace
            string nameSpace = (DeclaredInType as NamespaceUserType)?.FullClassName ?? Namespace;
            string enumBasicType = GetEnumBasicType(Symbol);

            if ((DeclaredInType == null || (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(indentation, "namespace {0}", nameSpace);
                output.WriteLine(indentation++, "{{");
            }

            // Write beginning of the enumeration
            if (generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
            {
                output.WriteLine(indentation, "// {0} (original name: {1})", ClassName, Symbol.Name);
            }

            if (AreValuesFlags())
            {
                output.WriteLine(indentation, @"[System.Flags]");
            }
            if (Symbol.Size != 0)
            {
                output.WriteLine(indentation, @"public enum {0} : {1}", ClassName, enumBasicType);
            }
            else
            {
                output.WriteLine(indentation, @"public enum {0}", ClassName);
            }
            output.WriteLine(indentation++, @"{{");

            // Write values
            foreach (var enumValue in Symbol.EnumValues)
            {
                string value = enumValue.Item2;

                if (!FitsBasicType(enumBasicType, ref value))
                {
                    output.WriteLine(indentation, "{0} = ({1}){2},", enumValue.Item1, enumBasicType, value);
                }
                else
                {
                    output.WriteLine(indentation, "{0} = {1},", enumValue.Item1, value);
                }
            }

            // Enumeration end
            output.WriteLine(--indentation, @"}}");
            if ((DeclaredInType == null || (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(--indentation, "}}");
            }
        }

        /// <summary>
        /// Checks whether value can be stored inside the specified enumeration basic type
        /// </summary>
        /// <param name="enumBasicType">Enumeration basic type</param>
        /// <param name="value">Value of the element</param>
        /// <returns><c>true</c> if no cast is needed to store value; <c>false</c> otherwise.</returns>
        private static bool FitsBasicType(string enumBasicType, ref string value)
        {
            ulong ulongValue;

            switch (enumBasicType)
            {
                case null:
                    return true;
                case "sbyte":
                    {
                        if (sbyte.TryParse(value, out sbyte unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((sbyte)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "byte":
                    {
                        if (byte.TryParse(value, out byte unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((byte)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "short":
                    {
                        if (short.TryParse(value, out short unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((short)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "ushort":
                    {
                        if (ushort.TryParse(value, out ushort unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((ushort)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "int":
                    {
                        if (int.TryParse(value, out int unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((int)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "uint":
                    {
                        if (uint.TryParse(value, out uint unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((uint)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "long":
                    {
                        if (long.TryParse(value, out long unused))
                        {
                            return true;
                        }
                        if (ulong.TryParse(value, out ulongValue))
                        {
                            value = ((long)ulongValue).ToString();
                            return true;
                        }
                        return false;
                    }
                case "ulong":
                    {
                        return ulong.TryParse(value, out ulong unused);
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified number is power of two.
        /// </summary>
        /// <param name="x">The number.</param>
        private static bool IsPowerOfTwo(long x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Checks whether values inside the enumeration are flags.
        /// </summary>
        private bool AreValuesFlags()
        {
            try
            {
                SortedSet<long> values = new SortedSet<long>();

                foreach (var enumValue in Symbol.EnumValues)
                {
                    values.Add(long.Parse(enumValue.Item2));
                }

                foreach (var value in values)
                {
                    if (!IsPowerOfTwo(value))
                    {
                        return false;
                    }
                }
                if (values.Count < 2 || (values.Contains(0) && values.Contains(1) && values.Count == 2)
                    || (values.Contains(0) && values.Contains(1) && values.Contains(2) && values.Count == 3))
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// Gets the basic type string for the specified enumeration symbol.
        /// </summary>
        /// <param name="symbol">The enumeration symbol.</param>
        internal static string GetEnumBasicType(Symbol symbol)
        {
            switch (symbol.BasicType)
            {
                case BasicType.Int:
                case BasicType.Long:
                    switch (symbol.Size)
                    {
                        case 8:
                            return "long";
                        case 4:
                            return "int";
                        case 2:
                            return "short";
                        case 1:
                            return "sbyte";
                        case 0:
                            return string.Empty;
                        default:
                            break;
                    }
                    break;

                case BasicType.UInt:
                case BasicType.ULong:
                    switch (symbol.Size)
                    {
                        case 8:
                            return "ulong";
                        case 4:
                            return "uint";
                        case 2:
                            return "ushort";
                        case 1:
                            return "byte";
                        case 0:
                            return string.Empty;
                        default:
                            break;
                    }
                    break;
                case BasicType.Char:
                    return "sbyte";

                default:
                    break;
            }

            throw new InvalidDataException("Unknown enum type.");
        }

        /// <summary>
        /// Gets the full name of the class, including namespace and "parent" type it is declared into.
        /// </summary>
        public override string FullClassName
        {
            get
            {
                if (DeclaredInType is TemplateUserType)
                {
                    // Enum cannot be instantiated in generic type.
                    // We must choose template specialization - first on the list.
                    TemplateUserType declaredInTemplateUserType = (DeclaredInType as TemplateUserType);
                    string declaredInSpecializedType = declaredInTemplateUserType.GetSpecializedStringVersion();

                    return string.Format("{0}.{1}", declaredInSpecializedType, ClassName);
                }
                else
                {
                    return base.FullClassName;
                }
            }
        }
    }
}
