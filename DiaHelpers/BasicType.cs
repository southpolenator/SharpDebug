namespace Dia2Lib
{
    /// <summary>
    /// Specifies the symbol's basic type.
    /// </summary>
    public enum BasicType
    {
        /// <summary>
        /// No basic type is specified.
        /// </summary>
        NoType = 0,

        /// <summary>
        /// Basic type is a void.
        /// </summary>
        Void = 1,

        /// <summary>
        /// Basic type is a char (C/C++ type).
        /// </summary>
        Char = 2,

        /// <summary>
        /// Basic type is a wide (Unicode) character (WCHAR).
        /// </summary>
        WChar = 3,

        /// <summary>
        /// Basic type is signed int (C/C++ type).
        /// </summary>
        Int = 6,

        /// <summary>
        /// Basic type is unsigned int (C/C++ type).
        /// </summary>
        UInt = 7,

        /// <summary>
        /// Basic type is a floating-point number (FLOAT).
        /// </summary>
        Float = 8,

        /// <summary>
        /// Basic type is a binary-coded decimal (BCD).
        /// </summary>
        BCD = 9,

        /// <summary>
        /// Basic type is a Boolean (BOOL).
        /// </summary>
        Bool = 10,

        /// <summary>
        /// Basic type is a long int (C/C++ type).
        /// </summary>
        Long = 13,

        /// <summary>
        /// Basic type is an unsigned long int (C/C++ type).
        /// </summary>
        ULong = 14,

        /// <summary>
        /// Basic type is currency.
        /// </summary>
        Currency = 25,

        /// <summary>
        /// Basic type is date/time (DATE).
        /// </summary>
        Date = 26,

        /// <summary>
        /// Basic type is a variable type structure (VARIANT).
        /// </summary>
        Variant = 27,

        /// <summary>
        /// Basic type is a complex number.
        /// </summary>
        Complex = 28,

        /// <summary>
        /// Basic type is a bit.
        /// </summary>
        Bit = 29,

        /// <summary>
        /// Basic type is a basic or binary string (BSTR).
        /// </summary>
        BSTR = 30,

        /// <summary>
        /// Basic type is an HRESULT.
        /// </summary>
        Hresult = 31
    };
}
