using CsDebugScript.CodeGen.CodeWriters;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Base class for converting symbol name into typed structured tree
    /// </summary>
    internal abstract class TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeInstance"/> class.
        /// </summary>
        /// <param name="codeWriter">Code writer used to output generated code.</param>
        public TypeInstance(ICodeWriter codeWriter)
        {
            CodeWriter = codeWriter;
        }

        /// <summary>
        /// Gets the code writer that is used to output generated code.
        /// </summary>
        public ICodeWriter CodeWriter { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public abstract string GetTypeString(bool truncateNamespace = false);

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
