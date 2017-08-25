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
        /// Determines whether the specified text is constant.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if the specified text is constant; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsConstant(string text)
        {
            if (text.StartsWith("("))
            {
                int index = text.LastIndexOf(')');

                if (index == -1)
                {
                    return false;
                }

                string castingType = text.Substring(1, index - 1);

                if (GetSymbol(castingType) == null)
                {
                    return false;
                }

                text = text.Substring(index + 1);
            }

            if (text.EndsWith("u"))
            {
                text = text.Substring(0, text.Length - 1);
            }
            else if (text.EndsWith("ul"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            else if (text.EndsWith("ull"))
            {
                text = text.Substring(0, text.Length - 3);
            }
            else if (text.EndsWith("ll"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            bool boolConstant;
            double numberConstant;

            return double.TryParse(text, out numberConstant) || bool.TryParse(text, out boolConstant);
        }

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        protected abstract IEnumerable<string> GetPublicSymbols();
    }
}
