using CsScriptManaged;
using System.Linq;
using DbgEngManaged;
using CsScriptManaged.Utility;

namespace CsScripts
{
    public class StackTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackTrace" /> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="frames">The frames.</param>
        /// <param name="frameContexts">The frame contexts.</param>
        internal StackTrace(Thread thread, _DEBUG_STACK_FRAME_EX[] frames, ThreadContext[] frameContexts)
        {
            Thread = thread;
            Frames = new StackFrame[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                Frames[i] = new StackFrame(this, frames[i], frameContexts[i]);
            }
        }

        /// <summary>
        /// Gets the owning thread.
        /// </summary>
        public Thread Thread { get; internal set; }

        /// <summary>
        /// Gets the frames.
        /// </summary>
        public StackFrame[] Frames { get; internal set; }

        /// <summary>
        /// Gets the current stack frame.
        /// </summary>
        public StackFrame CurrentFrame
        {
            get
            {
                using (ThreadSwitcher switcher = new ThreadSwitcher(Thread))
                {
                    uint current = Context.Symbols.GetCurrentScopeFrameIndex();

                    return Frames.FirstOrDefault(f => f.FrameNumber == current);
                }
            }
        }
    }
}
