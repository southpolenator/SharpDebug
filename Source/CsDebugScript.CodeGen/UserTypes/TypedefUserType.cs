using Dia2Lib;
using System.IO;
using System;
using System.Collections.Generic;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents Typedef.
    /// </summary>
    /// <seealso cref="UserType" />
    internal class TypedefUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="typeNamespace">The namespace it belongs to.</param>
        public TypedefUserType(Symbol symbol, XmlType xmlType, string typeNamespace)
            : base(symbol, xmlType, typeNamespace)
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
            string nameSpace = (DeclaredInType as NamespaceUserType)?.FullClassName ?? Namespace;
            if (!string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(indentation, "namespace {0}", nameSpace);
                output.WriteLine(indentation++, "{{");
            }

            string typedefTypeName = TypeToString.GetTypeString(Symbol.DiaSymbol.type);

            // output.WriteLine(indentation, string.Format("using {0} = {1};", ClassName, typedefTypeName));

            if ((DeclaredInType == null || (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(--indentation, "}}");
            }
        }
    }
}
