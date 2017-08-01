using CsDebugScript.CodeGen.UserTypes;
using Dia2Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CodeGen
{
    /// <summary>
    /// Class reperesents symbol during debugging.
    /// </summary>
    internal class Symbol
    {
        /// <summary>
        /// The DIA symbol
        /// </summary>
        private IDiaSymbol symbol;

        /// <summary>
        /// The cache of fields
        /// </summary>
        private SimpleCache<SymbolField[]> fields;

        /// <summary>
        /// The cache of base classes
        /// </summary>
        private SimpleCache<Symbol[]> baseClasses;

        /// <summary>
        /// The cache of element type
        /// </summary>
        private SimpleCache<Symbol> elementType;

        /// <summary>
        /// The cache of pointer type
        /// </summary>
        private SimpleCache<Symbol> pointerType;

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
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="symbol">The DIA symbol.</param>
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

            // Initialize caches
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
        public Module Module { get; private set; }

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
        public Symbol ElementType
        {
            get
            {
                return elementType.Value;
            }
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        public SymbolField[] Fields
        {
            get
            {
                return fields.Value;
            }
        }

        /// <summary>
        /// Gets the DIA symbol.
        /// </summary>
        internal IDiaSymbol DiaSymbol
        {
            get
            {
                return symbol;
            }
        }

        /// <summary>
        /// Gets the base classes.
        /// </summary>
        public Symbol[] BaseClasses
        {
            get
            {
                return baseClasses.Value;
            }
        }

        /// <summary>
        /// Gets the pointer type.
        /// </summary>
        public Symbol PointerType
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
        public SymbolField CastAsSymbolField()
        {
            return new SymbolField(this, symbol);
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        internal void InitializeCache()
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
        internal void ExtractDependantSymbols(HashSet<Symbol> extractedSymbols, XmlTypeTransformation[] transformations)
        {
            List<Symbol> symbols = Fields.Select(f => f.Type)
                .Union(BaseClasses).ToList();

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
        /// Links the symbols.
        /// </summary>
        internal void LinkSymbols(Symbol s)
        {
            s.baseClasses = baseClasses;
            s.elementType = elementType;
            s.fields = fields;
            s.pointerType = pointerType;
            s.userType = userType;
        }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        internal bool HasVTable()
        {
            if (symbol.GetChildren(SymTagEnum.SymTagVTable).Any())
                return true;
            foreach (Symbol baseClass in BaseClasses)
                if (baseClass.Offset == 0 && baseClass.HasVTable())
                    return true;
            return false;
        }

        /// <summary>
        /// Gets all base classes (including base classes of base classes).
        /// </summary>
        internal IEnumerable<Symbol> GetAllBaseClasses()
        {
            List<Symbol> unprocessed = BaseClasses.ToList();

            while (unprocessed.Count > 0)
            {
                List<Symbol> symbols = unprocessed;

                unprocessed = new List<Symbol>();
                foreach (var symbol in symbols)
                {
                    yield return symbol;
                    unprocessed.AddRange(symbol.BaseClasses);
                }
            }
        }

        /// <summary>
        /// Helper class for caching results - it is being used as lazy evaluation
        /// </summary>
        internal static class SimpleCache
        {
            /// <summary>
            /// Creates a new instance of the <see cref="SimpleCache{T}" /> class.
            /// </summary>
            /// <typeparam name="T">Type to be cached</typeparam>
            /// <param name="populateAction">The function that populates the cache on demand.</param>
            /// <returns>Simple cache of &lt;T&gt;</returns>
            public static SimpleCache<T> Create<T>(Func<T> populateAction)
            {
                return new SimpleCache<T>(populateAction);
            }
        }

        /// <summary>
        /// Helper class for caching results - it is being used as lazy evaluation
        /// </summary>
        /// <typeparam name="T">Type to be cached</typeparam>
        internal class SimpleCache<T>
        {
            /// <summary>
            /// The populate action
            /// </summary>
            private Func<T> populateAction;

            /// <summary>
            /// The value that is cached
            /// </summary>
            private T value;

            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleCache{T}"/> class.
            /// </summary>
            /// <param name="populateAction">The function that populates the cache on demand.</param>
            public SimpleCache(Func<T> populateAction)
            {
                this.populateAction = populateAction;
            }

            /// <summary>
            /// Gets a value indicating whether value is cached.
            /// </summary>
            /// <value>
            ///   <c>true</c> if cached; otherwise, <c>false</c>.
            /// </value>
            public bool Cached { get; internal set; }

            /// <summary>
            /// Gets or sets the value. The value will be populated if it wasn't cached.
            /// </summary>
            public T Value
            {
                get
                {
                    if (!Cached)
                    {
                        lock (this)
                        {
                            if (!Cached)
                            {
                                value = populateAction();
                                Cached = true;
                            }
                        }
                    }

                    return value;
                }

                set
                {
                    this.value = value;
                    Cached = true;
                }
            }
        }
    }

    /// <summary>
    /// Class represents symbol field during debugging.
    /// </summary>
    internal class SymbolField
    {
        /// <summary>
        /// The DIA symbol
        /// </summary>
        private IDiaSymbol symbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="symbol">The DIA symbol.</param>
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

        /// <summary>
        /// Gets a value indicating whether this instance is valid static field.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid static field; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidStatic
        {
            get
            {
                return IsStatic && Module.PublicSymbols.Contains(ParentType.Name + "::" + Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this field is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field is static; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic
        {
            get
            {
                return LocationType == LocationType.Static;
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module
        {
            get
            {
                return ParentType.Module;
            }
        }

        /// <summary>
        /// Gets the parent type.
        /// </summary>
        public Symbol ParentType { get; private set; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public Symbol Type { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the bit position.
        /// </summary>
        public int BitPosition { get; private set; }

        /// <summary>
        /// Gets the type of the location.
        /// </summary>
        public LocationType LocationType { get; private set; }

        /// <summary>
        /// Gets the data kind.
        /// </summary>
        public DataKind DataKind { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public dynamic Value { get; private set; }
    }
}
