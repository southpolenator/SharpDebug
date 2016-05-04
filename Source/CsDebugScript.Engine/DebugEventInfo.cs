using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsDebugScript.Engine.Native;

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
        public DEBUG_EVENT Type;

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
        /// Last Event Information.
        /// </summary>
        public DEBUG_LAST_EVENT_INFO LastEventInfo;

        /// <summary>
        /// Get Last occurent Event or Exception.
        /// </summary>
        public static DebugEventInfo LastEvent => Engine.Context.Debugger.GetLastEventInfo();
    }
}
