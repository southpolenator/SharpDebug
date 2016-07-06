using System;

namespace CsDebugScript.Exceptions
{
    /// <summary>
    /// Exception that is thrown when variable of unexpected code type has been given as an argument.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class WrongCodeTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongCodeTypeException" /> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="expectedText">The expected text.</param>
        public WrongCodeTypeException(Variable variable, string argumentName, string expectedText)
            : base(string.Format("Wrong code type [{0}] of passed parameter '{1}'. Expected {2}.", variable.GetCodeType().Name, argumentName, expectedText))
        {
            Variable = variable;
            CodeType = variable.GetCodeType();
            ArgumentName = argumentName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrongCodeTypeException"/> class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="expectedText">The expected text.</param>
        public WrongCodeTypeException(CodeType codeType, string argumentName, string expectedText)
            : base(string.Format("Wrong code type [{0}] of passed parameter '{1}'. Expected {2}.", codeType.Name, argumentName, expectedText))
        {
        }

        /// <summary>
        /// Gets the variable which was passed as an argument.
        /// </summary>
        public Variable Variable { get; private set; }

        /// <summary>
        /// Gets the code type which was passed as an argument.
        /// </summary>
        public CodeType CodeType { get; private set; }

        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        public string ArgumentName { get; private set; }
    }
}
