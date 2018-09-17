using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Represents one of the streams from PDB multi stream file (MSF).
    /// </summary>
    public class PdbStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbStream"/> class.
        /// </summary>
        /// <param name="blocks">Array of block indexes in the parent stream.</param>
        /// <param name="length">Length of this stream in bytes.</param>
        /// <param name="file">PDB file that contains this stream.</param>
        public PdbStream(uint[] blocks, uint length, PdbFile file)
        {
            Blocks = blocks;
            Length = length;
            File = file;
            Reader = new MappedBlockBinaryReader<MMFileReader>(blocks, file.SuperBlock.BlockSize, length, file.Reader);
        }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        /// Gets the array of block indexes in the parent stream.
        /// </summary>
        public uint[] Blocks { get; private set; }

        /// <summary>
        /// Gets the PDB file that contains this stream.
        /// </summary>
        public PdbFile File { get; private set; }

        /// <summary>
        /// Gets the binary reader associated with this PDB stream.
        /// </summary>
        public IBinaryReader Reader { get; private set; }
    }
}
