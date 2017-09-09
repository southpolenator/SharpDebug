using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies a flags that controls how the executable name is matched.
    /// </summary>
    [Flags]
    public enum DebugGetProc : uint
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_GET_PROC_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// ExeName specifies the full path name of the executable file name.
        /// If this flag is not set, this method will not use path names when searching for the process.
        /// <note>Maps to DEBUG_GET_PROC_FULL_MATCH.</note>
        /// </summary>
        FullMatch = 1,

        /// <summary>
        /// Require that only one process match the executable file name ExeName.
        /// <note>Maps to DEBUG_GET_PROC_ONLY_MATCH.</note>
        /// </summary>
        OnlyMatch = 2,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_GET_PROC_SERVICE_NAME.</note>
        /// </summary>
        ServiceName = 4,
    }
}
