using DbgEng;
using System;
using System.Collections.Generic;
using System.Threading;

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
        private readonly IDebugControl7 control;

        /// <summary>
        /// Dictionaryy containing all breakpoints.
        /// </summary>
        /// <remarks>Is this class even aware of process concept?</remarks>
        private Dictionary<uint, DbgEngBreakpoint> breakpoints = new Dictionary<uint, DbgEngBreakpoint>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugCallbacks"/> class.
        /// </summary>
        /// <param name="control">IDebugControl7 interface.</param>
        /// <param name="debugStatusGoEvent">Event used to signal when debuggee switches to release state.</param>
        public DebugCallbacks(IDebugControl7 control, AutoResetEvent debugStatusGoEvent)
        {
            // TODO: But the loop should always be running? Why this mess with events?
            //
            this.control = control;
            this.debugStatusGoEvent = debugStatusGoEvent;
        }

        /// <summary>
        /// Event used to signal go state. Used to unblock DebugFlow loop.
        /// </summary>
        private AutoResetEvent debugStatusGoEvent;

        /// <summary>
        /// I don't like this here.
        /// </summary>
        /// <param name="breakpoint"></param>
        public void AddBreakpoint(DbgEngBreakpoint breakpoint)
        {
            breakpoints.Add(breakpoint.GetId(), breakpoint);
        }

        public void RemoveBreakpoint(DbgEngBreakpoint breakpoint)
        {
            breakpoints.Remove(breakpoint.GetId());
        }

        /// <summary>
        /// Callback executed when breakpoint gets hit.
        /// </summary>
        /// <param name="Bp">Breakpoint that was hit.</param>
        /// <returns></returns>
        public int Breakpoint(IDebugBreakpoint Bp)
        {
            uint bpId = Bp.GetId();
            breakpoints[bpId].ExecuteAction();

            debugStatusGoEvent.Set();

            return (int)Defines.DebugStatusGo;
        }

        /// <summary>
        /// Callback on change debugee state.
        /// </summary>
        /// <param name="Flags"></param>
        /// <param name="Argument"></param>
        /// <returns></returns>
        public void ChangeDebuggeeState(uint Flags, ulong Argument)
        {
            return;
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
            return (uint)DebugEvent.ChangeDebuggeeState | (uint)DebugEvent.Breakpoint;
        }
    }
}
