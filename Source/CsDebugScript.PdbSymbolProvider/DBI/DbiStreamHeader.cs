using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI stream header structure.
    /// </summary>
    public struct DbiStreamHeader
    {
        /// <summary>
        /// Expected size of the header in bytes.
        /// </summary>
        public const int Size = 64;

        /// <summary>
        /// Gets the version signature.
        /// </summary>
        public int VersionSignature { get; private set; }

        /// <summary>
        /// Gets the version header.
        /// </summary>
        public DbiStreamVersion Version { get; private set; }

        /// <summary>
        /// Gets how "old" is this DBI Stream. Should match the age of the PDB InfoStream.
        /// </summary>
        public uint Age { get; private set; }

        /// <summary>
        /// Gets the global symbol stream index.
        /// </summary>
        public ushort GlobalSymbolStreamIndex { get; private set; }

        /// <summary>
        /// Gets the DBI build number.
        /// </summary>
        public DbiBuildNumber BuildNumber { get; private set; }

        /// <summary>
        /// Gets the public symbols stream index.
        /// </summary>
        public ushort PublicSymbolStreamIndex { get; private set; }

        /// <summary>
        /// Gets the version of mspdbNNN.dll
        /// </summary>
        public ushort PdbDllVersion { get; private set; }

        /// <summary>
        /// Gets the symbol records stream index.
        /// </summary>
        public ushort SymbolRecordStreamIndex { get; private set; }

        /// <summary>
        /// Gets the rbld number of mspdbNNN.dll
        /// </summary>
        public ushort PdbDllRbld { get; private set; }

        /// <summary>
        /// Gets the size of module info stream.
        /// </summary>
        public int ModuleInfoSubstreamSize { get; private set; }

        /// <summary>
        /// Gets the size of section contribution stream.
        /// </summary>
        public int SectionContributionSubstreamSize { get; private set; }

        /// <summary>
        /// Gets the size of section map substream.
        /// </summary>
        public int SectionMapSize { get; private set; }

        /// <summary>
        /// Gets the size of file info substream.
        /// </summary>
        public int FileInfoSize { get; private set; }

        /// <summary>
        /// Gets the size of type server map.
        /// </summary>
        public int TypeServerSize { get; private set; }

        /// <summary>
        /// Gets the index of MFC Type Server.
        /// </summary>
        public uint MFCTypeServerIndex { get; private set; }

        /// <summary>
        /// Gets the size of optional debug header.
        /// </summary>
        public int OptionalDebugHeaderSize { get; private set; }

        /// <summary>
        /// Gets the size of EC stream (what is EC?).
        /// </summary>
        public int ECSubstreamSize { get; private set; }

        /// <summary>
        /// DBI steam flags.
        /// </summary>
        public DbiFlags Flags { get; private set; }

        /// <summary>
        /// Gets the machine type.
        /// </summary>
        public PdbMachineType MachineType { get; private set; }

        /// <summary>
        /// Padding to 64 bytes
        /// </summary>
        public uint Padding { get; private set; }

        /// <summary>
        /// Reads <see cref="DbiStreamHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static DbiStreamHeader Read(IBinaryReader reader)
        {
            return new DbiStreamHeader
            {
                VersionSignature = reader.ReadInt(),
                Version = (DbiStreamVersion)reader.ReadUint(),
                Age = reader.ReadUint(),
                GlobalSymbolStreamIndex = reader.ReadUshort(),
                BuildNumber = DbiBuildNumber.Read(reader),
                PublicSymbolStreamIndex = reader.ReadUshort(),
                PdbDllVersion = reader.ReadUshort(),
                SymbolRecordStreamIndex = reader.ReadUshort(),
                PdbDllRbld = reader.ReadUshort(),
                ModuleInfoSubstreamSize = reader.ReadInt(),
                SectionContributionSubstreamSize = reader.ReadInt(),
                SectionMapSize = reader.ReadInt(),
                FileInfoSize = reader.ReadInt(),
                TypeServerSize = reader.ReadInt(),
                MFCTypeServerIndex = reader.ReadUint(),
                OptionalDebugHeaderSize = reader.ReadInt(),
                ECSubstreamSize = reader.ReadInt(),
                Flags = (DbiFlags)reader.ReadUshort(),
                MachineType = (PdbMachineType)reader.ReadUshort(),
                Padding = reader.ReadUint(),
            };
        }
    }
}
