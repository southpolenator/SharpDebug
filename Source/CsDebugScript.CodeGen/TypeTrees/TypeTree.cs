namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Base class for converting symbol name into typed structured tree
    /// </summary>
    internal abstract class TypeTree
    {
        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <param name="truncateNamespace">if set to <c>true</c> namespace will be truncated from generating type string.</param>
        /// <returns>The string representing this type tree in C# code.</returns>
        public abstract string GetTypeString(bool truncateNamespace = false);

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GetTypeString();
        }
    }
}
