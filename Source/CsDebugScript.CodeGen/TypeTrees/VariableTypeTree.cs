namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents unknown user type (and will be represented as Variable or UserType if it is base class)
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class VariableTypeTree : TypeTree
    {
        /// <summary>
        /// Flag if this instance is Variable.
        /// </summary>
        private bool isVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableTypeTree"/> class.
        /// </summary>
        /// <param name="isVariable">if set to <c>true</c> it will be Variable; otherwise it will be UserType.</param>
        public VariableTypeTree(bool isVariable = true)
        {
            this.isVariable = isVariable;
        }

        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <param name="truncateNamespace">if set to <c>true</c> namespace will be truncated from generating type string.</param>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return isVariable ? "Variable" : "UserType";
        }
    }
}
