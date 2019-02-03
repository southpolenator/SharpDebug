using CsDebugScript.CLR;
using CsDebugScript.Engine;
using SharpUtilities;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript
{
    /// <summary>
    /// Thread of the process being debugged.
    /// </summary>
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
        /// The CLR thread
        /// </summary>
        private SimpleCache<IClrThread> clrThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="Thread" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="systemId">The system identifier.</param>
        /// <param name="process">The process.</param>
        internal Thread(uint id, uint systemId, Process process)
        {
            Id = id;
            SystemId = systemId;
            Process = process;
            tebAddress = SimpleCache.Create(GetTEB);
            stackTrace = SimpleCache.Create(GetStackTrace);
            threadContext = SimpleCache.Create(GetThreadContext);
            clrThread = SimpleCache.Create(() => Process.ClrRuntimes.SelectMany(r => r.Threads).Where(t => t.SystemId == SystemId).FirstOrDefault());
        }

        /// <summary>
        /// Gets or sets the current thread in current process.
        /// </summary>
        public static Thread Current
        {
            get
            {
                return Process.Current.CurrentThread;
            }

            set
            {
                Context.Debugger.SetCurrentThread(value);
            }
        }

        /// <summary>
        /// Gets the array of all threads in current process.
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
        /// Gets the stack trace frames.
        /// </summary>
        public StackFrame[] Frames
        {
            get
            {
                return StackTrace.Frames;
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
        public ulong TebAddress
        {
            get
            {
                return tebAddress.Value;
            }
        }

        /// <summary>
        /// Gets the variable that represents TEB (Thread environment block).
        /// </summary>
        public Variable TEB
        {
            get
            {
                try
                {
                    List<string> searchModulesOrder = new List<string> { Process.Modules[0].Name.ToLower(), "wow64", "ntdll", "nt" };
                    IEnumerable<Module> modules = Process.Modules.OrderByDescending(m => searchModulesOrder.IndexOf(m.Name.ToLower()));

                    foreach (Module module in modules)
                    {
                        try
                        {
                            CodeType tebCodeType = CodeType.Create("_TEB", module);

                            return Variable.Create(tebCodeType, TebAddress, "TEB", "Thread.TEB");
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }

                return new NakedPointer(Process, TebAddress);
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
        /// Gets the CLR thread object correlated with this native thread.
        /// If this thread is not used by CLR, it will return null.
        /// </summary>
        public IClrThread ClrThread
        {
            get
            {
                return clrThread.Value;
            }

            internal set
            {
                clrThread.Value = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({Id}:{SystemId})";
        }

        /// <summary>
        /// Gets the stack trace.
        /// </summary>
        private StackTrace GetStackTrace()
        {
            return Context.Debugger.GetThreadStackTrace(this);
        }

        /// <summary>
        /// Get StackTrace from Thread Context.
        /// </summary>
        /// <param name="contextAddress">Thread Context Address.</param>
        /// <param name="contextSize">Thread Context Size.</param>
        /// <returns></returns>
        public static StackTrace GetStackTraceFromContext(ulong contextAddress, uint contextSize)
        {
            return Context.Debugger.GetStackTraceFromContext(Process.Current, contextAddress, contextSize);
        }


        /// <summary>
        /// Gets the thread context.
        /// </summary>
        private ThreadContext GetThreadContext()
        {
            return Context.Debugger.GetThreadContext(this);
        }

        /// <summary>
        /// Gets the TEB address.
        /// </summary>
        private ulong GetTEB()
        {
            return Context.Debugger.GetThreadEnvironmentBlockAddress(this);
        }
    }
}
