using SharpDebug.CodeGen.SymbolProviders;

namespace SharpDebug.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents template constant argument (<c>MyType&lt;bool, 5&gt;</c>)
    /// </summary>
    /// <seealso cref="UserType" />
    internal class TemplateArgumentConstantUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArgumentConstantUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public TemplateArgumentConstantUserType(Symbol symbol, UserTypeFactory factory)
            : base(symbol, null, null, factory)
        {
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.TypeName"/> property.
        /// </summary>
        /// <returns>User type name.</returns>
        protected override string GetTypeName()
        {
            if (Symbol is EnumConstantSymbol enumConstant)
                return CodeNaming.FixUserNaming($"{enumConstant.EnumSymbol.UserType.FullTypeName}_{enumConstant.Value}");

            if (Symbol is IntegralConstantSymbol integralConstant)
                return CodeNaming.FixUserNaming($"{CodeNaming.ToString(integralConstant.Value.GetType())}_{integralConstant.Value}");

            throw new System.NotImplementedException();
        }
    }
}
