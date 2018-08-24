namespace CsDebugScript.PdbSymbolProvider.TPI
{
    /// <summary>
    /// PDB file TPI stream known versions.
    /// </summary>
    public enum PdbTpiVersion : uint
    {
        /// <summary>
        /// Version 4.0 - 1995/04/10.
        /// </summary>
        V40 = 19950410,

        /// <summary>
        /// Version 4.1 - 1995/11/22.
        /// </summary>
        V41 = 19951122,

        /// <summary>
        /// Version 5.0 - 1996/10/31.
        /// </summary>
        V50 = 19961031,

        /// <summary>
        /// Version 7.0 - 1999/09/03.
        /// </summary>
        V70 = 19990903,

        /// <summary>
        /// Version 8.0 - 2004/02/03.
        /// </summary>
        V80 = 20040203,
    }
}
