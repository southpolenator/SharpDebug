using Dia2Lib;
using System.Collections.Generic;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    enum DataKind
    {
        DataIsUnknown,
        DataIsLocal,
        DataIsStaticLocal,
        DataIsParam,
        DataIsObjectPtr,
        DataIsFileStatic,
        DataIsGlobal,
        DataIsMember,
        DataIsStaticMember,
        DataIsConstant
    };

    enum BasicType
    {
        btNoType = 0,
        btVoid = 1,
        btChar = 2,
        btWChar = 3,
        btInt = 6,
        btUInt = 7,
        btFloat = 8,
        btBCD = 9,
        btBool = 10,
        btLong = 13,
        btULong = 14,
        btCurrency = 25,
        btDate = 26,
        btVariant = 27,
        btComplex = 28,
        btBit = 29,
        btBSTR = 30,
        btHresult = 31
    };

    static class DiaHelpers
    {
        public static IEnumerable<IDiaSymbol> Enum(this IDiaEnumSymbols container)
        {
            foreach (IDiaSymbol value in container)
            {
                yield return value;
            }
        }

        public static IDiaSymbol GetChild(this IDiaSymbol symbol, string name, SymTagEnum tag = SymTagEnum.SymTagNull)
        {
            IDiaEnumSymbols symbols;

            symbol.findChildren(tag, name, 0, out symbols);
            return symbols.Enum().FirstOrDefault();
        }

        public static IEnumerable<IDiaSymbol> GetChildren(this IDiaSymbol symbol, SymTagEnum tag = SymTagEnum.SymTagNull)
        {
            IDiaEnumSymbols symbols;

            symbol.findChildren(tag, null, 0, out symbols);
            return symbols.Enum();
        }

        public static IEnumerable<IDiaSymbol> GetBaseClasses(this IDiaSymbol symbol)
        {
            if ((SymTagEnum)symbol.symTag == SymTagEnum.SymTagData)
            {
                symbol = symbol.type;
            }

            if (symbol != null)
            {
                return symbol.GetChildren(SymTagEnum.SymTagBaseClass);
            }

            return new IDiaSymbol[] { };
        }

        public static IEnumerable<IDiaSymbol> GetAllBaseClasses(this IDiaSymbol symbol)
        {
            List<IDiaSymbol> unprocessed = symbol.GetBaseClasses().ToList();

            while (unprocessed.Count > 0)
            {
                List<IDiaSymbol> symbols = unprocessed;

                unprocessed = new List<IDiaSymbol>();
                foreach (var s in symbols)
                {
                    yield return s;
                    unprocessed.AddRange(s.GetBaseClasses());
                }
            }
        }
    }
}
