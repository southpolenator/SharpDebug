using Dia2Lib;
using System.IO;
using System;
using System.Collections.Generic;

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

        public override void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            string nameSpace = (DeclaredInType as NamespaceUserType)?.FullClassName ?? Namespace;

            if ((DeclaredInType == null || (!options.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(indentation, "namespace {0}", nameSpace);
                output.WriteLine(indentation++, "{{");
            }

            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                output.WriteLine(indentation, "// {0} (original name: {1})", ClassName, Symbol.Name);

            if (AreValuesFlags())
                output.WriteLine(indentation, @"[System.Flags]");
            if (Symbol.Size != 0)
                output.WriteLine(indentation, @"public enum {0} : {1}", ClassName, GetEnumType());
            else
                output.WriteLine(indentation, @"public enum {0}", ClassName);
            output.WriteLine(indentation++, @"{{");

            foreach (var enumValue in Symbol.GetEnumValues())
            {
                output.WriteLine(indentation, "{0} = {1},", enumValue.Item1, enumValue.Item2);
            }

            // Class end
            output.WriteLine(--indentation, @"}}");

            if ((DeclaredInType == null || (!options.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(--indentation, "}}");
            }
        }

        private static bool IsPowerOfTwo(long x)
        {
            return (x & (x - 1)) == 0;
        }

        private bool AreValuesFlags()
        {
            try
            {
                SortedSet<long> values = new SortedSet<long>();

                foreach (var enumValue in Symbol.GetEnumValues())
                    values.Add(long.Parse(enumValue.Item2));

                foreach (var value in values)
                    if (!IsPowerOfTwo(value))
                        return false;
                if (values.Count < 2 || (values.Contains(0) && values.Contains(1) && values.Count == 2)
                    || (values.Contains(0) && values.Contains(1) && values.Contains(2) && values.Count == 3))
                    return false;
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        private string GetEnumType()
        {
            switch (Symbol.BasicType)
            {
                case BasicType.Int:
                case BasicType.Long:
                    switch (Symbol.Size)
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
                    switch (Symbol.Size)
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

                default:
                    break;
            }

            throw new InvalidDataException("Unknown enum type.");
        }

        /// <summary>
        /// Full Class Name.
        /// Handle special logic for enums embedded in template types.
        /// </summary>
        public override string FullClassName
        {
            get
            {
                if (DeclaredInType as TemplateUserType != null)
                {
                    // Enum cannot be instantiated in generic type.
                    // We must choose template specialization - first on the list.
                    //
                    TemplateUserType declaredInTemplateUserType = (DeclaredInType as TemplateUserType);

                    string declaredInSpecializedType = declaredInTemplateUserType.GetSpecializedTypeDefinedInstance();

                    if (declaredInSpecializedType.Contains("SequencedObject"))
                    {

                    }

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
