using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies the static data member of a class. Once a static data member has been found
    /// in this list, its symbol is found by qualifying the name with its class (T::name) and then
    /// searching the symbol table for a symbol by that name with the correct type index.
    /// </summary>
    public class StaticDataMemberRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_STMEMBER
        };

        /// <summary>
        /// Gets the member field attributes.
        /// </summary>
        public MemberAttributes Attributes { get; private set; }

        /// <summary>
        /// Gets the index to type record for field.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the name of the member field.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="StaticDataMemberRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static StaticDataMemberRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new StaticDataMemberRecord
            {
                Kind = kind,
                Attributes = MemberAttributes.Read(reader),
                Type = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };
        }
    }
}
