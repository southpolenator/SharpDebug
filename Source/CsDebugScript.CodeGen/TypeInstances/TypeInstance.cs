namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Base class for converting symbol name into typed structured tree
    /// </summary>
    internal abstract class TypeInstance
    {
        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
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
