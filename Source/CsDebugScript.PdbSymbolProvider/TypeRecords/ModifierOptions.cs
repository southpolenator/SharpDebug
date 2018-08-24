using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_modifier_t.
    /// </summary>
    [Flags]
    public enum ModifierOptions : ushort
    {
        /// <summary>
        /// No modifiers.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// <c>const</c> modifier.
        /// </summary>
        Const = 0x0001,

        /// <summary>
        /// <c>volatile</c> modifier.
        /// </summary>
        Volatile = 0x0002,

        /// <summary>
        /// <c>unaligned</c> modifier.
        /// </summary>
        Unaligned = 0x0004,
    }
}
