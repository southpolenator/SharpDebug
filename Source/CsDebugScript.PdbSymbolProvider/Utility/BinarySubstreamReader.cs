namespace CsDebugScript.PdbSymbolProvider.Utility
{
    /// <summary>
    /// Represents substream <see cref="IBinaryReader"/>.
    /// </summary>
    /// <typeparam name="TStream">Type of the substream.</typeparam>
    internal class BinarySubstreamReader<TStream> : IBinaryReader
        where TStream : IBinaryReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MMFileReader"/> class.
        /// </summary>
        /// <param name="parentStream">Parent stream binary reader.</param>
        /// <param name="parentOffset">Position in the parent stream that is the beginning of this stream.</param>
        /// <param name="length">Length of this stream.</param>
        public BinarySubstreamReader(TStream parentStream, long parentOffset, long length)
        {
            ParentOffset = parentOffset;
            ParentStream = (TStream)parentStream.Duplicate();
            Length = length;
        }

        /// <summary>
        /// Gets the position in the parent stream that is the beginning of this stream.
        /// </summary>
        public long ParentOffset { get; private set; }

        /// <summary>
        /// Gets the parent stream binary reader.
        /// </summary>
        public TStream ParentStream { get; private set; }

        /// <summary>
        /// Gets or sets the position in the stream.
        /// </summary>
        public long Position { get => ParentStream.Position - ParentOffset; set => ParentStream.Position = value + ParentOffset; }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Gets the remaining number of bytes in the stream.
        /// </summary>
        public long BytesRemaining => Length - Position;

        /// <summary>
        /// Creates duplicate of this stream.
        /// </summary>
        public IBinaryReader Duplicate()
        {
            return new BinarySubstreamReader<TStream>(ParentStream, ParentOffset, Length);
        }

        /// <summary>
        /// Reads <c>byte</c> from the stream.
        /// </summary>
        public byte ReadByte()
        {
            return ParentStream.ReadByte();
        }

        /// <summary>
        /// Reads <c>short</c> from the stream.
        /// </summary>
        public short ReadShort()
        {
            return ParentStream.ReadShort();
        }

        /// <summary>
        /// Reads <c>ushort</c> from the stream.
        /// </summary>
        public ushort ReadUshort()
        {
            return ParentStream.ReadUshort();
        }

        /// <summary>
        /// Reads <c>int</c> from the stream.
        /// </summary>
        public int ReadInt()
        {
            return ParentStream.ReadInt();
        }

        /// <summary>
        /// Reads <c>uint</c> from the stream.
        /// </summary>
        public uint ReadUint()
        {
            return ParentStream.ReadUint();
        }

        /// <summary>
        /// Reads <c>long</c> from the stream.
        /// </summary>
        public long ReadLong()
        {
            return ParentStream.ReadLong();
        }

        /// <summary>
        /// Reads <c>ulong</c> from the stream.
        /// </summary>
        public ulong ReadUlong()
        {
            return ParentStream.ReadUlong();
        }

        /// <summary>
        /// Reads C-style string (null terminated) from the stream.
        /// </summary>
        public string ReadCString()
        {
            return ParentStream.ReadCString();
        }

        /// <summary>
        /// Moves position by the specified bytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to move the stream.</param>
        public void ReadFake(uint bytes)
        {
            ParentStream.ReadFake(bytes);
        }
    }
}
