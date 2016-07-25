using Dia2Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript.CodeGen
{
    /// <summary>
    /// Class represents module during debugging. It is being described by PDB.
    /// </summary>
    internal class Module
    {
        /// <summary>
        /// The DIA data source
        /// </summary>
        private IDiaDataSource dia;

        /// <summary>
        /// The DIA session
        /// </summary>
        private IDiaSession session;

        /// <summary>
        /// The dictionary cache of symbols by symbol ID.
        /// </summary>
        private ConcurrentDictionary<uint, Symbol> symbolById = new ConcurrentDictionary<uint, Symbol>();

        /// <summary>
        /// The dictionary cache of symbols by symbol name.
        /// </summary>
        private ConcurrentDictionary<string, Symbol> symbolByName = new ConcurrentDictionary<string, Symbol>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        /// <param name="name">The module name.</param>
        /// <param name="moduleNamespace">The default namespace.</param>
        /// <param name="commonNamespace">Namespace if type is deduplicated across modules.</param>
        /// <param name="dia">The DIA data source.</param>
        /// <param name="session">The DIA session.</param>
        private Module(string name, string moduleNamespace, string commonNamespace, IDiaDataSource dia, IDiaSession session)
        {
            this.session = session;
            this.dia = dia;
            Name = name;
            Namespace = moduleNamespace;
            CommonNamespace = commonNamespace;
            GlobalScope = GetSymbol(session.globalScope);

            PublicSymbols = new HashSet<string>(session.globalScope.GetChildren(SymTagEnum.SymTagPublicSymbol).Select((type) =>
            {
                if (type.code != 0 || type.function != 0 || (LocationType)type.locationType != LocationType.Static)
                    return string.Empty;

                string undecoratedName;

                type.get_undecoratedNameEx(0x1000, out undecoratedName);
                return undecoratedName ?? type.name;
            }));
        }

        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        public static Module Open(XmlModule module)
        {
            IDiaDataSource dia = new DiaSource();
            IDiaSession session;
            string moduleName = !string.IsNullOrEmpty(module.Name) ? module.Name : Path.GetFileNameWithoutExtension(module.PdbPath).ToLower();

            module.Name = moduleName;
            dia.loadDataFromPdb(module.PdbPath);
            dia.openSession(out session);
            return new Module(module.Name, module.Namespace, module.CommonNamespace, dia, session);
        }

        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        public Symbol GlobalScope { get; private set; }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        public string Namespace { get; private set; }

        public string CommonNamespace { get; private set; }

        /// <summary>
        /// Gets the set of public symbols.
        /// </summary>
        public HashSet<string> PublicSymbols { get; private set; }

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        public Symbol[] FindGlobalTypeWildcard(string nameWildcard)
        {
            return session.globalScope.GetChildrenWildcard(nameWildcard, SymTagEnum.SymTagUDT).Select(s => GetSymbol(s)).ToArray();
        }

        /// <summary>
        /// Gets all types defined in the symbol.
        /// </summary>
        public IEnumerable<Symbol> GetAllTypes()
        {
            // Get all types defined in the symbol
            var diaGlobalTypes = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).ToList();
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagEnum));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagBaseType));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagPointerType));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagArrayType));

            // Create symbols from types
            var convertedTypes = diaGlobalTypes.Select(s => new Symbol(this, s)).ToList();
            var resultingTypes = convertedTypes.Where(t => t.Tag == SymTagEnum.SymTagUDT || t.Tag == SymTagEnum.SymTagEnum).OrderBy(s => s.Name).ToArray();
            var cacheTypes = convertedTypes.OrderBy(s => s.Tag).ThenBy(s => s.Name).ToArray();

            // Remove duplicate symbols by searching for the by name
            var symbols = new List<Symbol>();
            var previousName = "";

            foreach (var s in resultingTypes)
                if (s.Name != previousName)
                {
                    IDiaSymbol ss = session.globalScope.GetChild(s.Name, s.Tag);

                    if (ss != null)
                    {
                        symbols.Add(GetSymbol(ss));
                    }
                    else
                    {
                        symbols.Add(GetSymbol(s.DiaSymbol));
                    }

                    previousName = s.Name;
                }

            // Cache symbols inside the module
            foreach (var s in cacheTypes)
            {
                var symbolCache = GetSymbol(s.DiaSymbol);
            }

            return symbols;
        }

        /// <summary>
        /// Gets the symbol from the cache or adds new entry in the cache if symbol wasn't previously found.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        internal Symbol GetSymbol(IDiaSymbol symbol)
        {
            if (symbol == null)
                return null;

            Symbol s;
            uint symbolId = symbol.symIndexId;

            if (!symbolById.TryGetValue(symbolId, out s))
            {
                s = new Symbol(this, symbol);
                lock (this)
                {
                    Symbol previousSymbol = null;

                    symbolById.TryAdd(symbolId, s);
                    if (s.Tag != SymTagEnum.SymTagExe)
                        if (!symbolByName.TryGetValue(s.Name, out previousSymbol))
                        {
                            symbolByName.TryAdd(s.Name, s);
                        }
                        else
                        {
                            previousSymbol.LinkSymbols(s);
                        }
                }

                s.InitializeCache();
            }

            return s;
        }

        /// <summary>
        /// Gets the symbol by name from the cache.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>Symbol if found; otherwise null.</returns>
        public Symbol GetSymbol(string name)
        {
            Symbol symbol;
            string originalName = name;
            int pointer = 0;

            FixSymbolSearchName(ref name);
            while (name.EndsWith("*"))
            {
                pointer++;
                name = name.Substring(0, name.Length - 1).TrimEnd();
                FixSymbolSearchName(ref name);
            }

            FixSymbolSearchName(ref name);

            if (name == "unsigned __int64")
                name = "unsigned long long";
            else if (name == "__int64")
                name = "long long";
            else if (name == "long")
                name = "int";
            else if (name == "unsigned long")
                name = "unsigned int";
            else if (name == "signed char")
                name = "char";

            if (!symbolByName.TryGetValue(name, out symbol) || symbol == null)
            {
#if DEBUG
                Console.WriteLine("   '{0}' not found", originalName);
#endif
            }
            else
                for (int i = 0; i < pointer; i++)
                    symbol = symbol.PointerType;
            return symbol;
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
