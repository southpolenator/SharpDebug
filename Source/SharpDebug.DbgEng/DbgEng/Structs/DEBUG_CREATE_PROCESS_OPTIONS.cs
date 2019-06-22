using System.Runtime.InteropServices;

namespace DbgEng
{
    /// <summary>
    /// The DEBUG_CREATE_PROCESS_OPTIONS structure specifies the process creation options to use when creating a new process.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_CREATE_PROCESS_OPTIONS
    {
        /// <summary>
        /// The flags to use when creating the process.
        /// </summary>
        public DebugCreateProcess CreateFlags;

        /// <summary>
        /// The engine specific flags used when creating the process.
        /// </summary>
        public DebugEcreateProcess EngCreateFlags;

        /// <summary>
        /// The Application Verifier flags. Only used if <see cref="DebugEcreateProcess.UseVerifierFlags"/> is set in the EngCreateFlags field.
        /// </summary>
        public uint VerifierFlags;

        /// <summary>
        /// Set to zero.
        /// </summary>
        public uint Reserved;
    }
}
