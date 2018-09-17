using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Bit fields are represented by an entry in the field list that indexes a bit field type definition.
    /// </summary>
    public class BitFieldRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_BITFIELD
        };

        /// <summary>
        /// Gets the type index of the field.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the length in bits of the object.
        /// </summary>
        public byte BitSize { get; private set; }

        /// <summary>
        /// Gets the starting position (from bit 0) of the object in the word.
        /// </summary>
        public byte BitOffset { get; private set; }

        /// <summary>
        /// Reads <see cref="BitFieldRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static BitFieldRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new BitFieldRecord
            {
                Kind = kind,
                Type = TypeIndex.Read(reader),
                BitSize = reader.ReadByte(),
                BitOffset = reader.ReadByte(),
            };
        }
    }
}
