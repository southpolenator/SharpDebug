using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents string id type record.
    /// </summary>
    public class StringIdRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_STRING_ID
        };

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public TypeIndex Id { get; private set; }

        /// <summary>
        /// Gets the string.
        /// </summary>
        public string String { get; private set; }

        /// <summary>
        /// Reads <see cref="StringIdRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static StringIdRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new StringIdRecord
            {
                Kind = kind,
                Id = TypeIndex.Read(reader),
                String = reader.ReadCString(),
            };
        }
    }
}
