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
        /// <param name="transformation">The transformation that will be applied.</param>
        public TransformationTypeInstance(UserTypeTransformation transformation)
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
            return Transformation.TransformType();
        }
    }
}
