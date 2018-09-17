using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record describes how to interpret the memory at the location pointed to by the virtual function table pointer.
    /// </summary>
    public class VirtualFunctionTableShapeRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_VTSHAPE
        };

        /// <summary>
        /// Gets the array of virtual function table slots.
        /// </summary>
        public VirtualFunctionTableSlotKind[] Slots { get; private set; }

        /// <summary>
        /// Reads <see cref="VirtualFunctionTableShapeRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static VirtualFunctionTableShapeRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            ushort count = reader.ReadUshort();
            VirtualFunctionTableSlotKind[] slots = new VirtualFunctionTableSlotKind[count];

            for (int i = 0; i < slots.Length; i += 2)
            {
                byte value = reader.ReadByte();

                slots[i] = (VirtualFunctionTableSlotKind)(value & 0xf);
                if (i + 1 < slots.Length)
                    slots[i + 1] = (VirtualFunctionTableSlotKind)(value >> 4);
            }
            return new VirtualFunctionTableShapeRecord
            {
                Kind = kind,
                Slots = slots,
            };
        }
    }
}
