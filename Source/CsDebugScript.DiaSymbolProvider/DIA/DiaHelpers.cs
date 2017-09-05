using CsDebugScript.Engine;
using System.Collections.Generic;
using System.Linq;

namespace DIA
{
    /// <summary>
    /// Collection of extension methods that makes using DIA lib easier.
    /// </summary>
    public static class DiaHelpers
    {
        /// <summary>
        /// Converts <see cref="IDiaEnumSymbols"/> container to <see cref="IEnumerable{IDiaSymbol}"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        public static IEnumerable<IDiaSymbol> Enum(this IDiaEnumSymbols container)
        {
            foreach (IDiaSymbol symbol in container)
            {
                yield return symbol;
            }
        }

        /// <summary>
        /// Converts <see cref="IDiaEnumLineNumbers"/> container to <see cref="IEnumerable{IDiaLineNumber}"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        public static IEnumerable<IDiaLineNumber> Enum(this IDiaEnumLineNumbers container)
        {
            foreach (IDiaLineNumber lineNumber in container)
            {
                yield return lineNumber;
            }
        }

        /// <summary>
        /// Gets the child symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="name">The name.</param>
        /// <param name="tag">The tag.</param>
        public static IDiaSymbol GetChild(this IDiaSymbol symbol, string name, SymTagEnum tag = SymTagEnum.Null)
        {
            IDiaEnumSymbols symbols = symbol.findChildren(tag, name, NameSearchOptions.None);

            return symbols.Enum().FirstOrDefault();
        }

        /// <summary>
        /// Gets the children using wildcard search: Applies a case-sensitive name match using asterisks (*) and question marks (?) as wildcards.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="nameWildcard">The name wildcard.</param>
        /// <param name="tag">The tag.</param>
        public static IEnumerable<IDiaSymbol> GetChildrenWildcard(this IDiaSymbol symbol, string nameWildcard, SymTagEnum tag = SymTagEnum.Null)
        {
            IDiaEnumSymbols symbols = symbol.findChildren(tag, nameWildcard, NameSearchOptions.RegularExpression);

            return symbols.Enum();
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="tag">The tag.</param>
        public static IEnumerable<IDiaSymbol> GetChildren(this IDiaSymbol symbol, SymTagEnum tag = SymTagEnum.Null)
        {
            IDiaEnumSymbols symbols = symbol.findChildren(tag, null, NameSearchOptions.None);

            return symbols.Enum();
        }

        /// <summary>
        /// Gets the base classes.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        public static IEnumerable<IDiaSymbol> GetBaseClasses(this IDiaSymbol symbol)
        {
            if (symbol.symTag == SymTagEnum.Data)
            {
                symbol = symbol.type;
            }

            if (symbol != null)
            {
                return symbol.GetChildren(SymTagEnum.BaseClass);
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

    /// <summary>
    /// Extension methods for <see cref="SymTagEnum"/>
    /// </summary>
    public static class SymTagExtensions
    {
        /// <summary>
        /// Converts <see cref="SymTagEnum"/> to <see cref="CodeTypeTag"/>.
        /// </summary>
        public static CodeTypeTag ToCodeTypeTag(this SymTagEnum tag)
        {
            switch (tag)
            {
                case SymTagEnum.ArrayType:
                    return CodeTypeTag.Array;
                case SymTagEnum.BaseType:
                    return CodeTypeTag.BuiltinType;
                case SymTagEnum.UDT:
                    // TODO: What about Structure/Union? IDiaSymbol.udtKind might help...
                    return CodeTypeTag.Class;
                case SymTagEnum.Enum:
                    return CodeTypeTag.Enum;
                case SymTagEnum.FunctionType:
                    return CodeTypeTag.Function;
                case SymTagEnum.PointerType:
                    return CodeTypeTag.Pointer;
                case SymTagEnum.BaseClass:
                    return CodeTypeTag.BaseClass;
                case SymTagEnum.Exe:
                    return CodeTypeTag.ModuleGlobals;
                default:
                    return CodeTypeTag.Unsupported;
            }
        }
    }
}
