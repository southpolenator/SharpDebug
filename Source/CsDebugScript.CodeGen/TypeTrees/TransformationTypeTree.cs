using CsDebugScript.CodeGen.UserTypes;

namespace CsDebugScript.CodeGen.TypeTrees
{
    /// <summary>
    /// Type tree that represents transformation to be applied.
    /// </summary>
    /// <seealso cref="TypeTree" />
    internal class TransformationTypeTree : TypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationTypeTree"/> class.
        /// </summary>
        /// <param name="transformation">The transformation that will be applied.</param>
        public TransformationTypeTree(UserTypeTransformation transformation)
        {
            Transformation = transformation;
        }

        /// <summary>
        /// Gets the transformation that will be applied.
        /// </summary>
        public UserTypeTransformation Transformation { get; private set; }

        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString()
        {
            return Transformation.TransformType();
        }
    }
}
