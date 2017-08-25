using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeTrees
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Tree type that represents template type argument.  
    /// Used when we know that the type is representing Generic Specialization.
    /// </summary>
    /// <seealso cref="TemplateTypeTree" />
    internal class TemplateArgumentTreeType : TemplateTypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentTreeType"/> class.
        /// </summary>
        /// <param name="templateArgumentNumber">The template type argument number.</param>
        /// <param name="templateSpecialization">The template specialization user type.</param>
        /// <param name="factory">The user type factory.</param>
        public TemplateArgumentTreeType(int templateArgumentNumber, UserType templateSpecialization, UserTypeFactory factory)
            : base(templateSpecialization, factory)
        {
            ArgumentNumber = templateArgumentNumber;
        }

        /// <summary>
        /// Gets the template type argument number.
        /// </summary>
        public int ArgumentNumber { get; private set; }

        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <param name="truncateNamespace">if set to <c>true</c> namespace will be truncated from generating type string.</param>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return "T" + (ArgumentNumber > 0 ? ArgumentNumber.ToString() : string.Empty);
        }
    }
}
