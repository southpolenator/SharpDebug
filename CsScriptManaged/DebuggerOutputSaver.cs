using DbgEngManaged;
using System.Text;

namespace CsScriptManaged
{
    /// <summary>
    /// Helper class for capturing debugger output while executing commands
    /// </summary>
    public class DebuggerOutputSaver : IDebugOutputCallbacksWide
    {
        /// <summary>
        /// The string builder
        /// </summary>
        private StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerOutputSaver"/> class.
        /// </summary>
        /// <param name="captureFlags">The capture flags.</param>
        public DebuggerOutputSaver(DebugOutput captureFlags)
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
}
