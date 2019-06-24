namespace SharpDebug.CLR
{
    /// <summary>
    /// This is a representation of the metadata element type. These values map CLR's CorElementType.
    /// </summary>
    public enum ClrElementType
    {
        /// <summary>
        /// ELEMENT_TYPE_R8
        /// </summary>
        Double,

        /// <summary>
        /// ELEMENT_TYPE_R4
        /// </summary>
        Float,

        /// <summary>
        /// ELEMENT_TYPE_FNPTR
        /// </summary>
        FunctionPointer,

        /// <summary>
        /// ELEMENT_TYPE_CHAR
        /// </summary>
        Char,

        /// <summary>
        /// ELEMENT_TYPE_BOOLEAN
        /// </summary>
        Boolean,

        /// <summary>
        /// ELEMENT_TYPE_STRING
        /// </summary>
        String,

        /// <summary>
        /// ELEMENT_TYPE_OBJECT
        /// </summary>
        Object,

        /// <summary>
        /// ELEMENT_TYPE_PTR
        /// </summary>
        Pointer,

        /// <summary>
        /// ELEMENT_TYPE_I1
        /// </summary>
        Int8,

        /// <summary>
        /// ELEMENT_TYPE_U1
        /// </summary>
        UInt8,

        /// <summary>
        /// ELEMENT_TYPE_I2
        /// </summary>
        Int16,

        /// <summary>
        /// ELEMENT_TYPE_U2
        /// </summary>
        UInt16,

        /// <summary>
        /// ELEMENT_TYPE_I4
        /// </summary>
        Int32,

        /// <summary>
        /// ELEMENT_TYPE_U4
        /// </summary>
        UInt32,

        /// <summary>
        /// ELEMENT_TYPE_I
        /// </summary>
        NativeInt,

        /// <summary>
        /// ELEMENT_TYPE_U
        /// </summary>
        NativeUInt,

        /// <summary>
        /// ELEMENT_TYPE_I8
        /// </summary>
        Int64,

        /// <summary>
        /// ELEMENT_TYPE_U8
        /// </summary>
        UInt64,

        /// <summary>
        /// ELEMENT_TYPE_ARRAY
        /// </summary>
        Array,

        /// <summary>
        /// ELEMENT_TYPE_CLASS
        /// </summary>
        Class,

        /// <summary>
        /// ELEMENT_TYPE_VALUETYPE
        /// </summary>
        Struct,

        /// <summary>
        /// ELEMENT_TYPE_SZARRAY
        /// </summary>
        SZArray
    }
}
