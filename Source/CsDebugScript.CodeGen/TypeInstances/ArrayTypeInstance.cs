namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents array type.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class ArrayTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeInstance"/> class.
        /// </summary>
        /// <param name="elementType">The element type instance.</param>
        public ArrayTypeInstance(TypeInstance elementType)
        {
            ElementType = elementType;
        }

        /// <summary>
        /// Gets the array element type instance.
        /// </summary>
        public TypeInstance ElementType { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            string elementTypeString = ElementType.GetTypeString(truncateNamespace);

            return $"CodeArray<{elementTypeString}";
        }
    }
}
