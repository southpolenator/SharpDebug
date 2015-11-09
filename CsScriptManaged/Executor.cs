using System;
using System.Linq;
using System.Runtime.InteropServices;
using DbgEngManaged;

namespace CsScriptManaged
{
    [Guid("E2C73928-FEA4-4DF6-AD34-82BAAFC024B8"), ComVisible(true)]
    public sealed class Executor : IExecutor
    {
        public void InitializeContext(IDebugClient client)
        {
            Context.Initalize(client);
        }

        public void ExecuteScript(string scriptPath, string[] args)
        {
            Context.Execute(scriptPath, args);
        }

        public void ExecuteScript(string arguments)
        {
            string[] args = arguments.Split(" ".ToCharArray());

            ExecuteScript(args[0], args.Skip(1).ToArray());
        }
    }
}
