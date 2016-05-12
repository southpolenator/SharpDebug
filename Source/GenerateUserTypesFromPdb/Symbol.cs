using CsDebugScript.Engine.Utility;
using Dia2Lib;
using GenerateUserTypesFromPdb.UserTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    internal class Symbol
    {
        private IDiaSymbol symbol;
        private SimpleCache<SymbolField[]> fields;
        private SimpleCache<Symbol[]> baseClasses;
        private SimpleCache<Symbol> elementType;
        private SimpleCache<Symbol> pointerType;
        private SimpleCache<UserType> userType;
        private SimpleCache<Tuple<string, string>[]> enumValues;
        private SimpleCache<List<string>> namespaces;

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
            Offset = symbol.offset;

            var size = symbol.length;
            if (size > int.MaxValue)
                throw new ArgumentException("Symbol size is unexpected");
            Size = (int)size;

            fields = SimpleCache.Create(() => symbol.GetChildren(SymTagEnum.SymTagData).Select(s => new SymbolField(this, s)).Where(f => f.Type != null).ToArray());
            baseClasses = SimpleCache.Create(() => symbol.GetChildren(SymTagEnum.SymTagBaseClass).Select(s => Module.GetSymbol(s)).ToArray());
            elementType = SimpleCache.Create(() =>
            {
                if (Tag == SymTagEnum.SymTagPointerType || Tag == SymTagEnum.SymTagArrayType)
                {
                    IDiaSymbol type = symbol.type;

                    if (type != null)
                    {
                        var result = Module.GetSymbol(type);
                        if (Tag == SymTagEnum.SymTagPointerType)
                            result.pointerType.Value = this;
                        return result;
                    }
                }

                return null;
            });
            pointerType = SimpleCache.Create(() =>
            {
                var result = Module.GetSymbol(symbol.objectPointerType);
                result.elementType.Value = this;
                return result;
            });
            userType = SimpleCache.Create(() => (UserType)null);
            enumValues = SimpleCache.Create(() =>
            {
                List<Tuple<string, string>> result = new List<Tuple<string, string>>();

                if (Tag == SymTagEnum.SymTagEnum)
                {
                    foreach (var enumValue in symbol.GetChildren())
                    {
                        result.Add(Tuple.Create(enumValue.name, enumValue.value.ToString()));
                    }
                }

                return result.ToArray();
            });
            namespaces = SimpleCache.Create(() => NameHelper.GetFullSymbolNamespaces(Name));
        }

        internal Symbol GetChild(string name)
        {
            return Module.GetSymbol(symbol.GetChild(name));
        }

        public string Name { get; private set; }

        public uint Id { get; private set; }

        public Module Module { get; private set; }

        public int Size { get; private set; }

        public int Offset { get; private set; }

        public SymTagEnum Tag { get; private set; }

        public BasicType BasicType { get; private set; }

        public UserType UserType
        {
            get
            {
                return userType.Value;
            }

            set
            {
                userType.Value = value;
            }
        }

        public List<string> Namespaces
        {
            get
            {
                return namespaces.Value;
            }
        }

        public Symbol ElementType
        {
            get
            {
                return elementType.Value;
            }
        }

        public SymbolField[] Fields
        {
            get
            {
                return fields.Value;
            }
        }

        internal IDiaSymbol DiaSymbol
        {
            get
            {
                return symbol;
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
                return baseClasses.Value;
            }
        }

        public Symbol PointerType
        {
            get
            {
                return pointerType.Value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (Size == 0)
                    return true;
                if (Fields.Length > 0)
                    return false;
                return BaseClasses.All(b => b.IsEmpty);
            }
        }

        public IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            return enumValues.Value;
        }

        public SymbolField CastAsSymbolField()
        {
            return new SymbolField(this, symbol);
        }

        internal void LinkSymbols(Symbol s)
        {
            s.baseClasses = baseClasses;
            s.elementType = elementType;
            s.fields = fields;
            s.pointerType = pointerType;
            s.userType = userType;
        }

        /// <summary>
        /// Force Symbol in declare Module.
        /// Do no treat as duplicate.
        /// </summary>
        public bool ForceInDeclareModule { get; set; }
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

        public bool IsValidStatic
        {
            get
            {
                return LocationType == LocationType.Static && Module.PublicSymbols.Contains(ParentType.Name + "::" + Name);
            }
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
