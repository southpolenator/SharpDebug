using CsDebugScript.CodeGen.CodeWriters;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents unknown user type (and will be represented as <see cref="Variable"/> or <see cref="UserType"/> if it is base class).
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class VariableTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableTypeInstance"/> class.
        /// </summary>
        /// <param name="codeWriter">Code writer used to output generated code.</param>
        /// <param name="isVariable">if set to <c>true</c> it will be Variable; otherwise it will be UserType.</param>
        public VariableTypeInstance(ICodeWriter codeWriter, bool isVariable = true)
            : base(codeWriter)
        {
            IsVariable = isVariable;
        }

        /// <summary>
        /// Flag if this instance is <see cref="Variable"/>.
        /// </summary>
        public bool IsVariable { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return IsVariable ? CodeWriter.ToString(typeof(Variable)) : CodeWriter.ToString(typeof(UserType));
        }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return true;
        }
    }
}
