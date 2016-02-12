using CsScriptManaged;
using System;

namespace CsScripts
{
    /// <summary>
    /// Base class for user defined types in C# scripts
    /// </summary>
    public class UserType : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public UserType(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the base class string.
        /// </summary>
        /// <param name="baseClassType">Type of the base class.</param>
        public static string GetBaseClassString(Type baseClassType)
        {
            if (!baseClassType.IsSubclassOf(typeof(Variable)))
                throw new Exception("Specified type doesn't inherit Variable class");

            // TODO: Make it work with exported template classes
            UserTypeMetadata metadata = UserTypeMetadata.ReadFromType(baseClassType);

            return metadata.TypeName;
        }
    }
}
