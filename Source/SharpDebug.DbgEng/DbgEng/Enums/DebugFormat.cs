using System;

namespace DbgEng
{
    /// <summary>
    /// The DEBUG_FORMAT_XXX bit-flags are used by <see cref="IDebugClient2.WriteDumpFile2"/> and <see cref="IDebugClient4.WriteDumpFileWide"/> to determine the format of a crash dump file and, for user-mode Minidumps, what information to include in the file.
    /// </summary>
    [Flags]
    public enum DebugFormat
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_CAB_SECONDARY_ALL_IMAGES.</note>
        /// </summary>
        CabSecondaryAllImages = 268435456,

        /// <summary>
        /// Package the crash dump file in a CAB file. The supplied file name or file handle is used for the CAB file; the crash dump is first created in a temporary file before being moved into the CAB file.
        /// <note>Maps to DEBUG_FORMAT_WRITE_CAB.</note>
        /// </summary>
        WriteCab = 536870912,

        /// <summary>
        /// Include the current symbols and mapped images in the CAB file.
        /// If <see cref="WriteCab"/> is not set, this flag is ignored.
        /// <note>Maps to DEBUG_FORMAT_CAB_SECONDARY_FILES.</note>
        /// </summary>
        CabSecondaryFiles = 1073741824,

        /// <summary>
        /// Do not overwrite existing files.
        /// <note>Maps to DEBUG_FORMAT_NO_OVERWRITE.</note>
        /// </summary>
        NoOverwrite = -2147483648,

        /// <summary>
        /// Add full memory data. All accessible committed pages owned by the target application will be included.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_FULL_MEMORY.</note>
        /// </summary>
        UserSmallFullMemory = 1,

        /// <summary>
        /// Add data about the handles that are associated with the target application.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_HANDLE_DATA.</note>
        /// </summary>
        UserSmallHandleData,

        /// <summary>
        /// Add unloaded module information. This information is available only in Windows Server 2003 and later versions of Windows.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_UNLOADED_MODULES.</note>
        /// </summary>
        UserSmallUnloadedModules = 4,

        /// <summary>
        /// Add indirect memory. A small region of memory that surrounds any address that is referenced by a pointer on the stack or backing store is included.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_INDIRECT_MEMORY.</note>
        /// </summary>
        UserSmallIndirectMemory = 8,

        /// <summary>
        /// Add all data segments within the executable images.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_DATA_SEGMENTS.</note>
        /// </summary>
        UserSmallDataSegments = 16,

        /// <summary>
        /// Set to zero all of the memory on the stack and in the backing store that is not useful for recreating the stack trace. This can make compression of the Minidump more efficient and increase privacy by removing unnecessary information.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_FILTER_MEMORY.</note>
        /// </summary>
        UserSmallFilterMemory = 32,

        /// <summary>
        /// Remove the module paths, leaving only the module names. This is useful for protecting privacy by hiding the directory structure (which may contain the user's name).
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_FILTER_PATHS.</note>
        /// </summary>
        UserSmallFilterPaths = 64,

        /// <summary>
        /// Add the process environment block (PEB) and thread environment block (TEB). This flag can be used to provide Windows system information for threads and processes.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_PROCESS_THREAD_DATA.</note>
        /// </summary>
        UserSmallProcessThreadData = 128,

        /// <summary>
        /// Add all committed private read-write memory pages.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_PRIVATE_READ_WRITE_MEMORY.</note>
        /// </summary>
        UserSmallPrivateReadWriteMemory = 256,

        /// <summary>
        /// Prevent privacy-sensitive data from being included in the Minidump. Currently, this flag excludes from the Minidump data that would have been added due to the following flags being set:
        /// <see cref="UserSmallProcessThreadData"/>,
        /// <see cref="UserSmallFullMemory"/>,
        /// <see cref="UserSmallIndirectMemory"/>,
        /// <see cref="UserSmallPrivateReadWriteMemory"/>.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_NO_OPTIONAL_DATA.</note>
        /// </summary>
        UserSmallNoOptionalData = 512,

        /// <summary>
        /// Add all basic memory information. This is the information returned by the <see cref="IDebugDataSpaces2.QueryVirtual"/> method. The information for all memory is included, not just valid memory, which allows the debugger to reconstruct the complete virtual memory layout from the Minidump.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_FULL_MEMORY_INFO.</note>
        /// </summary>
        UserSmallFullMemoryInfo = 1024,

        /// <summary>
        /// Add additional thread information, which includes execution time, start time, exit time, start address, and exit status.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_THREAD_INFO.</note>
        /// </summary>
        UserSmallThreadInfo = 2048,

        /// <summary>
        /// Add all code segments with the executable images.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_CODE_SEGMENTS.</note>
        /// </summary>
        UserSmallCodeSegments = 4096,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_NO_AUXILIARY_STATE.</note>
        /// </summary>
        UserSmallNoAuxiliaryState = 8192,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_FULL_AUXILIARY_STATE.</note>
        /// </summary>
        UserSmallFullAuxiliaryState = 16384,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_MODULE_HEADERS.</note>
        /// </summary>
        UserSmallModuleHeaders = 32768,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_FILTER_TRIAGE.</note>
        /// </summary>
        UserSmallFilterTriage = 65536,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_ADD_AVX_XSTATE_CONTEXT.</note>
        /// </summary>
        UserSmallAddAvxXstateContext = 131072,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_FORMAT_USER_SMALL_IGNORE_INACCESSIBLE_MEM.</note>
        /// </summary>
        UserSmallIgnoreInaccessibleMem = 134217728,
    }
}
