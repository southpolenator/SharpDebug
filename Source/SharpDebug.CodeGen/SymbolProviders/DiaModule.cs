using SharpDebug.Engine;
using DIA;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents module during debugging. It is being described by PDB.
    /// </summary>
    public class DiaModule : Module
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
        /// Initializes a new instance of the <see cref="DiaModule"/> class.
        /// </summary>
        /// <param name="name">The module name.</param>
        /// <param name="nameSpace">The default namespace.</param>
        /// <param name="dia">The DIA data source.</param>
        /// <param name="session">The DIA session.</param>
        private DiaModule(string name, string nameSpace, IDiaDataSource dia, IDiaSession session)
        {
            this.session = session;
            this.dia = dia;
            Name = name;
            Namespace = nameSpace;
        }

        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        public static Module Open(XmlModule module)
        {
            IDiaDataSource dia = DiaLoader.CreateDiaSource();
            IDiaSession session;
            string moduleName = !string.IsNullOrEmpty(module.Name) ? module.Name : Path.GetFileNameWithoutExtension(module.SymbolsPath).ToLower();

            module.Name = moduleName;
            dia.loadDataFromPdb(module.SymbolsPath);
            dia.openSession(out session);
            return new DiaModule(module.Name, module.Namespace, dia, session);
        }

        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        public override Symbol GlobalScope
        {
            get
            {
                return GetSymbol(session.globalScope);
            }
        }

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        public override Symbol[] FindGlobalTypeWildcard(string nameWildcard)
        {
            return session.globalScope.GetChildrenWildcard(nameWildcard, SymTagEnum.UDT).Select(s => GetSymbol(s)).Cast<Symbol>().ToArray();
        }

        /// <summary>
        /// Gets all types defined in the symbol.
        /// </summary>
        public override IEnumerable<Symbol> GetAllTypes()
        {
            // Get all types defined in the symbol
            var diaGlobalTypes = session.globalScope.GetChildren(SymTagEnum.UDT).ToList();
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.Enum));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.BaseType));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.PointerType));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.ArrayType));

            // Create symbols from types
            var convertedTypes = diaGlobalTypes.Select(s => new DiaSymbol(this, s)).ToList();
            var resultingTypes = convertedTypes
                .Where(t => t.Tag == CodeTypeTag.Class || t.Tag == CodeTypeTag.Structure || t.Tag == CodeTypeTag.Union || t.Tag == CodeTypeTag.Enum)
                .OrderBy(s => s.Name)
                .ToArray();
            var cacheTypes = convertedTypes.OrderBy(s => s.Tag).ThenBy(s => s.Name).ToArray();

            // Remove duplicate symbols by searching for the by name
            var symbols = new List<Symbol>();
            var previousName = "";

            foreach (DiaSymbol s in resultingTypes)
            {
                if (s.Name != previousName)
                {
                    IDiaSymbol ss = session.globalScope.GetChild(s.Name, ConvertToSymTag(s.Tag));

                    if (ss != null)
                    {
                        symbols.Add(GetSymbol(ss));
                    }
                    else
                    {
                        symbols.Add(GetSymbol(s.symbol));
                    }

                    previousName = s.Name;
                }
            }

            // Cache symbols inside the module
            foreach (DiaSymbol s in cacheTypes)
            {
                var symbolCache = GetSymbol(s.symbol);
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
            {
                return null;
            }

            Symbol s;
            uint symbolId = symbol.symIndexId;

            if (!symbolById.TryGetValue(symbolId, out s))
            {
                s = new DiaSymbol(this, symbol);
                lock (this)
                {
                    Symbol previousSymbol = null;

                    symbolById.TryAdd(symbolId, s);
                    if (s.Tag != CodeTypeTag.ModuleGlobals)
                    {
                        if (!symbolByName.TryGetValue(s.Name, out previousSymbol))
                        {
                            symbolByName.TryAdd(s.Name, s);
                        }
                        else
                        {
                            previousSymbol.LinkSymbols(s);
                        }
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
        public override Symbol GetSymbol(string name)
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
            {
                for (int i = 0; i < pointer; i++)
                {
                    symbol = symbol.PointerType;
                }
            }
            return symbol;
        }

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        protected override IEnumerable<string> GetPublicSymbols()
        {
            return session.globalScope.GetChildren(SymTagEnum.PublicSymbol).Select((type) =>
            {
                if (type.code || type.function || type.locationType != LocationType.Static)
                {
                    return string.Empty;
                }

                return type.get_undecoratedNameEx(UndecoratedNameOptions.NameOnly) ?? type.name;
            });
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

        /// <summary>
        /// Converts <see cref="CodeTypeTag"/> to <see cref="SymTagEnum"/>.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private static SymTagEnum ConvertToSymTag(CodeTypeTag tag)
        {
            switch (tag)
            {
                case CodeTypeTag.Array:
                    return SymTagEnum.ArrayType;
                case CodeTypeTag.BaseClass:
                    return SymTagEnum.BaseClass;
                case CodeTypeTag.BuiltinType:
                    return SymTagEnum.BaseType;
                case CodeTypeTag.Class:
                case CodeTypeTag.Structure:
                case CodeTypeTag.Union:
                    return SymTagEnum.UDT;
                case CodeTypeTag.Enum:
                    return SymTagEnum.Enum;
                case CodeTypeTag.Function:
                    return SymTagEnum.FunctionType;
                case CodeTypeTag.ModuleGlobals:
                    return SymTagEnum.Exe;
                case CodeTypeTag.Pointer:
                    return SymTagEnum.PointerType;
                default:
                case CodeTypeTag.Unsupported:
                    throw new NotImplementedException();
            }
        }
    }
}
