using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies the bit flags describing the target's memory in which the data resides.
    /// </summary>
    [Flags]
    public enum ExtTdf : uint
    {
        /// <summary>
        /// The typed data is in physical memory, and this physical memory uses the default memory caching.
        /// </summary>
        PhysicalDefault = 0x00000002,

        /// <summary>
        /// The typed data is in physical memory, and this physical memory is cached.
        /// </summary>
        PhysicalCached = 0x00000004,

        /// <summary>
        /// The typed data is in physical memory, and this physical memory is uncached.
        /// </summary>
        PhysicalUncached = 0x00000006,

        /// <summary>
        /// The typed data is in physical memory, and this physical memory is write-combined.
        /// </summary>
        PhysicalWriteCombined = 0x00000008,

        /// <summary>
        /// Mask for all physical flags.
        /// </summary>
        PhysicalMemory = 0x0000000e,
    }
}
