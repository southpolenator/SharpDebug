using CsScriptManaged;
using DbgEngManaged;
using System.Linq;

namespace CsScripts
{
    public class Thread
    {
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
        public Process Process { get; internal set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; internal set; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId { get; internal set; }

        /// <summary>
        /// Gets the stack trace.
        /// </summary>
        public StackTrace StackTrace
        {
            get
            {
                using (ThreadSwitcher switcher = new ThreadSwitcher(this))
                {
                    MarshalArrayReader<_DEBUG_STACK_FRAME> buffer = new MarshalArrayReader<_DEBUG_STACK_FRAME>(1024);
                    uint framesCount;

                    Context.Control.GetStackTrace(0, 0, 0, buffer.Pointer, (uint)buffer.Count, out framesCount);
                    return new StackTrace(this, buffer.Elements.Take((int)framesCount).ToArray());
                }
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
                using (ThreadSwitcher switcher = new ThreadSwitcher(this))
                {
                    return Context.SystemObjects.GetCurrentThreadTeb();
                }
            }
        }
    }
}
