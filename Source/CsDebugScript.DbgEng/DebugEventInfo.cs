using System;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;

namespace CsDebugScript
{
    /// <summary>
    /// Indicates the kind of debug event.
    /// </summary>
    [Flags]
    public enum DebugEvent
    {
        /// <summary>
        /// A breakpoint exception occurred in the target.
        /// </summary>
        Breakpoint = 0x00000001,

        /// <summary>
        /// An exception debugging event occurred in the target.
        /// </summary>
        Exception = 0x00000002,

        /// <summary>
        /// A create-thread debugging event occurred in the target.
        /// </summary>
        CreateThread = 0x00000004,

        /// <summary>
        /// An exit-thread debugging event occurred in the target.
        /// </summary>
        ExitThread = 0x00000008,

        /// <summary>
        /// A create-process debugging event occurred in the target.
        /// </summary>
        CreateProcess = 0x00000010,

        /// <summary>
        /// An exit-process debugging event occurred in the target.
        /// </summary>
        ExitProcess = 0x00000020,

        /// <summary>
        /// A module-load debugging event occurred in the target.
        /// </summary>
        LoadModule = 0x00000040,

        /// <summary>
        /// A module-unload debugging event occurred in the target.
        /// </summary>
        UnloadModule = 0x00000080,

        /// <summary>
        /// A system error occurred in the target.
        /// </summary>
        SystemError = 0x00000100,

        /// <summary>
        /// A change has occurred in the session status.
        /// </summary>
        SessionStatus = 0x00000200,

        /// <summary>
        /// The engine has made or detected a change in the target status.
        /// </summary>
        ChangeDebugeeState = 0x00000400,

        /// <summary>
        /// The engine state has changed.
        /// </summary>
        ChangeEngineState = 0x00000800,

        /// <summary>
        /// The symbol state has changed.
        /// </summary>
        ChangeSymbolState = 0x00001000,
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
        /// Process where Event occurred.
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
        /// Serialized Event Details.
        /// </summary>
        public byte[] EventExtraInfo;

        /// <summary>
        /// Get Last occurred Event or Exception.
        /// </summary>
        public static DebugEventInfo LastEvent => ((DbgEngDll)Context.Debugger).GetLastEventInfo();
    }
}
