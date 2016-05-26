using CsDebugScript.CodeGen.UserTypes;
using System;

namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents UserType.
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class UserTypeTree : TypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeTree"/> class.
        /// </summary>
        /// <param name="userType">The user type.</param>
        protected UserTypeTree(UserType userType)
        {
            UserType = userType;
        }

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public UserType UserType { get; private set; }

        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <param name="truncateNamespace">if set to <c>true</c> namespace will be truncated from generating type string.</param>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return truncateNamespace ? UserType.ClassName : UserType.FullClassName;
        }

        /// <summary>
        /// Creates type tree that represents UserType base on the specified UserType and user type factory.
        /// </summary>
        /// <param name="userType">The user type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <returns>Type tree that represents UserType</returns>
        internal static UserTypeTree Create(UserType userType, UserTypeFactory factory)
        {
            // Check arguments
            if (userType == null)
                throw new ArgumentNullException(nameof(userType));

            // If user type is template or declared in template user type,
            // we need to force template because of possible template types used from "parent" type.
            var type = userType;

            while (type != null)
            {
                var templateType = type as TemplateUserType;

                if (templateType != null)
                    return new TemplateTypeTree(userType, factory);
                type = type.DeclaredInType;
            }

            // Check if user type is enumeration
            var enumType = userType as EnumUserType;

            if (enumType != null)
                return new EnumTreeType(enumType);

            // We are now certain that it is regular user type
            return new UserTypeTree(userType);
        }
    }
}
