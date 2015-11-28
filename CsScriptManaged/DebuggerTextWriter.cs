using DbgEngManaged;
using System.IO;
using System.Text;

namespace CsScriptManaged
{
    /// <summary>
    /// Helper class for redirecting console out/error to debuggers output
    /// </summary>
    public class DebuggerTextWriter : TextWriter
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
        /// Initializes a new instance of the <see cref="DebuggerTextWriter"/> class.
        /// </summary>
        /// <param name="outputType">Type of the output.</param>
        public DebuggerTextWriter(DebugOutput outputType)
        {
            OutputType = outputType;
            outputCallbacksWide = Context.Client.GetOutputCallbacksWide();
            outputCallbacks = Context.Client.GetOutputCallbacks();
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
    }
}
