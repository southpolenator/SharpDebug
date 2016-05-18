using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents single base class and notes that user type also implements some interfaces (types without fields and have just vtable).
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class SingleClassInheritanceWithInterfacesTypeTree : TypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleClassInheritanceWithInterfacesTypeTree"/> class.
        /// </summary>
        /// <param name="baseClassUserType">The base class user type.</param>
        /// <param name="factory">The user type factory.</param>
        public SingleClassInheritanceWithInterfacesTypeTree(UserType baseClassUserType, UserTypeFactory factory)
        {
            BaseClassUserType = UserTypeTree.Create(baseClassUserType, factory);
        }

        /// <summary>
        /// Gets the base class user type.
        /// </summary>
        public UserTypeTree BaseClassUserType { get; private set; }

        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString()
        {
            return BaseClassUserType.GetTypeString();
        }
    }
}
