using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This record is used to indicate the <c>const</c>, <c>volatile</c> and <c>unaligned</c> properties for any particular type.
    /// </summary>
    public class ModifierRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_MODIFIER
        };

        /// <summary>
        /// Gets the type index of the modified type.
        /// </summary>
        public TypeIndex ModifiedType { get; private set; }

        /// <summary>
        /// Gets the modifier options.
        /// </summary>
        public ModifierOptions Modifiers { get; private set; }

        /// <summary>
        /// Reads <see cref="ModifierRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static ModifierRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new ModifierRecord
            {
                Kind = kind,
                ModifiedType = TypeIndex.Read(reader),
                Modifiers = (ModifierOptions)reader.ReadUshort(),
            };
        }
    }
}
