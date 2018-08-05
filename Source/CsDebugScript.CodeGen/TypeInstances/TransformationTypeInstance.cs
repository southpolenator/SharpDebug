using CsDebugScript.CodeGen.CodeWriters;
using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents transformation to be applied.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class TransformationTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationTypeInstance"/> class.
        /// </summary>
        /// <param name="codeWriter">Code writer used to output generated code.</param>
        /// <param name="transformation">The transformation that will be applied.</param>
        public TransformationTypeInstance(ICodeWriter codeWriter, UserTypeTransformation transformation)
            : base(codeWriter)
        {
            Transformation = transformation;
        }

        /// <summary>
        /// Gets the transformation that will be applied.
        /// </summary>
        public UserTypeTransformation Transformation { get; private set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            return Transformation.TypeString;
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
