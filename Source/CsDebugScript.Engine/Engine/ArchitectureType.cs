namespace CsDebugScript.Engine
{
    /// <summary>
    /// The architecture type of the computer.
    /// </summary>
    public enum ArchitectureType
    {
        /// <summary>
        /// Should never be exposed except in case of error.
        /// </summary>
        Unknown,

        /// <summary>
        /// The Intel X86 processor family. (32-bit architecture)
        /// </summary>
        X86,

        /// <summary>
        /// The AMD X64 processor family. (64-bit architecture)
        /// </summary>
        Amd64,

        /// <summary>
        /// The Intel X86 code executed over AMD 64 processor family. (32-bit pointers on 64-bit architecture)
        /// </summary>
        X86OverAmd64,

        /// <summary>
        /// The ARM processor family.
        /// </summary>
        Arm,
    }
}
