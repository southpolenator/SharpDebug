using CsScriptManaged;
using DbgEngManaged;

namespace CsScripts
{
    /// <summary>
    /// Helper class that controls the debugger
    /// </summary>
    public class Debugger
    {
        /// <summary>
        /// Executes the specified command and captures its output.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static string ExecuteAndCapture(string command, params object[] parameters)
        {
            var callbacks = new DebuggerOutputSaver();
            using (OutputCallbacksSwitcher switcher = new OutputCallbacksSwitcher(callbacks))
            {
                Execute(command, parameters);
                return callbacks.Text;
            }
        }

        /// <summary>
        /// Executes the specified command, but leaves its output visible to the user.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Execute(string command, params object[] parameters)
        {
            command = string.Join(" ", command, string.Join(" ", parameters));
            Context.Control.Execute((uint)DebugOutctl.ThisClient, command, (uint)(DebugExecute.NotLogged_ | DebugExecute.NoRepeat));
        }
    }
}
