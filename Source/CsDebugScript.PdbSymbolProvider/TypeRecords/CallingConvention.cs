namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// These values correspond to the CV_call_e enumeration, and are documented
    /// at the following locations:
    ///   https://msdn.microsoft.com/en-us/library/b2fc64ek.aspx
    ///   https://msdn.microsoft.com/en-us/library/windows/desktop/ms680207(v=vs.85).aspx
    /// </summary>
    public enum CallingConvention : byte
    {
        /// <summary>
        /// Near right to left push, caller pops stack.
        /// </summary>
        NearC = 0x00,

        /// <summary>
        /// Far right to left push, caller pops stack.
        /// </summary>
        FarC = 0x01,

        /// <summary>
        /// Near left to right push, callee pops stack.
        /// </summary>
        NearPascal = 0x02,

        /// <summary>
        /// Far left to right push, callee pops stack.
        /// </summary>
        FarPascal = 0x03,

        /// <summary>
        /// Near left to right push with regs, callee pops stack.
        /// </summary>
        NearFast = 0x04,

        /// <summary>
        /// Far left to right push with regs, callee pops stack.
        /// </summary>
        FarFast = 0x05,

        /// <summary>
        /// Near standard call.
        /// </summary>
        NearStdCall = 0x07,

        /// <summary>
        /// Far standard call.
        /// </summary>
        FarStdCall = 0x08,

        /// <summary>
        /// Near sys call.
        /// </summary>
        NearSysCall = 0x09,

        /// <summary>
        /// Far sys call.
        /// </summary>
        FarSysCall = 0x0a,

        /// <summary>
        /// this call (this passed in register)
        /// </summary>
        ThisCall = 0x0b,

        /// <summary>
        /// Mips call
        /// </summary>
        MipsCall = 0x0c,

        /// <summary>
        /// Generic call sequence.
        /// </summary>
        Generic = 0x0d,

        /// <summary>
        /// Alpha call
        /// </summary>
        AlphaCall = 0x0e,

        /// <summary>
        /// PPC call
        /// </summary>
        PpcCall = 0x0f,

        /// <summary>
        /// Hitachi SuperH call
        /// </summary>
        SHCall = 0x10,

        /// <summary>
        /// ARM call
        /// </summary>
        ArmCall = 0x11,

        /// <summary>
        /// AM33 call
        /// </summary>
        AM33Call = 0x12,

        /// <summary>
        /// TriCore Call
        /// </summary>
        TriCall = 0x13,

        /// <summary>
        /// Hitachi SuperH-5 call
        /// </summary>
        SH5Call = 0x14,

        /// <summary>
        /// M32R Call
        /// </summary>
        M32RCall = 0x15,

        /// <summary>
        /// clr call
        /// </summary>
        ClrCall = 0x16,

        /// <summary>
        /// Marker for routines always inlined and thus lacking a convention
        /// </summary>
        Inline = 0x17,

        /// <summary>
        /// Near left to right push with regs, callee pops stack.
        /// </summary>
        NearVector = 0x18,
    }
}
