using CsScripts;
using System;

namespace CsScriptManaged
{
    /// <summary>
    /// Used for scoped process switching
    /// </summary>
    public class ProcessSwitcher : IDisposable
    {
        /// <summary>
        /// The old process identifier
        /// </summary>
        private uint oldProcessId;

        /// <summary>
        /// The new process identifier
        /// </summary>
        private uint newProcessId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSwitcher"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        public ProcessSwitcher(Process process)
            : this(process.Id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSwitcher"/> class.
        /// </summary>
        /// <param name="newProcessId">The new process identifier.</param>
        public ProcessSwitcher(uint newProcessId)
        {
            oldProcessId = Context.SystemObjects.GetCurrentProcessId();
            this.newProcessId = newProcessId;

            SetProcessId(newProcessId);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            SetProcessId(oldProcessId);
        }

        /// <summary>
        /// Executes specified action in protected scope.
        /// </summary>
        /// <typeparam name="T">The specified type</typeparam>
        /// <param name="process">The process.</param>
        /// <param name="action">The action.</param>
        public static Func<T> DelegateProtector<T>(Process process, Func<T> action)
        {
            return () =>
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(process))
                {
                    return action();
                }
            };
        }

        /// <summary>
        /// Sets the current process to the process identifier.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        private void SetProcessId(uint processId)
        {
            if (Context.SystemObjects.GetCurrentProcessId() != processId)
            {
                Context.SystemObjects.SetCurrentProcessId(processId);
            }
        }
    }
}
