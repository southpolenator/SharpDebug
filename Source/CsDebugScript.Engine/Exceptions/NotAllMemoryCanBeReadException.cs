using System;
using System.Runtime.Serialization;

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
        /// Initializes a new instance of the NotAllMemoryCanBeReadException class with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected NotAllMemoryCanBeReadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Address = info.GetUInt64("Address");
            RequestedSize = info.GetUInt32("RequestedSize");
            AvailableSize = info.GetUInt32("AvailableSize");
        }

        /// <summary>
        /// Sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Address", Address);
            info.AddValue("RequestedSize", RequestedSize);
            info.AddValue("AvailableSize", AvailableSize);
        }

        /// <summary>
        /// Gets the accessed address.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Gets the requested size.
        /// </summary>
        public uint RequestedSize { get; }

        /// <summary>
        /// Gets the available size.
        /// </summary>
        public uint AvailableSize { get; }
    }
}
