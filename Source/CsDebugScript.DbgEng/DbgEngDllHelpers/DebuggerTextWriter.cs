using DbgEng;
using System.IO;
using System.Text;

namespace CsDebugScript.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Helper class for redirecting console out/error to debuggers output
    /// </summary>
    internal class DebuggerTextWriter : TextWriter
    {
        /// <summary>
        /// The output callbacks
        /// </summary>
        private IDebugOutputCallbacks outputCallbacks;

        /// <summary>
        /// The output callbacks wide
        /// </summary>
        private IDebugOutputCallbacksWide outputCallbacksWide;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerTextWriter" /> class.
        /// </summary>
        /// <param name="dbgEngDll">The DbgEngDll debugger engine.</param>
        /// <param name="outputType">Type of the output.</param>
        public DebuggerTextWriter(DbgEngDll dbgEngDll, DebugOutput outputType)
        {
            OutputType = outputType;
            outputCallbacksWide = dbgEngDll.Client.GetOutputCallbacksWide();
            outputCallbacks = dbgEngDll.Client.GetOutputCallbacks();
        }

        /// <summary>
        /// Gets or sets the type of the output.
        /// </summary>
        public DebugOutput OutputType { get; set; }

        /// <summary>
        /// When overridden in a derived class, returns the character encoding in which the output is written.
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }

        /// <summary>
        /// Writes a character to the text string or stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        public override void Write(char value)
        {
            if (outputCallbacksWide != null)
            {
                outputCallbacksWide.Output((uint)OutputType, value.ToString());
            }
            else if (outputCallbacks != null)
            {
                outputCallbacks.Output((uint)OutputType, value.ToString());
            }
        }

        /// <summary>
        /// Writes a string to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public override void Write(string value)
        {
            if (outputCallbacksWide != null)
            {
                outputCallbacksWide.Output((uint)OutputType, value);
            }
            else if (outputCallbacks != null)
            {
                outputCallbacks.Output((uint)OutputType, value);
            }
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }
    }
}
