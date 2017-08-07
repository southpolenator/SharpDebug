using CsDebugScript.Engine.Utility;
using System.Collections.Generic;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Interface represents module (set of symbols) during debugging.
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// The public symbols cache
        /// </summary>
        private SimpleCache<HashSet<string>> publicSymbols;

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        public Module()
        {
            publicSymbols = SimpleCache.Create(() => new HashSet<string>(GetPublicSymbols()));
        }

        /// <summary>
        /// Gets the set of public symbols.
        /// </summary>
        public HashSet<string> PublicSymbols
        {
            get
            {
                return publicSymbols.Value;
            }
        }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        public string Namespace { get; protected set; }

        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        public abstract Symbol GlobalScope { get; }

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        public abstract Symbol[] FindGlobalTypeWildcard(string nameWildcard);

        /// <summary>
        /// Gets all types defined in the symbol.
        /// </summary>
        public abstract IEnumerable<Symbol> GetAllTypes();

        /// <summary>
        /// Gets the symbol by name from the cache.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>Symbol if found; otherwise null.</returns>
        public abstract Symbol GetSymbol(string name);

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        protected abstract IEnumerable<string> GetPublicSymbols();
    }
}
