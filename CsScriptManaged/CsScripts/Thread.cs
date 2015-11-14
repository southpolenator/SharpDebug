using CsScriptManaged;
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
        public Variable[] Locals
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
        /// Gets the stack trace.
        /// </summary>
        /// <returns></returns>
        private StackTrace GetStackTrace()
        {
            using (ThreadSwitcher switcher = new ThreadSwitcher(this))
            {
                MarshalArrayReader<_DEBUG_STACK_FRAME> buffer = new MarshalArrayReader<_DEBUG_STACK_FRAME>(1024);
                uint framesCount;

                Context.Control.GetStackTrace(0, 0, 0, buffer.Pointer, (uint)buffer.Count, out framesCount);
                return new StackTrace(this, buffer.Elements.Take((int)framesCount).ToArray());
            }
        }

        /// <summary>
        /// Gets the TEB address.
        /// </summary>
        /// <returns></returns>
        private ulong GetTEB()
        {
            using (ThreadSwitcher switcher = new ThreadSwitcher(this))
            {
                return Context.SystemObjects.GetCurrentThreadTeb();
            }
        }
    }
}
