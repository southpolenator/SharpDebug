namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Type of the simple built-in type.
    /// </summary>
    public enum SimpleTypeKind : uint
    {
        /// <summary>
        /// Uncharacterized type (no type)
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Void type.
        /// </summary>
        Void = 0x0003,

        /// <summary>
        /// Type not translated by cvpack.
        /// </summary>
        NotTranslated = 0x0007,

        /// <summary>
        /// OLE/COM HRESULT.
        /// </summary>
        HResult = 0x0008,

        /// <summary>
        /// 8 bit signed.
        /// </summary>
        SignedCharacter = 0x0010,

        /// <summary>
        /// 8 bit unsigned.
        /// </summary>
        UnsignedCharacter = 0x0020,

        /// <summary>
        /// Really a <c>char</c>.
        /// </summary>
        NarrowCharacter = 0x0070,

        /// <summary>
        /// Wide char (<c>wchar_t</c>).
        /// </summary>
        WideCharacter = 0x0071,

        /// <summary>
        /// char16_t
        /// </summary>
        Character16 = 0x007a,

        /// <summary>
        /// char32_t
        /// </summary>
        Character32 = 0x007b,

        /// <summary>
        /// 8 bit signed int
        /// </summary>
        SByte = 0x0068,

        /// <summary>
        /// 8 bit unsigned int
        /// </summary>
        Byte = 0x0069,

        /// <summary>
        /// 16 bit signed
        /// </summary>
        Int16Short = 0x0011,

        /// <summary>
        /// 16 bit unsigned
        /// </summary>
        UInt16Short = 0x0021,

        /// <summary>
        /// 16 bit signed int
        /// </summary>
        Int16 = 0x0072,

        /// <summary>
        /// 16 bit unsigned int
        /// </summary>
        UInt16 = 0x0073,

        /// <summary>
        /// 32 bit signed
        /// </summary>
        Int32Long = 0x0012,

        /// <summary>
        /// 32 bit unsigned
        /// </summary>
        UInt32Long = 0x0022,

        /// <summary>
        /// 32 bit signed int
        /// </summary>
        Int32 = 0x0074,

        /// <summary>
        /// 32 bit unsigned int
        /// </summary>
        UInt32 = 0x0075,

        /// <summary>
        /// 64 bit signed
        /// </summary>
        Int64Quad = 0x0013,

        /// <summary>
        /// 64 bit unsigned
        /// </summary>
        UInt64Quad = 0x0023,

        /// <summary>
        /// 64 bit signed int
        /// </summary>
        Int64 = 0x0076,

        /// <summary>
        /// 64 bit unsigned int
        /// </summary>
        UInt64 = 0x0077,

        /// <summary>
        /// 128 bit signed int
        /// </summary>
        Int128Oct = 0x0014,

        /// <summary>
        /// 128 bit unsigned int
        /// </summary>
        UInt128Oct = 0x0024,

        /// <summary>
        /// 128 bit signed int
        /// </summary>
        Int128 = 0x0078,

        /// <summary>
        /// 128 bit unsigned int
        /// </summary>
        UInt128 = 0x0079,

        /// <summary>
        /// 16 bit real
        /// </summary>
        Float16 = 0x0046,

        /// <summary>
        /// 32 bit real
        /// </summary>
        Float32 = 0x0040,

        /// <summary>
        /// 32 bit PP real
        /// </summary>
        Float32PartialPrecision = 0x0045,

        /// <summary>
        /// 48 bit real
        /// </summary>
        Float48 = 0x0044,

        /// <summary>
        /// 64 bit real
        /// </summary>
        Float64 = 0x0041,

        /// <summary>
        /// 80 bit real
        /// </summary>
        Float80 = 0x0042,

        /// <summary>
        /// 128 bit real
        /// </summary>
        Float128 = 0x0043,

        /// <summary>
        /// 16 bit complex
        /// </summary>
        Complex16 = 0x0056,

        /// <summary>
        /// 32 bit complex
        /// </summary>
        Complex32 = 0x0050,

        /// <summary>
        /// 32 bit PP complex
        /// </summary>
        Complex32PartialPrecision = 0x0055,

        /// <summary>
        /// 48 bit complex
        /// </summary>
        Complex48 = 0x0054,

        /// <summary>
        /// 64 bit complex
        /// </summary>
        Complex64 = 0x0051,

        /// <summary>
        /// 80 bit complex
        /// </summary>
        Complex80 = 0x0052,

        /// <summary>
        /// 128 bit complex
        /// </summary>
        Complex128 = 0x0053,

        /// <summary>
        /// 8 bit boolean
        /// </summary>
        Boolean8 = 0x0030,

        /// <summary>
        /// 16 bit boolean
        /// </summary>
        Boolean16 = 0x0031,

        /// <summary>
        /// 32 bit boolean
        /// </summary>
        Boolean32 = 0x0032,

        /// <summary>
        /// 64 bit boolean
        /// </summary>
        Boolean64 = 0x0033,

        /// <summary>
        /// 128 bit boolean
        /// </summary>
        Boolean128 = 0x0034,
    }
}
