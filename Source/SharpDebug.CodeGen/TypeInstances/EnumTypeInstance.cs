using SharpDebug.CodeGen.UserTypes;

namespace SharpDebug.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents enumeration user type.
    /// </summary>
    /// <seealso cref="UserTypeInstance" />
    internal class EnumTypeInstance : UserTypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeInstance"/> class.
        /// </summary>
        /// <param name="enumUserType">The enumeration user type.</param>
        public EnumTypeInstance(EnumUserType enumUserType)
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
