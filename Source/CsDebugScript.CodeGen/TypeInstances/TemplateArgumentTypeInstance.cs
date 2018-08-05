using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeInstances
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Type instance that represents template type argument.
    /// Used when we know that the type is representing Generics Specialization.
    /// </summary>
    /// <seealso cref="TemplateTypeInstance" />
    internal class TemplateArgumentTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentTypeInstance"/> class.
        /// </summary>
        /// <param name="templateArgumentName">The template type argument name.</param>
        /// <param name="factory">The user type factory.</param>
        public TemplateArgumentTypeInstance(string templateArgumentName, UserTypeFactory factory)
            : base(factory.CodeWriter)
        {
            ArgumentName = templateArgumentName;
        }

        /// <summary>
        /// Gets the template type argument name.
        /// </summary>
        public string ArgumentName { get; private set; }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return false;
        }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return ArgumentName;
        }
    }
}
