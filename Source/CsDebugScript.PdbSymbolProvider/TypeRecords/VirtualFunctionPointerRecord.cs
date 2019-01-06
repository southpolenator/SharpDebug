using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies virtual table pointers within the class. It is a requirement that this record be
    /// emitted in the field list before any virtual functions are emitted to the field list.
    /// </summary>
    public class VirtualFunctionPointerRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_VFUNCTAB
        };

        /// <summary>
        /// Padding or unkown data?
        /// </summary>
        public ushort Padding { get; private set; }

        /// <summary>
        /// Gets the index to the pointer record describing the pointer. The pointer will in
        /// turn have an <see cref="VirtualFunctionTableShapeRecord"/> as the underlying type. Note
        /// that the offset of the virtual function table pointer from the address
        /// point of the class is always zero.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Reads <see cref="VirtualFunctionPointerRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static VirtualFunctionPointerRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new VirtualFunctionPointerRecord
            {
                Kind = kind,
                Padding = reader.ReadUshort(),
                Type = TypeIndex.Read(reader),
            };
        }
    }
}
