namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_ptrtype_e.
    /// </summary>
    public enum PointerKind : byte
    {
        /// <summary>
        /// 16 bit pointer
        /// </summary>
        Near16 = 0x00,

        /// <summary>
        /// 16:16 far pointer
        /// </summary>
        Far16 = 0x01,

        /// <summary>
        /// 16:16 huge pointer
        /// </summary>
        Huge16 = 0x02,

        /// <summary>
        /// based on segment
        /// </summary>
        BasedOnSegment = 0x03,

        /// <summary>
        /// based on value of base
        /// </summary>
        BasedOnValue = 0x04,

        /// <summary>
        /// based on segment value of base
        /// </summary>
        BasedOnSegmentValue = 0x05,

        /// <summary>
        /// based on address of base
        /// </summary>
        BasedOnAddress = 0x06,

        /// <summary>
        /// based on segment address of base
        /// </summary>
        BasedOnSegmentAddress = 0x07,

        /// <summary>
        /// based on type
        /// </summary>
        BasedOnType = 0x08,

        /// <summary>
        /// based on self
        /// </summary>
        BasedOnSelf = 0x09,

        /// <summary>
        /// 32 bit pointer
        /// </summary>
        Near32 = 0x0a,

        /// <summary>
        /// 16:32 pointer
        /// </summary>
        Far32 = 0x0b,

        /// <summary>
        /// 64 bit pointer
        /// </summary>
        Near64 = 0x0c,
    }
}
