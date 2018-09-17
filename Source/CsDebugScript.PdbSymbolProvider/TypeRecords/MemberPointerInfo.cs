using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Does not correspond to any tag, this is the tail of an <see cref="PointerRecord"/>
    /// if it represents a member pointer.
    /// </summary>
    public class MemberPointerInfo
    {
        /// <summary>
        /// Gets the type index of the containing class of the pointer.
        /// </summary>
        public TypeIndex ContainingType { get; private set; }

        /// <summary>
        /// Gets the pointer to member representation.
        /// </summary>
        public PointerToMemberRepresentation Representation { get; private set; }

        /// <summary>
        /// Reads <see cref="MemberPointerInfo"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static MemberPointerInfo Read(IBinaryReader reader)
        {
            return new MemberPointerInfo
            {
                ContainingType = TypeIndex.Read(reader),
                Representation = (PointerToMemberRepresentation)reader.ReadUshort(),
            };
        }
    }
}
