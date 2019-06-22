using SharpDebug.CodeGen.TypeInstances;

namespace SharpDebug.CodeGen.UserTypes.Members
{
    /// <summary>
    /// Represents member that can appear in user type. Examples: data field, constant, base class property...
    /// </summary>
    internal class UserTypeMember
    {
        /// <summary>
        /// Gets or sets the name of the member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public TypeInstance Type { get; set; }

        /// <summary>
        /// Gets or sets access level.
        /// </summary>
        public AccessLevel AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets user type where this member is being declared.
        /// </summary>
        public UserType UserType { get; set; }
    }
}
