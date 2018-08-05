using CsDebugScript.CodeGen.SymbolProviders;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents template argument (T1, T2, etc.)
    /// </summary>
    /// <seealso cref="UserType" />
    internal class TemplateArgumentUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentUserType"/> class.
        /// </summary>
        /// <param name="typeName">The template argument type name.</param>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public TemplateArgumentUserType(string typeName, Symbol symbol, UserTypeFactory factory)
            : base(symbol, null, null, factory)
        {
            TemplateArgumentName = typeName;
        }

        /// <summary>
        /// The template argument type name
        /// </summary>
        public string TemplateArgumentName { get; private set; }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.TypeName"/> property.
        /// </summary>
        /// <returns>User type name.</returns>
        protected override string GetTypeName()
        {
            return TemplateArgumentName;
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.FullTypeName"/> property.
        /// </summary>
        /// <returns>User type full name.</returns>
        protected override string GetFullTypeName()
        {
            return TemplateArgumentName;
        }
    }
}
