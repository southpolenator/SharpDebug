using DbgEngManaged;
using System;

namespace CsScriptManaged.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Helper class for replacing debugger output callbacks - used when capturing command output. Example usage:
    /// <para>using (var switcher = new OutputCallbacksSwitcher(outputCallbacks) { }</para>
    /// </summary>
    internal class OutputCallbacksSwitcher : IDisposable
    {
        /// <summary>
        /// The previous callbacks interface
        /// </summary>
        private IDebugOutputCallbacksWide previousCallbacks;

        /// <summary>
        /// The DbgEngDll debugger engine.
        /// </summary>
        private DbgEngDll dbgEngDll;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputCallbacksSwitcher" /> class.
        /// </summary>
        /// <param name="dbgEngDll">The DbgEngDll debugger engine.</param>
        /// <param name="newCallbacks">The new callbacks interface.</param>
        public OutputCallbacksSwitcher(DbgEngDll dbgEngDll, IDebugOutputCallbacksWide newCallbacks)
        {
            previousCallbacks = dbgEngDll.Client.GetOutputCallbacksWide();
            dbgEngDll.Client.SetOutputCallbacksWide(newCallbacks);
            this.dbgEngDll = dbgEngDll;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            dbgEngDll.Client.SetOutputCallbacksWide(previousCallbacks);
        }
    }
}
