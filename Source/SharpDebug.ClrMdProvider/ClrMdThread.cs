using SharpDebug.CLR;
using SharpUtilities;
using System.Collections.Generic;
using System.Linq;

namespace SharpDebug.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of the <see cref="IClrThread"/>.
    /// </summary>
    internal class ClrMdThread : IClrThread
    {
        /// <summary>
        /// The cache of CLR runtime
        /// </summary>
        SimpleCache<IClrRuntime> runtime;

        /// <summary>
        /// The cache of CLR AppDomain
        /// </summary>
        SimpleCache<IClrAppDomain> appDomain;

        /// <summary>
        /// The cache of CLR stack trace
        /// </summary>
        SimpleCache<StackTrace> clrStackTrace;

        /// <summary>
        /// The cache of last thrown exception
        /// </summary>
        SimpleCache<Variable> lastThrownException;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdThread"/> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="clrThread">The CLR thread.</param>
        /// <param name="process">The process.</param>
        internal ClrMdThread(Thread thread, Microsoft.Diagnostics.Runtime.ClrThread clrThread, Process process)
        {
            Thread = thread;
            Process = process;
            ClrThread = clrThread;
            runtime = SimpleCache.Create(() => Process.ClrRuntimes.Single(r => ((ClrMdRuntime)r).ClrRuntime == clrThread.Runtime));
            appDomain = SimpleCache.Create(() => Runtime.AppDomains.Single(a => a.Address == clrThread.AppDomain));
            clrStackTrace = SimpleCache.Create(() =>
            {
                CLR.ClrMdProvider provider = ((ClrMdRuntime)Runtime).Provider;
                StackTrace stackTrace = new StackTrace(Thread);
                uint frameNumber = 0;

                stackTrace.Frames = ClrThread.StackTrace.Where(f => f.Method != null).Select(f =>
                {
                    return new StackFrame(stackTrace, new ThreadContext(f.InstructionPointer, f.StackPointer, ulong.MaxValue, null))
                    {
                        FrameNumber = frameNumber++,
                        InstructionOffset = f.InstructionPointer,
                        StackOffset = f.StackPointer,
                        FrameOffset = ulong.MaxValue,
                        ReturnOffset = ulong.MaxValue,
                        ClrStackFrame = new ClrMdStackFrame(provider, f),
                    };
                }).ToArray();
                return stackTrace;
            });
            lastThrownException = SimpleCache.Create(() =>
            {
                if (ClrThread.CurrentException != null)
                {
                    CLR.ClrMdProvider provider = ((ClrMdRuntime)Runtime).Provider;
                    Microsoft.Diagnostics.Runtime.ClrException clrException = ClrThread.CurrentException;

                    return Variable.CreatePointer(Process.FromClrType(provider.FromClrType(clrException.Type)), clrException.Address);
                }
                return null;
            });
        }

        /// <summary>
        /// Gets the native thread.
        /// </summary>
        public Thread Thread { get; private set; }

        /// <summary>
        /// Gets the native process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the CLR thread.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrThread ClrThread { get; private set; }

        /// <summary>
        /// Gets the runtime associated with this thread.
        /// </summary>
        public IClrRuntime Runtime
        {
            get
            {
                return runtime.Value;
            }
        }

        /// <summary>
        /// Gets the AppDomain the thread is running in.
        /// </summary>
        public IClrAppDomain AppDomain
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

        /// <summary>
        /// Gets the last thrown exception on the thread. Note that this field may be null.
        /// It may be stale...meaning the thread could be done processing the exception but
        /// a crash dump was taken before the exception was cleared off the field.
        /// </summary>
        public Variable LastThrownException
        {
            get
            {
                return lastThrownException.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is finalizer thread.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is finalizer thread; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinalizerThread
        {
            get
            {
                return ClrThread.IsFinalizer;
            }
        }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId
        {
            get
            {
                return ClrThread.OSThreadId;
            }
        }

        /// <summary>
        /// Enumerates the GC references (objects) on the stack.
        /// </summary>
        public IEnumerable<Variable> EnumerateStackObjects()
        {
            CLR.ClrMdProvider provider = ((ClrMdRuntime)Runtime).Provider;

            foreach (Microsoft.Diagnostics.Runtime.ClrRoot root in ClrThread.EnumerateStackObjects())
            {
                if (root.Type.IsFree || root.Type.Module == null)
                {
                    continue;
                }

                Variable field = Variable.CreateNoCast(Process.FromClrType(provider.FromClrType(root.Type)), root.Address);

                yield return Variable.UpcastClrVariable(field);
            }
        }

        /// <summary>
        /// Gets the CLR stack frame with the specified instruction offset.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        public IClrStackFrame GetClrStackFrame(ulong instructionOffset)
        {
            var clrStackFrame = ClrThread.StackTrace?.FirstOrDefault(f => f.InstructionPointer == instructionOffset);

            if (clrStackFrame != null && clrStackFrame.Method != null)
            {
                CLR.ClrMdProvider provider = ((ClrMdRuntime)Runtime).Provider;

                return new ClrMdStackFrame(provider, clrStackFrame);
            }
            return null;
        }
    }
}
