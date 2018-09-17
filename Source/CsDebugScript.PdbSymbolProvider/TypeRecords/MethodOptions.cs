#pragma warning disable 1591

using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_fldattr_t bitfield.
    /// </summary>
    [Flags]
    public enum MethodOptions : ushort
    {
        /// <summary>
        /// No method options.
        /// </summary>
        None = 0x0000,
        Pseudo = 0x0020,
        NoInherit = 0x0040,
        NoConstruct = 0x0080,
        CompilerGenerated = 0x0100,
        Sealed = 0x0200
    }
}
