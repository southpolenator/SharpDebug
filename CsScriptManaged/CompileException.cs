using System;

namespace CsScriptManaged
{
    /// <summary>
    /// Exception that contains all compiler errors that happened
    /// </summary>
    public class CompileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompileException"/> class.
        /// </summary>
        /// <param name="errors">The errors.</param>
        public CompileException(CompileError[] errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// Gets the array of errors.
        /// </summary>
        public CompileError[] Errors { get; private set; }
    }
}
