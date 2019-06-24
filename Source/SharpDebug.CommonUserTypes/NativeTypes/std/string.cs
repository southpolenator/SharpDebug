namespace SharpDebug.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::string
    /// </summary>
    public class @string : basic_string
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="string"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public @string(Variable variable)
            : base(variable)
        {
        }
    }
}
