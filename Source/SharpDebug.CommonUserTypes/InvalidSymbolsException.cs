using System;

namespace SharpDebug.CommonUserTypes
{
    /// <summary>
    /// Exception that is being thrown when symbols are not valid.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidSymbolsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSymbolsException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidSymbolsException(string message)
            : base(message)
        {
        }
    }
}
