using System;

namespace CsDebugScript.Exceptions
{
    /// <summary>
    /// Thrown when user wants to access address that is not available in the process being debugged.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidMemoryAddressException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMemoryAddressException"/> class.
        /// </summary>
        /// <param name="address">The accessed address.</param>
        public InvalidMemoryAddressException(ulong address)
            : base(string.Format("Tried to access memory address (0x{0:X}) that is not found in the process.", address))
        {
            Address = address;
        }

        /// <summary>
        /// Gets the accessed address.
        /// </summary>
        public ulong Address { get; private set; }
    }
}
