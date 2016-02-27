using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb.UserTypes
{
    class EnumUserType : UserType
    {
        public EnumUserType(IDiaSymbol symbol, string moduleName)
            : base(symbol, new XmlType() { Name = symbol.name }, moduleName)
        {
        }

        public override void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            if (DeclaredInType == null)
            {
                // always make module namespace
                if (Namespace != null)
                {
                    output.WriteLine(indentation, "namespace {0}.{1}", ModuleName, Namespace);
                }
                else
                {
                    output.WriteLine(indentation, "namespace {0}", ModuleName);
                }
                output.WriteLine(indentation++, "{{");
            }

            output.WriteLine(indentation, @"public enum {0} {1}", ClassName, (Symbol.length != 0) ? " : " + GetEnumType() : string.Empty);
            output.WriteLine(indentation++, @"{{");

            foreach (var enumValue in Symbol.GetChildren())
            {
                output.WriteLine(indentation, "{0} = {1},", enumValue.name, enumValue.value);
            }

            // Class end
            output.WriteLine(--indentation, @"}}");

            if (DeclaredInType == null)
            {
                output.WriteLine(--indentation, "}}");
            }
        }

        public override string ClassName
        {
            get
            {
                string className = base.ClassName;

                return className;
            }
        }

        public string GetEnumType()
        {
            switch ((BasicType)Symbol.baseType)
            {
                case BasicType.Int:
                case BasicType.Long:
                    switch (Symbol.length)
                    {
                        case 8:
                            return "long";
                        case 4:
                            return "int";
                        case 2:
                            return "short";
                        case 1:
                            return "char";
                        case 0:
                            return string.Empty;
                        default:
                            break;
                    }
                    break;

                case BasicType.UInt:
                case BasicType.ULong:
                    switch (Symbol.length)
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
