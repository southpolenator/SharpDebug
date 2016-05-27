namespace CsDebugScript
{
    /// <summary>
    /// Region of memory that application is using.
    /// </summary>
    public struct MemoryRegion
    {
        /// <summary>
        /// The base address
        /// </summary>
        public ulong BaseAddress;

        /// <summary>
        /// The region size
        /// </summary>
        public ulong RegionSize;

        internal ulong MemoryStart
        {
            get
            {
                return BaseAddress;
            }
        }

        internal ulong MemoryEnd
        {
            get
            {
                return BaseAddress + RegionSize;
            }

            set
            {
                RegionSize = value - BaseAddress;
            }
        }
    }
}
