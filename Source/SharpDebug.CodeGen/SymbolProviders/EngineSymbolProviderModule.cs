using SharpDebug.Engine;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents module during debugging. It is being described by engine <see cref="ISymbolProviderModule"/>.
    /// </summary>
    public class EngineSymbolProviderModule : Module
    {
        /// <summary>
        /// The cache of symbols by identifier
        /// </summary>
        private DictionaryCache<uint, Symbol> symbolsById;

        /// <summary>
        /// The cache of symbols by name
        /// </summary>
        private DictionaryCache<string, Symbol> symbolsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineSymbolProviderModule"/> class.
        /// </summary>
        /// <param name="module">The engine module.</param>
        /// <param name="xmlModule">The XML module description.</param>
        public EngineSymbolProviderModule(SharpDebug.Module module, XmlModule xmlModule)
            : this(module, xmlModule, Context.SymbolProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineSymbolProviderModule" /> class.
        /// </summary>
        /// <param name="module">The engine module.</param>
        /// <param name="xmlModule">The XML module description.</param>
        /// <param name="symbolProvider">The engine symbol provider.</param>
        public EngineSymbolProviderModule(SharpDebug.Module module, XmlModule xmlModule, ISymbolProvider symbolProvider)
        {
            if (string.IsNullOrEmpty(xmlModule.Name))
            {
                xmlModule.Name = module.Name;
            }
            Name = xmlModule.Name;
            Namespace = xmlModule.Namespace;
            EngineModule = module;
            EngineModuleProvider = symbolProvider.GetSymbolProviderModule(module);
            symbolsById = new DictionaryCache<uint, Symbol>(CreateSymbol);
            symbolsByName = new DictionaryCache<string, Symbol>(FindSymbol);
        }

        /// <summary>
        /// Gets the engine module.
        /// </summary>
        public SharpDebug.Module EngineModule { get; private set; }

        /// <summary>
        /// Gets the engine module provider.
        /// </summary>
        public ISymbolProviderModule EngineModuleProvider { get; private set; }

        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        public override Symbol GlobalScope
        {
            get
            {
                return GetSymbol(EngineModuleProvider.GetGlobalScope());
            }
        }

        /// <summary>
        /// Gets the symbol by name from the cache.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>Symbol if found; otherwise null.</returns>
        public override Symbol GetSymbol(string name)
        {
            return symbolsByName[name];
        }

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        public override Symbol[] FindGlobalTypeWildcard(string nameWildcard)
        {
            // TODO: We should add this functionality to ISymbolProviderModule
            Symbol s = GetSymbol(nameWildcard);

            if (s != null)
            {
                return new Symbol[1] { s };
            }

            return new Symbol[0];
        }

        /// <summary>
        /// Gets all types defined in the module.
        /// </summary>
        public override IEnumerable<Symbol> GetAllTypes()
        {
            foreach (uint id in EngineModuleProvider.GetAllTypes())
            {
                yield return GetSymbol(id);
            }
        }

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        protected override IEnumerable<string> GetPublicSymbols()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the symbol.
        /// </summary>
        /// <param name="id">The symbol identifier.</param>
        internal Symbol GetSymbol(uint id)
        {
            return symbolsById[id];
        }

        /// <summary>
        /// Creates the symbol.
        /// </summary>
        /// <param name="id">The symbol identifier.</param>
        private Symbol CreateSymbol(uint id)
        {
            return new EngineSymbolProviderSymbol(this, id);
        }

        /// <summary>
        /// Finds the specified symbol by name in the engine module.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        private Symbol FindSymbol(string name)
        {
            string originalName = name;

            try
            {
                Symbol symbol;
                int pointer = 0;

                FixSymbolSearchName(ref name);
                while (name.EndsWith("*"))
                {
                    pointer++;
                    name = name.Substring(0, name.Length - 1).TrimEnd();
                    FixSymbolSearchName(ref name);
                }

                FixSymbolSearchName(ref name);

                uint typeId;

                if (EngineModuleProvider.TryGetTypeId(name, out typeId))
                {
                    symbol = GetSymbol(typeId);
                    for (int i = 0; i < pointer; i++)
                        symbol = symbol.PointerType;
                    return symbol;
                }
            }
            catch
            {
            }
#if DEBUG
            Console.WriteLine("   '{0}' not found", originalName);
#endif
            return null;
        }

        /// <summary>
        /// Fixes the symbol name for the search.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        private static void FixSymbolSearchName(ref string name)
        {
            name = name.Trim();
            if (name.EndsWith(" const"))
                name = name.Substring(0, name.Length - 6);
            if (name.EndsWith(" volatile"))
                name = name.Substring(0, name.Length - 9);
            if (name.StartsWith("enum "))
                name = name.Substring(5);
        }
    }
}
