using System;
using System.Text;

namespace CsDebugScript.PdbSymbolProvider.Utility
{
    /// <summary>
    /// Represents <see cref="IBinaryReader"/> on top of the existing stream mapped with array of blocks of the same size.
    /// </summary>
    /// <typeparam name="TStream">Base stream type.</typeparam>
    internal class MappedBlockBinaryReader<TStream> : IBinaryReader
        where TStream : IBinaryReader
    {
        /// <summary>
        /// Current position in the stream.
        /// </summary>
        private long position;

        /// <summary>
        /// Index of the block that is currently being read.
        /// </summary>
        private int blockIndex;

        /// <summary>
        /// Number of bytes that still remains in the block that is currently being read.
        /// </summary>
        private uint blockRemaining;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedBlockBinaryReader{TStream}"/> class.
        /// </summary>
        /// <param name="blocks">Array of block indexes in the parent stream.</param>
        /// <param name="blockSize">Single block size in bytes.</param>
        /// <param name="length">Length of this stream in bytes.</param>
        /// <param name="baseReader">Base stream binary reader.</param>
        public MappedBlockBinaryReader(uint[] blocks, uint blockSize, long length, TStream baseReader)
        {
            Blocks = blocks;
            BlockSize = blockSize;
            BaseReader = (TStream)baseReader.Duplicate();
            Length = length;
            Position = 0;
        }

        /// <summary>
        /// Gets the array of block indexes in the parent stream.
        /// </summary>
        public uint[] Blocks { get; private set; }

        /// <summary>
        /// Gets a single block size in bytes.
        /// </summary>
        public uint BlockSize { get; private set; }

        /// <summary>
        /// Gets the base stream binary reader.
        /// </summary>
        public TStream BaseReader { get; private set; }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Gets the remaining number of bytes in the stream.
        /// </summary>
        public long BytesRemaining => Length - Position;

        /// <summary>
        /// Gets or sets the position in the stream.
        /// </summary>
        public long Position
        {
            get
            {
                return position;
            }

            set
            {
                if (Blocks.Length > 0)
                {
                    long offset = value;

                    if (offset < 0)
                        offset = 0;
                    else if (offset > Length)
                        offset = Length;
                    long blockIndex = offset / BlockSize;
                    uint blockPosition = (uint)(offset % BlockSize);
                    long readerPosition = Blocks[blockIndex] * (long)BlockSize + blockPosition;
                    BaseReader.Position = readerPosition;
                    position = offset;
                    this.blockIndex = (int)blockIndex;
                    blockRemaining = BlockSize - blockPosition;
                }
                else
                {
                    position = 0;
                    blockRemaining = 0;
                    this.blockIndex = 0;
                }
            }
        }

        /// <summary>
        /// Creates duplicate of this stream.
        /// </summary>
        public IBinaryReader Duplicate()
        {
            return new MappedBlockBinaryReader<TStream>(Blocks, BlockSize, Length, BaseReader)
            {
                Position = Position,
            };
        }

        /// <summary>
        /// Reads <c>byte</c> from the stream.
        /// </summary>
        public byte ReadByte()
        {
            byte[] buffer = MoveInternal(1);
            byte value;

            if (buffer != null)
                value = buffer[0];
            else
                value = BaseReader.ReadByte();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads <c>short</c> from the stream.
        /// </summary>
        public short ReadShort()
        {
            byte[] buffer = MoveInternal(2);
            short value;

            if (buffer != null)
                value = BitConverter.ToInt16(buffer, 0);
            else
                value = BaseReader.ReadShort();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads <c>ushort</c> from the stream.
        /// </summary>
        public ushort ReadUshort()
        {
            byte[] buffer = MoveInternal(2);
            ushort value;

            if (buffer != null)
                value = BitConverter.ToUInt16(buffer, 0);
            else
                value = BaseReader.ReadUshort();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads <c>int</c> from the stream.
        /// </summary>
        public int ReadInt()
        {
            byte[] buffer = MoveInternal(4);
            int value;

            if (buffer != null)
                value = BitConverter.ToInt32(buffer, 0);
            else
                value = BaseReader.ReadInt();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads <c>uint</c> from the stream.
        /// </summary>
        public uint ReadUint()
        {
            byte[] buffer = MoveInternal(4);
            uint value;

            if (buffer != null)
                value = BitConverter.ToUInt32(buffer, 0);
            else
                value = BaseReader.ReadUint();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads <c>long</c> from the stream.
        /// </summary>
        public long ReadLong()
        {
            byte[] buffer = MoveInternal(8);
            long value;

            if (buffer != null)
                value = BitConverter.ToInt64(buffer, 0);
            else
                value = BaseReader.ReadLong();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads <c>ulong</c> from the stream.
        /// </summary>
        public ulong ReadUlong()
        {
            byte[] buffer = MoveInternal(8);
            ulong value;

            if (buffer != null)
                value = BitConverter.ToUInt64(buffer, 0);
            else
                value = BaseReader.ReadUlong();
            CheckMoveReader();
            return value;
        }

        /// <summary>
        /// Reads C-style string (null terminated) from the stream.
        /// </summary>
        public string ReadCString()
        {
            long basePosition = BaseReader.Position;
            string value = BaseReader.ReadCString();
            uint size = (uint)value.Length + 1;

            if (size <= blockRemaining)
            {
                blockRemaining -= size;
                position += size;
                CheckMoveReader();
                return value;
            }

            // Check if we are reading from two consecutive blocks
            if (size < BlockSize * 2 && blockIndex + 1 < Blocks.Length && Blocks[blockIndex] + 1 == Blocks[blockIndex + 1])
            {
                uint secondBlockRead = size - blockRemaining;

                position += size;

                // Seek for next block
                blockIndex++;
                if (blockIndex + 1 == Blocks.Length)
                    blockRemaining = (uint)(Length - position);
                else
                    blockRemaining = BlockSize;
                blockRemaining -= secondBlockRead;
                return value;
            }

            // Rewind and fallback to slow reader (byte per byte)
            BaseReader.Position = basePosition;

            StringBuilder sb = new StringBuilder();
            byte b = ReadByte();

            while (b != 0)
            {
                sb.Append((char)b);
                b = ReadByte();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Moves position by the specified bytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to move the stream.</param>
        public void ReadFake(uint bytes)
        {
            while (bytes > blockRemaining)
            {
                bytes -= blockRemaining;
                ReadFake(blockRemaining);
            }

            position += bytes;
            blockRemaining -= bytes;
            BaseReader.ReadFake(bytes);
            CheckMoveReader();
        }

        /// <summary>
        /// Checks if we have encountered end of current block and moves base reader to the position of new block.
        /// </summary>
        private void CheckMoveReader()
        {
            if (blockRemaining == 0)
            {
                // Seek for next block
                blockIndex++;
                if (blockIndex < Blocks.Length)
                {
                    if (blockIndex + 1 == Blocks.Length)
                        blockRemaining = (uint)(Length - position);
                    else
                        blockRemaining = BlockSize;
                    if (Blocks[blockIndex - 1] + 1 != Blocks[blockIndex])
                        BaseReader.Position = Blocks[blockIndex] * (long)BlockSize;
                }
            }
        }

        /// <summary>
        /// Moves all internal data for the specified number of bytes. If not all bytes are available from current block,
        /// it returns requested number of bytes read from current and folowing block.
        /// </summary>
        /// <param name="size">Number of bytes that will be read.</param>
        /// <returns><c>null</c> if specified number of bytes is available in the current block; otherwise bytes that are read from the current position.</returns>
        private byte[] MoveInternal(uint size)
        {
            if (blockRemaining < size)
            {
                // Check if blocks are consecutive
                if (blockIndex + 1 < Blocks.Length && Blocks[blockIndex] + 1 == Blocks[blockIndex + 1])
                {
                    // Seek to the next block
                    if (blockIndex + 2 == Blocks.Length)
                        blockRemaining = (uint)(Length - position);
                    else
                        blockRemaining += BlockSize;
                    blockIndex++;
                }
                else
                {
                    // We would like here to read byte buffer and read uint from it.
                    return this.ReadByteArray((int)size);
                }
            }

            position += size;
            blockRemaining -= size;
            return null;
        }
    }
}
