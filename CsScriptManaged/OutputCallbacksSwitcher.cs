using DbgEngManaged;
using System;

namespace CsScriptManaged
{
    public class OutputCallbacksSwitcher : IDisposable
    {
        private IDebugOutputCallbacksWide previousCallbacks;

        public OutputCallbacksSwitcher(IDebugOutputCallbacksWide newCallbacks)
        {
            previousCallbacks = Context.Client.GetOutputCallbacksWide();
            Context.Client.SetOutputCallbacksWide(newCallbacks);
        }

        public void Dispose()
        {
            Context.Client.SetOutputCallbacksWide(previousCallbacks);
        }
    }
}
