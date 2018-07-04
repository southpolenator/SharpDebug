namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents unknown user type (and will be represented as <see cref="Variable"/> or <see cref="UserType"/> if it is base class).
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class VariableTypeInstance : TypeInstance
    {
        /// <summary>
        /// Flag if this instance is <see cref="Variable"/>.
        /// </summary>
        private bool isVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableTypeInstance"/> class.
        /// </summary>
        /// <param name="isVariable">if set to <c>true</c> it will be Variable; otherwise it will be UserType.</param>
        public VariableTypeInstance(bool isVariable = true)
        {
            this.isVariable = isVariable;
        }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return isVariable ? "Variable" : "UserType";
        }
    }
}
