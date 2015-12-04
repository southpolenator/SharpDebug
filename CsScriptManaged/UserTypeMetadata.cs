using System;

namespace CsScriptManaged
{
    class UserTypeMetadata
    {
        public UserTypeMetadata(string moduleName, string typeName, Type type)
        {
            ModuleName = moduleName;
            TypeName = typeName;
            Type = type;
        }

        public string ModuleName { get; private set; }

        public string TypeName { get; private set; }

        public Type Type { get; private set; }

        public UserTypeDescription ConvertToDescription()
        {
            return new UserTypeDescription(ModuleName, TypeName, Type);
        }
    }
}
