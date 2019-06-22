using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.Engine;
using DIA;
using System.Collections.Generic;
using System.Text;

namespace CsDebugScript.CodeGen
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    internal static class GlobalCache
    {
        private static Dictionary<string, Symbol[]> deduplicatedSymbols = new Dictionary<string, Symbol[]>();

        public static Symbol GetSymbol(string typeName, SymbolProviders.Module module)
        {
            Symbol[] symbols;

            if (deduplicatedSymbols.TryGetValue(typeName, out symbols))
            {
                return symbols[0];
            }
            return module.GetSymbol(typeName);
        }

        public static UserType GetUserType(string typeName, SymbolProviders.Module module)
        {
            Symbol symbol = GetSymbol(typeName, module);

            return GetUserType(symbol);
        }

        public static UserType GetUserType(Symbol symbol)
        {
            if (symbol != null && (symbol.Tag == CodeTypeTag.Enum || symbol.Tag == CodeTypeTag.Class || symbol.Tag == CodeTypeTag.Structure || symbol.Tag == CodeTypeTag.Union || symbol.Tag == CodeTypeTag.BaseClass))
            {
                if (symbol.UserType == null)
                {
                    symbol = GetSymbol(symbol.Name, symbol.Module) ?? symbol;
                }

                if (symbol.UserType == null && symbol.Name.EndsWith("*"))
                {
                    // Try to use Pointer
                    symbol = GetSymbol(symbol.Name.Substring(0, symbol.Name.Length - 1), symbol.Module) ?? symbol;
                }
                else if (symbol.UserType == null && symbol.Tag == CodeTypeTag.Array)
                {
                    symbol = GetSymbol(symbol.ElementType.Name, symbol.Module) ?? symbol;
                }

                return symbol.UserType;
            }

            return null;
        }

        internal static void Update(Dictionary<string, Symbol[]> deduplicatedSymbols)
        {
            GlobalCache.deduplicatedSymbols = deduplicatedSymbols;
        }

        internal static IEnumerable<string> GetSymbolModuleNames(Symbol symbol)
        {
            Symbol[] symbols;

            if (deduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
            {
                foreach (var s in symbols)
                {
                    if (symbol.Size > 0 && s.Size == 0)
                    {
                        continue;
                    }
                    yield return s.Module.Name;
                }
            }
            else
            {
                yield return symbol.Module.Name;
            }
        }

        internal static List<Symbol> GetSymbolStaticFieldsSymbols(Symbol symbol)
        {
            Symbol[] symbols;

            if (!deduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
                return null;
            else
            {
                List<Symbol> result = null;

                foreach (var s in symbols)
                    foreach (var field in s.Fields)
                        if (field.DataKind == DataKind.StaticMember && field.IsValidStatic)
                        {
                            if (result == null)
                                result = new List<Symbol>();
                            result.Add(s);
                            break;
                        }
                return result;
            }
        }

        internal static IEnumerable<SymbolField> GetSymbolStaticFields(Symbol symbol)
        {
            Symbol[] symbols;

            if (!deduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
            {
                symbols = new Symbol[] { symbol };
            }

            foreach (var s in symbols)
            {
                foreach (var field in s.Fields)
                {
                    if (field.DataKind == DataKind.StaticMember && field.IsValidStatic)
                    {
                        yield return field;
                    }
                }
            }
        }

        public static string GenerateClassCodeTypeInfo(Symbol symbol, string typeName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var module in GetSymbolModuleNames(symbol))
            {
                sb.Append(string.Format("\"{0}!{1}\", ", module, typeName));
            }

            sb.Length -= 2;

            return sb.ToString();
        }
    }
}
