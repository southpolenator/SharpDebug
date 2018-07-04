using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents enumeration user type.
    /// </summary>
    /// <seealso cref="UserTypeInstance" />
    internal class EnumTreeInstance : UserTypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTreeInstance"/> class.
        /// </summary>
        /// <param name="enumUserType">The enumeration user type.</param>
        public EnumTreeInstance(EnumUserType enumUserType)
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
