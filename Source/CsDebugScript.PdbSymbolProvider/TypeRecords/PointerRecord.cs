using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents pointer type record.
    /// </summary>
    public class PointerRecord : TypeRecord
    {
        /// <summary>
        /// Mask for getting pointer kind from <see cref="Attribute"/>.
        /// </summary>
        public const uint PointerKindMask = 0x1F;

        /// <summary>
        /// Bit shift for getting pointer kind from <see cref="Attribute"/>.
        /// </summary>
        public const int PointerKindShift = 0;

        /// <summary>
        /// Mask for getting pointer mode from <see cref="Attribute"/>.
        /// </summary>
        public const uint PointerModeMask = 0x07;

        /// <summary>
        /// Bit shift for getting pointer mode from <see cref="Attribute"/>.
        /// </summary>
        public const int PointerModeShift = 5;

        /// <summary>
        /// Mask for getting pointer options from <see cref="Attribute"/>.
        /// </summary>
        public const uint PointerOptionsMask = 0xFF00; // TODO: This might not be correct for WinRT smart pointer. Find one PDB for WinRT and check it.

        /// <summary>
        /// Mask for getting pointer size from <see cref="Attribute"/>.
        /// </summary>
        public const uint PointerSizeMask = 0xFF;

        /// <summary>
        /// Bit shift for getting pointer size from <see cref="Attribute"/>.
        /// </summary>
        public const int PointerSizeShift = 13;

        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_POINTER
        };

        /// <summary>
        /// Gets the pointer kind.
        /// </summary>
        public PointerKind PointerKind => (PointerKind)((Attribute >> PointerKindShift) & PointerKindMask);

        /// <summary>
        /// Gets the pointer mode.
        /// </summary>
        public PointerMode Mode => (PointerMode)((Attribute >> PointerModeShift) & PointerModeMask);

        /// <summary>
        /// Gets the pointer options.
        /// </summary>
        public PointerOptions Options => (PointerOptions)(Attribute & PointerOptionsMask);

        /// <summary>
        /// Gets the size in bytes.
        /// </summary>
        public byte Size => (byte)((Attribute >> PointerSizeShift) & PointerSizeMask);

        /// <summary>
        /// <c>true</c> if it is pointer to a member: <see cref="PointerMode.PointerToDataMember"/> or <see cref="PointerMode.PointerToMemberFunction"/>.
        /// </summary>
        public bool IsPointerToMember => Mode == PointerMode.PointerToDataMember || Mode == PointerMode.PointerToMemberFunction;

        /// <summary>
        /// <c>true</c> if 16:32 pointer.
        /// </summary>
        public bool IsFlat => (Options & PointerOptions.Flat32) != 0;

        /// <summary>
        /// <c>true</c> if pointer is <c>const</c>.
        /// </summary>
        public bool IsConst => (Options & PointerOptions.Const) != 0;

        /// <summary>
        /// <c>true</c> if pointer is <c>volatile</c>.
        /// </summary>
        public bool IsVolatile => (Options & PointerOptions.Volatile) != 0;

        /// <summary>
        /// <c>true</c> if pointer is <c>unaligned</c>.
        /// </summary>
        public bool IsUnaligned => (Options & PointerOptions.Unaligned) != 0;

        /// <summary>
        /// Gets the type index of object pointed to.
        /// </summary>
        public TypeIndex ReferentType { get; private set; }

        /// <summary>
        /// Consists of 4 bit fields returned by properties: <see cref="PointerKind"/>, <see cref="Mode"/>, <see cref="Options"/>, <see cref="Size"/>.
        /// </summary>
        public uint Attribute { get; private set; }

        /// <summary>
        /// Gets the member info if <see cref="IsPointerToMember"/> is <c>true</c>.
        /// </summary>
        public MemberPointerInfo MemberInfo { get; private set; }

        /// <summary>
        /// Reads <see cref="PointerRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static PointerRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            PointerRecord record = new PointerRecord
            {
                Kind = kind,
                ReferentType = TypeIndex.Read(reader),
                Attribute = reader.ReadUint(),
            };

            if (record.IsPointerToMember)
                record.MemberInfo = MemberPointerInfo.Read(reader);
            return record;
        }
    }
}
