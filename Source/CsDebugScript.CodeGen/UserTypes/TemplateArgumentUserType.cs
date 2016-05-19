namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents template argument (T1, T2, etc.)
    /// </summary>
    /// <seealso cref="UserType" />
    internal class TemplateArgumentUserType : UserType
    {
        /// <summary>
        /// The template argument type name
        /// </summary>
        private string typeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentUserType"/> class.
        /// </summary>
        /// <param name="typeName">The template argument type name.</param>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        public TemplateArgumentUserType(string typeName, Symbol symbol)
            : base(symbol, null, null)
        {
            this.typeName = typeName;
        }

        /// <summary>
        /// Gets the class name for this user type. Class name doesn't contain namespace.
        /// </summary>
        public override string ClassName
        {
            get
            {
                return typeName;
            }
        }

        /// <summary>
        /// Gets the full name of the class, including namespace and "parent" type it is declared into.
        /// </summary>
        public override string FullClassName
        {
            get
            {
                return typeName;
            }
        }
    }
}
