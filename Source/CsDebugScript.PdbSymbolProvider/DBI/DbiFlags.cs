using System;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI stream header flags.
    /// </summary>
    [Flags]
    public enum DbiFlags : ushort
    {
        /// <summary>
        /// Was is linked incrementally.
        /// </summary>
        Incremental = 0x0001,

        /// <summary>
        /// Are private symbols present?
        /// </summary>
        Stripped = 0x0002,

        /// <summary>
        /// Was it linked with /debug:ctypes flag?
        /// </summary>
        HasCTypes = 0x0004,
    }
}
