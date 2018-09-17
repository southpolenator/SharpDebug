using CsDebugScript.CodeGen.UserTypes;
using System;

namespace CsDebugScript.CodeGen.TypeInstances
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Type instance that represents user type.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class UserTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeInstance"/> class.
        /// </summary>
        /// <param name="userType">The user type.</param>
        protected UserTypeInstance(UserType userType)
            : base(userType.CodeNaming)
        {
            UserType = userType;
        }

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public UserType UserType { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return truncateNamespace ? UserType.TypeName : UserType.FullTypeName;
        }

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            return typeConverter.GetType(UserType);
        }

        /// <summary>
        /// Creates type tree that represents UserType base on the specified UserType and user type factory.
        /// </summary>
        /// <param name="userType">The user type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <returns>Type tree that represents UserType</returns>
        internal static UserTypeInstance Create(UserType userType, UserTypeFactory factory)
        {
            // Check arguments
            if (userType == null)
                throw new ArgumentNullException(nameof(userType));

            // If user type is template or declared in template user type,
            // we need to force template because of possible template types used from "parent" type.
            var type = userType;

            while (type != null)
            {
                var templateType = type as SpecializedTemplateUserType;

                if (templateType != null)
                    return new TemplateTypeInstance(userType, factory);
                type = type.DeclaredInType;
            }

            // Check if user type is enumeration
            var enumType = userType as EnumUserType;

            if (enumType != null)
                return new EnumTypeInstance(enumType);

            // We are now certain that it is regular user type
            return new UserTypeInstance(userType);
        }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return false;
        }
    }
}
