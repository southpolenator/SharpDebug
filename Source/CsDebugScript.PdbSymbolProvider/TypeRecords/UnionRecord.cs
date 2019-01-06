using CsDebugScript.PdbSymbolProvider.Utility;
using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents union type record.
    /// </summary>
    public class UnionRecord : TagRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_UNION
        };

        /// <summary>
        /// Gets the size in bytes of the union.
        /// </summary>
        public ulong Size { get; private set; }

        /// <summary>
        /// Reads <see cref="UnionRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static UnionRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            UnionRecord record = new UnionRecord
            {
                Kind = kind,
                MemberCount = reader.ReadUshort(),
                Options = (ClassOptions)reader.ReadUshort(),
                FieldList = TypeIndex.Read(reader),
                Size = Convert.ToUInt64(reader.ReadEncodedInteger()),
                Name = reader.ReadCString(),
            };

            if (record.HasUniqueName)
                record.UniqueName = reader.ReadCString();
            return record;
        }
    }
}
