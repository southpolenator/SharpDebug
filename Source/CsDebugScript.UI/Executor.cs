using System;
using System.Linq;
using System.Runtime.InteropServices;
using DbgEngManaged;
using CsDebugScript.UI;
using CsDebugScript.Engine;

namespace CsDebugScript
{
    /// <summary>
    /// Exported class for transferring execution from native to managed code
    /// </summary>
    [Guid("E2C73928-FEA4-4DF6-AD34-82BAAFC024B8"), ComVisible(true)]
    public sealed class Executor : IExecutor
    {
        /// <summary>
        /// The interactive execution
        /// </summary>
        internal static InteractiveExecution InteractiveExecution = new InteractiveExecution();

        #region IExecutor implementation
        /// <summary>
        /// Initializes the context.
        /// </summary>
        /// <param name="client">The client.</param>
        public void InitializeContext(IDebugClient client)
        {
            Context.Initalize(client);
        }

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        /// <param name="args">The arguments.</param>
        public void ExecuteScript(string scriptPath, string[] args)
        {
            Execute(scriptPath, args);
        }

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        public void ExecuteScript(string arguments)
        {
            string[] args = arguments.Split(" ".ToCharArray());

            ExecuteScript(args[0], args.Skip(1).ToArray());
        }

        /// <summary>
        /// Enters the interactive mode.
        /// </summary>
        /// <param name="arguments">The arguments</param>
        public void EnterInteractiveMode(string arguments)
        {
            Context.Debugger.ExecuteAction(() => InteractiveExecution.Run());
        }

        /// <summary>
        /// Interprets C# code.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        public void Interpret(string arguments)
        {
            InterpretInteractive(arguments);
        }

        /// <summary>
        /// Opens the UI.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        public void OpenUI(string arguments)
        {
            string[] args = arguments.Split(" ".ToCharArray());
            bool showModal = false;

            if (args.Length > 0 && !bool.TryParse(args[0], out showModal))
            {
                showModal = false;
            }

            ShowInteractiveWindow(showModal);
        }
        #endregion

        /// <summary>
        /// Executes the specified script.
        /// </summary>
        /// <param name="path">The script path.</param>
        /// <param name="args">The arguments.</param>
        public static void Execute(string path, params string[] args)
        {
            Context.Debugger.ExecuteAction(() =>
            {
                using (ScriptExecution execution = new ScriptExecution())
                {
                    execution.Execute(path, args);
                }
            });
        }

        /// <summary>
        /// Shows the interactive window.
        /// </summary>
        /// <param name="modal">if set to <c>true</c> window will be shown as modal dialog.</param>
        public static void ShowInteractiveWindow(bool modal)
        {
            if (modal)
            {
                InteractiveWindow.ShowModalWindow();
            }
            else
            {
                InteractiveWindow.ShowWindow();
            }
        }

        /// <summary>
        /// Interprets the C# code as in interactive mode.
        /// </summary>
        /// <param name="code">The C# code.</param>
        public static void InterpretInteractive(string code)
        {
            Context.Debugger.ExecuteAction(() => InteractiveExecution.Interpret(code));
        }
    }
}
