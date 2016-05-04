using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsDebugScript.Engine.Native;

namespace CsDebugScript
{
    /// <summary>
    /// Indicates the kind of debug event.
    /// </summary>
    public enum DebugEvent
    {
        /// <summary>
        /// A breakpoint exception occurred in the target.
        /// </summary>
        Breakpoint,

        /// <summary>
        /// An exception debugging event occurred in the target.
        /// </summary>
        Exception,

        /// <summary>
        /// A create-thread debugging event occurred in the target.
        /// </summary>
        CreateThread,

        /// <summary>
        /// An exit-thread debugging event occurred in the target.
        /// </summary>
        ExitThread,

        /// <summary>
        /// A create-process debugging event occurred in the target.
        /// </summary>
        CreateProcess,

        /// <summary>
        /// An exit-process debugging event occurred in the target.
        /// </summary>
        ExitProcess,

        /// <summary>
        /// A module-load debugging event occurred in the target.
        /// </summary>
        LoadModule,

        /// <summary>
        /// A module-unload debugging event occurred in the target.
        /// </summary>
        UnloadModule,

        /// <summary>
        /// A system error occurred in the target.
        /// </summary>
        SystemError,
    }

    /// <summary>
    /// Class Describing Debugger Event.
    /// </summary>
    public class DebugEventInfo
    {
        /// <summary>
        /// Event Type.
        /// </summary>
        public DebugEvent Type;

        /// <summary>
        /// Process where Event occured.
        /// </summary>
        public Process Process;

        /// <summary>
        /// Thread where Event occurred.
        /// </summary>
        public Thread Thread;

        /// <summary>
        /// Event description.
        /// </summary>
        public string Description;

        /// <summary>
        /// Get Last occurent Event or Exception.
        /// </summary>
        public static DebugEventInfo LastEvent => Engine.Context.Debugger.GetLastEventInfo();
    }
}
