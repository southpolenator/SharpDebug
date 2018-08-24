using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents enum user type record.
    /// </summary>
    public class EnumRecord : TagRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_ENUM
        };

        /// <summary>
        /// Gets the underlying type of enum.
        /// </summary>
        public TypeIndex UnderlyingType { get; private set; }

        /// <summary>
        /// Reads <see cref="EnumRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static EnumRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            EnumRecord record = new EnumRecord
            {
                Kind = kind,
                MemberCount = reader.ReadUshort(),
                Options = (ClassOptions)reader.ReadUshort(),
                UnderlyingType = TypeIndex.Read(reader),
                FieldList = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };

            if (record.HasUniqueName)
                record.UniqueName = reader.ReadCString();
            return record;
        }
    }
}
