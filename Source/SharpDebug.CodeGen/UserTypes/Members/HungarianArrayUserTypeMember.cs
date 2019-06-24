namespace SharpDebug.CodeGen.UserTypes.Members
{
    /// <summary>
    /// Represents generated array property based on data fields declared in user type that match Hungarian notation.
    /// </summary>
    internal class HungarianArrayUserTypeMember : UserTypeMember
    {
        /// <summary>
        /// Pointer data field name.
        /// </summary>
        public string PointerFieldName { get; set; }

        /// <summary>
        /// Counter data field name.
        /// </summary>
        public string CounterFieldName { get; set; }

        /// <summary>
        /// Gets the comment describing this member.
        /// </summary>
        public string Comment => $"// From Hungarian notation: {PointerFieldName}[{CounterFieldName}]";
    }
}
