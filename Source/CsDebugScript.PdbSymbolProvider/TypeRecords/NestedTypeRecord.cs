using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies nested type definition with classes, structures, unions, or enums.
    /// </summary>
    public class NestedTypeRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_NESTTYPE
        };

        /// <summary>
        /// Padding or unknown field.
        /// </summary>
        public ushort Padding { get; private set; }

        /// <summary>
        /// Gets the type index of nested type.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the name of the nested type.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="NestedTypeRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static NestedTypeRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new NestedTypeRecord
            {
                Kind = kind,
                Padding = reader.ReadUshort(),
                Type = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };
        }
    }
}
