using CsDebugScript.CodeGen.SymbolProviders;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class representing template user type factory. It is used to inject template arguments into existing user type factory.
    /// </summary>
    /// <seealso cref="UserTypeFactory" />
    class TemplateUserTypeFactory : UserTypeFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateUserTypeFactory"/> class.
        /// </summary>
        /// <param name="originalFactory">The original user type factory.</param>
        /// <param name="templateType">The template user type.</param>
        public TemplateUserTypeFactory(UserTypeFactory originalFactory, TemplateUserType templateType)
            : base(originalFactory)
        {
            TemplateType = templateType;
            OriginalFactory = originalFactory;
        }

        /// <summary>
        /// Gets the template user type.
        /// </summary>
        public TemplateUserType TemplateType { get; private set; }

        /// <summary>
        /// Gets the original factory user type factory.
        /// </summary>
        public UserTypeFactory OriginalFactory { get; private set; }

        /// <summary>
        /// Look up for user type based on the specified symbol.
        /// </summary>
        /// <param name="type">The symbol.</param>
        /// <param name="userType">The found user type.</param>
        /// <returns><c>true</c> if user type was found.</returns>
        internal override bool GetUserType(ISymbol type, out UserType userType)
        {
            string argumentName;
            string typeString = type.Name;

            if (TryGetTemplateArgument(typeString, out argumentName))
            {
                // TODO: #fixme investigate this
                userType = new TemplateArgumentUserType(argumentName, type);
                return true;
            }

            return base.GetUserType(type, out userType);
        }

        /// <summary>
        /// Look up for user type based on the specified module and type string.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeString">The type string.</param>
        /// <param name="userType">The found user type.</param>
        /// <returns><c>true</c> if user type was found.</returns>
        internal override bool GetUserType(IModule module, string typeString, out UserType userType)
        {
            string argumentName;

            if (TryGetTemplateArgument(typeString, out argumentName))
            {
                // TODO: #fixme investigate this
                userType = new TemplateArgumentUserType(argumentName, null);
                return true;
            }

            return base.GetUserType(module, typeString, out userType);
        }

        /// <summary>
        /// Tries to match the specified type name against template arguments.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="argumentName">The found argument name.</param>
        /// <returns><c>true</c> if template argument was matched.</returns>
        private bool TryGetTemplateArgument(string typeName, out string argumentName)
        {
            if (TemplateType.TryGetTemplateArgument(typeName, out argumentName))
            {
                return true;
            }

            if (typeName == "wchar_t")
            {
                if (TemplateType.TryGetTemplateArgument("unsigned short", out argumentName))
                    return true;
            }
            else if (typeName == "unsigned short")
            {
                if (TemplateType.TryGetTemplateArgument("whcar_t", out argumentName))
                    return true;
            }
            else if (typeName == "unsigned long long")
            {
                if (TemplateType.TryGetTemplateArgument("unsigned __int64", out argumentName))
                    return true;
            }
            else if (typeName == "unsigned __int64")
            {
                if (TemplateType.TryGetTemplateArgument("unsigned long long", out argumentName))
                    return true;
            }
            else if (typeName == "long long")
            {
                if (TemplateType.TryGetTemplateArgument("__int64", out argumentName))
                    return true;
            }
            else if (typeName == "__int64")
            {
                if (TemplateType.TryGetTemplateArgument("long long", out argumentName))
                    return true;
            }

            return false;
        }
    }
}
