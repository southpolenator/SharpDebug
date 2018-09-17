namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Base class for <see cref="EnumRecord"/>, <see cref="ClassRecord"/> and <see cref="UnionRecord"/>.
    /// </summary>
    public class TagRecord : TypeRecord
    {
        /// <summary>
        /// Mask for getting <see cref="HfaKind"/> from the <see cref="Options"/>.
        /// </summary>
        public const int HfaKindMask = 0x1800;

        /// <summary>
        /// Bit shift for getting <see cref="HfaKind"/> from the <see cref="Options"/>.
        /// </summary>
        public const int HfaKindShift = 11;

        /// <summary>
        /// Mask for getting <see cref="WindowsRTClassKind"/> from the <see cref="Options"/>.
        /// </summary>
        public const int WinRTKindMask = 0xC000;

        /// <summary>
        /// Bit shift for getting <see cref="WindowsRTClassKind"/> from the <see cref="Options"/>.
        /// </summary>
        public const int WinRTKindShift = 14;

        /// <summary>
        /// Gets the number of elements in the class or structure. This count includes
        /// direct, virtual, and indirect virtual bases, and methods including
        /// overloads, data members, static data members, friends, and so on.
        /// </summary>
        public ushort MemberCount { get; protected set; }

        /// <summary>
        /// Gets the class options.
        /// </summary>
        public ClassOptions Options { get; protected set; }

        /// <summary>
        /// Gets the type index of the field list for this class.
        /// </summary>
        public TypeIndex FieldList { get; protected set; }

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the unique type name.
        /// </summary>
        public string UniqueName { get; protected set; }

        /// <summary>
        /// <c>true</c> if <see cref="Options"/> contain <see cref="ClassOptions.HasUniqueName"/>.
        /// </summary>
        public bool HasUniqueName => (Options & ClassOptions.HasUniqueName) != ClassOptions.None;

        /// <summary>
        /// <c>true</c> if <see cref="Options"/> contain <see cref="ClassOptions.Nested"/>.
        /// </summary>
        public bool IsNested => (Options & ClassOptions.Nested) != ClassOptions.None;

        /// <summary>
        /// <c>true</c> if <see cref="Options"/> contain <see cref="ClassOptions.ForwardReference"/>.
        /// </summary>
        public bool IsForwardReference => (Options & ClassOptions.ForwardReference) != ClassOptions.None;

        /// <summary>
        /// Gets the <see cref="HfaKind"/> from the <see cref="Options"/>.
        /// </summary>
        public HfaKind Hfa => (HfaKind)(((ushort)Options & HfaKindMask) >> HfaKindShift);

        /// <summary>
        /// Gets the <see cref="WindowsRTClassKind"/> from the <see cref="Options"/>.
        /// </summary>
        public WindowsRTClassKind WinRTKind => (WindowsRTClassKind)(((ushort)Options & WinRTKindMask) >> WinRTKindShift);
    }
}
