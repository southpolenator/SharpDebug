using SharpDebug.CodeGen.SymbolProviders;

namespace SharpDebug.CodeGen.UserTypes.Members
{
    /// <summary>
    /// Represents constant declared in user type.
    /// </summary>
    internal class ConstantUserTypeMember : UserTypeMember
    {
        /// <summary>
        /// Gets or sets the symbol describing constant member.
        /// </summary>
        public SymbolField Symbol { get; set; }

        /// <summary>
        /// Gets the comment associated with this member.
        /// </summary>
        public string Comment => $"// {Symbol.Type.Name} {Symbol.Name} = {Symbol.Value};";

        /// <summary>
        /// Gets the constant value.
        /// </summary>
        public object Value => Symbol.Value;
    }
}
