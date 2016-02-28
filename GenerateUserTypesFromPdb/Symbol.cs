using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    class Symbol
    {
        private IDiaSymbol symbol;

        public Symbol(IDiaSymbol symbol)
        {
            this.symbol = symbol;
            Name = TypeToString.GetTypeString(symbol);
        }

        public string Name { get; private set; }

        public IDiaSymbol Dia
        {
            get
            {
                return symbol;
            }
        }
    }
}
