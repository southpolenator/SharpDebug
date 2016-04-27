namespace CsDebugScript.Engine.Native
{
    /// <summary>
    /// The EXT_TDOP enumeration is used in the Operation member of the EXT_TYPED_DATA structure to
    /// specify which suboperation the DEBUG_REQUEST_EXT_TYPED_DATA_ANSI Request operation will perform.
    /// </summary>
    internal enum ExtTdop : uint
    {
        /// <summary>
        /// Makes a copy of a typed data description.
        /// </summary>
        Copy,

        /// <summary>
        /// Releases a typed data description.
        /// </summary>
        Release,

        /// <summary>
        /// Returns the value of an expression.
        /// </summary>
        SetFromExpr,

        /// <summary>
        /// Returns the value of an expression. An optional address can be provided as a parameter to the expression.
        /// </summary>
        SetFromU64Expr,

        /// <summary>
        /// Returns a member of a structure.
        /// </summary>
        GetField,

        /// <summary>
        /// Returns the value of an expression. An optional value can be provided as a parameter to the expression.
        /// </summary>
        Evaluate,

        /// <summary>
        /// Returns the type name for typed data.
        /// </summary>
        GetTypeName,

        /// <summary>
        /// Prints the type name for typed data.
        /// </summary>
        OutputTypeName,

        /// <summary>
        /// Prints the value of typed data.
        /// </summary>
        OutputSimpleValue,

        /// <summary>
        /// Prints the type and value for typed data.
        /// </summary>
        OutputFullValue,

        /// <summary>
        /// Determines whether a structure contains a specified member.
        /// </summary>
        HasField,

        /// <summary>
        /// Returns the offset of a member within a structure.
        /// </summary>
        GetFieldOffset,

        /// <summary>
        /// Returns the offset of a member within a structure.
        /// </summary>
        GetArrayElement,

        /// <summary>
        /// Dereferences a pointer, returning the value it points to.
        /// </summary>
        GetDereference,

        /// <summary>
        /// Returns the size of the specified typed data.
        /// </summary>
        GetTypeSize,

        /// <summary>
        /// Prints the definition of the type for the specified typed data.
        /// </summary>
        OutputTypeDefinition,

        /// <summary>
        /// Returns a new typed data description that represents a pointer to specified typed data.
        /// </summary>
        GetPointerTo,

        /// <summary>
        /// Creates a typed data description from a type and memory location.
        /// </summary>
        SetFromTypeIdAndU64,

        /// <summary>
        /// Creates a typed data description representing a pointer to a specified memory location with specified type.
        /// </summary>
        SetPtrFromTypeIdAndU64,

        /// <summary>
        /// Does not specify an operation. Instead, it represents the number of suboperations defined in the EXT_TDOP enumeration. 
        /// </summary>
        Count,
    }
}
