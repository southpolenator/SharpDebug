namespace CsDebugScript.CommonUserTypes.NativeTypes.cv
{
    /// <summary>
    /// Type of array/matrix element.
    /// </summary>
    public enum ElementType : int
    {
        /// <summary>
        /// Single byte
        /// </summary>
        Byte = 0,

        /// <summary>
        /// Single signed byte
        /// </summary>
        SByte = 1,

        /// <summary>
        /// Two bytes: Unsigned short
        /// </summary>
        UShort = 2,

        /// <summary>
        /// Two bytes: Signed short
        /// </summary>
        Short = 3,

        /// <summary>
        /// Four bytes: Signed integer
        /// </summary>
        Int = 4,

        /// <summary>
        /// Four bytes: Float
        /// </summary>
        Float = 5,

        /// <summary>
        /// Eight bytes: Double
        /// </summary>
        Double = 6,

        /// <summary>
        /// User defined type
        /// </summary>
        UserType = 7,

        /// <summary>
        /// Mask to get element type
        /// </summary>
        CoverageMask = 7,
    }
}
