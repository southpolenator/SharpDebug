using SharpDebug.CodeGen.SymbolProviders;

namespace SharpDebug.CodeGen.UserTypes.Members
{
    /// <summary>
    /// Represents base class property declared as a member in user type.
    /// </summary>
    internal class BaseClassPropertyUserTypeMember : UserTypeMember
    {
        /// <summary>
        /// Gets or sets the base class symbol.
        /// </summary>
        public Symbol Symbol { get; set; }

        /// <summary>
        /// Gets or sets the base class index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets the comment associated with this property in generated code.
        /// </summary>
        public string Comment => $"// Property for getting base class: {Symbol.Name}";
    }
}
