using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsScripts;
using CsDebugScript;
using CsDebugScript.Native;

namespace CsScripts
{
    public class DebugEventInfo
    {
        public DEBUG_EVENT Type;

        public Process Process;

        public Thread Thread;

        public string Description;

        public DEBUG_LAST_EVENT_INFO LastEventInfo;

        /// <summary>
        /// Get Last occurent Event or Exception.
        /// </summary>
        public static DebugEventInfo LastEvent => EngineContext.Debugger.GetLastEventInfo();
    }
}
