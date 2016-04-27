using CsDebugScript.Debuggers.DbgEngDllHelpers;
using DbgEngManaged;
using System.IO;

namespace CsDebugScript.Utility
{
    /// <summary>
    /// Helper class for capturing debugger output while executing commands. Example usage:
    /// <para>using (var callbacks = DebuggerOutputToTextWriter.Create(textWriter, captureFlags) { }</para>
    /// </summary>
    public class DebuggerOutputToTextWriter
    {
        /// <summary>
        /// Creates new debugger output capturer.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="captureFlags">The capture flags.</param>
        public static DebuggerOutputToTextWriter Create(TextWriter textWriter, DebugOutput captureFlags)
        {
            return new DbgEngDebuggerOutputToTextWriter(textWriter, captureFlags);
        }
    }
}
