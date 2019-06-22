﻿using CsDebugScript;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using CsDebugScript.Engine.SymbolProviders;
using DbgEng;
using System;
using System.Management.Automation;

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
    public class StartDbgSession : Cmdlet
    {
        /// <summary>
        /// Path of the process to start under debugger.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string ProcessPath { get; set; }

        /// <summary>
        /// Symbol path.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string SymbolPath { get; set; }


        /// <summary>
        /// Command main function.
        /// </summary>
        protected override void ProcessRecord()
        {
            ConnectionState state = ConnectionState.GetConnectionState();
            state.IsConnected = true;
            state.ProcessPath = ProcessPath;

            if (SymbolPath == null)
            {
                SymbolPath = "srv*";
            }

            IDebugClient client = DebugClient.OpenProcess(ProcessPath, null, SymbolPath, (uint)(Defines.DebugEngoptInitialBreak | Defines.DebugEngoptFinalBreak));
            WriteDebug("Connection successfully initialized");

            DbgEngDll.InitializeContext(client);
        }
    }

    /// <summary>
    /// Gets the callstacks of all the threads in the system.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DebugCallStack")]
    [OutputType(typeof(StackTrace))]
    public class GetCallstack : Cmdlet
    {
        /// <summary>
        /// Command main function.
        /// </summary>
        protected override void ProcessRecord()
        {
            Console.WriteLine("Threads: {0}", Thread.All.Length);
            Console.WriteLine("Current thread: {0}", Thread.Current.Id);
            StackFrame[] frames = Thread.Current.StackTrace.Frames;

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

            WriteObject(Thread.Current.StackTrace);
        }
    }
}
