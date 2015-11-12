using CsScripts;
using System;

namespace CsScriptManaged
{
    public class ThreadSwitcher : IDisposable
    {
        /// <summary>
        /// The process switcher
        /// </summary>
        private ProcessSwitcher processSwitcher;

        /// <summary>
        /// The old thread identifier
        /// </summary>
        private uint oldThreadId;

        /// <summary>
        /// The new thread identifier
        /// </summary>
        private uint newThreadId;

        public ThreadSwitcher(Thread thread)
        {
            processSwitcher = new ProcessSwitcher(thread.Process);
            oldThreadId = Context.SystemObjects.GetCurrentThreadId();
            this.newThreadId = thread.Id;

            SetThreadId(newThreadId);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            SetThreadId(oldThreadId);
            processSwitcher.Dispose();
        }

        private void SetThreadId(uint newThreadId)
        {
            Context.SystemObjects.SetCurrentThreadId(newThreadId);
        }
    }
}
