namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents multiple base classes and that UserType should be used.
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class MultiClassInheritanceTypeTree : VariableTypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiClassInheritanceTypeTree"/> class.
        /// </summary>
        public MultiClassInheritanceTypeTree()
            : base(false)
        {
        }
    }
}
