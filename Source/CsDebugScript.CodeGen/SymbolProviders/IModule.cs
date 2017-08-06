using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    internal interface IModule
    {
        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        ISymbol GlobalScope { get; }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the set of public symbols.
        /// </summary>
        HashSet<string> PublicSymbols { get; }

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        ISymbol[] FindGlobalTypeWildcard(string nameWildcard);

        /// <summary>
        /// Gets all types defined in the symbol.
        /// </summary>
        IEnumerable<ISymbol> GetAllTypes();

        /// <summary>
        /// Gets the symbol by name from the cache.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>Symbol if found; otherwise null.</returns>
        ISymbol GetSymbol(string name);
    }
}
