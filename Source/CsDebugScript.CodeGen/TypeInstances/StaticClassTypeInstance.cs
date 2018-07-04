namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents exported static class (class that exports global variables).
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class StaticClassTypeInstance : TypeInstance
    {
        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return string.Empty;
        }
    }
}
