using Dia2Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    internal class Module
    {
        private IDiaSession session;
        private ConcurrentDictionary<uint, Symbol> symbolById = new ConcurrentDictionary<uint, Symbol>();
        private ConcurrentDictionary<string, Symbol> symbolByName = new ConcurrentDictionary<string, Symbol>();

        public Module(string name, IDiaSession session)
        {
            this.session = session;
            Name = name;
            GlobalScope = GetSymbol(session.globalScope);
        }

        public Symbol GlobalScope { get; private set; }

        public string Name { get; private set; }

        public Symbol[] FindGlobalTypeWildcard(string nameWildcard)
        {
            return session.globalScope.GetChildrenWildcard(nameWildcard, SymTagEnum.SymTagUDT).Select(s => GetSymbol(s)).ToArray();
        }

        public Symbol[] GetAllTypes()
        {
            var diaGlobalTypes = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).ToList();
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagEnum));
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagBaseType));

            return diaGlobalTypes.Select(s => GetSymbol(s)).ToArray();

            // TODO: See why we have duplicates
            //var allSymbols = diaGlobalTypes.Select(s => GetSymbol(s)).OrderBy(s => s.Name);
            //var symbols = new List<Symbol>();
            //var previousName = "";

            //foreach (var s in allSymbols)
            //    if (s.Name != previousName)
            //    {
            //        symbols.Add(s);
            //        previousName = s.Name;
            //    }
            //return symbols.ToArray();
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
                    symbolById.TryAdd(symbolId, s);
                    if ((s.Tag == SymTagEnum.SymTagUDT || s.Tag == SymTagEnum.SymTagBaseType) && !symbolByName.ContainsKey(s.Name))
                        symbolByName.TryAdd(s.Name, s);
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

            while (name.EndsWith("*"))
            {
                pointer++;
                name = name.Substring(0, name.Length - 1).TrimEnd();
            }

            name = name.Trim();
            if (name.StartsWith("::"))
                name = name.Substring(2);
            if (name.EndsWith(" const"))
                name = name.Substring(0, name.Length - 6);
            if (name.EndsWith(" volatile"))
                name = name.Substring(0, name.Length - 9);
            if (name.StartsWith("enum "))
                name = name.Substring(5);

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

            if (!symbolByName.TryGetValue(name, out symbol))
                symbol = GetSymbol(session.globalScope.GetChild(name));

            if (symbol == null)
                Console.WriteLine("   '{0}' not found", originalName);
            else
                for (int i = 0; i < pointer; i++)
                    symbol = symbol.PointerType;
            return symbol;
        }
    }
}
