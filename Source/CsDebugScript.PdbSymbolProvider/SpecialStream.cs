namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Predefined PDB stream types.
    /// </summary>
    public enum SpecialStream : uint
    {
        /// <summary>
        /// Stream 0 contains the copy of previous version of the MSF directory.
        /// We are not currently using it, but technically if we find the main
        /// MSF is corrupted, we could fallback to it.
        /// </summary>
        OldMSFDirectory = 0,

        /// <summary>
        /// PDB info stream.
        /// </summary>
        StreamPDB = 1,

        /// <summary>
        /// PDB type info (TPI) stream.
        /// </summary>
        StreamTPI = 2,

        /// <summary>
        /// PDB debug info (DBI) stream.
        /// </summary>
        StreamDBI = 3,

        /// <summary>
        /// PDB source info (IPI) stream.
        /// </summary>
        StreamIPI = 4,
    }
}
