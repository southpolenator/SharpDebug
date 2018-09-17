namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI section contribution substream known versions.
    /// </summary>
    public enum DbiSectionContributionVersion : uint
    {
        /// <summary>
        /// First known version of DBI section contribution substream.
        /// </summary>
        V60 = 0xeffe0000 + 19970605,

        /// <summary>
        /// Second known version of DBI section contribution substream.
        /// </summary>
        V2 = 0xeffe0000 + 20140516
    }
}
