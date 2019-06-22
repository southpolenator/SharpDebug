namespace SharpDebug.Engine
{
    /// <summary>
    /// Defines <see cref="CodeType"/> tags.
    /// </summary>
    public enum CodeTypeTag
    {
        /// <summary>
        /// Unsupported by the engine
        /// </summary>
        Unsupported,

        /// <summary>
        /// User defined class
        /// </summary>
        Class,

        /// <summary>
        /// User defined structure
        /// </summary>
        Structure,

        /// <summary>
        /// User defined union
        /// </summary>
        Union,

        /// <summary>
        /// User defined enumeration
        /// </summary>
        Enum,

        /// <summary>
        /// Built-in basic type
        /// </summary>
        BuiltinType,

        /// <summary>
        /// The array type
        /// </summary>
        Array,

        /// <summary>
        /// The pointer type
        /// </summary>
        Pointer,

        /// <summary>
        /// The function type
        /// </summary>
        Function,

        /// <summary>
        /// The base class. It is used only in CodeGen.
        /// </summary>
        BaseClass,

        /// <summary>
        /// Type representing module global variables. Is is used only in CodeGen.
        /// </summary>
        ModuleGlobals,

        /// <summary>
        /// Type representing constant used as template type argument. It is used only in CodeGen.
        /// </summary>
        TemplateArgumentConstant,
    }
}
