using System;
using System.CodeDom.Compiler;

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
        public CompileException(CompilerError[] errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// Gets the array of errors.
        /// </summary>
        public CompilerError[] Errors { get; private set; }
    }
}
