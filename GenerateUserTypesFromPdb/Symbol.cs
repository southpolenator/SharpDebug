using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    internal class Symbol
    {
        private IDiaSymbol symbol;
        private SymbolField[] fields;
        private Symbol[] baseClasses;
        private Symbol elementType;

        public Symbol(Module module, IDiaSymbol symbol)
        {
            this.symbol = symbol;
            Module = module;
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

        public Module Module { get; private set; }

        public int Size { get; private set; }

        public SymTagEnum Tag { get; private set; }

        public BasicType BasicType { get; private set; }

        public Symbol ElementType
        {
            get
            {
                if (elementType == null)
                {
                    // TODO: We need to cache these values
                    IDiaSymbol type = symbol.type;

                    if (type != null)
                        elementType = Module.GetSymbol(type);
                }

                return elementType;
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

        internal void InitializeCache()
        {
            if (Tag != SymTagEnum.SymTagExe)
            {
                var fields = this.Fields;
                var baseClasses = this.BaseClasses;
                var elementType = this.ElementType;
            }
        }

        public Symbol[] BaseClasses
        {
            get
            {
                if (baseClasses == null)
                    baseClasses = symbol.GetChildren(SymTagEnum.SymTagBaseClass).Select(s => Module.GetSymbol(s)).ToArray();
                return baseClasses;
            }
        }

        public Symbol PointerType
        {
            get
            {
                return Module.GetSymbol(symbol.objectPointerType);
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

    internal class SymbolField
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

            IDiaSymbol type = symbol.type;
            if (type != null)
                Type = Module.GetSymbol(type);
        }

        public Module Module
        {
            get
            {
                return ParentType.Module;
            }
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
