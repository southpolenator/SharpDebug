using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb.UserTypes
{
    internal class NamespaceUserType : UserType
    {
        internal NamespaceUserType(string namespaceSymbol, string moduleName)
            : base(null /*diaSymbol*/, null /* xmlType */, moduleName)
        {
            NamespaceSymbol = namespaceSymbol;
        }

        /// <summary>
        /// Write Code
        /// </summary>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <param name="indentation"></param>
        public override void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            Debug.Assert(DeclaredInType != null, "NamespaceUserType must be used inside other type.");

            //
            //  TODO unnecessary split
            //
            string[] innerNamespaces = Namespace.Split('.');

            // Declared In Type with namespace
            foreach (string innerClass in innerNamespaces)
            {
                output.WriteLine(indentation, "public static class {0}", innerClass);
                output.WriteLine(indentation++, @"{{");
            }

            // Inner types
            foreach (var innerType in InnerTypes)
            {
                output.WriteLine();
                innerType.WriteCode(output, error, factory, options, indentation);
            }

            // Declared In Type with namespace
            foreach (string innerClass in innerNamespaces)
            {
                output.WriteLine(--indentation, "}}");
            }
        }

        public override string FullClassName
        {
            get
            {
                Debug.Assert(DeclaredInType != null, "NamespaceUserType must be used inside other type.");

                return string.Format("{0}.{1}", DeclaredInType.FullClassName, Namespace);
            }
        }
    }
}
