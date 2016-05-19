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
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString()
        {
            return "CodeFunction";
        }
    }
}
