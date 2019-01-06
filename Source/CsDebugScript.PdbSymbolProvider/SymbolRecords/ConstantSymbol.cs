using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// This record is used to output constants and C enumerations. If used to output an enumeration,
    /// then the type index refers to the containing enum.
    /// </summary>
    public class ConstantSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_CONSTANT, SymbolRecordKind.S_MANCONSTANT
        };

        /// <summary>
        /// Gets the type of symbol or containing enum.
        /// </summary>
        public TypeIndex TypeIndex { get; private set; }

        /// <summary>
        /// Gets the value of symbol.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the constant name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="ConstantSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static ConstantSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new ConstantSymbol
            {
                Kind = kind,
                TypeIndex = TypeIndex.Read(reader),
                Value = reader.ReadEncodedInteger(),
                Name = reader.ReadCString(),
            };
        }
    }
}
