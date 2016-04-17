using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsScriptManaged;
using CsScripts;
using CsScriptManaged.Native;

namespace CsScripts
{
    public class DebugEventInfo
    {
        public uint Type;

        public Process Process;

        public Thread Thread;

        public string Description;

        public DEBUG_LAST_EVENT_INFO LastEventInfo;

        /// <summary>
        /// Get Last occurent Event or Exception.
        /// </summary>
        public static DebugEventInfo LastEvent => Context.Debugger.GetLastEventInfo();
    }
}
