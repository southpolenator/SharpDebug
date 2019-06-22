using System;
using System.Collections.Generic;

namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Simple helper class for symbol providers that don't know how to find pointer type to symbol.
    /// </summary>
    internal class FakePointerSymbol : Symbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakePointerSymbol"/> class.
        /// </summary>
        /// <param name="elementType">Element type which this pointer point to.</param>
        public FakePointerSymbol(Symbol elementType)
            : base(elementType.Module)
        {
            ElementType = elementType;
            Tag = Engine.CodeTypeTag.Pointer;
            BasicType = DIA.BasicType.NoType;
            Name = ElementType.Name + "*";
            Size = 4; // TODO: This needs to get pointer size from module
        }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public override bool HasVTable()
        {
            return false;
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public override void InitializeCache()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected override IEnumerable<Symbol> GetBaseClasses()
        {
            yield break;
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            // This should never be called as it is cached in constructor.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected override IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            yield break;
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            yield break;
        }
    }
}
