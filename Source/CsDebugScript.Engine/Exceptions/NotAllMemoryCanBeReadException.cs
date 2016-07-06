using System;

namespace CsDebugScript.Exceptions
{
    /// <summary>
    /// Thrown when user wants to read memory that is not fully available in the process being debugged.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class NotAllMemoryCanBeReadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotAllMemoryCanBeReadException"/> class.
        /// </summary>
        /// <param name="address">The accessed address.</param>
        /// <param name="requestedSize">The requested size.</param>
        /// <param name="availableSize">The available size.</param>
        public NotAllMemoryCanBeReadException(ulong address, uint requestedSize, uint availableSize)
            : base(string.Format("Not all memory can be read at address (0x{0:X}). Requested memory block ({1}) is larger than available memory block ({2}).", address, requestedSize, availableSize))
        {
            Address = address;
            RequestedSize = requestedSize;
            AvailableSize = availableSize;
        }

        /// <summary>
        /// Gets the accessed address.
        /// </summary>
        public ulong Address { get; private set; }

        /// <summary>
        /// Gets the requested size.
        /// </summary>
        public uint RequestedSize { get; private set; }

        /// <summary>
        /// Gets the available size.
        /// </summary>
        public uint AvailableSize { get; private set; }
    }
}
