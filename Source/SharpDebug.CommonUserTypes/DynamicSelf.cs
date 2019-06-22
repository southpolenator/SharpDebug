namespace CsDebugScript.CommonUserTypes
{
    /// <summary>
    /// Helper class that allows using dynamic field self and easily read members in scripting.
    /// </summary>
    /// <seealso cref="CsDebugScript.Variable" />
    public class DynamicSelfVariable : Variable
    {
        /// <summary>
        /// The dynamic object pointing to self
        /// </summary>
        protected dynamic self;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSelfVariable"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public DynamicSelfVariable(Variable variable)
            : base(variable)
        {
            self = variable;
        }
    }

    /// <summary>
    /// Helper class that allows using dynamic field self and easily read members in scripting.
    /// </summary>
    /// <seealso cref="CsDebugScript.Variable" />
    public class DynamicSelfUserType : UserType
    {
        /// <summary>
        /// The dynamic object pointing to self
        /// </summary>
        protected dynamic self;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSelfUserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public DynamicSelfUserType(Variable variable)
            : base(variable)
        {
            self = variable;
        }
    }
}
