namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents exported static class (class that exports global variables).
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class StaticClassTypeTree : TypeTree
    {
        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString()
        {
            return string.Empty;
        }
    }
}
