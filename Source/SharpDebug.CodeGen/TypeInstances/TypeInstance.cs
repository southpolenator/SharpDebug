using System;
using SharpDebug.CodeGen.CodeWriters;

namespace SharpDebug.CodeGen.TypeInstances
{
    using UserType = SharpDebug.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Interface for converting user types to types.
    /// </summary>
    internal interface ITypeConverter
    {
        /// <summary>
        /// Gets type associated with user type.
        /// </summary>
        /// <param name="userType">The user type.</param>
        Type GetType(UserType userType);

        /// <summary>
        /// Gets type generic parameter by parameter name.
        /// </summary>
        /// <param name="userType">The user type</param>
        /// <param name="parameter">Parameter name</param>
        Type GetGenericParameter(UserType userType, string parameter);

        /// <summary>
        /// Resolves type for the specified type name.
        /// </summary>
        /// <param name="typeName">The type name to be resolved.</param>
        /// <returns>Resolved type.</returns>
        Type GetTypeByName(string typeName);
    }

    /// <summary>
    /// Base class for converting symbol name into typed structured tree
    /// </summary>
    internal abstract class TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeInstance"/> class.
        /// </summary>
        /// <param name="codeNaming">Code naming used to generate code names.</param>
        public TypeInstance(ICodeNaming codeNaming)
        {
            CodeNaming = codeNaming;
        }

        /// <summary>
        /// Gets the code naming that is used to generate code names.
        /// </summary>
        public ICodeNaming CodeNaming { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public abstract string GetTypeString(bool truncateNamespace = false);

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public abstract Type GetType(ITypeConverter typeConverter);

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public abstract bool ContainsUndefinedType();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GetTypeString();
        }
    }
}
