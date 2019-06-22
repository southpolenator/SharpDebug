using System;

namespace DbgEng
{
    /// <summary>
    /// The engine specific flags used when creating the process.
    /// </summary>
    [Flags]
    public enum DebugEcreateProcess
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_ECREATE_PROCESS_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// The new process will inherit system handles from the debugger or process server.
        /// <note>Maps to DEBUG_ECREATE_PROCESS_INHERIT_HANDLES.</note>
        /// </summary>
        InheritHandles = 1,

        /// <summary>
        /// (Windows Vista and later) Use Application Verifier flags in the VerifierFlags field.
        /// <note>Maps to DEBUG_ECREATE_PROCESS_USE_VERIFIER_FLAGS.</note>
        /// </summary>
        UseVerifierFlags = 2,

        /// <summary>
        /// Use the debugger's or process server's implicit command line to start the process instead of a supplied command line.
        /// <note>Maps to DEBUG_ECREATE_PROCESS_USE_IMPLICIT_COMMAND_LINE.</note>
        /// </summary>
        UseImplicitCommandLine = 4,
    }
}
