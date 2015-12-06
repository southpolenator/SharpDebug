using CsScripts;
using System;
using System.Reflection;

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

        public static UserTypeMetadata ReadFromType(Type type)
        {
            UserTypeAttribute attribute = type.GetCustomAttribute<UserTypeAttribute>();
            bool derivedFromUserType = IsDerivedFrom(type, typeof(UserType));

            if (attribute != null && !derivedFromUserType)
            {
                throw new Exception(string.Format("Type {0} has defined UserTypeAttribute, but it does not inherit UserType", type.FullName));
            }
            else if (derivedFromUserType)
            {
                string moduleName = attribute != null ? attribute.ModuleName : null;
                string typeName = attribute != null ? attribute.TypeName : type.Name;

                return new UserTypeMetadata(moduleName, typeName, type);
            }

            return null;
        }

        public UserTypeDescription ConvertToDescription()
        {
            return new UserTypeDescription(ModuleName, TypeName, Type);
        }

        /// <summary>
        /// Determines whether type is derived from the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="derivedType">Type of the derived.</param>
        private static bool IsDerivedFrom(Type type, Type derivedType)
        {
            while (type != null)
            {
                if (type == derivedType)
                    return true;

                type = type.BaseType;
            }

            return false;
        }
    }
}
