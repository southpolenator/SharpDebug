using System.Collections.Generic;
using System.Linq;

namespace Dia2Lib
{
    /// <summary>
    /// Collection of extension methods that makes using DIA lib easier.
    /// </summary>
    public static class DiaHelpers
    {
        /// <summary>
        /// Converts IDiaEnumSymbols container to <see cref="IEnumerable{IDiaSymbol}"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        public static IEnumerable<IDiaSymbol> Enum(this IDiaEnumSymbols container)
        {
            foreach (IDiaSymbol value in container)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Gets the child symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="name">The name.</param>
        /// <param name="tag">The tag.</param>
        public static IDiaSymbol GetChild(this IDiaSymbol symbol, string name, SymTagEnum tag = SymTagEnum.SymTagNull)
        {
            IDiaEnumSymbols symbols;

            symbol.findChildren(tag, name, 0, out symbols);
            return symbols.Enum().FirstOrDefault();
        }


        /// <summary>
        /// Gets the children using wildcard search: Applies a case-sensitive name match using asterisks (*) and question marks (?) as wildcards.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="nameWildcard">The name wildcard.</param>
        /// <param name="tag">The tag.</param>
        public static IEnumerable<IDiaSymbol> GetChildrenWildcard(this IDiaSymbol symbol, string nameWildcard, SymTagEnum tag = SymTagEnum.SymTagNull)
        {
            IDiaEnumSymbols symbols;
            const uint nsfRegularExpression = 0x8; // https://msdn.microsoft.com/en-us/library/yat28ads.aspx

            symbol.findChildren(tag, nameWildcard, nsfRegularExpression, out symbols);
            return symbols.Enum();
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="tag">The tag.</param>
        public static IEnumerable<IDiaSymbol> GetChildren(this IDiaSymbol symbol, SymTagEnum tag = SymTagEnum.SymTagNull)
        {
            IDiaEnumSymbols symbols;

            symbol.findChildren(tag, null, 0, out symbols);
            return symbols.Enum();
        }

        /// <summary>
        /// Gets the base classes.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
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

        /// <summary>
        /// Gets all base classes (including base classes of base classes).
        /// </summary>
        /// <param name="symbol">The symbol.</param>
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
