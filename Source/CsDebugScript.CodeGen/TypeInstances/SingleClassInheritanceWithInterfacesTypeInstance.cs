using CsDebugScript.CodeGen.UserTypes;
using System;

namespace CsDebugScript.CodeGen.TypeInstances
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Type instance that represents single base class and anotates that user type also implements some interfaces (types without fields that just have vtable).
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class SingleClassInheritanceWithInterfacesTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleClassInheritanceWithInterfacesTypeInstance"/> class.
        /// </summary>
        /// <param name="baseClassUserType">The base class user type.</param>
        /// <param name="factory">The user type factory.</param>
        public SingleClassInheritanceWithInterfacesTypeInstance(UserType baseClassUserType, UserTypeFactory factory)
            : base(factory.CodeNaming)
        {
            BaseClassUserType = UserTypeInstance.Create(baseClassUserType, factory);
        }

        /// <summary>
        /// Gets the base class user type.
        /// </summary>
        public UserTypeInstance BaseClassUserType { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return BaseClassUserType.GetTypeString(truncateNamespace);
        }

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            return BaseClassUserType.GetType(typeConverter);
        }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return BaseClassUserType.ContainsUndefinedType();
        }
    }
}
