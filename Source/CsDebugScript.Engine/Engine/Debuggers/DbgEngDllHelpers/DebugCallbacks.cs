using DbgEngManaged;
using System;

namespace CsDebugScript.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Debug callbacks called during WaitForEvent callback.
    /// This class in future can be extended to support callbacks provided
    /// on certain actions (e.g. breakpoint hit, thread create, module load etc.)
    /// </summary>
    class DebugCallbacks : IDebugEventCallbacks
    {
        /// <summary>
        /// IDebugClient.
        /// </summary>
        private IDebugClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugCallbacks"/> class.
        /// </summary>
        /// <param name="client">IDebugClient interface.</param>
        /// <param name="debugStatusGoEvent">Event used to signal when debuggee switches to release state.</param>
        public DebugCallbacks(IDebugClient client, System.Threading.AutoResetEvent debugStatusGoEvent)
        {
            this.client = client;
            this.debugStatusGoEvent = debugStatusGoEvent;
            this.client.SetEventCallbacks(this);
        }

        /// <summary>
        /// Event used to signal go state. Used to unblock DebugFlow loop.
        /// </summary>
        private System.Threading.AutoResetEvent debugStatusGoEvent;

        public void Breakpoint(IDebugBreakpoint Bp)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Callback on change debugee state.
        /// </summary>
        /// <param name="Flags"></param>
        /// <param name="Argument"></param>
        /// <returns></returns>
        public void ChangeDebuggeeState(uint Flags, ulong Argument)
        {
            uint executionStatus = ((IDebugControl7)client).GetExecutionStatus();

            if (executionStatus == (uint)Defines.DebugStatusGo)
            {
                debugStatusGoEvent.Set();
            }
        }

        public void ChangeEngineState(uint Flags, ulong Argument)
        {
            throw new NotImplementedException();
        }

        public void ChangeSymbolState(uint Flags, ulong Argument)
        {
            throw new NotImplementedException();
        }

        public void CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset)
        {
            throw new NotImplementedException();
        }

        public void CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset)
        {
            throw new NotImplementedException();
        }

        public void Exception(ref _EXCEPTION_RECORD64 Exception, uint FirstChance)
        {
            throw new NotImplementedException();
        }

        public void ExitProcess(uint ExitCode)
        {
            throw new NotImplementedException();
        }

        public void ExitThread(uint ExitCode)
        {
            throw new NotImplementedException();
        }

        public void LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp)
        {
            throw new NotImplementedException();
        }

        public void SessionStatus(uint Status)
        {
            throw new NotImplementedException();
        }

        public void SystemError(uint Error, uint Level)
        {
            throw new NotImplementedException();
        }

        public void UnloadModule(string ImageBaseName, ulong BaseOffset)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Interest mask for events for which
        /// callbacks will be executed.
        /// </summary>
        /// <returns></returns>
        public uint GetInterestMask()
        {
            return (uint)Defines.DebugEventChangeDebuggeeState;
        }
    }
}
