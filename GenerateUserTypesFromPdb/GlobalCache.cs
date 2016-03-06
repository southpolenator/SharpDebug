using GenerateUserTypesFromPdb.UserTypes;

namespace GenerateUserTypesFromPdb
{
    internal static class GlobalCache
    {
        public static Symbol GetSymbol(string typeName, Module module)
        {
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
    };
}
