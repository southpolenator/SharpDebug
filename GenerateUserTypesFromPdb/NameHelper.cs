using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    internal static class NameHelper
    {
        public static bool IsTemplateType(Symbol diaSymbol)
        {
            string symbolName = diaSymbol.Name;
            return IsTemplateType(symbolName);
        }

        public static bool HasNamespace(string symbolName)
        {
            return symbolName.Contains("::");
        }

        public static bool IsTemplateType(string symbolName)
        {
            symbolName = NameHelper.GetFullSymbolNamespaces(symbolName).Last();
            return symbolName.Contains("<") && symbolName.EndsWith(">") && !symbolName.StartsWith("<");
        }

        public static bool ContainsTemplateType(string symbolName)
        {
            return symbolName.Contains("<") && symbolName.Contains(">") && !symbolName.StartsWith("<");
        }

        public static string GetLookupForNamespace(string namespaceSymbol)
        {
            int index = namespaceSymbol.IndexOf("<");

            if (index > 0)
            {
                namespaceSymbol = namespaceSymbol.Substring(0, index) + "<>";
            }

            return namespaceSymbol;
        }


        public static string GetLookupNameForSymbol(string symbolName)
        {
            if (string.IsNullOrEmpty(symbolName))
            {
                return string.Empty;
            }

            List<string> namespaces = NameHelper.GetFullSymbolNamespaces(symbolName);

            return string.Join("::", namespaces.Select(r => NameHelper.GetLookupForNamespace(r)));
        }

        public static string GetLookupNameForSymbol(Symbol diaSymbol)
        {
            return GetLookupNameForSymbol(diaSymbol.Name);
        }

        public static string GetSimpleLookupNameForSymbol(Symbol diaSymbol)
        {
            string symbolName = diaSymbol.Name;

            if (string.IsNullOrEmpty(symbolName))
            {
                return string.Empty;
            }

            return symbolName;
        }

        public static List<string> GetSymbolNamespaces(string symbolName)
        {
            List<string> namespaces = new List<string>();

            string scope = string.Empty;
            int templateNestingLevel = 0;
            foreach (char token in symbolName)
            {
                switch (token)
                {
                    case '<':
                        ++templateNestingLevel;
                        break;
                    case '>':
                        if (--templateNestingLevel == 0)
                        {
                            scope += "<>";
                        }
                        break;
                    case ':':
                        if (templateNestingLevel == 0)
                        {
                            if (!string.IsNullOrEmpty(scope))
                            {
                                namespaces.Add(scope);
                            }
                            scope = string.Empty;
                        }
                        break;
                    default:
                        if (templateNestingLevel == 0)
                        {
                            scope += token;
                        }
                        break;
                }
            }

            return namespaces;
        }

        public static string GetSymbolScopedClassName(string symbolName)
        {
            List<string> namespaces = new List<string>();

            string scope = string.Empty;
            int templateNestingLevel = 0;
            foreach (char token in symbolName)
            {
                switch (token)
                {
                    case '<':
                        ++templateNestingLevel;
                        break;
                    case '>':
                        if (--templateNestingLevel == 0)
                        {
                            scope += "<>";
                        }
                        break;
                    case ':':
                        if (templateNestingLevel == 0)
                        {
                            if (!string.IsNullOrEmpty(scope))
                            {
                                namespaces.Add(scope);
                            }
                            scope = string.Empty;
                        }
                        break;
                    default:
                        if (templateNestingLevel == 0)
                        {
                            scope += token;
                        }
                        break;
                }
            }

            return scope;
        }



        public static string NamespacesToString(IEnumerable<string> namespaces)
        {
            if (namespaces.Any() == false)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            foreach (string ns in namespaces)
            {
                sb.Append(ns);
                sb.Append("::");
            }

            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
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

            string scope = string.Empty;
            int templateNestingLevel = 0;
            foreach (char token in symbolName)
            {
                switch (token)
                {
                    case '<':
                        scope += token;
                        ++templateNestingLevel;
                        break;
                    case '>':
                        scope += token;
                        --templateNestingLevel;
                        break;
                    case ':':
                        if (templateNestingLevel == 0)
                        {
                            if (!string.IsNullOrEmpty(scope))
                            {
                                namespaces.Add(scope);
                            }
                            scope = string.Empty;
                        }
                        else
                        {
                            scope += token;
                        }
                        break;
                    default:
                        scope += token;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(scope))
            {
                namespaces.Add(scope);
            }

            return namespaces;
        }

        /// <summary>
        /// Get T
        /// </summary>
        /// <param name="symbolName"></param>
        /// <returns></returns>
        public static List<string> GetTemplateSpecializationArguments(string symbolName)
        {
            List<string> specializations = new List<string>();

            string scope = string.Empty;
            int templateNestingLevel = 0;
            foreach (char token in symbolName)
            {
                switch (token)
                {
                    case '<':
                        if (templateNestingLevel > 0)
                        {
                            scope += token;
                        }
                        ++templateNestingLevel;
                        break;
                    case '>':
                        --templateNestingLevel;

                        if (templateNestingLevel > 0)
                        {
                            scope += token;
                        }
                        break;
                    case ',':
                        if (templateNestingLevel == 1)
                        {
                            if (!string.IsNullOrEmpty(scope))
                            {
                                specializations.Add(scope.Trim());
                            }
                            scope = string.Empty;
                        }
                        else
                        {
                            scope += token;
                        }
                        break;
                    default:
                        if (templateNestingLevel > 0)
                        {
                            scope += token;
                        }
                        break;
                }
            }

            if (!string.IsNullOrEmpty(scope))
            {
                specializations.Add(scope.Trim());
            }

            return specializations;
        }
    }
}
