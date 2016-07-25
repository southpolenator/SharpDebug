using System.Collections.Generic;
using System.Linq;
using CsDebugScript.CodeGen.UserTypes;
using System.Text;
using Dia2Lib;

namespace CsDebugScript.CodeGen
{
    internal static class GlobalCache
    {
        private static Dictionary<string, Symbol[]> deduplicatedSymbols = new Dictionary<string, Symbol[]>();

        public static Symbol GetSymbol(string typeName, Module module)
        {
            Symbol[] symbols;

            if (deduplicatedSymbols.TryGetValue(typeName, out symbols))
                return symbols[0];
            return module.GetSymbol(typeName);
        }

        public static UserType GetUserType(string typeName, Module module)
        {
            Symbol symbol = GetSymbol(typeName, module);

            return GetUserType(symbol);
        }

        public static UserType GetUserType(Symbol symbol)
        {
            if (symbol != null)
            {
                if (symbol.UserType == null)
                {
                    symbol = GetSymbol(symbol.Name, symbol.Module);
                }

                if (symbol.UserType == null && symbol.Name.EndsWith("*"))
                {
                    // Try to use Pointer
                    symbol = GetSymbol(symbol.Name.Substring(0, symbol.Name.Length - 1), symbol.Module);
                }
                else if (symbol.UserType == null && symbol.Tag == SymTagEnum.SymTagArrayType)
                {
                    symbol = GetSymbol(symbol.ElementType.Name, symbol.Module);
                }
                return symbol.UserType;
            }

            return null;
        }

        internal static void Update(Dictionary<string, Symbol[]> deduplicatedSymbols)
        {
            GlobalCache.deduplicatedSymbols = deduplicatedSymbols;
        }

        /// <summary>
        /// Get list of modules where symbol is located sorted in alphabetical order.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetSymbolModuleNames(Symbol symbol)
        {
            List<string> result = GetSymbolModuleNamesInternal(symbol).ToList();
            result.Sort();

            return result;
        }

        private static IEnumerable<string> GetSymbolModuleNamesInternal(Symbol symbol)
        {
            Symbol[] symbols;

            if (deduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
            {
                foreach (Symbol deduplicatedSymbol in symbols)
                {
                    if (symbol.Size != deduplicatedSymbol.Size)
                    {
                        continue;
                    }

                    yield return deduplicatedSymbol.Module.Name;
                }
            }
            else
            {
                yield return symbol.Module.Name;
            }
        }

        internal static IEnumerable<Symbol> GetSymbolStaticFieldsSymbols(Symbol symbol)
        {
            Symbol[] symbols;

            if (!deduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
                yield return symbol;
            else
                foreach (var s in symbols)
                    foreach (var field in s.Fields)
                        if (field.DataKind == DataKind.StaticMember && field.IsValidStatic)
                        {
                            yield return s;
                            break;
                        }
        }

        internal static IEnumerable<SymbolField> GetSymbolStaticFields(Symbol symbol)
        {
            Symbol[] symbols;

            if (!deduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
                symbols = new Symbol[] { symbol };

            foreach (var s in symbols)
                foreach (var field in s.Fields)
                    if (field.DataKind == DataKind.StaticMember && field.IsValidStatic)
                        yield return field;
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
