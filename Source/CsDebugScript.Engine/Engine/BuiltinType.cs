namespace CsDebugScript.Engine
{
    /// <summary>
    /// Defines built-in types available from the debugged code.
    /// </summary>
    public enum BuiltinType
    {
        /// <summary>
        /// We couldn't determine underlying built-in type or it is unsupported for selected <see cref="CodeTypeTag"/>.
        /// </summary>
        NoType,

        /// <summary>
        /// The void type.
        /// </summary>
        Void,

        /// <summary>
        /// The boolean type.
        /// </summary>
        Bool,

        /// <summary>
        /// The 8-bit integer type.
        /// </summary>
        Int8,

        /// <summary>
        /// The 16-bit integer type.
        /// </summary>
        Int16,

        /// <summary>
        /// The 32-bit integer type.
        /// </summary>
        Int32,

        /// <summary>
        /// The 64-bit integer type.
        /// </summary>
        Int64,

        /// <summary>
        /// The 128-bit integer type.
        /// </summary>
        Int128,

        /// <summary>
        /// The 8-bit unsigned integer type.
        /// </summary>
        UInt8,

        /// <summary>
        /// The 16-bit unsigned integer type.
        /// </summary>
        UInt16,

        /// <summary>
        /// The 32-bit unsigned integer type.
        /// </summary>
        UInt32,

        /// <summary>
        /// The 64-bit unsigned integer type.
        /// </summary>
        UInt64,

        /// <summary>
        /// The 128-bit unsigned integer type.
        /// </summary>
        UInt128,

        /// <summary>
        /// The 8-bit char type
        /// </summary>
        Char8,

        /// <summary>
        /// The 16-bit char type
        /// </summary>
        Char16,

        /// <summary>
        /// The 32-bit char type
        /// </summary>
        Char32,

        /// <summary>
        /// The 32-bit floating point type
        /// </summary>
        Float32,

        /// <summary>
        /// The 64-bit floating point type
        /// </summary>
        Float64,

        /// <summary>
        /// The 80-bit floating point type
        /// </summary>
        Float80,
    }
}
