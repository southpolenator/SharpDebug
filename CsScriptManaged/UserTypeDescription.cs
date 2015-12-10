using CsScripts;
using System;
using System.Linq;

namespace CsScriptManaged
{
    class UserTypeDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDescription"/> class.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="type">The type.</param>
        public UserTypeDescription(string moduleName, string typeName, Type type)
        {
            if (moduleName != null)
            {
                Module = Process.Current.ModulesByName[moduleName];
            }
            else
            {
                CodeType userType;
                var modules = Process.Current.Modules.Where(m => m.TypesByName.TryGetValue(typeName, out userType)).ToArray();

                if (modules.Length > 1)
                {
                    throw new Exception(string.Format("Type {0} exists in multiple modules: {1}", typeName, string.Join(", ", modules.Select(m => m.Name))));
                }

                if (modules.Length <= 0)
                {
                    throw new Exception(string.Format("Type {0} wasn't found in any module", typeName));
                }

                Module = modules[0];
            }

            UserType = Module.TypesByName[typeName];
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
    }
}
