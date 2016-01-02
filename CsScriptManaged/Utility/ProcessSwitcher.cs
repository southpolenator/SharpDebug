using CsScripts;
using System;

namespace CsScriptManaged.Utility
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
    public class ProcessSwitcher : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSwitcher"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        public ProcessSwitcher(Process process)
        {
            Context.StateCache.SwitchProcess(process);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Executes specified action in protected scope.
        /// </summary>
        /// <typeparam name="T">The specified type</typeparam>
        /// <param name="process">The process.</param>
        /// <param name="action">The action.</param>
        internal static Func<T> DelegateProtector<T>(Process process, Func<T> action)
        {
            return () =>
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(process))
                {
                    return action();
                }
            };
        }
    }
}
