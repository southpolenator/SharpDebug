using CsDebugScript.CodeGen.SymbolProviders;
using Dia2Lib;
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
                output.WriteLine(indentation, @"public enum {0} : {1}", ClassName, GetEnumBasicType(Symbol));
            }
            else
            {
                output.WriteLine(indentation, @"public enum {0}", ClassName);
            }
            output.WriteLine(indentation++, @"{{");

            // Write values
            foreach (var enumValue in Symbol.EnumValues)
            {
                output.WriteLine(indentation, "{0} = {1},", enumValue.Item1, enumValue.Item2);
            }

            // Enumeration end
            output.WriteLine(--indentation, @"}}");
            if ((DeclaredInType == null || (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(--indentation, "}}");
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
