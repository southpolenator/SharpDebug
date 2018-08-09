using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.CodeGen.TypeInstances;
using System;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class representing template user type factory. It is used to inject template arguments into existing user type factory.
    /// </summary>
    /// <seealso cref="UserTypeFactory" />
    internal class TemplateUserTypeFactory : UserTypeFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateUserTypeFactory"/> class.
        /// </summary>
        /// <param name="originalFactory">The original user type factory.</param>
        /// <param name="templateType">The template user type.</param>
        public TemplateUserTypeFactory(UserTypeFactory originalFactory, SpecializedTemplateUserType templateType)
            : base(originalFactory)
        {
            TemplateType = templateType;
            OriginalFactory = originalFactory;
        }

        /// <summary>
        /// Gets the template user type.
        /// </summary>
        public SpecializedTemplateUserType TemplateType { get; private set; }

        /// <summary>
        /// Gets the original factory user type factory.
        /// </summary>
        public UserTypeFactory OriginalFactory { get; private set; }

        /// <summary>
        /// Gets the type instance for the specified symbol.
        /// </summary>
        /// <param name="parentType">The user type from which this symbol comes from (examples: field type, template type...).</param>
        /// <param name="symbol">The original type.</param>
        /// <param name="bitLength">Number of bits used for this symbol.</param>
        internal override TypeInstance GetSymbolTypeInstance(UserType parentType, Symbol symbol, int bitLength = 0)
        {
            string argumentName;

            if (TryGetTemplateArgument(symbol.Name, out argumentName))
                return new TemplateArgumentTypeInstance(argumentName, this);
            return base.GetSymbolTypeInstance(parentType, symbol, bitLength);
        }

        /// <summary>
        /// Some of the known "features" of MSVC compiler differently generated symbol names in templates.
        /// </summary>
        private static readonly Tuple<string, string>[] ConversionTypes = new Tuple<string, string>[]
            {
                Tuple.Create("wchar_t", "unsigned short"),
                Tuple.Create("unsigned long long", "unsigned __int64"),
                Tuple.Create("long long", "__int64"),
            };

        /// <summary>
        /// Tries to match the specified type name against template arguments.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="argumentName">The found argument name.</param>
        /// <returns><c>true</c> if template argument was matched.</returns>
        private bool TryGetTemplateArgument(string typeName, out string argumentName)
        {
            if (TemplateType.TryGetTemplateArgument(typeName, out argumentName))
            {
                return true;
            }

            foreach (Tuple<string, string> tps in ConversionTypes)
            {
                if (typeName == tps.Item1 && TemplateType.TryGetTemplateArgument(tps.Item2, out argumentName))
                    return true;
                if (typeName == tps.Item2 && TemplateType.TryGetTemplateArgument(tps.Item1, out argumentName))
                    return true;
            }

            return false;
        }
    }
}
