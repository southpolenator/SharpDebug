using System;
using System.Runtime.Serialization;

namespace CsDebugScript.Exceptions
{
    /// <summary>
    /// Thrown when user wants to access address that is not available in the process being debugged.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
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
        /// Initializes a new instance of the InvalidMemoryAddressException class with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected InvalidMemoryAddressException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Address = info.GetUInt64("Address");
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
        }

        /// <summary>
        /// Gets the accessed address.
        /// </summary>
        public ulong Address { get; }
    }
}
