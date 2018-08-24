using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_fldattr_t in cvinfo.h.
    /// </summary>
    public struct MemberAttributes
    {
        /// <summary>
        /// Mask for getting <see cref="MethodKind"/> from <see cref="Attributes"/>.
        /// </summary>
        public const ushort MethodKindMask = 0x001c;

        /// <summary>
        /// Bit shift for getting <see cref="MethodKind"/> from <see cref="Attributes"/>.
        /// </summary>
        public const ushort MethodKindShift = 2;

        /// <summary>
        /// Mask for getting <see cref="MemberAccess"/> from <see cref="Attributes"/>.
        /// </summary>
        public const ushort MemberAccessMask = 0x0003;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAttributes"/> class.
        /// </summary>
        /// <param name="attributes">Member attributes value.</param>
        public MemberAttributes(ushort attributes)
        {
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the member attributes value.
        /// </summary>
        public ushort Attributes { get; private set; }

        /// <summary>
        /// Gets the <see cref="MemberAccess"/> from <see cref="Attributes"/>.
        /// </summary>
        public MemberAccess Access => (MemberAccess)(Attributes & MemberAccessMask);

        /// <summary>
        /// Gets the <see cref="MethodKind"/> from <see cref="Attributes"/>.
        /// </summary>
        public MethodKind MethodKind => (MethodKind)((Attributes & MethodKindMask) >> MethodKindShift);

        /// <summary>
        /// Gets the <see cref="MethodOptions"/> from <see cref="Attributes"/>.
        /// </summary>
        public MethodOptions Flags => (MethodOptions)(Attributes & ~(MemberAccessMask | MethodKindMask));

        /// <summary>
        /// Gets the flag indicating that this member is virtual.
        /// </summary>
        public bool IsVirtual => MethodKind == MethodKind.Virtual || MethodKind == MethodKind.PureVirtual || IsIntroducedVirtual;

        /// <summary>
        /// Gets the flag indicating that this member introduced a new virtual method.
        /// </summary>
        public bool IsIntroducedVirtual => MethodKind == MethodKind.IntroducingVirtual || MethodKind == MethodKind.PureIntroducingVirtual;

        /// <summary>
        /// Reads <see cref="MemberAttributes"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static MemberAttributes Read(IBinaryReader reader)
        {
            return new MemberAttributes(reader.ReadUshort());
        }
    }
}
