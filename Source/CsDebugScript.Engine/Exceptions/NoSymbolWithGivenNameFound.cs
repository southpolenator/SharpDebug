using System;

namespace CsDebugScript.Exceptions
{
    /// <summary>
    /// Exception thrown when search for symbol doesn't return unique result.
    /// </summary>
    public class NoSymbolWithGivenNameFound : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoSymbolWithGivenNameFound"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        public NoSymbolWithGivenNameFound(string symbol)
            : base(string.Format("No symbol could be found with the specified name. Symbol name {0}.", symbol))
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Gets the symbol name.
        /// </summary>
        public string Symbol { get; private set; }
    }
}
