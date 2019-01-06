using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Represents data symbol record.
    /// </summary>
    public class DataSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_LDATA32, SymbolRecordKind.S_GDATA32, SymbolRecordKind.S_LMANDATA,
            SymbolRecordKind.S_GMANDATA
        };

        /// <summary>
        /// Gets the data type (either simple built-in or index of complex one).
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the offset portion of symbol address.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the segment portion of symbol address.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the data symbol name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="DataSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static DataSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new DataSymbol
            {
                Kind = kind,
                Type = TypeIndex.Read(reader),
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
