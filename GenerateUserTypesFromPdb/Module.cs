using Dia2Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    internal class Module
    {
        private IDiaDataSource dia;
        private IDiaSession session;
        private ConcurrentDictionary<uint, Symbol> symbolById = new ConcurrentDictionary<uint, Symbol>();
        private ConcurrentDictionary<string, Symbol> symbolByName = new ConcurrentDictionary<string, Symbol>();

        public Module(string name, string nameSpace, IDiaDataSource dia, IDiaSession session)
        {
            this.session = session;
            this.dia = dia;
            Name = name;
            Namespace = nameSpace;
            GlobalScope = GetSymbol(session.globalScope);
        }

        public Symbol GlobalScope { get; private set; }

        public string Name { get; private set; }

        public string Namespace { get; private set; }

        public Symbol[] FindGlobalTypeWildcard(string nameWildcard)
        {
            return session.globalScope.GetChildrenWildcard(nameWildcard, SymTagEnum.SymTagUDT).Select(s => GetSymbol(s)).ToArray();
        }

        public IEnumerable<Symbol> GetAllTypes()
        {
            var diaGlobalTypes = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).ToList();
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagEnum));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagBaseType));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagPointerType));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagArrayType));

            var convertedTypes = diaGlobalTypes.Select(s => GetSymbol(s)).ToList();
            var resultingTypes = convertedTypes.Where(t => t.Tag == SymTagEnum.SymTagUDT || t.Tag == SymTagEnum.SymTagEnum).OrderBy(s => s.Name).ToArray();

            // Remove duplicates
            var symbols = new List<Symbol>();
            var previousName = "";

            foreach (var s in resultingTypes)
                if (s.Name != previousName)
                {
                    symbols.Add(s);
                    previousName = s.Name;
                }
            return symbols;
        }

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

        public Symbol GetTypeSymbol(string name)
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
