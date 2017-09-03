namespace DIA
{
    /// <summary>
    /// Indicates the particular scope of a data value.
    /// </summary>
    public enum DataKind : uint
    {
        /// <summary>
        /// Data symbol cannot be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Data item is a local variable.
        /// </summary>
        Local,

        /// <summary>
        /// Data item is a static local variable.
        /// </summary>
        StaticLocal,

        /// <summary>
        /// Data item is a formal parameter.
        /// </summary>
        Param,

        /// <summary>
        /// Data item is an object pointer (this).
        /// </summary>
        ObjectPtr,

        /// <summary>
        /// Data item is a file-scoped variable.
        /// </summary>
        FileStatic,

        /// <summary>
        /// Data item is a global variable.
        /// </summary>
        Global,

        /// <summary>
        /// Data item is an object member variable.
        /// </summary>
        Member,

        /// <summary>
        /// Data item is a class static variable.
        /// </summary>
        StaticMember,

        /// <summary>
        /// Data item is a constant value.
        /// </summary>
        Constant,
    };
}
