using DIA;
using System;

namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents symbol field during debugging.
    /// </summary>
    public class DiaSymbolField : SymbolField
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
            : base(parentType)
        {
            this.symbol = symbol;
            Name = symbol.name;
            LocationType = symbol.locationType;
            DataKind = symbol.dataKind;
            Offset = symbol.offset;
            Value = symbol.value;

            ulong size = symbol.length;
            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;
            if (LocationType == LocationType.BitField)
            {
                BitSize = Size;
            }
            else
            {
                BitSize = Size * 8;
            }

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
        public override bool IsValidStatic
        {
            get
            {
                return IsStatic && Module.PublicSymbols.Contains(ParentType.Name + "::" + Name);
            }
        }
    }
}
