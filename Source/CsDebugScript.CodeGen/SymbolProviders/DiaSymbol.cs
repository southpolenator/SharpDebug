using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents symbol during debugging.
    /// </summary>
    internal class DiaSymbol : Symbol
    {
        /// <summary>
        /// The DIA symbol
        /// </summary>
        internal IDiaSymbol symbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiaSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="symbol">The DIA symbol.</param>
        public DiaSymbol(DiaModule module, IDiaSymbol symbol)
            : base(module)
        {
            this.symbol = symbol;
            Tag = (SymTagEnum)symbol.symTag;
            BasicType = (BasicType)symbol.baseType;
            Id = symbol.symIndexId;
            if (Tag != SymTagEnum.SymTagExe)
            {
                Name = TypeToString.GetTypeString(symbol);
                Name = Name.Replace("<enum ", "<").Replace(",enum ", ",");
            }
            else
            {
                Name = "";
            }

            Offset = symbol.offset;

            ulong size = symbol.length;

            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;
        }

        /// <summary>
        /// Gets the DIA module.
        /// </summary>
        internal DiaModule DiaModule
        {
            get
            {
                return (DiaModule)Module;
            }
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected override IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            if (Tag == SymTagEnum.SymTagEnum)
            {
                foreach (var enumValue in symbol.GetChildren())
                {
                    yield return Tuple.Create(enumValue.name, enumValue.value.ToString());
                }
            }
        }

        /// <summary>
        /// Casts as symbol field.
        /// </summary>
        public override SymbolField CastAsSymbolField()
        {
            return new DiaSymbolField(this, symbol);
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public override void InitializeCache()
        {
            if (Tag != SymTagEnum.SymTagExe)
            {
                var elementType = this.ElementType;
            }
        }

        /// <summary>
        /// Extracts the dependant symbols into extractedSymbols if they are not recognized as transformations.
        /// </summary>
        /// <param name="extractedSymbols">The extracted symbols.</param>
        /// <param name="transformations">The transformations.</param>
        public override void ExtractDependantSymbols(HashSet<Symbol> extractedSymbols, XmlTypeTransformation[] transformations)
        {
            List<Symbol> symbols = Fields.Select(f => f.Type).Union(BaseClasses).ToList();

            if (ElementType != null)
            {
                symbols.Add(ElementType);
            }

            foreach (Symbol symbol in symbols)
            {
                if (transformations.Any(t => t.Matches(symbol.Name)))
                {
                    continue;
                }

                Symbol s = symbol;

                if (s.Tag == SymTagEnum.SymTagBaseClass)
                {
                    s = s.Module.FindGlobalTypeWildcard(s.Name).Single();
                }

                if (extractedSymbols.Add(s))
                {
                    s.ExtractDependantSymbols(extractedSymbols, transformations);
                }
            }
        }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public override bool HasVTable()
        {
            if (symbol.GetChildren(SymTagEnum.SymTagVTable).Any())
            {
                return true;
            }
            foreach (Symbol baseClass in BaseClasses)
            {
                if (baseClass.Offset == 0 && baseClass.HasVTable())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            return symbol.GetChildren(SymTagEnum.SymTagData).Select(s => new DiaSymbolField(this, s)).Where(f => f.Type != null).Cast<SymbolField>();
        }

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected override IEnumerable<Symbol> GetBaseClasses()
        {
            return symbol.GetChildren(SymTagEnum.SymTagBaseClass).Select(s => DiaModule.GetSymbol(s)).Cast<Symbol>();
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            if (Tag == SymTagEnum.SymTagPointerType || Tag == SymTagEnum.SymTagArrayType)
            {
                IDiaSymbol type = symbol.type;

                if (type != null)
                {
                    Symbol result = DiaModule.GetSymbol(type);

                    if (Tag == SymTagEnum.SymTagPointerType)
                    {
                        result.PointerType = this;
                    }
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the type of the pointer.
        /// </summary>
        protected override Symbol GetPointerType()
        {
            Symbol result = DiaModule.GetSymbol(symbol.objectPointerType);

            result.ElementType = this;
            return result;
        }
    }
}
