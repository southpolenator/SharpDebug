using DbgEngManaged;
using System;

namespace CsScriptManaged.Utility
{
    /// <summary>
    /// Helper class for replacing debugger output callbacks - used when capturing command output
    /// </summary>
    public class OutputCallbacksSwitcher : IDisposable
    {
        /// <summary>
        /// The previous callbacks interface
        /// </summary>
        private IDebugOutputCallbacksWide previousCallbacks;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputCallbacksSwitcher"/> class.
        /// </summary>
        /// <param name="newCallbacks">The new callbacks interface.</param>
        public OutputCallbacksSwitcher(IDebugOutputCallbacksWide newCallbacks)
        {
            previousCallbacks = Context.Client.GetOutputCallbacksWide();
            Context.Client.SetOutputCallbacksWide(newCallbacks);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Context.Client.SetOutputCallbacksWide(previousCallbacks);
        }
    }
}
