using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    class Symbol
    {
        private IDiaSymbol symbol;
        private SymbolField[] fields;
        private Symbol[] baseClasses;

        public Symbol(IDiaSymbol symbol)
        {
            this.symbol = symbol;
            Tag = (SymTagEnum)symbol.symTag;
            BasicType = (BasicType)symbol.baseType;
            if (Tag != SymTagEnum.SymTagExe)
                Name = TypeToString.GetTypeString(symbol);
            else
                Name = "";

            var size = symbol.length;
            if (size > int.MaxValue)
                throw new ArgumentException("Symbol size is unexpected");
            Size = (int)size;
        }

        public string Name { get; private set; }

        public int Size { get; private set; }

        public SymTagEnum Tag { get; private set; }

        public BasicType BasicType { get; private set; }

        public Symbol ElementType
        {
            get
            {
                // TODO: We need to cache these values
                IDiaSymbol type = symbol.type;

                if (type != null)
                    return new Symbol(type);
                return null;
            }
        }

        public SymbolField[] Fields
        {
            get
            {
                if (fields == null)
                    fields = symbol.GetChildren(SymTagEnum.SymTagData).Select(s => new SymbolField(this, s)).ToArray();
                return fields;
            }
        }

        public Symbol[] BaseClasses
        {
            get
            {
                if (baseClasses == null)
                    baseClasses = symbol.GetChildren(SymTagEnum.SymTagBaseClass).Select(s => new Symbol(s)).ToArray();
                return baseClasses;
            }
        }

        public IEnumerable<Tuple<string, ulong>> GetEnumValues()
        {
            if (Tag == SymTagEnum.SymTagEnum)
            {
                foreach (var enumValue in symbol.GetChildren())
                {
                    yield return Tuple.Create(enumValue.name, (ulong)enumValue.value);
                }
            }
        }

        public SymbolField CastAsSymbolField()
        {
            return new SymbolField(this, symbol);
        }
    }

    class SymbolField
    {
        private IDiaSymbol symbol;

        public SymbolField(Symbol parentType, IDiaSymbol symbol)
        {
            this.symbol = symbol;
            ParentType = parentType;
            Name = symbol.name;
            LocationType = (LocationType)symbol.locationType;
            DataKind = (DataKind)symbol.dataKind;
            Offset = symbol.offset;
            Value = symbol.value;

            var size = symbol.length;
            if (size > int.MaxValue)
                throw new ArgumentException("Symbol size is unexpected");
            Size = (int)size;

            var bitPosition = symbol.bitPosition;
            if (bitPosition > int.MaxValue)
                throw new ArgumentException("Symbol bit position is unexpected");
            BitPosition = (int)bitPosition;

            // TODO: We need to cache these values
            IDiaSymbol type = symbol.type;
            if (type != null)
                Type = new Symbol(type);
        }

        public Symbol ParentType { get; private set; }

        public Symbol Type { get; private set; }

        public string Name { get; private set; }

        public int Size { get; private set; }

        public int Offset { get; private set; }

        public int BitPosition { get; private set; }

        public LocationType LocationType { get; private set; }

        public DataKind DataKind { get; private set; }

        public dynamic Value { get; private set; }
    }
}
