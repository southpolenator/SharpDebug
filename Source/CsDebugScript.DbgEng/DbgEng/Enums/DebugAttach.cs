using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies the flags that control how the debugger engine attaches to a user-mode process.
    /// </summary>
    [Flags]
    public enum DebugAttach
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_ATTACH_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// Attach to the target noninvasively.
        /// If this flag is set, then the flags <see cref="Existing"/>, <see cref="InvasiveNoInitialBreak"/>, and <see cref="InvasiveResumeProcess"/> must not be set.
        /// <note>Maps to DEBUG_ATTACH_NONINVASIVE.</note>
        /// </summary>
        Noninvasive = 1,

        /// <summary>
        /// Re-attach to an application to which a debugger has already attached (and possibly abandoned).
        /// If this flag is set, then the other <see cref="DebugAttach"/> flags must not be set.
        /// <note>Maps to DEBUG_ATTACH_EXISTING.</note>
        /// </summary>
        Existing = 2,

        /// <summary>
        /// Do not suspend the target's threads when attaching noninvasively.
        /// If this flag is set, then the flag <see cref="Noninvasive"/> must also be set.
        /// <note>Maps to DEBUG_ATTACH_NONINVASIVE_NO_SUSPEND.</note>
        /// </summary>
        NoninvasiveNoSuspend = 4,

        /// <summary>
        /// (Windows XP and later) Do not request an initial break-in when attaching to the target.
        /// If this flag is set, then the flags <see cref="Noninvasive"/> and <see cref="Existing"/> must not be set.
        /// <note>Maps to DEBUG_ATTACH_INVASIVE_NO_INITIAL_BREAK.</note>
        /// </summary>
        InvasiveNoInitialBreak = 8,

        /// <summary>
        /// Resume all of the target's threads when attaching invasively.
        /// If this flag is set, then the flags <see cref="Noninvasive"/> and <see cref="Existing"/> must not be set.
        /// <note>Maps to DEBUG_ATTACH_INVASIVE_RESUME_PROCESS.</note>
        /// </summary>
        InvasiveResumeProcess = 16,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        /// <note>Maps to DEBUG_ATTACH_NONINVASIVE_ALLOW_PARTIAL.</note>
        NoninvasiveAllowPartial = 32,
    }
}
