using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript.CodeGen
{
    /// <summary>
    /// Helper functions that aid with symbol name string manipulations.
    /// </summary>
    internal static class SymbolNameHelper
    {
        /// <summary>
        /// Determines whether the specified symbol name contains template type.
        /// </summary>
        /// <param name="symbolName">The symbol name.</param>
        public static bool ContainsTemplateType(string symbolName)
        {
            return symbolName.Contains("<") && symbolName.Contains(">") && !symbolName.StartsWith("<");
        }

        /// <summary>
        /// Get list of symbol namespaces separated by '::'. Namespace includes full name.
        /// </summary>
        /// <param name="symbolName">The symbol name.</param>
        /// <returns>List of namespaces</returns>
        public static List<string> GetSymbolNamespaces(string symbolName)
        {
            List<string> namespaces = new List<string>();
            StringBuilder scope = new StringBuilder();
            int templateNestingLevel = 0;
            foreach (char token in symbolName)
            {
                switch (token)
                {
                    case '<':
                        scope.Append(token);
                        ++templateNestingLevel;
                        break;
                    case '>':
                        scope.Append(token);
                        --templateNestingLevel;
                        break;
                    case ':':
                        if (templateNestingLevel == 0)
                        {
                            if (scope.Length > 0)
                                namespaces.Add(scope.ToString());
                            scope.Clear();
                        }
                        else
                            scope.Append(token);
                        break;
                    default:
                        scope.Append(token);
                        break;
                }
            }

            if (scope.Length > 0)
                namespaces.Add(scope.ToString());
            return namespaces;
        }

        /// <summary>
        /// Creates the lookup for single namespace.
        /// </summary>
        /// <param name="namespaceSymbol">The namespace symbol.</param>
        private static string CreateLookupForNamespace(string namespaceSymbol)
        {
            int index = namespaceSymbol.IndexOf("<");

            if (index > 0)
            {
                // Get number of template arguments.
                int argCount = XmlTypeTransformation.GetTemplateArgCount(namespaceSymbol, index);

                // Include number of template arguments in lookup name.
                string lookupSymbol = namespaceSymbol.Substring(0, index) + "<" + string.Empty.PadRight(argCount, ',') + ">";
                return lookupSymbol;
            }

            return namespaceSymbol;
        }

        /// <summary>
        /// Creates the lookup name for the specified symbol.
        /// This helps grouping template symbols together.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        public static string CreateLookupNameForSymbol(Symbol symbol)
        {
            return string.Join("::", symbol.Namespaces.Select(r => SymbolNameHelper.CreateLookupForNamespace(r)));
        }
    }
}
