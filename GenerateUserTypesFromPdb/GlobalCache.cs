using System.Collections.Generic;
using GenerateUserTypesFromPdb.UserTypes;

namespace GenerateUserTypesFromPdb
{
    internal static class GlobalCache
    {
        private static Dictionary<string, Symbol[]> deduplicatedSymbols = new Dictionary<string, Symbol[]>();

        public static Symbol GetSymbol(string typeName, Module module)
        {
            Symbol[] symbols;

            if (deduplicatedSymbols.TryGetValue(typeName, out symbols))
                return symbols[0];
            return module.GetTypeSymbol(typeName);
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
                    symbol = GetSymbol(symbol.Name, symbol.Module);

                return symbol.UserType;
            }

            return null;
        }

        internal static void Update(Dictionary<string, Symbol[]> deduplicatedSymbols)
        {
            GlobalCache.deduplicatedSymbols = deduplicatedSymbols;
        }
    };
}
