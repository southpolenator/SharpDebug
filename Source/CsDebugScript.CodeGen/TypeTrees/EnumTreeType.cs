using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents enumeration user type.
    /// </summary>
    /// <seealso cref="UserTypeTree" />
    internal class EnumTreeType : UserTypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTreeType"/> class.
        /// </summary>
        /// <param name="enumUserType">The enumeration user type.</param>
        public EnumTreeType(EnumUserType enumUserType)
            : base(enumUserType)
        {
        }

        /// <summary>
        /// Gets the enumeration user type.
        /// </summary>
        public EnumUserType EnumUserType
        {
            get
            {
                return (EnumUserType)UserType;
            }
        }
    }
}
