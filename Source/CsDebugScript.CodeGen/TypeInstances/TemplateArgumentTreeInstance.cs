using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeInstances
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Tree instance that represents template type argument.
    /// Used when we know that the type is representing Generics Specialization.
    /// </summary>
    /// <seealso cref="TemplateTypeInstance" />
    internal class TemplateArgumentTreeInstance : TemplateTypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentTreeInstance"/> class.
        /// </summary>
        /// <param name="templateArgumentNumber">The template type argument number.</param>
        /// <param name="templateSpecialization">The template specialization user type.</param>
        /// <param name="factory">The user type factory.</param>
        public TemplateArgumentTreeInstance(int templateArgumentNumber, UserType templateSpecialization, UserTypeFactory factory)
            : base(templateSpecialization, factory)
        {
            ArgumentNumber = templateArgumentNumber;
        }

        /// <summary>
        /// Gets the template type argument number.
        /// </summary>
        public int ArgumentNumber { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return ArgumentNumber > 0 ? $"T{ArgumentNumber}" : "T";
        }
    }
}
