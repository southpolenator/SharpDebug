namespace DIA
{
    /// <summary>
    /// Specifies the stack frame type.
    /// </summary>
    public enum StackFrameTypeEnum : uint
    {
        /// <summary>
        /// Frame pointer omitted; FPO info available.
        /// </summary>
        FPO,

        /// <summary>
        /// Kernel Trap frame.
        /// </summary>
        Trap,

        /// <summary>
        /// Kernel Trap frame.
        /// </summary>
        TSS,

        /// <summary>
        /// Standard EBP stack frame.
        /// </summary>
        Standard,

        /// <summary>
        /// Frame pointer omitted; Frame data info available.
        /// </summary>
        FrameData,

        /// <summary>
        /// Frame that does not have any debug info.
        /// </summary>
        Unknown = uint.MaxValue,
    }
}
