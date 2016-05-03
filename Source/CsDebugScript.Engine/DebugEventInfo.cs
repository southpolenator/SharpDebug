using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsDebugScript.Engine.Native;

namespace CsDebugScript
{
    public class DebugEventInfo
    {
        /// <summary>
        /// Event Type.
        /// </summary>
        public DEBUG_EVENT Type;

        /// <summary>
        /// Current Process.
        /// </summary>
        public Process Process;

        /// <summary>
        /// Thread where event occurred.
        /// </summary>
        public Thread Thread;

        /// <summary>
        /// Event description.
        /// </summary>
        public string Description;

        public DEBUG_LAST_EVENT_INFO LastEventInfo;

        /// <summary>
        /// Get Last occurent Event or Exception.
        /// </summary>
        public static DebugEventInfo LastEvent => Engine.Context.Debugger.GetLastEventInfo();
    }
}
