using CsScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScriptManaged
{
    public class StackFrameSwitcher
    {
        /// <summary>
        /// The thread switcher
        /// </summary>
        private ThreadSwitcher threadSwitcher;

        /// <summary>
        /// The old stack frame identifier
        /// </summary>
        private uint oldStackFrameId;

        /// <summary>
        /// The new stack frame identifier
        /// </summary>
        private uint newStackFrameId;

        public StackFrameSwitcher(StackFrame stackFrame)
        {
            threadSwitcher = new ThreadSwitcher(stackFrame.Thread);
            oldStackFrameId = Context.Symbols.GetCurrentScopeFrameIndex();
            newStackFrameId = stackFrame.FrameNumber;

            SetThreadId(newStackFrameId);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            SetThreadId(oldStackFrameId);
            threadSwitcher.Dispose();
        }

        private void SetThreadId(uint newStackFrameId)
        {
            Context.Symbols.SetScopeFrameByIndex(newStackFrameId);
        }
    }
}
