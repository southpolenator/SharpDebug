using System;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Corresponds to the CV_PROCFLAGS bitfield.
    /// </summary>
    [Flags]
    public enum ProcedureFlags : byte
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,

        /// <summary>
        /// Frame pointer is present.
        /// </summary>
        HasFramePointer = 0x01,

        /// <summary>
        /// Has interrupt return.
        /// </summary>
        HasIRET = 0x02,

        /// <summary>
        /// Has far return.
        /// </summary>
        HasFRET = 0x04,

        /// <summary>
        /// Function doesn't return.
        /// </summary>
        IsNoReturn = 0x08,

        /// <summary>
        /// Label isn't fallen into.
        /// </summary>
        IsUnreachable = 0x10,

        /// <summary>
        /// Custom calling convention.
        /// </summary>
        HasCustomCallingConv = 0x20,

        /// <summary>
        /// Function is marked as noinline.
        /// </summary>
        IsNoInline = 0x40,

        /// <summary>
        /// Function has debug information for optimized code.
        /// </summary>
        HasOptimizedDebugInfo = 0x80,
    }
}
