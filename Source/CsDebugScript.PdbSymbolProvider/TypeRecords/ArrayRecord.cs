using CsDebugScript.PdbSymbolProvider.Utility;
using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents array type record.
    /// </summary>
    public class ArrayRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_ARRAY
        };

        /// <summary>
        /// Gets the type index of each array element.
        /// </summary>
        public TypeIndex ElementType { get; private set; }

        /// <summary>
        /// Gets the type index of indexing variable.
        /// </summary>
        public TypeIndex IndexType { get; private set; }

        /// <summary>
        /// Gets the length of array in bytes.
        /// </summary>
        public ulong Size { get; private set; }

        /// <summary>
        /// Gets the array name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="ArrayRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static ArrayRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new ArrayRecord
            {
                Kind = kind,
                ElementType = TypeIndex.Read(reader),
                IndexType = TypeIndex.Read(reader),
                Size = Convert.ToUInt64(reader.ReadEncodedInteger()),
                Name = reader.ReadCString(),
            };
        }
    }
}
