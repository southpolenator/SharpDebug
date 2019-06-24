using SharpDebug.CodeGen.UserTypes;

namespace SharpDebug.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents template type argument which is constant.
    /// Used when we know that the type is representing Generics Specialization.
    /// </summary>
    /// <seealso cref="TemplateTypeInstance" />
    internal class TemplateArgumentConstantTypeInstance : UserTypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentConstantTypeInstance"/> class.
        /// </summary>
        /// <param name="constant">The template type argument constant.</param>
        public TemplateArgumentConstantTypeInstance(TemplateArgumentConstantUserType constant)
            : base(constant)
        {
            Constant = constant;
        }

        /// <summary>
        /// Gets the template type argument constant.
        /// </summary>
        public TemplateArgumentConstantUserType Constant { get; private set; }
    }
}
