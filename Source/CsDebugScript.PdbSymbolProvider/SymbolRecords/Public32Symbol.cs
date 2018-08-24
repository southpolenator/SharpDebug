using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Represents public 32bit symbol record.
    /// </summary>
    public class Public32Symbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_PUB32
        };

        /// <summary>
        /// Gets publi symbol flags.
        /// </summary>
        public PublicSymbolFlags Flags { get; private set; }

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
        /// Reads <see cref="Public32Symbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static Public32Symbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new Public32Symbol
            {
                Kind = kind,
                Flags = (PublicSymbolFlags)reader.ReadUint(),
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
