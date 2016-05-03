using CsDebugScript.Engine.Debuggers;
using CsDebugScript.Engine.Debuggers.DbgEngDllHelpers;
using System;

namespace CsDebugScript.Engine.Utility
{
    /// <summary>
    /// Helper class for replacing debugger output callbacks - used when capturing command output. Example usage:
    /// <para>using (var switcher = OutputCallbacksSwitcher.Create(outputCallbacks) { }</para>
    /// </summary>
    public abstract class OutputCallbacksSwitcher : IDisposable
    {
        /// <summary>
        /// Creates the output callbacks switcher.
        /// </summary>
        /// <param name="callbacks">The callbacks.</param>
        public static OutputCallbacksSwitcher Create(DebuggerOutputToTextWriter callbacks)
        {
            return new DbgEngOutputCallbacksSwitcher((DbgEngDll)Context.Debugger, (DbgEngDebuggerOutputToTextWriter)callbacks);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();
    }
}
