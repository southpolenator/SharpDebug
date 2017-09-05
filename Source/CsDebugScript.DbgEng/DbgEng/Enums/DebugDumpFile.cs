namespace DbgEng
{
    /// <summary>
    /// Specifies the type of the debug dump file.
    /// </summary>
    public enum DebugDumpFile : uint
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_DUMP_FILE_BASE</note>
        /// </summary>
        Base = uint.MaxValue,

        /// <summary>
        /// File containing paging file information.
        /// <note>Maps to DEBUG_DUMP_FILE_PAGE_FILE_DUMP</note>
        /// </summary>
        PageFileDump = 0,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_DUMP_FILE_LOAD_FAILED_INDEX</note>
        /// </summary>
        LoadFailedIndex = uint.MaxValue,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_DUMP_FILE_ORIGINAL_CAB_INDEX</note>
        /// </summary>
        OriginalCabIndex = uint.MaxValue - 1,
    }
}
