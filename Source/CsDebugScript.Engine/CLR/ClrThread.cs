using CsDebugScript.Engine.Utility;
using System.Linq;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code Thread. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public class ClrThread : Thread
    {
        /// <summary>
        /// The cache of CLR runtime
        /// </summary>
        SimpleCache<Runtime> runtime;

        /// <summary>
        /// The cache of CLR AppDomain
        /// </summary>
        SimpleCache<AppDomain> appDomain;

        /// <summary>
        /// The cache of CLR stack trace
        /// </summary>
        SimpleCache<StackTrace> clrStackTrace;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrThread"/> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="clrThread">The CLR thread.</param>
        /// <param name="process">The process.</param>
        internal ClrThread(Thread thread, Microsoft.Diagnostics.Runtime.ClrThread clrThread, Process process)
            : base(thread != null ? thread.Id : uint.MaxValue, clrThread.OSThreadId, process)
        {
            ClrThread = clrThread;
            runtime = SimpleCache.Create(() => Process.ClrRuntimes.Single(r => r.ClrRuntime == clrThread.Runtime));
            appDomain = SimpleCache.Create(() => Runtime.AppDomains.Single(a => a.ClrAppDomain.Address == clrThread.AppDomain));
            clrStackTrace = SimpleCache.Create(() =>
            {
                StackTrace stackTrace = new StackTrace(this);
                uint frameNumber = 0;

                stackTrace.Frames = ClrThread.StackTrace.Select(f =>
                {
                    return new StackFrame(stackTrace, new ThreadContext(f.InstructionPointer, f.StackPointer, ulong.MaxValue))
                    {
                        FrameNumber = frameNumber++,
                        InstructionOffset = f.InstructionPointer,
                        StackOffset = f.StackPointer,
                        FrameOffset = ulong.MaxValue,
                        ReturnOffset = ulong.MaxValue,
                        ClrStackFrame = f,
                    };
                }).ToArray();
                return stackTrace;
            });
        }

        /// <summary>
        /// Gets the runtime associated with this thread.
        /// </summary>
        public Runtime Runtime
        {
            get
            {
                return runtime.Value;
            }
        }

        /// <summary>
        /// Gets the AppDomain the thread is running in.
        /// </summary>
        public AppDomain AppDomain
        {
            get
            {
                return appDomain.Value;
            }
        }

        /// <summary>
        /// Gets the managed stack trace of the thread.
        /// </summary>
        /// <remarks>
        /// <note type="note">This property may return incomplete data in the case of a bad stack unwind or if there is a very large
        /// number of methods on the stack. (This is usually caused by a stack overflow on
        /// the target thread, stack corruption which leads to a bad stack unwind, or other
        /// inconsistent state in the target debuggee.)</note>
        /// <note type="note">
        /// This property uses a heuristic to attempt to detect bad unwinds to stop enumerating frames by inspecting the
        /// stack pointer and instruction pointer of each frame to ensure the stack walk
        /// is "making progress". Additionally we cap the number of frames returned by this
        /// method as another safegaurd. This means we may not have all frames even if the
        /// stack walk was making progress. If you want to ensure that you receive an un-clipped
        /// stack trace, you should use EnumerateStackTrace instead of this property, and
        /// be sure to handle the case of repeating stack frames.
        /// </note>
        /// </remarks>
        public StackTrace ClrStackTrace
        {
            get
            {
                return clrStackTrace.Value;
            }
        }
    }
}
