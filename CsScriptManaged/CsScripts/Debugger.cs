using CsScriptManaged;
using DbgEngManaged;

namespace CsScripts
{
    public class Debugger
    {
        public static string ExecuteAndCapture(string command, params object[] parameters)
        {
            var callbacks = new DebuggerOutputSaver();
            using (OutputCallbacksSwitcher switcher = new OutputCallbacksSwitcher(callbacks))
            {
                Execute(command, parameters);
                return callbacks.Text;
            }
        }

        public static void Execute(string command, params object[] parameters)
        {
            command = string.Join(" ", command, string.Join(" ", parameters));
            Context.Control.Execute((uint)DebugOutctl.ThisClient, command, (uint)(DebugExecute.NotLogged_ | DebugExecute.NoRepeat));
        }
    }
}
