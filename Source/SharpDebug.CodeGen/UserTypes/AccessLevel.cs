namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Enumeration of .NET access levels.
    /// </summary>
    internal enum AccessLevel
    {
        /// <summary>
        /// Unspecified access level - when it is omitted.
        /// </summary>
        Default,

        /// <summary>
        /// Private access level.
        /// </summary>
        Private,

        /// <summary>
        /// Protected access level.
        /// </summary>
        Protected,

        /// <summary>
        /// Internal access level.
        /// </summary>
        Internal,

        /// <summary>
        /// Public access level.
        /// </summary>
        Public,
    }
}
