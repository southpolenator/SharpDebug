namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents basic type.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class BasicTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTypeInstance"/> class.
        /// </summary>
        /// <param name="basicType">The basic type string.</param>
        public BasicTypeInstance(string basicType)
        {
            BasicType = basicType;
        }

        /// <summary>
        /// Gets the basic type string.
        /// </summary>
        public string BasicType { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return BasicType;
        }
    }
}
