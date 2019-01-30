using CsDebugScript.CLR;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of the <see cref="IClrThread"/>.
    /// </summary>
    internal class VSClrThread : IClrThread
    {
        /// <summary>
        /// The cache of CLR stack frames
        /// </summary>
        private SimpleCache<VSClrStackFrame[]> clrStackFramesCache;

        /// <summary>
        /// The cache of CLR stack trace
        /// </summary>
        private SimpleCache<StackTrace> stackTraceCache;

        /// <summary>
        /// The cache of CLR AppDomain
        /// </summary>
        private SimpleCache<IClrAppDomain> appDomainCache;

        /// <summary>
        /// The cache of last thrown exception
        /// </summary>
        private SimpleCache<Variable> lastThrownExceptionCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdThread"/> class.
        /// </summary>
        /// <param name="runtime">The Visual Studio runtime.</param>
        /// <param name="threadId">The thread system id.</param>
        /// <param name="isFinalizerThread">Is this finalizer thread.</param>
        /// <param name="appDomainAddress">The application domain address.</param>
        public VSClrThread(VSClrRuntime runtime, uint threadId, bool isFinalizerThread, ulong appDomainAddress)
        {
            VSRuntime = runtime;
            SystemId = threadId;
            IsFinalizerThread = isFinalizerThread;
            clrStackFramesCache = SimpleCache.Create(() =>
            {
                Tuple<int, ulong, ulong, ulong>[] frames = Proxy.GetClrThreadFrames(Runtime.Process.Id, runtime.Id, SystemId);
                VSClrStackFrame[] clrFrames = new VSClrStackFrame[frames.Length];

                for (int i = 0; i < frames.Length; i++)
                    clrFrames[i] = new VSClrStackFrame(this, frames[i].Item1, frames[i].Item2, frames[i].Item3, frames[i].Item4);
                return clrFrames;
            });
            stackTraceCache = SimpleCache.Create(() =>
            {
                Thread thread = VSRuntime.Process.Threads.First(t => t.SystemId == SystemId);
                StackTrace stackTrace = new StackTrace(thread);
                uint frameNumber = 0;

                stackTrace.Frames = clrStackFramesCache.Value.Select(f =>
                {
                    return new StackFrame(stackTrace, new ThreadContext(f.InstructionPointer, f.StackPointer, ulong.MaxValue, null))
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
            appDomainCache = SimpleCache.Create(() => Runtime.AppDomains.Single(a => a.Address == appDomainAddress));
            lastThrownExceptionCache = SimpleCache.Create(() =>
            {
                Tuple<ulong, int> tuple = Proxy.GetClrThreadLastException(Runtime.Process.Id, runtime.Id, SystemId);
                ulong address = tuple.Item1;
                IClrType clrType = runtime.GetClrType(tuple.Item2);

                if (clrType == null)
                    return null;
                return Variable.CreatePointer(runtime.Process.FromClrType(clrType), address);
            });
        }

        /// <summary>
        /// Gets the Visual Studio implementation of the runtime.
        /// </summary>
        public VSClrRuntime VSRuntime { get; private set; }

        /// <summary>
        /// Gets the Visual Studio debugger proxy.
        /// </summary>
        public VSDebuggerProxy Proxy => VSRuntime.Proxy;

        /// <summary>
        /// Gets the runtime associated with this thread.
        /// </summary>
        public IClrRuntime Runtime => VSRuntime;

        /// <summary>
        /// Gets the AppDomain the thread is running in.
        /// </summary>
        public IClrAppDomain AppDomain => appDomainCache.Value;

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
        public StackTrace ClrStackTrace => stackTraceCache.Value;

        /// <summary>
        /// Gets the last thrown exception on the thread. Note that this field may be null.
        /// It may be stale...meaning the thread could be done processing the exception but
        /// a crash dump was taken before the exception was cleared off the field.
        /// </summary>
        public Variable LastThrownException => lastThrownExceptionCache.Value;

        /// <summary>
        /// Gets a value indicating whether this is finalizer thread.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is finalizer thread; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinalizerThread { get; private set; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId { get; private set; }

        /// <summary>
        /// Enumerates the GC references (objects) on the stack.
        /// </summary>
        public IEnumerable<Variable> EnumerateStackObjects()
        {
            Tuple<int, Tuple<ulong, int>[]> firstBatch = Proxy.EnumerateClrThreadStackObjects(VSRuntime.Process.Id, VSRuntime.Id, SystemId, VSClrHeap.EnumerationBatchSize);

            return VSClrHeap.EnumerateVariables(VSRuntime, firstBatch);
        }

        /// <summary>
        /// Gets the CLR stack frame with the specified instruction offset.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        public IClrStackFrame GetClrStackFrame(ulong instructionOffset)
        {
            // TODO: Instruction offset is not enough in recursive calls...
            return clrStackFramesCache.Value.FirstOrDefault(f => f.InstructionPointer == instructionOffset);
        }
    }
}
