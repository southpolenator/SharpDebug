namespace DbgEng
{
    /// <summary>
    /// The DEBUG_DUMP_XXX constants are used by the methods <see cref="IDebugClient.WriteDumpFile"/>, <see cref="IDebugClient2.WriteDumpFile2"/>, and <see cref="IDebugClient4.WriteDumpFileWide"/> to specify the type of crash dump file to create.
    /// </summary>
    public enum DebugDump : uint
    {
        /// <summary>
        /// Creates a Small Memory Dump (kernel-mode) or Minidump (user-mode).
        /// <note>Maps to DEBUG_DUMP_SMALL with aliases DEBUG_KERNEL_SMALL_DUMP and DEBUG_USER_WINDOWS_SMALL_DUMP.</note>
        /// </summary>
        Small = 1024,

        /// <summary>
        /// Creates a Full User-Mode Dump (user-mode) or Kernel Summary Dump (kernel-mode).
        /// </summary>
        /// <note>Maps to DEBUG_DUMP_DEFAULT with aliases DEBUG_KERNEL_DUMP and DEBUG_USER_WINDOWS_DUMP.</note>
        Default,

        /// <summary>
        /// Creates a Complete Memory Dump (kernel-mode only).
        /// <note>Maps to DEBUG_DUMP_FULL with alias DEBUG_KERNEL_FULL_DUMP.</note>
        /// </summary>
        Full,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_DUMP_IMAGE_FILE.</note>
        /// </summary>
        ImageFile,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_DUMP_TRACE_LOG.</note>
        /// </summary>
        TraceLog,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_DUMP_WINDOWS_CE.</note>
        /// </summary>
        WindowsCe,
    }
}
