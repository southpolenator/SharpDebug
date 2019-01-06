namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Source-level access specifier. Represents CV_access_e.
    /// </summary>
    public enum MemberAccess : byte
    {
        /// <summary>
        /// No member access modifier.
        /// </summary>
        None = 0,

        /// <summary>
        /// <c>private</c> access modifier.
        /// </summary>
        Private = 1,

        /// <summary>
        /// <c>protected</c> access modifier.
        /// </summary>
        Protected = 2,

        /// <summary>
        /// <c>public</c> access modifier.
        /// </summary>
        Public = 3
    }
}
