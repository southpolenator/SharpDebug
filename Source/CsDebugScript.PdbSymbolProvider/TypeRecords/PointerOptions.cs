using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Ordinal specifying mode of pointer.
    /// </summary>
    [Flags]
    public enum PointerOptions : uint
    {
        /// <summary>
        /// Near.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// 16:32 pointer
        /// </summary>
        Flat32 = 0x00000100,

        /// <summary>
        /// Pointer is volatile.
        /// </summary>
        Volatile = 0x00000200,

        /// <summary>
        /// Pointer is const.
        /// </summary>
        Const = 0x00000400,

        /// <summary>
        /// Pointer is unaligned.
        /// </summary>
        Unaligned = 0x00000800,

        /// <summary>
        /// Pointer is restricted.
        /// </summary>
        Restrict = 0x00001000,

        /// <summary>
        /// Pointer is WinRT smart pointer.
        /// </summary>
        WinRTSmartPointer = 0x00080000
    }
}
