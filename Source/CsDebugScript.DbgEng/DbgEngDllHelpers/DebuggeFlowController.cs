using DbgEngManaged;
using System;

namespace CsDebugScript.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Controller for Debugee actions during live debugging.
    /// </summary>
    class DebuggeeFlowController
    {
        /// <summary>
        /// Signal fired during interactive process debugging when debugee is released.
        /// </summary>
        public System.Threading.AutoResetEvent DebugStatusGo { get; private set; }

        /// <summary>
        /// Signal fired during interactive process debugging when debugee is interrupted.
        /// </summary>
        public System.Threading.AutoResetEvent DebugStatusBreak { get; private set; }

        /// <summary>
        /// Loop responsible for catching debug events and signaling debugee state.
        /// </summary>
        private System.Threading.Thread debuggerStateLoop;

        /// <summary>
        /// Reference to <see cref="DbgEngDll"/>.
        /// </summary>
        private DbgEngDll dbgEngDll;

        /// <summary>
        /// Synchronization signaling that debug callbacks are installed.
        /// </summary>
        private static readonly object eventCallbacksReady = new Object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggeeFlowController"/> class.
        /// </summary>
        /// <param name="dbgEngDll">The DbgEngDll.</param>
        public DebuggeeFlowController(DbgEngDll dbgEngDll)
        {
            // Default is that we start in break mode.
            // TODO: Needs to be changed when we allow non intrusive attach/start for example.
            //
            DebugStatusGo = new System.Threading.AutoResetEvent(false);
            DebugStatusBreak = new System.Threading.AutoResetEvent(true);

            this.dbgEngDll = dbgEngDll;
            lock (eventCallbacksReady)
            {
                debuggerStateLoop =
                    new System.Threading.Thread(() => DebuggerStateLoop()) { IsBackground = true };
                debuggerStateLoop.SetApartmentState(System.Threading.ApartmentState.MTA);
                debuggerStateLoop.Start();

                // Wait for loop thread to become ready.
                //
                System.Threading.Monitor.Wait(eventCallbacksReady);
            }
        }

        /// <summary>
        /// Loop responsible to wait for debug events.
        /// Needs to be run in separate thread.
        /// </summary>
        private void DebuggerStateLoop()
        {
            bool hasClientExited = false;
            IDebugControl7 loopControl = dbgEngDll.Control;
            DebugCallbacks eventCallbacks = new DebugCallbacks(dbgEngDll.ThreadClient, DebugStatusGo);

            lock (eventCallbacksReady)
            {
                System.Threading.Monitor.Pulse(eventCallbacksReady);
            }

            // Default is to start in break mode, wait for release.
            //
            DebugStatusGo.WaitOne();

            while (!hasClientExited)
            {
                loopControl.WaitForEvent(0, UInt32.MaxValue);
                uint executionStatus = loopControl.GetExecutionStatus();

                while (executionStatus == (uint)Defines.DebugStatusBreak)
                {
                    DebugStatusBreak.Set();
                    DebugStatusGo.WaitOne();

                    executionStatus = loopControl.GetExecutionStatus();
                }

                hasClientExited = executionStatus == (uint)Defines.DebugStatusNoDebuggee;
            }
        }

        /// <summary>
        /// Waits for debugger loop thread to finish.
        /// </summary>
        public void WaitForDebuggerLoopToExit()
        {
            debuggerStateLoop.Join();
        }
    }
}
