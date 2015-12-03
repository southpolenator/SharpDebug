namespace CsScripts
{
    /// <summary>
    /// Base class for user defined types in C# scripts
    /// </summary>
    public class UserType : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public UserType(Variable variable)
            : base(variable)
        {
        }
    }
}
