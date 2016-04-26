using CsScripts;
using System;

namespace CsDebugScript.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Used for scoped thread switching.
    /// <example><code>
    ///     using (var switcher = new ThreadSwitcher(thread))
    ///     {
    ///         // Invoke DbgEng.dll interface function
    ///     }
    /// </code></example>
    /// <remarks>Use this class for accessing thread information from DbgEng.dll interfaces to insure correct thread information access.</remarks>
    /// <remarks>For performance reasons, after using scope, previous thread won't be set until it is needed. Always use this class to insure correctness.</remarks>
    /// </summary>
    internal class ThreadSwitcher : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSwitcher" /> class.
        /// </summary>
        /// <param name="stateCache">The state cache.</param>
        /// <param name="thread">The thread.</param>
        public ThreadSwitcher(StateCache stateCache, Thread thread)
        {
            stateCache.SwitchThread(thread);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
