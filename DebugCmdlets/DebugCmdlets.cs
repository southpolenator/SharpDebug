using System.Management.Automation;
using CsScriptManaged;
using CsScriptManaged.Utility;
using CsScripts;
using DbgEngManaged;
using System;
using System.Runtime.InteropServices;

namespace PowershellDebugSession 
{
    /// <summary>
    /// Singleton class for interacting with currently opened debug session.
    /// </summary>
    class ConnectionState
    {
        public bool IsConnected { get; set; }
        public string ProcessPath { get; set; }

        public static ConnectionState GetConnectionState()
        {
            if (connectionState == null)
            {
                connectionState = new ConnectionState();
                return connectionState;
            }
            else
            {
                return connectionState;
            }
        }

        private static ConnectionState connectionState = null;
    }

    /// <summary>
    /// Cmdlet for staring debug session.
    /// </summary>
    [Cmdlet(VerbsCommunications.Connect, "StartDebugSession")]
    public class StartDbgSession: Cmdlet
    {
        [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", SetLastError = false)]
        public static extern int DebugCreate(Guid iid, out IDebugClient client);

        [DllImport("dbgeng.dll", EntryPoint = "DebugCreateEx", SetLastError = false)]
        public static extern int DebugCreateEx(Guid iid, UInt32 flags, out IDebugClient client);

        /// <summary>
        /// Path of the process to start under debugger.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ProcessPath { get; set; }


        protected override void ProcessRecord()
        {
            ConnectionState state = ConnectionState.GetConnectionState();
            state.IsConnected = true;
            state.ProcessPath = ProcessPath;

            IDebugClient client;
            int hresult = DebugCreateEx(Marshal.GenerateGuidForType(typeof(IDebugClient)), 0x60, out client);

            if (hresult > 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            ((IDebugClient7)client).CreateProcessAndAttach(0, ProcessPath, 0x00000002); 
            ((IDebugControl7)client).WaitForEvent(0, uint.MaxValue);
            ((IDebugSymbols5)client).SetSymbolPathWide(@"srv*;.\");

            Context.Initalize(client);

            WriteDebug("Connection successfully initialized");

        }
    }

    /// <summary>
    /// Gets the callstacks of all the threads in the system.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DebugCallStack")]
    [OutputType(typeof(StackTrace))]
    public class GetCallstack : Cmdlet
    {
        protected override void ProcessRecord()
        {
            Console.WriteLine("Threads: {0}", Thread.All.Length);
            Console.WriteLine("Current thread: {0}", Thread.Current.Id);
            var frames = Thread.Current.GetStackTrace().Frames;

            Console.WriteLine("Call stack:");
            foreach (var frame in frames)
            {
                try
                {
                    Console.WriteLine("  {0,3:x} {1}+0x{2:x}   ({3}:{4})", frame.FrameNumber, frame.FunctionName, frame.FunctionDisplacement, frame.SourceFileName, frame.SourceFileLine);
                }
                catch (Exception)
                {
                    Console.WriteLine("  {0,3:x} {1}+0x{2:x}", frame.FrameNumber, frame.FunctionName, frame.FunctionDisplacement);
                }
            }

            WriteObject(Thread.Current.GetStackTrace());
        }
    }
}
