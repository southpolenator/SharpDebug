using CsScripts;
using System;

namespace CsScriptManaged.Utility
{
    /// <summary>
    /// Used for scoped thread switching. Example usage:
    /// <para>using (var switcher = new ThreadSwitcher(thread) { }</para>
    /// <para>Note: Use this class for accessing thread information from DbgEng.dll interfaces to insure correct thread information access.</para>
    /// <para>Note: For performance reasons, after using scope, previous thread won't be set until is needed. Use this class to insure correctness.</para>
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSwitcher"/> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
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

        /// <summary>
        /// Sets the current thread identifier.
        /// </summary>
        /// <param name="newThreadId">The new thread identifier.</param>
        private void SetThreadId(uint newThreadId)
        {
            Context.SystemObjects.SetCurrentThreadId(newThreadId);
        }
    }
}
