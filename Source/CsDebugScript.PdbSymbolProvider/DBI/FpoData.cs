using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// One of the records in FPO stream.
    /// </summary>
    public struct FpoData
    {
        /// <summary>
        /// Number of bytes needed to store this structure.
        /// </summary>
        public const int Size = 16;

        /// <summary>
        /// Gets the offset of 1st byte of function code
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the function code size in bytes.
        /// </summary>
        public uint ProcedureSize { get; private set; }

        /// <summary>
        /// Gets the number of bytes in locals/4.
        /// </summary>
        public uint NumLocals { get; private set; }

        /// <summary>
        /// Gets the number of bytes in params/4.
        /// </summary>
        public ushort NumParams { get; private set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        public ushort Attributes { get; private set; }

        /// <summary>
        /// Reads <see cref="FpoData"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static FpoData Read(IBinaryReader reader)
        {
            return new FpoData
            {
                Offset = reader.ReadUint(),
                ProcedureSize = reader.ReadUint(),
                NumLocals = reader.ReadUint(),
                NumParams = reader.ReadUshort(),
                Attributes = reader.ReadUshort(),
            };
        }
    }
}
