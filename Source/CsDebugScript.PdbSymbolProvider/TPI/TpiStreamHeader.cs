using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TPI
{
    /// <summary>
    /// TPI stream header structure.
    /// </summary>
    public struct TpiStreamHeader
    {
        /// <summary>
        /// Size of <see cref="TpiStreamHeader"/> structure in bytes.
        /// </summary>
        public const int Size = 56;

        /// <summary>
        /// Embedded buffer helper structure.
        /// </summary>
        public struct EmbeddedBuffer
        {
            /// <summary>
            /// Gets the buffer offset.
            /// </summary>
            public int Offset { get; private set; }

            /// <summary>
            /// Gets the buffer length in bytes.
            /// </summary>
            public uint Length { get; private set; }

            /// <summary>
            /// Reads <see cref="EmbeddedBuffer"/> from the stream.
            /// </summary>
            /// <param name="reader">Stream binary reader.</param>
            public static EmbeddedBuffer Read(IBinaryReader reader)
            {
                return new EmbeddedBuffer
                {
                    Offset = reader.ReadInt(),
                    Length = reader.ReadUint(),
                };
            }
        }

        /// <summary>
        /// Gets the TPI stream version.
        /// </summary>
        public PdbTpiVersion Version { get; private set; }

        /// <summary>
        /// Gets the header size.
        /// </summary>
        public uint HeaderSize { get; private set; }

        /// <summary>
        /// Gets the type index begin.
        /// </summary>
        public uint TypeIndexBegin { get; private set; }

        /// <summary>
        /// Gets the type index end.
        /// </summary>
        public uint TypeIndexEnd { get; private set; }

        /// <summary>
        /// Gets the type record bytes.
        /// </summary>
        public uint TypeRecordBytes { get; private set; }

        /// <summary>
        /// Gets the hash stream index.
        /// </summary>
        public ushort HashStreamIndex { get; private set; }

        /// <summary>
        /// Gets the hash auxilary stream index.
        /// </summary>
        public ushort HashAuxStreamIndex { get; private set; }

        /// <summary>
        /// Gets the hash key size.
        /// </summary>
        public uint HashKeySize { get; private set; }

        /// <summary>
        /// Gets the number of hash buckets.
        /// </summary>
        public uint HashBucketsCount { get; private set; }

        /// <summary>
        /// Gets the hash value buffer data.
        /// </summary>
        public EmbeddedBuffer HashValueBuffer { get; private set; }

        /// <summary>
        /// Gets the index offset buffer data.
        /// </summary>
        public EmbeddedBuffer IndexOffsetBuffer { get; private set; }

        /// <summary>
        /// Gets the hash adjusters buffer data.
        /// </summary>
        public EmbeddedBuffer HashAdjustersBuffer { get; private set; }

        /// <summary>
        /// Reads <see cref="TpiStreamHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Strem binary reader.</param>
        public static TpiStreamHeader Read(IBinaryReader reader)
        {
            return new TpiStreamHeader
            {
                Version = (PdbTpiVersion)reader.ReadUint(),
                HeaderSize = reader.ReadUint(),
                TypeIndexBegin = reader.ReadUint(),
                TypeIndexEnd = reader.ReadUint(),
                TypeRecordBytes = reader.ReadUint(),
                HashStreamIndex = reader.ReadUshort(),
                HashAuxStreamIndex = reader.ReadUshort(),
                HashKeySize = reader.ReadUint(),
                HashBucketsCount = reader.ReadUint(),
                HashValueBuffer = EmbeddedBuffer.Read(reader),
                IndexOffsetBuffer = EmbeddedBuffer.Read(reader),
                HashAdjustersBuffer = EmbeddedBuffer.Read(reader),
            };
        }
    }
}
