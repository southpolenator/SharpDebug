using System;

namespace DbgEng
{
    /// <summary>
    /// The flags describing the target's memory in which the data resides.
    /// </summary>
    [Flags]
    public enum DebugTypedDataFlags : uint
    {
        /// <summary>
        /// The data is in the target's memory and is available.
        /// </summary>
        IsInMemory = 0x00000001,

        /// <summary>
        /// Offset is a physical memory address, and the physical memory at Offset uses the default memory caching.
        /// </summary>
        PhysicalDefault = 0x00000002,

        /// <summary>
        /// Offset is a physical memory address, and the physical memory at Offset is cached.
        /// </summary>
        PhysicalCached = 0x00000004,

        /// <summary>
        /// Offset is a physical memory address, and the physical memory at Offset is uncached.
        /// </summary>
        PhysicalUncached = 0x00000006,

        /// <summary>
        /// Offset is a physical memory address, and the physical memory at Offset is write-combined.
        /// </summary>
        PhysicalWriteCombined = 0x00000008,

        /// <summary>
        /// Mask for all physical flags.
        /// </summary>
        PhysicalMemory = 0x0000000e,
    }
}
