using CsDebugScript.CodeGen.CodeWriters;

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
        /// <param name="codeWriter">Code writer used to output generated code.</param>
        public StaticClassTypeInstance(ICodeWriter codeWriter)
            : base(codeWriter)
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
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return false;
        }
    }
}
