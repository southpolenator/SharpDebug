using CsDebugScript.CodeGen.CodeWriters;
using System;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents exported static class (class that exports global variables).
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class StaticClassTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticClassTypeInstance"/> class.
        /// </summary>
        /// <param name="codeNaming">Code naming used to output generate code names.</param>
        public StaticClassTypeInstance(ICodeNaming codeNaming)
            : base(codeNaming)
        {
        }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return false;
        }
    }
}
