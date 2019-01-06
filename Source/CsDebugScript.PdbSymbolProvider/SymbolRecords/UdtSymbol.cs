using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// This specifies a C typedef or user-defined type, such as classes, structures, unions, or enums.
    /// </summary>
    public class UdtSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_UDT, SymbolRecordKind.S_COBOLUDT
        };

        /// <summary>
        /// Gets the type of symbol.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the symbol name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="UdtSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static UdtSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new UdtSymbol
            {
                Kind = kind,
                Type = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };
        }
    }
}
