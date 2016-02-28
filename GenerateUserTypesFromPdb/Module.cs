using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    internal class Module
    {
        private IDiaSession session;
        private Dictionary<uint, Symbol> symbolById = new Dictionary<uint, Symbol>();
        private Dictionary<string, Symbol> symbolByName = new Dictionary<string, Symbol>();

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
            diaGlobalTypes.AddRange(session.globalScope.GetChildren(SymTagEnum.SymTagBaseType));

            return diaGlobalTypes.Select(s => GetSymbol(s)).ToArray();
        }

        internal Symbol GetSymbol(IDiaSymbol symbol)
        {
            if (symbol == null)
                return null;

            lock (this)
            {
                Symbol s;
                uint symbolId = symbol.symIndexId;

                if (!symbolById.TryGetValue(symbolId, out s))
                {
                    s = new Symbol(this, symbol);
                    symbolById.Add(symbolId, s);
                    if ((s.Tag == SymTagEnum.SymTagUDT || s.Tag == SymTagEnum.SymTagBaseType) && !symbolByName.ContainsKey(s.Name))
                        symbolByName.Add(s.Name, s);
//                    s.InitializeCache();
                }

                return s;
            }
        }

        private Dictionary<string, Symbol> basicTypes;

        public Symbol GetTypeSymbol(string name)
        {
            Symbol symbol;
            string originalName = name;
            int pointer = 0;

            while (name.EndsWith("*"))
            {
                pointer++;
                name = name.Substring(0, name.Length - 1);
            }

            name = name.Trim();
            if (name.EndsWith(" const"))
                name = name.Substring(0, name.Length - 6);
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
