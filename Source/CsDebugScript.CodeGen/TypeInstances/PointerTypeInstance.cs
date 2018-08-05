namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents pointer to a type.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class PointerTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerTypeInstance"/> class.
        /// </summary>
        /// <param name="elementType">The element type instance.</param>
        public PointerTypeInstance(TypeInstance elementType)
            : base(elementType.CodeWriter)
        {
            ElementType = elementType;
        }

        /// <summary>
        /// Gets the element type instance.
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

            return $"{CodeWriter.ToString(typeof(CodePointer))}<{elementTypeString}>";
        }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return ElementType.ContainsUndefinedType();
        }
    }
}
