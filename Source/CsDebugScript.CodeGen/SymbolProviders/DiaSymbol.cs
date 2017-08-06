using CsDebugScript.Engine.Utility;
using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Class represents symbol during debugging.
    /// </summary>
    internal class DiaSymbol : ISymbol
    {
        /// <summary>
        /// The DIA symbol
        /// </summary>
        internal IDiaSymbol symbol;

        /// <summary>
        /// The cache of fields
        /// </summary>
        private SimpleCache<ISymbolField[]> fields;

        /// <summary>
        /// The cache of base classes
        /// </summary>
        private SimpleCache<ISymbol[]> baseClasses;

        /// <summary>
        /// The cache of element type
        /// </summary>
        private SimpleCache<ISymbol> elementType;

        /// <summary>
        /// The cache of pointer type
        /// </summary>
        private SimpleCache<ISymbol> pointerType;

        /// <summary>
        /// The cache of user type
        /// </summary>
        private SimpleCache<UserType> userType;

        /// <summary>
        /// The cache of enum values
        /// </summary>
        private SimpleCache<Tuple<string, string>[]> enumValues;

        /// <summary>
        /// The cache of namespaces
        /// </summary>
        private SimpleCache<List<string>> namespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiaSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="symbol">The DIA symbol.</param>
        public DiaSymbol(DiaModule module, IDiaSymbol symbol)
        {
            this.symbol = symbol;
            Module = module;
            Tag = (SymTagEnum)symbol.symTag;
            BasicType = (BasicType)symbol.baseType;
            Id = symbol.symIndexId;
            if (Tag != SymTagEnum.SymTagExe)
            {
                Name = TypeToString.GetTypeString(symbol);
            }
            else
            {
                Name = "";
            }

            Name = Name.Replace("<enum ", "<").Replace(",enum ", ",");
            Offset = symbol.offset;

            ulong size = symbol.length;

            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;

            // Initialize caches
            fields = SimpleCache.Create(() => symbol.GetChildren(SymTagEnum.SymTagData).Select(s => new DiaSymbolField(this, s)).Where(f => f.Type != null).Cast<ISymbolField>().ToArray());
            baseClasses = SimpleCache.Create(() => symbol.GetChildren(SymTagEnum.SymTagBaseClass).Select(s => ((DiaModule)Module).GetSymbol(s)).Cast<ISymbol>().ToArray());
            elementType = SimpleCache.Create(() =>
            {
                if (Tag == SymTagEnum.SymTagPointerType || Tag == SymTagEnum.SymTagArrayType)
                {
                    IDiaSymbol type = symbol.type;

                    if (type != null)
                    {
                        DiaSymbol result = ((DiaModule)Module).GetSymbol(type);
                        if (Tag == SymTagEnum.SymTagPointerType)
                        {
                            result.pointerType.Value = this;
                        }
                        return (ISymbol)result;
                    }
                }

                return null;
            });
            pointerType = SimpleCache.Create(() =>
            {
                DiaSymbol result = ((DiaModule)Module).GetSymbol(symbol.objectPointerType);
                result.elementType.Value = this;
                return (ISymbol)result;
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
            namespaces = SimpleCache.Create(() => SymbolNameHelper.GetSymbolNamespaces(Name));
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public IModule Module { get; private set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public SymTagEnum Tag { get; private set; }

        /// <summary>
        /// Gets the basic type.
        /// </summary>
        public BasicType BasicType { get; private set; }

        /// <summary>
        /// Gets or sets the user type associated with this symbol.
        /// </summary>
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

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        public List<string> Namespaces
        {
            get
            {
                return namespaces.Value;
            }
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        public ISymbol ElementType
        {
            get
            {
                return elementType.Value;
            }
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        public ISymbolField[] Fields
        {
            get
            {
                return fields.Value;
            }
        }

        /// <summary>
        /// Gets the base classes.
        /// </summary>
        public ISymbol[] BaseClasses
        {
            get
            {
                return baseClasses.Value;
            }
        }

        /// <summary>
        /// Gets the pointer type.
        /// </summary>
        public ISymbol PointerType
        {
            get
            {
                return pointerType.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this symbol is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this symbol is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                if (Size == 0)
                    return true;
                if (Fields.Where(f => !f.IsStatic).Any())
                    return false;
                return !BaseClasses.Where(b => !b.IsEmpty).Any();
            }
        }

        /// <summary>
        /// Gets the enum values.
        /// </summary>
        public IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            return enumValues.Value;
        }

        /// <summary>
        /// Casts as symbol field.
        /// </summary>
        public ISymbolField CastAsSymbolField()
        {
            return new DiaSymbolField(this, symbol);
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public void InitializeCache()
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
        public void ExtractDependantSymbols(HashSet<ISymbol> extractedSymbols, XmlTypeTransformation[] transformations)
        {
            List<ISymbol> symbols = Fields.Select(f => f.Type).Union(BaseClasses).ToList();

            if (ElementType != null)
            {
                symbols.Add(ElementType);
            }

            foreach (ISymbol symbol in symbols)
            {
                if (transformations.Any(t => t.Matches(symbol.Name)))
                {
                    continue;
                }

                ISymbol s = symbol;

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
        /// Links the symbols.
        /// </summary>
        public void LinkSymbols(ISymbol symbol)
        {
            DiaSymbol s = (DiaSymbol)symbol;

            s.baseClasses = baseClasses;
            s.elementType = elementType;
            s.fields = fields;
            s.pointerType = pointerType;
            s.userType = userType;
        }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public bool HasVTable()
        {
            if (symbol.GetChildren(SymTagEnum.SymTagVTable).Any())
            {
                return true;
            }
            foreach (ISymbol baseClass in BaseClasses)
            {
                if (baseClass.Offset == 0 && baseClass.HasVTable())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all base classes (including base classes of base classes).
        /// </summary>
        public IEnumerable<ISymbol> GetAllBaseClasses()
        {
            List<ISymbol> unprocessed = BaseClasses.ToList();

            while (unprocessed.Count > 0)
            {
                List<ISymbol> symbols = unprocessed;

                unprocessed = new List<ISymbol>();
                foreach (ISymbol symbol in symbols)
                {
                    yield return symbol;
                    unprocessed.AddRange(symbol.BaseClasses);
                }
            }
        }
    }
}
