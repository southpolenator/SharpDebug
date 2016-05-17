using System;
using System.Linq;

namespace CsDebugScript.Engine
{
    internal class UserTypeDescription : IEquatable<UserTypeDescription>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDescription"/> class.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="type">The type.</param>
        public UserTypeDescription(string moduleName, string typeName, Type type)
            : this(Process.Current, moduleName, typeName, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDescription" /> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="type">The type.</param>
        public UserTypeDescription(Process process, string moduleName, string typeName, Type type)
        {
            if (moduleName != null)
            {
                Module = process.ModulesByName[moduleName];
            }
            else
            {
                // TODO: Check if this is needed for some scenario...
                //CodeType userType;
                //var modules = process.Modules.Where(m => m.TypesByName.TryGetValue(typeName, out userType)).ToArray();

                //if (modules.Length > 1)
                //{
                //    throw new Exception(string.Format("Type {0} exists in multiple modules: {1}", typeName, string.Join(", ", modules.Select(m => m.Name))));
                //}

                //if (modules.Length <= 0)
                //{
                //    throw new Exception(string.Format("Type {0} wasn't found in any module", typeName));
                //}

                //Module = modules[0];
            }

            if (!typeName.EndsWith("<>"))
            {
                UserType = Module.TypesByName[typeName];
            }

            Type = type;
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the user code type.
        /// </summary>
        public CodeType UserType { get; private set; }

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="UserTypeDescription" />, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="UserTypeDescription" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="UserTypeDescription" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(UserTypeDescription other)
        {
            return other.Module == Module && other.UserType == UserType && other.Type == Type;
        }
    }

    static class UserTypeDescriptionExtensions
    {
        /// <summary>
        /// Finds description that comes from the specified module or first if not found.
        /// </summary>
        /// <param name="descriptions">The descriptions.</param>
        /// <param name="module">The module.</param>
        public static UserTypeDescription FromModuleOrFirst(this UserTypeDescription[] descriptions, Module module)
        {
            foreach (var description in descriptions)
                if (description.Module == module)
                    return description;
            return descriptions.FirstOrDefault();
        }
    }
}
