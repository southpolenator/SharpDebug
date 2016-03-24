using DbgEngManaged;
using System.IO;
using System.Text;

namespace CsScriptManaged.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Helper class for capturing debugger output while executing commands
    /// </summary>
    internal class DebuggerOutputCapture : IDebugOutputCallbacksWide
    {
        /// <summary>
        /// The string builder
        /// </summary>
        private StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerOutputCapture"/> class.
        /// </summary>
        /// <param name="captureFlags">The capture flags.</param>
        public DebuggerOutputCapture(DebugOutput captureFlags)
        {
            CaptureFlags = captureFlags;
        }

        /// <summary>
        /// Gets or sets the capture flags.
        /// </summary>
        public DebugOutput CaptureFlags { get; set; }

        /// <summary>
        /// Gets the captured text.
        /// </summary>
        public string Text
        {
            get
            {
                return sb.ToString();
            }
        }

        /// <summary>
        /// Captures the specified text.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="text">The text.</param>
        public void Output(uint mask, string text)
        {
            if ((mask & (uint)CaptureFlags) != 0)
            {
                sb.Append(text);
            }
        }
    }

    /// <summary>
    /// Helper class for capturing debugger output while executing commands
    /// </summary>
    internal class DebuggerOutputToTextWriter : IDebugOutputCallbacksWide
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerOutputToTextWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="captureFlags">The capture flags.</param>
        public DebuggerOutputToTextWriter(TextWriter textWriter, DebugOutput captureFlags)
        {
            CaptureFlags = captureFlags;
            TextWriter = textWriter;
        }

        /// <summary>
        /// Gets or sets the capture flags.
        /// </summary>
        public DebugOutput CaptureFlags { get; set; }

        /// <summary>
        /// Gets the output (text writer).
        /// </summary>
        public TextWriter TextWriter { get; private set; }

        /// <summary>
        /// Captures the specified text.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="text">The text.</param>
        public void Output(uint mask, string text)
        {
            if ((mask & (uint)CaptureFlags) != 0)
            {
                TextWriter.Write(text);
            }
        }
    }
}
