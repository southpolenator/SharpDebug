using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents build info type record.
    /// </summary>
    public class BuildInfoRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_STRING_ID
        };

        /// <summary>
        /// Gets the build info indexes.
        /// </summary>
        public TypeIndex[] Indexes { get; private set; }

        /// <summary>
        /// Reads <see cref="BuildInfoRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static BuildInfoRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            uint count = reader.ReadUshort();
            TypeIndex[] indexes = new TypeIndex[count];

            for (int i = 0; i < indexes.Length; i++)
                indexes[i] = TypeIndex.Read(reader);
            return new BuildInfoRecord
            {
                Kind = kind,
                Indexes = indexes,
            };
        }
    }
}
