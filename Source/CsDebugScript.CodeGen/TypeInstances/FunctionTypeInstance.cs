namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type tree that represents function.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class FunctionTypeInstance : TypeInstance
    {
        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return "CodeFunction";
        }
    }
}
