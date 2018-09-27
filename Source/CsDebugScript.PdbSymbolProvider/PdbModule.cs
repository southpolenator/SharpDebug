using CsDebugScript.CodeGen;
using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.Engine.Utility;
using CsDebugScript.PdbSymbolProvider.SymbolRecords;
using CsDebugScript.PdbSymbolProvider.TypeRecords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript.PdbSymbolProvider
{
    using Module = CsDebugScript.CodeGen.SymbolProviders.Module;
    using ConstantSymbol = SymbolRecords.ConstantSymbol;

    /// <summary>
    /// Class represents CodeGen module for PDB reader.
    /// </summary>
    public class PdbModule : Module, IDisposable
    {
        /// <summary>
        /// Cache of the global scope symbol.
        /// </summary>
        private SimpleCacheStruct<PdbGlobalScope> globalScopeCache;

        /// <summary>
        /// Dictionary cache of all built-in symbols by index type.
        /// </summary>
        private DictionaryCache<TypeIndex, PdbSymbol> builtinSymbolsCache;

        /// <summary>
        /// Array cache of all symbols by index type.
        /// </summary>
        private ArrayCache<PdbSymbol> allSymbolsCache;

        /// <summary>
        /// Array cache of defined symbols by index type.
        /// </summary>
        private ArrayCache<PdbSymbol> definedSymbolsCache;

        /// <summary>
        /// Cache of defined constants.
        /// </summary>
        private SimpleCacheStruct<IReadOnlyDictionary<string, ConstantSymbol>> constantsCache;

        /// <summary>
        /// Dictionary of all symbols by unique name (needed to search for defines symbol).
        /// </summary>
        private Dictionary<string, PdbSymbol> symbolsByUniqueName;

        /// <summary>
        /// Dictionary of all symbols by name.
        /// </summary>
        private Dictionary<string, PdbSymbol> symbolsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbModule"/> class.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        public PdbModule(XmlModule module)
            : this(module, new PdbFile(module.PdbPath))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbModule"/> class.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        /// <param name="pdbFile">Already opened PDB file.</param>
        public PdbModule(XmlModule module, PdbFile pdbFile)
        {
            PdbFile = pdbFile;
            Name = !string.IsNullOrEmpty(module.Name) ? module.Name : Path.GetFileNameWithoutExtension(module.PdbPath).ToLower();
            Namespace = module.Namespace;
            globalScopeCache = SimpleCache.CreateStruct(() => new PdbGlobalScope(this));
            builtinSymbolsCache = new DictionaryCache<TypeIndex, PdbSymbol>(CreateBuiltinSymbol);
            allSymbolsCache = new ArrayCache<PdbSymbol>(PdbFile.TpiStream.TypeRecordCount, CreateSymbol);
            definedSymbolsCache = new ArrayCache<PdbSymbol>(PdbFile.TpiStream.TypeRecordCount, true, GetDefinedSymbol);
            constantsCache = SimpleCache.CreateStruct(() =>
            {
                Dictionary<string, ConstantSymbol> constants = new Dictionary<string, ConstantSymbol>();

                foreach (SymbolRecordKind kind in ConstantSymbol.Kinds)
                    foreach (ConstantSymbol c in PdbFile.PdbSymbolStream[kind].OfType<ConstantSymbol>())
                        if (!constants.ContainsKey(c.Name))
                            constants.Add(c.Name, c);
                return (IReadOnlyDictionary<string, ConstantSymbol>)constants;
            });
        }

        /// <summary>
        /// Gets the associated PDB file.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets all constants defined in PDB.
        /// </summary>
        public IReadOnlyDictionary<string, ConstantSymbol> Constants => constantsCache.Value;

        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        public override Symbol GlobalScope => globalScopeCache.Value;

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        public override Symbol[] FindGlobalTypeWildcard(string nameWildcard)
        {
            // TODO:
            return new Symbol[0];
        }

        /// <summary>
        /// Gets all types defined in the symbol.
        /// </summary>
        public override IEnumerable<Symbol> GetAllTypes()
        {
            TypeLeafKind[] kinds = new TypeLeafKind[]
            {
                TypeLeafKind.LF_CLASS, TypeLeafKind.LF_STRUCTURE, TypeLeafKind.LF_INTERFACE,
                TypeLeafKind.LF_UNION, TypeLeafKind.LF_ENUM
            };
            List<Symbol> selectedSymbols = new List<Symbol>();

            // Find all symbols that will be returned and populate symbolsByUniqueName.
            symbolsByUniqueName = new Dictionary<string, PdbSymbol>();
            foreach (TypeLeafKind kind in kinds)
                foreach (TypeIndex typeIndex in PdbFile.TpiStream.GetIndexes(kind))
                {
                    PdbSymbol symbol = allSymbolsCache[(int)typeIndex.ArrayIndex] = CreateSymbol((int)typeIndex.ArrayIndex), oldSymbol;

                    if (string.IsNullOrEmpty(symbol.UniqueName))
                        selectedSymbols.Add(symbol);
                    else if (!symbolsByUniqueName.TryGetValue(symbol.UniqueName, out oldSymbol))
                        symbolsByUniqueName.Add(symbol.UniqueName, symbol);
                    else if (oldSymbol.IsForwardReference && !symbol.IsForwardReference)
                        symbolsByUniqueName[symbol.UniqueName] = symbol;
                }
            selectedSymbols.AddRange(symbolsByUniqueName.Values);

            // Now that we have symbolsByUniqueName, we can search for defined symbols.
            symbolsByName = new Dictionary<string, PdbSymbol>();
            foreach (TypeLeafKind kind in kinds)
                foreach (TypeIndex typeIndex in PdbFile.TpiStream.GetIndexes(kind))
                {
                    PdbSymbol symbol = definedSymbolsCache[(int)typeIndex.ArrayIndex];

                    if (!string.IsNullOrEmpty(symbol.Name) && !symbolsByName.ContainsKey(symbol.Name))
                        symbolsByName.Add(symbol.Name, symbol);
                }

            // Add all known built-in types into symbolsByName
            foreach (TypeIndex typeIndex in TypeIndex.BuiltinTypes)
            {
                PdbSymbol symbol = builtinSymbolsCache[typeIndex];

                if (!string.IsNullOrEmpty(symbol.Name) && !symbolsByName.ContainsKey(symbol.Name))
                    symbolsByName.Add(symbol.Name, symbol);
            }

            // Add pointer to all known built-in types into symbolsByName
            foreach (TypeIndex t in TypeIndex.BuiltinTypes)
            {
                TypeIndex typeIndex = new TypeIndex(t.SimpleKind, SimpleTypeMode.NearPointer);
                PdbSymbol symbol = builtinSymbolsCache[typeIndex];

                if (!string.IsNullOrEmpty(symbol.Name) && !symbolsByName.ContainsKey(symbol.Name))
                    symbolsByName.Add(symbol.Name, symbol);
            }

            return selectedSymbols;
        }

        /// <summary>
        /// Gets the symbol by name from the cache.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>Symbol if found; otherwise null.</returns>
        public override Symbol GetSymbol(string name)
        {
            PdbSymbol pdbSymbol;
            Symbol symbol = null;
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

            if (name == "unsigned int")
                name = "unsigned";

            if (!symbolsByName.TryGetValue(name, out pdbSymbol) || pdbSymbol == null)
            {
#if DEBUG
                System.Console.WriteLine("   '{0}' not found", originalName);
#endif
            }
            else
            {
                symbol = pdbSymbol;
                for (int i = 0; i < pointer; i++)
                    symbol = symbol.PointerType;
            }
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

        /// <summary>
        /// Gets PDB symbol for the specified type index.
        /// </summary>
        /// <param name="index">Type index.</param>
        public PdbSymbol GetSymbol(TypeIndex index)
        {
            if (index.IsSimple)
                return builtinSymbolsCache[index];
            return definedSymbolsCache[(int)index.ArrayIndex];
        }

        /// <summary>
        /// Gets PDB symbol for the specified type record.
        /// </summary>
        /// <param name="typeRecord">Type record.</param>
        public PdbSymbol GetSymbol(TypeRecord typeRecord)
        {
            return new PdbSymbol(this, uint.MaxValue, typeRecord);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            PdbFile.Dispose();
        }

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        protected override IEnumerable<string> GetPublicSymbols()
        {
            SymbolRecordKind[] dataKinds = new[] { SymbolRecordKind.S_LDATA32, SymbolRecordKind.S_GDATA32, SymbolRecordKind.S_LMANDATA, SymbolRecordKind.S_GMANDATA };

            foreach (SymbolRecordKind kind in dataKinds)
                foreach (DataSymbol data in PdbFile.PdbSymbolStream[kind].OfType<DataSymbol>())
                    yield return data.Name;
        }

        /// <summary>
        /// Gets defined symbol (follows forward references by unique name).
        /// </summary>
        /// <param name="index">Type index.</param>
        /// <returns>Symbol definition if exists or forward reference.</returns>
        private PdbSymbol GetDefinedSymbol(int index)
        {
            PdbSymbol symbol = allSymbolsCache[index];

            if (string.IsNullOrEmpty(symbol.UniqueName))
                return symbol;

            PdbSymbol definedSymbol;

            if (!symbolsByUniqueName.TryGetValue(symbol.UniqueName, out definedSymbol))
                return symbol;
            return definedSymbol;
        }

        /// <summary>
        /// Creates new <see cref="PdbSymbol"/> for the specified built-in type index.
        /// </summary>
        /// <param name="index">Built-in type index.</param>
        private PdbSymbol CreateBuiltinSymbol(TypeIndex typeIndex)
        {
            return new PdbSymbol(this, typeIndex);
        }

        /// <summary>
        /// Creates new <see cref="PdbSymbol"/> for the specified type index.
        /// </summary>
        /// <param name="index">Type index in the TPI stream.</param>
        private PdbSymbol CreateSymbol(int index)
        {
            TypeIndex typeIndex = TypeIndex.FromArrayIndex(index);

            return new PdbSymbol(this, typeIndex.Index, PdbFile.TpiStream[typeIndex]);
        }
    }
}
