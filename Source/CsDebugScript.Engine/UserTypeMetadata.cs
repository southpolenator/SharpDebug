using CsScripts;
using System;
using System.Linq;
using System.Reflection;

namespace CsDebugScript
{
    class UserTypeMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeMetadata"/> class.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="type">The type.</param>
        public UserTypeMetadata(string moduleName, string typeName, Type type)
        {
            ModuleName = moduleName;
            TypeName = typeName;
            Type = type;
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string ModuleName { get; private set; }

        /// <summary>
        /// Gets the name of the user code type.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Reads metadata from the type.
        /// </summary>
        /// <param name="type">The type.</param>
        public static UserTypeMetadata[] ReadFromType(Type type)
        {
            UserTypeAttribute[] attributes = type.GetCustomAttributes<UserTypeAttribute>(false).ToArray();
            bool derivedFromUserType = IsDerivedFrom(type, typeof(UserType));

            /* #fixme temp workaround for genertic types
            if (type.IsGenericType)
            {
                foreach(Type genericArgument in type.GetGenericArguments())
                {
                    UserTypeAttribute genericAttribute = genericArgument.GetCustomAttribute<UserTypeAttribute>();
                    if (genericAttribute != null)
                    {
                        string moduleName = genericAttribute.ModuleName;
                        // #fixme temp workaround
                        string typeName = type.Name + "<>";

                        return new UserTypeMetadata(moduleName, typeName, type);
                    }
                }
            }
            */

            if (attributes.Length > 0 && !derivedFromUserType)
            {
                throw new Exception(string.Format("Type {0} has defined UserTypeAttribute, but it does not inherit UserType", type.FullName));
            }
            else if (derivedFromUserType)
            {
                UserTypeMetadata[] metadata = new UserTypeMetadata[attributes.Length];

                for (int i = 0; i < metadata.Length; i++)
                {
                    UserTypeAttribute attribute = attributes[i];
                    string moduleName = attribute != null ? attribute.ModuleName : null;
                    string typeName = attribute != null ? attribute.TypeName : !type.IsGenericType ? type.Name : type.Name.Substring(0, type.Name.IndexOf('`')) + "<>"; // TODO: Form better name for generics type

                    metadata[i] = new UserTypeMetadata(moduleName, typeName, type);
                }

                return metadata;
            }

            return new UserTypeMetadata[0];
        }

        /// <summary>
        /// Converts metadata to description using the current process.
        /// </summary>
        public UserTypeDescription ConvertToDescription()
        {
            return new UserTypeDescription(ModuleName, TypeName, Type);
        }

        /// <summary>
        /// Converts metadata to description.
        /// </summary>
        /// <param name="process">The process.</param>
        public UserTypeDescription ConvertToDescription(Process process)
        {
            return new UserTypeDescription(process, ModuleName, TypeName, Type);
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

    static class UserTypeMetadataExtensions
    {
        /// <summary>
        /// Finds metadata that comes from the specified module or first if not found.
        /// </summary>
        /// <param name="metadatas">The metadatas.</param>
        /// <param name="module">The module.</param>
        public static UserTypeMetadata FromModuleOrFirst(this UserTypeMetadata[] metadatas, CsScripts.Module module)
        {
            foreach (var metadata in metadatas)
                if (metadata.ModuleName == module.Name)
                    return metadata;
            return metadatas.First();
        }
    }
}
