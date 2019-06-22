using SharpDebug.Exceptions;
using SharpUtilities;
using System;
using System.Collections;

namespace SharpDebug.CommonUserTypes.CLR.System
{
    /// <summary>
    /// CLR code Exception. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    /// <seealso cref="SharpDebug.Variable" />
    public class Exception : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exception"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public Exception(Variable variable)
            : base(variable)
        {
            // Check if code type is exception type
            ClrCodeType codeType = variable.GetCodeType() as ClrCodeType;
            bool found = false;

            while (!found && codeType != null)
            {
                if (codeType.Name == "System.Exception")
                {
                    found = true;
                }
                else
                {
                    codeType = (ClrCodeType)codeType.InheritedClass;
                }
            }

            if (!found)
            {
                throw new WrongCodeTypeException(variable.GetCodeType(), nameof(variable), "System.Exception");
            }
        }

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        /// <remarks>This property is marked as virtual in System.Exception. Here, we are just reading what is saved in System.Exception and that might not be what you are expecting.</remarks>
        /// <exception cref="NotImplementedException"></exception>
        public new IDictionary Data
        {
            get
            {
                Variable field = GetField("_data");

                // TODO: Implement
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a link to the help file associated with this exception.
        /// </summary>
        /// <remarks>This property is marked as virtual in System.Exception. Here, we are just reading what is saved in System.Exception and that might not be what you are expecting.</remarks>
        public string HelpLink
        {
            get
            {
                return GetString("_helpURL");
            }
        }

        /// <summary>
        /// Gets the HRESULT, a coded numerical value that is assigned to a specific exception.
        /// </summary>
        public int HResult
        {
            get
            {
                return (int)GetField("_HResult");
            }
        }

        /// <summary>
        /// Gets the Exception instance that caused the current exception.
        /// </summary>
        public Exception InnerException
        {
            get
            {
                return new Exception(GetField("_innerException"));
            }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <remarks>This property is marked as virtual in System.Exception. Here, we are just reading what is saved in System.Exception and that might not be what you are expecting.</remarks>
        public string Message
        {
            get
            {
                return GetString("_message");
            }
        }
        /// <summary>
        /// Gets the the name of the application or the object that causes the error.
        /// </summary>
        /// <remarks>This property is marked as virtual in System.Exception. Here, we are just reading what is saved in System.Exception and that might not be what you are expecting.</remarks>
        public string Source
        {
            get
            {
                return GetString("_source");
            }
        }

        /// <summary>
        /// Gets the StackTrace for this exception. This returns null if no stack trace is associated with this exception object.
        /// </summary>
        /// <remarks>
        /// Note that this may be empty or partial depending on the state of the exception in the process.
        /// (It may have never been thrown or we may be in the middle of constructing the stackwalk.).
        /// </remarks>
        public CodeFunction[] StackTrace
        {
            get
            {
                // Get field containing the stack trace
                Variable field = GetField("_stackTrace");

                if (field.IsNull())
                {
                    return null;
                }

                // Read the stack trace from the bytes buffer
                CodeArray<byte> codeArray = new CodeArray<byte>(field);
                MemoryBuffer buffer = codeArray.ReadMemory();
                Process process = GetCodeType().Module.Process;
                int pointerSize = (int)process.GetPointerSize();
                int offset = 0;
                ulong frameCount = UserType.ReadPointer(buffer, offset, pointerSize);
                CodeFunction[] stackTrace = new CodeFunction[frameCount];

                offset += pointerSize * 2;
                for (ulong i = 0; i < frameCount; i++)
                {
                    ulong instructionPointer = UserType.ReadPointer(buffer, offset, pointerSize);

                    stackTrace[i] = new CodeFunction(instructionPointer, process);
                    offset += pointerSize * 4;
                }

                return stackTrace;
            }
        }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        /// <remarks>This property is marked as virtual in System.Exception. Here, we are just reading what is saved in System.Exception and that might not be what you are expecting.</remarks>
        public string StackTraceString
        {
            get
            {
                return GetString("_stackTraceString");
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Message;
        }

        /// <summary>
        /// Gets the string from the specified string field given by name.
        /// </summary>
        /// <param name="stringFieldName">Name of the string field.</param>
        private string GetString(string stringFieldName)
        {
            Variable field = GetField(stringFieldName);

            if (!field.IsNull())
            {
                return new String(field).Text;
            }

            return null;
        }
    }
}
