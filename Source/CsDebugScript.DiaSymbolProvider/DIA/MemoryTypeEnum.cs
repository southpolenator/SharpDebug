namespace DIA
{
    /// <summary>
    /// Specifies the type of memory to access.
    /// </summary>
    public enum MemoryTypeEnum
    {
        /// <summary>
        /// Accesses only code memory.
        /// </summary>
        Code,

        /// <summary>
        /// Accesses data or stack memory.
        /// </summary>
        Data,

        /// <summary>
        /// Accesses only stack memory.
        /// </summary>
        Stack,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        CodeOnHeap,

        /// <summary>
        /// Accesses any kind of memory.
        /// </summary>
        Any = -1
    }
}
