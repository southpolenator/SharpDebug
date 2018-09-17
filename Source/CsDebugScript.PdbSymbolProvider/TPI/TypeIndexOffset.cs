using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TPI
{
    /// <summary>
    /// Used for pseudo-indexing an array of type records. An array of such records
    /// sorted by TypeIndex can allow log(N) lookups even though such a type record
    /// stream does not provide random access.
    /// </summary>
    public struct TypeIndexOffset
    {
        /// <summary>
        /// Size of <see cref="TypeIndexOffset"/> structure in bytes.
        /// </summary>
        public const int Size = 8;

        /// <summary>
        /// Gets the type index.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Reads <see cref="TypeIndexOffset"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static TypeIndexOffset Read(IBinaryReader reader)
        {
            return new TypeIndexOffset
            {
                Type = TypeIndex.Read(reader),
                Offset = reader.ReadUint(),
            };
        }
    }
}
