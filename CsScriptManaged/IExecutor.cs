using System;
using System.Runtime.InteropServices;
using DbgEngManaged;

namespace CsScriptManaged
{
    /// <summary>
    /// Exported interface for transferring execution from native to managed code
    /// </summary>
    [Guid("6DA6ED88-0349-4BAB-B87F-69988261EB3B"), ComVisible(true)]
    public interface IExecutor
    {
        /// <summary>
        /// Initializes the context.
        /// </summary>
        /// <param name="client">The client.</param>
        void InitializeContext(IDebugClient client);

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        /// <param name="args">The arguments.</param>
        void ExecuteScript(string scriptPath, string[] args);

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        void ExecuteScript(string arguments);

        /// <summary>
        /// Enters the interactive mode.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        void EnterInteractiveMode(string arguments);
    }
}
