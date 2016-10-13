using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class that represents namespace for the user types
    /// </summary>
    /// <seealso cref="UserType" />
    internal class NamespaceUserType : UserType
    {
        /// <summary>
        /// The list of namespaces represented by this instance
        /// </summary>
        private string[] namespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceUserType"/> class.
        /// </summary>
        /// <param name="namespaces">The namespaces.</param>
        /// <param name="nameSpace">The namespace.</param>
        internal NamespaceUserType(IEnumerable<string> namespaces, string nameSpace)
            : base(symbol: null, xmlType: null, nameSpace: null)
        {
            this.namespaces = namespaces.Select(s => NormalizeSymbolNamespace(s)).ToArray();
            NamespaceSymbol = string.Join(".", this.namespaces);
            if (!string.IsNullOrEmpty(nameSpace))
                NamespaceSymbol = nameSpace + "." + NamespaceSymbol;
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
                innerType.WriteCode(output, error, factory, generationFlags, indentation);
            }

            // Declared In Type with namespace
            if (DeclaredInType != null)
                foreach (string innerClass in namespaces)
                    output.WriteLine(--indentation, "}}");
            else
                output.WriteLine(--indentation, "}}");
        }

        /// <summary>
        /// Gets the class name for this user type. Class name doesn't contain namespace.
        /// </summary>
        public override string OriginalClassName
        {
            get
            {
                return Namespace;
            }
        }

        /// <summary>
        /// Gets the full name of the class, including namespace and "parent" type it is declared into.
        /// </summary>
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

        /// <summary>
        /// Gets the full name of the class (specialized version), including namespace and "parent" type it is declared into.
        /// This specialized version of FullClassName returns it with original specialization.
        /// </summary>
        internal override string SpecializedFullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.SpecializedFullClassName, Namespace);
                }

                return string.Format("{0}", Namespace);
            }
        }

        /// <summary>
        /// Gets the full name of the class (non-specialized version), including namespace and "parent" type it is declared into.
        /// This non-specialized version of FullClassName returns it with template being trimmed to just &lt;&gt;.
        /// </summary>
        internal override string NonSpecializedFullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.NonSpecializedFullClassName, Namespace);
                }

                return string.Format("{0}", Namespace);
            }
        }
    }
}
