using CsDebugScript.PdbSymbolProvider.SymbolRecords;
using CsDebugScript.PdbSymbolProvider.TypeRecords;
using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Prefix of the <see cref="TypeRecord"/> or <see cref="SymbolRecord"/>.
    /// </summary>
    public struct RecordPrefix
    {
        /// <summary>
        /// Size of <see cref="RecordPrefix"/> structure in bytes.
        /// </summary>
        public const int Size = 4;

        /// <summary>
        /// Record length, starting from <see cref="RecordKind"/>.
        /// </summary>
        public ushort RecordLength { get; private set; }

        /// <summary>
        /// Record kind enum (<see cref="SymbolRecordKind"/> or <see cref="TypeLeafKind"/>)
        /// </summary>
        public ushort RecordKind { get; private set; }

        /// <summary>
        /// Gets the record data length.
        /// </summary>
        public ushort DataLen => (ushort)(RecordLength - 2);

        /// <summary>
        /// Reads <see cref="RecordPrefix"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static RecordPrefix Read(IBinaryReader reader)
        {
            return new RecordPrefix
            {
                RecordLength = reader.ReadUshort(),
                RecordKind = reader.ReadUshort(),
            };
        }
    }
}
