using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using DIA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Interface represents symbol during debugging.
    /// </summary>
    public abstract class Symbol
    {
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
        /// The cache of enumeration values
        /// </summary>
        private SimpleCache<Tuple<string, string>[]> enumValues;

        /// <summary>
        /// The cache of namespaces
        /// </summary>
        private SimpleCache<List<string>> namespaces;

        /// <summary>
        /// The user type
        /// </summary>
        private SimpleCache<UserType> userType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public Symbol(Module module)
        {
            Module = module;
            fields = SimpleCache.Create(() => GetFields().ToArray());
            baseClasses = SimpleCache.Create(() => GetBaseClasses().ToArray());
            elementType = SimpleCache.Create(() => GetElementType());
            pointerType = SimpleCache.Create(() => GetPointerType());
            enumValues = SimpleCache.Create(() => GetEnumValues().ToArray());
            namespaces = SimpleCache.Create(() => SymbolNameHelper.GetSymbolNamespaces(Name));
            userType = SimpleCache.Create(() => (UserType)null);
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

            set
            {
                elementType.Value = value;
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

            set
            {
                pointerType.Value = value;
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
                {
                    return true;
                }
                if (Fields.Where(f => !f.IsStatic).Any())
                {
                    return false;
                }
                return !BaseClasses.Where(b => !b.IsEmpty).Any();
            }
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        public Tuple<string, string>[] EnumValues
        {
            get
            {
                return enumValues.Value;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; protected set; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset { get; protected set; }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        public CodeTypeTag Tag { get; protected set; }

        /// <summary>
        /// Gets the basic type.
        /// </summary>
        public BasicType BasicType { get; protected set; }

        /// <summary>
        /// Gets or sets the user type associated with this symbol.
        /// </summary>
        internal UserType UserType
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
        /// Gets all base classes (including base classes of base classes).
        /// </summary>
        public IEnumerable<Symbol> GetAllBaseClasses()
        {
            List<Symbol> unprocessed = BaseClasses.ToList();

            while (unprocessed.Count > 0)
            {
                List<Symbol> symbols = unprocessed;

                unprocessed = new List<Symbol>();
                foreach (Symbol symbol in symbols)
                {
                    yield return symbol;
                    unprocessed.AddRange(symbol.BaseClasses);
                }
            }
        }

        /// <summary>
        /// Extracts the dependent symbols into extractedSymbols if they are not recognized as transformations.
        /// </summary>
        /// <param name="extractedSymbols">The extracted symbols.</param>
        /// <param name="transformations">The transformations.</param>
        public void ExtractDependentSymbols(HashSet<Symbol> extractedSymbols, XmlTypeTransformation[] transformations)
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

                if (s.Tag == CodeTypeTag.BaseClass)
                {
                    s = s.Module.FindGlobalTypeWildcard(s.Name).Single();
                }

                if (extractedSymbols.Add(s))
                {
                    s.ExtractDependentSymbols(extractedSymbols, transformations);
                }
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            Symbol other = obj as Symbol;

            if (other == null)
            {
                return false;
            }
            return Id == other.Id;
        }

        /// <summary>
        /// Casts as symbol field.
        /// </summary>
        public abstract SymbolField CastAsSymbolField();

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public abstract void InitializeCache();

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public abstract bool HasVTable();

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected abstract IEnumerable<Tuple<string, string>> GetEnumValues();

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected abstract IEnumerable<SymbolField> GetFields();

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected abstract IEnumerable<Symbol> GetBaseClasses();

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected abstract Symbol GetElementType();

        /// <summary>
        /// Gets the pointer type to this symbol.
        /// </summary>
        protected abstract Symbol GetPointerType();

        /// <summary>
        /// Links the symbols.
        /// </summary>
        /// <param name="symbol">The symbol to be linked with.</param>
        internal void LinkSymbols(Symbol symbol)
        {
            symbol.baseClasses = baseClasses;
            symbol.elementType = elementType;
            symbol.fields = fields;
            symbol.pointerType = pointerType;
            symbol.enumValues = enumValues;
            symbol.userType = userType;
        }
    }
}
