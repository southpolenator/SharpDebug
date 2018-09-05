using System;
using System.Linq;
using System.Reflection;

namespace CsDebugScript.Engine
{
    internal class UserTypeMetadata
    {
        /// <summary>
        /// Delegate for vefication that the specified code type can be handled by this user type metadata.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if user type metadata can handle code type.</returns>
        public delegate bool CodeTypeVerificationDelegate(CodeType codeType);

        /// <summary>
        /// Delegate for code type verification.
        /// </summary>
        private CodeTypeVerificationDelegate verificationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeMetadata"/> class.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="codeTypeVerificationFunction">Name of the static function in <paramref name="type"/> that can verify code type.</param>
        /// <param name="type">The type.</param>
        public UserTypeMetadata(string moduleName, string typeName, string codeTypeVerificationFunction, Type type)
        {
            ModuleName = moduleName;
            TypeName = typeName;
            Type = type;
            try
            {
                if (!string.IsNullOrEmpty(codeTypeVerificationFunction))
                {
                    // Search for method
                    Type t = type;
                    MethodInfo methodInfo = null;

                    while (methodInfo == null && t != null)
                    {
                        methodInfo = t.GetMethod(codeTypeVerificationFunction);
                        t = type.BaseType;
                    }

                    if (methodInfo != null)
                        verificationDelegate = (CodeTypeVerificationDelegate)Delegate.CreateDelegate(typeof(CodeTypeVerificationDelegate), methodInfo);
                }
            }
            catch
            {
                // Silently fail delegate initialization
            }
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

                    metadata[i] = new UserTypeMetadata(moduleName, typeName, attribute.CodeTypeVerification, type);
                }

                return metadata;
            }

            return new UserTypeMetadata[0];
        }

        /// <summary>
        /// Verifies that type user type from this metadata can work with the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if user type from this metadata can work with the specified code type; <c>false</c> otherwise</returns>
        public bool VerifyCodeType(CodeType codeType)
        {
            return verificationDelegate == null || verificationDelegate(codeType);
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
