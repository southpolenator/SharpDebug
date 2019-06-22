using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies a bit-set that determines which servers to output.
    /// </summary>
    [Flags]
    public enum DebugServers : uint
    {
        /// <summary>
        /// Output the debugging servers on the computer.
        /// <note>Maps to DEBUG_SERVERS_DEBUGGER.</note>
        /// </summary>
        Debugger = 1,

        /// <summary>
        /// Output the process servers on the computer.
        /// <note>Maps to DEBUG_SERVERS_PROCESS.</note>
        /// </summary>
        Process = 2,

        /// <summary>
        /// Output the all servers on the computer.
        /// <note>Maps to DEBUG_SERVERS_ALL.</note>
        /// </summary>
        All = Debugger | Process,
    }
}
