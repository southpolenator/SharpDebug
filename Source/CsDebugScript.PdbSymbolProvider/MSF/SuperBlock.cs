using CsDebugScript.PdbSymbolProvider.Utility;
using System;
using System.Linq;

namespace CsDebugScript.PdbSymbolProvider.MSF
{
    /// <summary>
    /// The superblock is overlaid at the beginning of the file (offset 0).
    /// It starts with a magic header and is followed by information which
    /// describes the layout of the file system.
    /// </summary>
    public class SuperBlock
    {
        /// <summary>
        /// Magic string that is expected to be at the beginning of the block.
        /// </summary>
        public const string Magic = "Microsoft C/C++ MSF 7.00\r\n\u001aDS\0\0\0";

        /// <summary>
        /// Array of bytes at the beggining of the file. It needs to match <see cref="Magic"/> constant.
        /// </summary>
        public byte[] MagicBytes { get; private set; }

        /// <summary>
        /// The file system is split into a variable number of fixed size elements.
        /// These elements are referred to as blocks. The size of a block may vary
        /// from system to system.
        /// </summary>
        public uint BlockSize { get; private set; }

        /// <summary>
        /// The index of the free block map.
        /// </summary>
        public uint FreeBlockMapBlock { get; private set; }

        /// <summary>
        /// This contains the number of blocks resident in the file system. In
        /// practice, NumBlocks * BlockSize is equivalent to the size of the MSF
        /// file.
        /// </summary>
        public uint NumBlocks { get; private set; }

        /// <summary>
        /// This contains the number of bytes which make up the directory.
        /// </summary>
        public uint NumDirectoryBytes { get; private set; }

        /// <summary>
        /// This field's purpose is not yet known.
        /// </summary>
        public uint Unknown1 { get; private set; }

        /// <summary>
        /// This contains the block # of the block map.
        /// </summary>
        public uint BlockMapAddr { get; private set; }

        /// <summary>
        /// Gets <see cref="MagicBytes"/> as ANSI strings.
        /// </summary>
        public string MagicString => new string(MagicBytes.Select(b => (char)b).ToArray());

        /// <summary>
        /// Gets the block map offset in the file.
        /// </summary>
        public ulong BlockMapOffset => BlockMapAddr * BlockSize;

        /// <summary>
        /// Gets number of block which make up the directory.
        /// </summary>
        public uint NumDirectoryBlocks => BytesToBlocks(NumDirectoryBytes);

        /// <summary>
        /// Gets number of blocks required to store the specified number of bytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to be stored.</param>
        public uint BytesToBlocks(uint bytes)
        {
            return (bytes + BlockSize - 1) / BlockSize;
        }

        /// <summary>
        /// Gets number of bytes stored within the specified number of blocks.
        /// </summary>
        /// <param name="blocks">Number of blocks.</param>
        public ulong BlocksToBytes(uint blocks)
        {
            return blocks * (ulong)BlockSize;
        }

        /// <summary>
        /// Reads <see cref="SuperBlock"/> from the stream.
        /// </summary>
        /// <param name="reader">Binary reader stream.</param>
        public static SuperBlock Read(IBinaryReader reader)
        {
            return new SuperBlock
            {
                MagicBytes = reader.ReadByteArray(Magic.Length),
                BlockSize = reader.ReadUint(),
                FreeBlockMapBlock = reader.ReadUint(),
                NumBlocks = reader.ReadUint(),
                NumDirectoryBytes = reader.ReadUint(),
                Unknown1 = reader.ReadUint(),
                BlockMapAddr = reader.ReadUint()
            };
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public void Validate()
        {
            // Check the magic bytes.
            if (Magic.Length != MagicBytes.Length)
                throw new Exception("MSF magic header doesn't match");
            for (int i = 0; i < MagicBytes.Length; i++)
                if (Magic[i] != MagicBytes[i])
                    throw new Exception("MSF magic header doesn't match");

            if (!IsValidBlockSize(BlockSize))
                throw new Exception("Unsupported block size.");

            // We don't support directories whose sizes aren't a multiple of four bytes.
            if (NumDirectoryBytes % 4 != 0)
                throw new Exception("Directory size is not multiple of 4.");

            // The directory, as we understand it, is a block which consists of a list of
            // block numbers. It is unclear what would happen if the number of blocks
            // couldn't fit on a single block.
            if (NumDirectoryBlocks > BlockSize / 4) // 4 = DirectoryBlock element size
                throw new Exception("Too many directory blocks.");

            if (BlockMapAddr == 0)
                throw new Exception("Block 0 is reserved");

            if (BlockMapAddr >= NumBlocks)
                throw new Exception("Block map address is invalid.");

            if (FreeBlockMapBlock != 1 && FreeBlockMapBlock != 2)
                throw new Exception("The free block map isn't at block 1 or block 2.");
        }

        /// <summary>
        /// Checks wheter the specified block size is valid.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <returns></returns>
        public static bool IsValidBlockSize(uint blockSize)
        {
            switch (blockSize)
            {
                case 512:
                case 1024:
                case 2048:
                case 4096:
                    return true;
                default:
                    return false;
            }
        }
    }
}
