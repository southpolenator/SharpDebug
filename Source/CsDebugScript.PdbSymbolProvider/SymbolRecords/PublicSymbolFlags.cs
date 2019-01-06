using System;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Corresponds to the CV_PUBSYMFLAGS bitfield.
    /// </summary>
    [Flags]
    public enum PublicSymbolFlags : uint
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Set if internal symbol refers to a code address.
        /// </summary>
        Code = 0x00000001,

        /// <summary>
        /// Set if internal symbol is a function.
        /// </summary>
        Function = 0x00000002,

        /// <summary>
        /// Set if managed code (native or IL).
        /// </summary>
        Managed = 0x00000004,

        /// <summary>
        /// Set if managed IL code.
        /// </summary>
        MSIL = 0x00000008,
    }
}
