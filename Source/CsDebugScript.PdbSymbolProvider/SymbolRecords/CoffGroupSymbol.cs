using CsDebugScript.PdbSymbolProvider.DBI;
using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Represents COFF group symbol record.
    /// </summary>
    public class CoffGroupSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_COFFGROUP
        };

        /// <summary>
        /// Gets the size.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Gets the COFF group characteristics.
        /// </summary>
        public ImageSectionCharacteristics Characteristics { get; private set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the segment.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="CoffGroupSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static CoffGroupSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new CoffGroupSymbol
            {
                Kind = kind,
                Size = reader.ReadUint(),
                Characteristics = (ImageSectionCharacteristics)reader.ReadUint(),
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
