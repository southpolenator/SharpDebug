using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// Each record of module info substream has this format.
    /// </summary>
    public struct SectionContributionEntry
    {
        /// <summary>
        /// Size of <see cref="SectionContributionEntry"/> structure in bytes.
        /// </summary>
        public const int Size = 28;

        /// <summary>
        /// Gets the section index.
        /// </summary>
        public ushort Section { get; private set; }

        /// <summary>
        /// Padding so the next field is 4-byte aligned.
        /// </summary>
        public ushort Padding1 { get; private set; }

        /// <summary>
        /// Gets the section offset.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the section size.
        /// </summary>
        public int SectionSize { get; private set; }

        /// <summary>
        /// Gets the characteristics of the image.
        /// </summary>
        public ImageSectionCharacteristics Characteristics { get; private set; }

        /// <summary>
        /// Gets the module index.
        /// </summary>
        public ushort ModuleIndex { get; private set; }

        /// <summary>
        /// Padding so the next field is 4-byte aligned.
        /// </summary>
        public ushort Padding2 { get; private set; }

        /// <summary>
        /// Gets the data CRC.
        /// </summary>
        public uint DataCrc { get; private set; }

        /// <summary>
        /// Gets the reloc CRC.
        /// </summary>
        public uint RelocCrc { get; private set; }

        /// <summary>
        /// Reads <see cref="SectionContributionEntry"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static SectionContributionEntry Read(IBinaryReader reader)
        {
            return new SectionContributionEntry
            {
                Section = reader.ReadUshort(),
                Padding1 = reader.ReadUshort(),
                Offset = reader.ReadInt(),
                SectionSize = reader.ReadInt(),
                Characteristics = (ImageSectionCharacteristics)reader.ReadUint(),
                ModuleIndex = reader.ReadUshort(),
                Padding2 = reader.ReadUshort(),
                DataCrc = reader.ReadUint(),
                RelocCrc = reader.ReadUint(),
            };
        }
    }
}
