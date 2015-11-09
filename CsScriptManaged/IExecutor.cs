using System;
using System.Runtime.InteropServices;
using DbgEngManaged;

namespace CsScriptManaged
{
    [Guid("6DA6ED88-0349-4BAB-B87F-69988261EB3B"), ComVisible(true)]
    public interface IExecutor
    {
        void InitializeContext(IDebugClient client);

        void ExecuteScript(string scriptPath, string[] args);

        void ExecuteScript(string arguments);
    }
}
