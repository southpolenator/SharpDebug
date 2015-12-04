using CsScripts;
using System;
using System.Linq;

namespace CsScriptManaged
{
    class UserTypeDescription
    {
        public UserTypeDescription(string moduleName, string typeName, Type type)
        {
            if (moduleName != null)
            {
                Module = Process.Current.ModulesByName[moduleName];
            }
            else
            {
                DType userType;
                var modules = Process.Current.Modules.Where(m => m.TypesByName.TryGetValue(typeName, out userType)).ToArray();

                if (modules.Length > 1)
                {
                    throw new Exception(string.Format("Type {0} exists in multiple modules: {1}", typeName, string.Join(", ", modules.Select(m => m.Name))));
                }

                Module = modules[0];
            }

            UserType = Module.TypesByName[typeName];
            Type = type;
        }

        public Module Module { get; private set; }

        public DType UserType { get; private set; }

        public Type Type { get; private set; }
    }
}
