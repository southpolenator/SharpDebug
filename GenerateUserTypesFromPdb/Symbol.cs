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
        private Symbol pointerType;

        public Symbol(Module module, IDiaSymbol symbol)
        {
            this.symbol = symbol;
            Module = module;
            Tag = (SymTagEnum)symbol.symTag;
            BasicType = (BasicType)symbol.baseType;
            Id = symbol.symIndexId;
            if (Tag != SymTagEnum.SymTagExe)
                Name = TypeToString.GetTypeString(symbol);
            else
                Name = "";

            Name = Name.Replace("<enum ", "<").Replace(",enum ", ",");

            var size = symbol.length;
            if (size > int.MaxValue)
                throw new ArgumentException("Symbol size is unexpected");
            Size = (int)size;
        }

        public string Name { get; private set; }

        public uint Id { get; private set; }

        public Module Module { get; private set; }

        public int Size { get; private set; }

        public SymTagEnum Tag { get; private set; }

        public BasicType BasicType { get; private set; }

        public Symbol ElementType
        {
            get
            {
                if (elementType == null && (Tag == SymTagEnum.SymTagPointerType || Tag == SymTagEnum.SymTagArrayType))
                {
                    IDiaSymbol type = symbol.type;

                    if (type != null)
                    {
                        elementType = Module.GetSymbol(type);
                        elementType.pointerType = this;
                    }
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
                if (pointerType == null)
                {
                    pointerType = Module.GetSymbol(symbol.objectPointerType);
                    pointerType.elementType = this;
                }
                return pointerType;
            }
        }

        public IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            if (Tag == SymTagEnum.SymTagEnum)
            {
                foreach (var enumValue in symbol.GetChildren())
                {
                    yield return Tuple.Create(enumValue.name, enumValue.value.ToString());
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
