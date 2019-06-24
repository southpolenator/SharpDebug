using SharpDebug.CodeGen.SymbolProviders;

namespace SharpDebug.CodeGen.UserTypes.Members
{
    /// <summary>
    /// Represents data field declared in user type.
    /// </summary>
    internal class DataFieldUserTypeMember : UserTypeMember
    {
        /// <summary>
        /// Gets or sets symbol describing data field.
        /// </summary>
        public SymbolField Symbol { get; set; }

        /// <summary>
        /// Gets flag representing if this is static field.
        /// </summary>
        public bool IsStatic => Symbol.IsStatic || UserType is GlobalsUserType;

        /// <summary>
        /// Gets the comment associated with data field.
        /// </summary>
        public string Comment => $"// {Symbol.Type.Name} {Symbol.Name};";
    }
}
