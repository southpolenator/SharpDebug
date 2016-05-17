using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript.CodeGen.UserTypes
{
    internal class NamespaceUserType : UserType
    {
        private readonly string[] namespaces;

        internal NamespaceUserType(IEnumerable<string> namespaces, string nameSpace)
            : base(symbol: null, xmlType: null, nameSpace: null)
        {
            this.namespaces = namespaces.Select(NormalizeSymbolName).ToArray();
            NamespaceSymbol = string.Join(".", this.namespaces);
            if (!string.IsNullOrEmpty(nameSpace))
                NamespaceSymbol = nameSpace + "." + NamespaceSymbol;
        }

        public override void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            // Declared In Type with namespace
            if (DeclaredInType != null)
            {
                foreach (string innerClass in namespaces)
                {
                    output.WriteLine(indentation, "public static class {0}", innerClass);
                    output.WriteLine(indentation++, @"{{");
                }
            }
            else
            {
                output.WriteLine(indentation, "namespace {0}", Namespace);
                output.WriteLine(indentation++, @"{{");
            }

            // Inner types
            foreach (var innerType in InnerTypes)
            {
                output.WriteLine();
                innerType.WriteCode(output, error, factory, options, indentation);
            }

            // Declared In Type with namespace
            if (DeclaredInType != null)
                foreach (string innerClass in namespaces)
                    output.WriteLine(--indentation, "}}");
            else
                output.WriteLine(--indentation, "}}");
        }

        public override string ClassName
        {
            get
            {
                return Namespace;
            }
        }

        public override string FullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.FullClassName, Namespace);
                }

                return string.Format("{0}", Namespace);
            }
        }
    }
}
