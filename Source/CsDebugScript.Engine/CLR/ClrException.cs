using CsDebugScript.Exceptions;
using System;
using System.Collections;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code Exception. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    /// <seealso cref="CsDebugScript.Variable" />
    public class ClrException : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrException"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public ClrException(Variable variable)
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
        /// Initializes a new instance of the <see cref="ClrException"/> class.
        /// </summary>
        /// <param name="clrThread">The CLR thread.</param>
        /// <param name="clrException">The CLR exception.</param>
        internal ClrException(ClrThread clrThread, Microsoft.Diagnostics.Runtime.ClrException clrException)
            : base(clrThread.Process.FromClrType(clrException.Type), ulong.MaxValue, Variable.ComputedName, Variable.UnknownPath, clrException.Address)
        {
        }

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        /// <remarks>This property is marked as virtual in System.Exception. Here, we are just reading what is saved in System.Exception and that might not be what you are expecting.</remarks>
        /// <exception cref="System.NotImplementedException"></exception>
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
        public ClrException InnerException
        {
            get
            {
                return new ClrException(GetField("_innerException"));
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
        public StackTrace StackTrace
        {
            get
            {
                Variable field = GetField("_stackTrace");

                // TODO: Implement
                throw new NotImplementedException();
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
        /// Gets the string from the specified string field given by name.
        /// </summary>
        /// <param name="stringFieldName">Name of the string field.</param>
        private string GetString(string stringFieldName)
        {
            Variable field = GetField(stringFieldName);

            if (field != null && !field.IsNullPointer())
            {
                return new ClrString(field).Text;
            }

            return null;
        }
    }
}
