namespace CsDebugScript
{
    /// <summary>
    /// Wrapper class that represents a void type
    /// </summary>
    public class VoidType : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoidType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="System.Exception">Wrong code type of passed variable  + GetCodeType().Name</exception>
        public VoidType(Variable variable)
            : base(variable)
        {
        }
    }
}
