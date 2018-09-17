using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies the name and value of an enumerate within an enumeration.
    /// </summary>
    public class EnumeratorRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_ENUMERATE
        };

        /// <summary>
        /// Gets the member attributes
        /// </summary>
        public MemberAttributes Attritubes { get; private set; }

        /// <summary>
        /// Gets the value of enumeration.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the enumeration name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="EnumeratorRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static EnumeratorRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new EnumeratorRecord
            {
                Kind = kind,
                Attritubes = MemberAttributes.Read(reader),
                Value = reader.ReadEncodedInteger(),
                Name = reader.ReadCString(),
            };
        }
    }
}
