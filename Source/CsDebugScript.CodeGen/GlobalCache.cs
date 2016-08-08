using System;
using System.Collections.Generic;
using System.Linq;
using CsDebugScript.CodeGen.UserTypes;
using System.Text;
using Dia2Lib;

namespace CsDebugScript.CodeGen
{
    internal static class GlobalCache
    {
        private static readonly Dictionary<string, List<Symbol>> DeduplicatedSymbols = new Dictionary<string, List<Symbol>>();

        private static readonly Dictionary<Tuple<string, Module>, Symbol> SymbolNameModuleCache  = new Dictionary<Tuple<string, Module>, Symbol>();

        public static void UpdateGlobalDeduplicatedSymbols(
            Dictionary<Symbol, List<Symbol>> duplicatedSymbols,
            Symbol symbol)
        {
            List<Symbol> duplicates;
            if (!duplicatedSymbols.TryGetValue(symbol, out duplicates))
            {
                duplicates = new List<Symbol>();
            }

            // update global list
            if (DeduplicatedSymbols.ContainsKey(symbol.Name))
            {
                UpdateSymbolModulesCache(duplicates);

                DeduplicatedSymbols[symbol.Name].AddRange(duplicates);
            }
            else
            {
                UpdateSymbolModulesCache(duplicates);

                DeduplicatedSymbols.Add(symbol.Name, duplicates);
            }
        }

        /// <summary>
        /// Update Symbol Name, Module Cache.
        /// </summary>
        /// <param name="symbols"></param>
        public static void UpdateSymbolModulesCache(IEnumerable<Symbol> symbols)
        {
            Symbol userTypeSymbol = symbols.First();
            string codeTypeName = userTypeSymbol.Name;

            foreach (Symbol symbol in symbols)
            {
                SymbolNameModuleCache.Add(new Tuple<string, Module>(codeTypeName, symbol.Module), userTypeSymbol);
            }
        }

        /// <summary>
        /// Get symbol by name in context of given symbol.
        /// Search in all modules then context symbol is defined or declared.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="contextSymbol"></param>
        /// <returns></returns>
        public static Symbol GetSymbol(string typeName, Symbol contextSymbol)
        {
            return GetSymbolModules(contextSymbol).Select(declaredInModule => GetSymbol(typeName, declaredInModule)).FirstOrDefault(symbol => symbol != null);
        }

        /// <summary>
        /// Get symbol by name for given module.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static Symbol GetSymbol(string typeName, Module module)
        {
            Symbol symbol;

            bool hasValue = SymbolNameModuleCache.TryGetValue(new Tuple<string, Module>(typeName, module), out symbol);

            if (hasValue)
            {
                return symbol;
            }

            symbol = module.GetSymbol(typeName);

            // Cache results.
            SymbolNameModuleCache.Add(new Tuple<string, Module>(typeName, module), symbol);

            return symbol;
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

        /// <summary>
        /// Get list of modules where symbol is located sorted in alphabetical order.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal static IEnumerable<Module> GetSymbolModules(Symbol symbol)
        {
            return GetSymbolModuleInternal(symbol);
        }

        /// <summary>
        /// Get list of modules where symbol is located sorted in alphabetical order.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetSymbolModuleNames(Symbol symbol)
        {
            List<string> result = GetSymbolModuleInternal(symbol).Select(r=>r.Name).ToList();
            result.Sort();

            return result;
        }

        private static IEnumerable<Module> GetSymbolModuleInternal(Symbol symbol)
        {
            List<Symbol> symbols;

            if (DeduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
            {
                foreach (Symbol deduplicatedSymbol in symbols)
                {
                    if (symbol.Size != deduplicatedSymbol.Size)
                    {
                        continue;
                    }

                    yield return deduplicatedSymbol.Module;
                }
            }
            else
            {
                yield return symbol.Module;
            }
        }

        internal static IEnumerable<Symbol> GetSymbolStaticFieldsSymbols(Symbol symbol)
        {
            List<Symbol> symbols;

            if (!DeduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
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

        internal static IEnumerable<Symbol> GetDeduplicatedSymbols(Symbol symbol)
        {
            List<Symbol> dedupSymbols;

            if (DeduplicatedSymbols.TryGetValue(symbol.Name, out dedupSymbols))
            {
                foreach (Symbol dedupSymbol in dedupSymbols)
                {
                    yield return dedupSymbol;
                }
            }
        }

        internal static IEnumerable<SymbolField> GetSymbolStaticFields(Symbol symbol)
        {
            List<Symbol> symbols;

            if (!DeduplicatedSymbols.TryGetValue(symbol.Name, out symbols))
                symbols = new List<Symbol> { symbol };

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
