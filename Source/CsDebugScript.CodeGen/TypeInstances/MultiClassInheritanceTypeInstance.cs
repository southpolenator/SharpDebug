using CsDebugScript.CodeGen.CodeWriters;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents multiple base classes and that <see cref="UserType"/> should be used.
    /// </summary>
    /// <seealso cref="VariableTypeInstance" />
    internal class MultiClassInheritanceTypeInstance : VariableTypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiClassInheritanceTypeInstance"/> class.
        /// </summary>
        /// <param name="codeWriter">Code writer used to output generated code.</param>
        public MultiClassInheritanceTypeInstance(ICodeWriter codeWriter)
            : base(codeWriter, false)
        {
        }
    }
}
