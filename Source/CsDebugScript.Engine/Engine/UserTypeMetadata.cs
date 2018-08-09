using System;
using System.Linq;
using System.Reflection;

namespace CsDebugScript.Engine
{
    internal class UserTypeMetadata
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

            if (attributes.Length > 0 && !derivedFromUserType)
                throw new Exception($"Type {type.FullName} has defined UserTypeAttribute, but it does not inherit UserType");
            else if (derivedFromUserType)
            {
                UserTypeMetadata[] metadata = new UserTypeMetadata[attributes.Length];
                string defaultTypeName = type.Name; // TODO: Form better name for generics type

                for (int i = 0; i < metadata.Length; i++)
                {
                    UserTypeAttribute attribute = attributes[i];
                    string moduleName = attribute?.ModuleName;
                    string typeName = attribute?.TypeName ?? defaultTypeName;

                    metadata[i] = new UserTypeMetadata(moduleName, typeName, type);
                }

                return metadata;
            }

            return new UserTypeMetadata[0];
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

    internal static class UserTypeMetadataExtensions
    {
        /// <summary>
        /// Finds metadata that comes from the specified module or first if not found.
        /// </summary>
        /// <param name="metadatas">The metadatas.</param>
        /// <param name="module">The module.</param>
        public static UserTypeMetadata FromModuleOrFirst(this UserTypeMetadata[] metadatas, CsDebugScript.Module module)
        {
            foreach (var metadata in metadatas)
                if (metadata.ModuleName == module.Name)
                    return metadata;
            return metadatas.First();
        }
    }
}
