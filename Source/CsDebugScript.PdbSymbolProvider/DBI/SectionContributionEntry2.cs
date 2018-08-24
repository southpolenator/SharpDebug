using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// Newer version of <see cref="SectionContributionEntry"/>.
    /// </summary>
    public struct SectionContributionEntry2
    {
        /// <summary>
        /// Size of <see cref="SectionContributionEntry2"/> structure in bytes.
        /// </summary>
        public const int Size = SectionContributionEntry.Size + 4;

        /// <summary>
        /// Previos version of entry header.
        /// </summary>
        public SectionContributionEntry Base { get; private set; }

        /// <summary>
        /// Gets the COFF section index.
        /// </summary>
        public uint CoffSectionIndex { get; private set; }

        /// <summary>
        /// Reads <see cref="SectionContributionEntry2"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static SectionContributionEntry2 Read(IBinaryReader reader)
        {
            return new SectionContributionEntry2
            {
                Base = SectionContributionEntry.Read(reader),
                CoffSectionIndex = reader.ReadUint(),
            };
        }
    };
}
