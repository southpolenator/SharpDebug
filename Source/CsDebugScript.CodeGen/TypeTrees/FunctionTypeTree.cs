namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents function.
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class FunctionTypeTree : TypeTree
    {
        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <param name="truncateNamespace">if set to <c>true</c> namespace will be truncated from generating type string.</param>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return "CodeFunction";
        }
    }
}
