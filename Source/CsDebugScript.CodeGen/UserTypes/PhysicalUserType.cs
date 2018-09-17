using CsDebugScript.CodeGen.SymbolProviders;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Physical representation of the user type
    /// </summary>
    /// <seealso cref="UserType" />
    internal class PhysicalUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="xmlType">The XML description of the type.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public PhysicalUserType(Symbol symbol, XmlType xmlType, string nameSpace, UserTypeFactory factory)
            : base(symbol, xmlType, nameSpace, factory)
        {
        }
    }
}
