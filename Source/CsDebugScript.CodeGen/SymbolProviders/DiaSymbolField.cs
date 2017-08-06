using Dia2Lib;
using System;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents symbol field during debugging.
    /// </summary>
    internal class DiaSymbolField : ISymbolField
    {
        /// <summary>
        /// The DIA symbol
        /// </summary>
        private IDiaSymbol symbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiaSymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="symbol">The DIA symbol.</param>
        public DiaSymbolField(DiaSymbol parentType, IDiaSymbol symbol)
        {
            this.symbol = symbol;
            ParentType = parentType;
            Name = symbol.name;
            LocationType = (LocationType)symbol.locationType;
            DataKind = (DataKind)symbol.dataKind;
            Offset = symbol.offset;
            Value = symbol.value;

            ulong size = symbol.length;
            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;

            uint bitPosition = symbol.bitPosition;
            if (bitPosition > int.MaxValue)
            {
                throw new ArgumentException("Symbol bit position is unexpected");
            }
            BitPosition = (int)bitPosition;

            IDiaSymbol type = symbol.type;
            if (type != null)
            {
                Type = ((DiaModule)Module).GetSymbol(type);
            }
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
        public IModule Module
        {
            get
            {
                return ParentType.Module;
            }
        }

        /// <summary>
        /// Gets the parent type.
        /// </summary>
        public ISymbol ParentType { get; private set; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public ISymbol Type { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string PropertyName { get; set; }

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
