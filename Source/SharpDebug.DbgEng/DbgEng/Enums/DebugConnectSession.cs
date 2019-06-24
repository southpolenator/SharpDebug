using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies a bit-set of option flags for connecting to the session.
    /// </summary>
    [Flags]
    public enum DebugConnectSession : uint
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_CONNECT_SESSION_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// Do not output the debugger engine's version to this client.
        /// <note>Maps to DEBUG_CONNECT_SESSION_NO_VERSION.</note>
        /// </summary>
        NoVersion = 1,

        /// <summary>
        /// Do not output a message notifying other clients that this client has connected.
        /// <note>Maps to DEBUG_CONNECT_SESSION_NO_ANNOUNCE.</note>
        /// </summary>
        NoAnnounce = 2,
    }
}
