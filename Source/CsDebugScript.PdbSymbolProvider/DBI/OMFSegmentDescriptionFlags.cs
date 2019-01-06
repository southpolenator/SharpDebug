using System;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// OMF segment description flags.
    /// </summary>
    [Flags]
    public enum OMFSegmentDescriptionFlags : ushort
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Segment is readable.
        /// </summary>
        Read = 1 << 0,

        /// <summary>
        /// Segment is writable.
        /// </summary>
        Write = 1 << 1,

        /// <summary>
        /// Segment is executable.
        /// </summary>
        Execute = 1 << 2,

        /// <summary>
        /// Descriptor describes a 32-bit linear address.
        /// </summary>
        AddressIs32Bit = 1 << 3,

        /// <summary>
        /// Frame represents a selector.
        /// </summary>
        IsSelector = 1 << 8,

        /// <summary>
        /// Frame represents an absolute address.
        /// </summary>
        IsAbsoluteAddress = 1 << 9,

        /// <summary>
        /// If set, descriptor represents a group.
        /// </summary>
        IsGroup = 1 << 10,
    }
}
