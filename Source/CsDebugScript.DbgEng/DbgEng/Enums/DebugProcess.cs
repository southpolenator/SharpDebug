using System;

namespace DbgEng
{
    /// <summary>
    /// The process options are a bit set that control how the debugger engine treats user-modeprocesses. Some of these process options are global; others are specific to a process.
    /// The process options only apply to live user-mode debugging.
    /// </summary>
    [Flags]
    public enum DebugProcess : uint
    {
        /// <summary>
        /// (Windows XP and later) The debugger automatically detaches itself from the target process when the debugger exits. This is a global setting.
        /// <note>Maps to DEBUG_PROCESS_DETACH_ON_EXIT.</note>
        /// </summary>
        DetachOnExit = 1,

        /// <summary>
        /// (Windows XP and later) The debugger will not debug child processes that are created by this process.
        /// <note>Maps to DEBUG_PROCESS_ONLY_THIS_PROCESS.</note>
        /// </summary>
        OnlyThisProcess = 2,
    }
}
