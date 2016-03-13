using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    internal static class NameHelper
    {
        public static bool ContainsTemplateType(string symbolName)
        {
            return symbolName.Contains("<") && symbolName.Contains(">") && !symbolName.StartsWith("<");
        }

        private static string GetLookupForNamespace(string namespaceSymbol)
        {
            int index = namespaceSymbol.IndexOf("<");

            if (index > 0)
            {
                namespaceSymbol = namespaceSymbol.Substring(0, index) + "<>";
            }

            return namespaceSymbol;
        }

        public static string GetLookupNameForSymbol(Symbol symbol)
        {
            return string.Join("::", symbol.Namespaces.Select(r => NameHelper.GetLookupForNamespace(r)));
        }

        /// <summary>
        /// Get list of full symbol namespaces
        /// Separated by ::
        /// Namespace includes full name.
        /// </summary>
        /// <param name="symbolName"></param>
        /// <returns></returns>
        public static List<string> GetFullSymbolNamespaces(string symbolName)
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
    }
}
