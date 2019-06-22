using CsDebugScript.CodeGen.CodeWriters;
using System;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type tree that represents function.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class FunctionTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTypeInstance"/> class.
        /// </summary>
        /// <param name="codeNaming">Code naming used to generate code names.</param>
        public FunctionTypeInstance(ICodeNaming codeNaming)
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
            return CodeNaming.ToString(typeof(CodeFunction));
        }

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            return typeof(CodeFunction);
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
