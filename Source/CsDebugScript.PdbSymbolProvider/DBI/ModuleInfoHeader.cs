using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI module descriptor header.
    /// Following this header are two zero terminated strings.
    /// char ModuleName[];
    /// char ObjFileName[];
    /// </summary>
    public struct ModuleInfoHeader
    {
        /// <summary>
        /// Currently opened module. This field is a pointer in the reference
        /// implementation, but that won't work on 64-bit systems, and anyway it
        /// doesn't make sense to read a pointer from a file. For now it is unused,
        /// so just ignore it.
        /// </summary>
        public uint ModulePointer { get; private set; }

        /// <summary>
        /// First section contribution of this module.
        /// </summary>
        public SectionContributionEntry SectionContribution { get; private set; }

        /// <summary>
        /// Module info flags
        /// </summary>
        public ModuleInfoFlags Flags { get; private set; }

        /// <summary>
        /// Stream index of module debug info.
        /// </summary>
        public ushort ModuleStreamIndex { get; private set; }

        /// <summary>
        /// Size of local symbol debug info in above stream
        /// </summary>
        public uint SymbolDebugInfoByteSize { get; private set; }

        /// <summary>
        /// Size of C11 line number info in above stream
        /// </summary>
        public uint C11LineInfoByteSize { get; private set; }

        /// <summary>
        /// Size of C13 line number info in above stream
        /// </summary>
        public uint C13LineInfoByteSize { get; private set; }

        /// <summary>
        /// Number of files contributing to this module
        /// </summary>
        public ushort NumberOfFiles { get; private set; }

        /// <summary>
        /// Padding so the next field is 4-byte aligned.
        /// </summary>
        public ushort Padding { get; private set; }

        /// <summary>
        /// Array of [0..<see cref="NumberOfFiles"/>) DBI name buffer offsets. In the reference
        /// implementation this field is a pointer. But since you can't portably
        /// serialize a pointer, on 64-bit platforms they copy all the values except
        /// this one into the 32-bit version of the struct and use that for
        /// serialization. Regardless, this field is unused, it is only there to
        /// store a pointer that can be accessed at runtime.
        /// </summary>
        public uint FileNameOffsetsPointer { get; private set; }

        /// <summary>
        /// Name Index for source file name.
        /// </summary>
        public uint SourceFileNameIndex { get; private set; }

        /// <summary>
        /// Name Index for path to compiler PDB.
        /// </summary>
        public uint PdbFilePathNameIndex { get; private set; }

        /// <summary>
        /// Reads <see cref="ModuleInfoHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static ModuleInfoHeader Read(IBinaryReader reader)
        {
            return new ModuleInfoHeader
            {
                ModulePointer = reader.ReadUint(),
                SectionContribution = SectionContributionEntry.Read(reader),
                Flags = ModuleInfoFlags.Read(reader),
                ModuleStreamIndex = reader.ReadUshort(),
                SymbolDebugInfoByteSize = reader.ReadUint(),
                C11LineInfoByteSize = reader.ReadUint(),
                C13LineInfoByteSize = reader.ReadUint(),
                NumberOfFiles = reader.ReadUshort(),
                Padding = reader.ReadUshort(),
                FileNameOffsetsPointer = reader.ReadUint(),
                SourceFileNameIndex = reader.ReadUint(),
                PdbFilePathNameIndex = reader.ReadUint(),
            };
        }
    }
}
