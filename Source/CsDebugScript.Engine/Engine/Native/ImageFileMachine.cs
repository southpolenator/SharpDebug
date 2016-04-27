namespace CsDebugScript.Engine.Native
{
    /// <summary>
    /// The architecture type of the computer. An image file can only be run on the specified computer
    /// or a system that emulates the specified computer. This member can be one of the following values.
    /// </summary>
    public enum ImageFileMachine
    {
        /// <summary>
        /// x86 architecture
        /// </summary>
        I386 = 0x014c,

        /// <summary>
        /// ARM architecture
        /// </summary>
        ARM = 0x01c0,

        /// <summary>
        /// Intel Itanium architecture
        /// </summary>
        IA64 = 0x0200,

        /// <summary>
        /// x64 architecture
        /// </summary>
        AMD64 = 0x8664,

        /// <summary>
        /// EFI byte code architecture
        /// </summary>
        EBC = 0x0EBC,
    }
}
