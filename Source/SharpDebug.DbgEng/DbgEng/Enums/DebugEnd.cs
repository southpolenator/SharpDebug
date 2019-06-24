namespace DbgEng
{
    /// <summary>
    /// Specifies how to end the session.
    /// </summary>
    public enum DebugEnd : uint
    {
        /// <summary>
        /// Perform cleanup for the session.
        /// <note>Maps to DEBUG_END_PASSIVE.</note>
        /// </summary>
        Passive = 0,

        /// <summary>
        /// Attempt to terminate all user-mode targets before performing cleanup for the session.
        /// <note>Maps to DEBUG_END_ACTIVE_TERMINATE.</note>
        /// </summary>
        ActiveTerminate,

        /// <summary>
        /// Attempt to disconnect from all targets before performing cleanup for the session.
        /// <note>Maps to DEBUG_END_ACTIVE_DETACH.</note>
        /// </summary>
        ActiveDetach,

        /// <summary>
        /// Perform only the cleanup that doesn't require acquiring locks.
        /// <note>Maps to DEBUG_END_REENTRANT.</note>
        /// </summary>
        Reentrant,

        /// <summary>
        /// Do not end the session. Disconnect the client from the session and disable the client.
        /// This flag is intended for when remote clients disconnect. It generates a server message about the disconnection.
        /// <note>Maps to DEBUG_END_DISCONNECT.</note>
        /// </summary>
        Disconnect,
    }
}
