using CsDebugScript.PdbSymbolProvider.Utility;
using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies non-static data members of a class.
    /// </summary>
    public class DataMemberRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_MEMBER
        };

        /// <summary>
        /// Gets the member attributes.
        /// </summary>
        public MemberAttributes Attributes { get; private set; }

        /// <summary>
        /// Gets the index to type record for field.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the offset of field in the structure.
        /// </summary>
        public ulong FieldOffset { get; private set; }

        /// <summary>
        /// Gets the name of the member field.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="DataMemberRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static DataMemberRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new DataMemberRecord
            {
                Kind = kind,
                Attributes = MemberAttributes.Read(reader),
                Type = TypeIndex.Read(reader),
                FieldOffset = Convert.ToUInt64(reader.ReadEncodedInteger()),
                Name = reader.ReadCString(),
            };
        }
    }
}
