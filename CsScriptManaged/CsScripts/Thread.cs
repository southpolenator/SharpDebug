using CsScriptManaged;
using CsScriptManaged.Marshaling;
using CsScriptManaged.Utility;
using DbgEngManaged;
using System.Linq;

namespace CsScripts
{
    public class Thread
    {
        /// <summary>
        /// The TEB address
        /// </summary>
        private SimpleCache<ulong> tebAddress;

        /// <summary>
        /// The stack trace
        /// </summary>
        private SimpleCache<StackTrace> stackTrace;

        /// <summary>
        /// The thread context
        /// </summary>
        private SimpleCache<ThreadContext> threadContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Thread"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="systemId">The system identifier.</param>
        public Thread(uint id, uint systemId, Process process)
        {
            Id = id;
            SystemId = systemId;
            Process = process;
            tebAddress = SimpleCache.Create(GetTEB);
            stackTrace = SimpleCache.Create(GetStackTrace);
            threadContext = SimpleCache.Create(GetThreadContext);
        }

        /// <summary>
        /// Gets the current thread in current process.
        /// </summary>
        public static Thread Current
        {
            get
            {
                return Process.Current.CurrentThread;
            }
        }

        /// <summary>
        /// Gets all threads in current process.
        /// </summary>
        public static Thread[] All
        {
            get
            {
                return Process.Current.Threads;
            }
        }

        /// <summary>
        /// Gets the owning process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId { get; private set; }

        /// <summary>
        /// Gets the stack trace.
        /// </summary>
        public StackTrace StackTrace
        {
            get
            {
                return stackTrace.Value;
            }
        }

        /// <summary>
        /// Gets the local variables on current stack frame.
        /// </summary>
        public VariableCollection Locals
        {
            get
            {
                return StackTrace.CurrentFrame.Locals;
            }
        }

        /// <summary>
        /// Gets the TEB (Thread environment block) address.
        /// </summary>
        public ulong TEB
        {
            get
            {
                return tebAddress.Value;
            }
        }

        /// <summary>
        /// Gets the thread context.
        /// </summary>
        public ThreadContext ThreadContext
        {
            get
            {
                return threadContext.Value;
            }
        }

        /// <summary>
        /// Gets the stack trace.
        /// </summary>
        private StackTrace GetStackTrace()
        {
            const int MaxCallStack = 1024;
            using (ThreadSwitcher switcher = new ThreadSwitcher(this))
            using (MarshalArrayReader<_DEBUG_STACK_FRAME_EX> frameBuffer = new RegularMarshalArrayReader<_DEBUG_STACK_FRAME_EX>(MaxCallStack))
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(MaxCallStack))
            {
                uint framesCount;

                Context.Control.GetContextStackTraceEx(System.IntPtr.Zero, 0, frameBuffer.Pointer, (uint)frameBuffer.Count, threadContextBuffer.Pointer, (uint)(threadContextBuffer.Size * threadContextBuffer.Count), (uint)threadContextBuffer.Size, out framesCount);
                return new StackTrace(this, frameBuffer.Elements.Take((int)framesCount).ToArray(), threadContextBuffer.Elements.Take((int)framesCount).ToArray());
            }
        }

        /// <summary>
        /// Gets the thread context.
        /// </summary>
        private ThreadContext GetThreadContext()
        {
            using (ThreadSwitcher switcher = new ThreadSwitcher(this))
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(1))
            {
                Context.Advanced.GetThreadContext(threadContextBuffer.Pointer, (uint)(threadContextBuffer.Count * threadContextBuffer.Size));

                return threadContextBuffer.Elements.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the TEB address.
        /// </summary>
        private ulong GetTEB()
        {
            using (ThreadSwitcher switcher = new ThreadSwitcher(this))
            {
                return Context.SystemObjects.GetCurrentThreadTeb();
            }
        }
    }
}
