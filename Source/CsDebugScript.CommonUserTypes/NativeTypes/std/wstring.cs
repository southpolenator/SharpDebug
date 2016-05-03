namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::wstring
    /// </summary>
    public class wstring : basic_string
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="wstring"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public wstring(Variable variable)
            : base(variable)
        {
        }
    }
}
