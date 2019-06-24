using System;

namespace SharpDebug.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Used for scoped process switching.
    /// <example><code>
    ///     using (var switcher = new ProcessSwitcher(process))
    ///     {
    ///         // Invoke DbgEng.dll interface function
    ///     }
    /// </code></example>
    /// <remarks>Use this class for accessing process information from DbgEng.dll interfaces to insure correct process information access.</remarks>
    /// <remarks>For performance reasons, after using scope, previous process won't be set until it is needed. Always use this class to insure correctness.</remarks>
    /// </summary>
    internal class ProcessSwitcher : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSwitcher" /> class.
        /// </summary>
        /// <param name="stateCache">The state cache.</param>
        /// <param name="process">The process.</param>
        public ProcessSwitcher(StateCache stateCache, Process process)
        {
            stateCache.SwitchProcess(process);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
