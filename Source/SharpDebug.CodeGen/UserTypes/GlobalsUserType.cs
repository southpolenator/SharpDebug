using SharpDebug.CodeGen.SymbolProviders;
using SharpDebug.CodeGen.TypeInstances;
using System;
using System.Collections.Generic;

namespace SharpDebug.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents static class for getting global variables located in Module.
    /// </summary>
    /// <seealso cref="UserType" />
    internal class GlobalsUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalsUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="xmlType">The XML description of the type.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public GlobalsUserType(Symbol symbol, XmlType xmlType, string nameSpace, UserTypeFactory factory)
            : base(symbol, xmlType, nameSpace, factory)
        {
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.TypeName"/> property.
        /// </summary>
        /// <returns>User type name.</returns>
        protected override string GetTypeName()
        {
            return "ModuleGlobals";
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.Constructors"/> property.
        /// </summary>
        protected override IEnumerable<UserTypeConstructor> GetConstructors()
        {
            yield return UserTypeConstructor.Static;
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.BaseClass"/> and <see cref="UserType.BaseClassOffset"/> properties.
        /// </summary>
        protected override Tuple<TypeInstance, int> GetBaseClass(Symbol symbol)
        {
            return Tuple.Create<TypeInstance, int>(new StaticClassTypeInstance(CodeNaming), 0);
        }
    }
}
