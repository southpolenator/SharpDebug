using CsDebugScript.CodeGen.CodeWriters;
using CsDebugScript.CodeGen.UserTypes;
using System;

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
        /// <param name="codeNaming">Code naming used to generate code names.</param>
        /// <param name="transformation">The transformation that will be applied.</param>
        public TransformationTypeInstance(ICodeNaming codeNaming, UserTypeTransformation transformation)
            : base(codeNaming)
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
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            // TODO: What should we do here?
            return typeof(Variable);
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
