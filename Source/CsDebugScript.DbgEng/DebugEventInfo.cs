using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;

namespace CsDebugScript
{
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
