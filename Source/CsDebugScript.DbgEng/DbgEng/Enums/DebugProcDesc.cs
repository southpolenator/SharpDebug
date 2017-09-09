using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies flags containing options that affect the behavior of <see cref="IDebugClient.GetRunningProcessDescription"/> method.
    /// </summary>
    [Flags]
    public enum DebugProcDesc : uint
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_PROC_DESC_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// Return only file names without path names.
        /// <note>Maps to DEBUG_PROC_DESC_NO_PATHS.</note>
        /// </summary>
        NoPaths = 1,

        /// <summary>
        /// Do not look up service names.
        /// <note>Maps to DEBUG_PROC_DESC_NO_SERVICES.</note>
        /// </summary>
        NoServices = 2,

        /// <summary>
        /// Do not look up MTS package names.
        /// <note>Maps to DEBUG_PROC_DESC_NO_MTS_PACKAGES.</note>
        /// </summary>
        NoMtsPackages = 4,

        /// <summary>
        /// Do not retrieve the command line.
        /// <note>Maps to DEBUG_PROC_DESC_NO_COMMAND_LINE.</note>
        /// </summary>
        NoCommandLine = 8,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_PROC_DESC_NO_SESSION_ID.</note>
        /// </summary>
        NoSessionId = 16,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_PROC_DESC_NO_USER_NAME.</note>
        /// </summary>
        NoUserName = 32,
    }
}
