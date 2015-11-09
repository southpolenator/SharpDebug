using CsScriptManaged;
using System;
using System.Linq;

namespace CsScripts
{
    public class Process
    {
        /// <summary>
        /// Gets the current process.
        /// </summary>
        public static Process Current
        {
            get
            {
                uint currentId = Context.SystemObjects.GetCurrentProcessId();

                return All.FirstOrDefault(p => p.Id == currentId);
            }
        }

        /// <summary>
        /// Gets all processes.
        /// </summary>
        public static Process[] All
        {
            get
            {
                uint processCount = Context.SystemObjects.GetNumberProcesses();
                Process[] processes = new Process[processCount];

                for (uint i = 0; i < processCount; i++)
                {
                    uint id, sysid;

                    Context.SystemObjects.GetProcessIdsByIndex(i, 1, out id, out sysid);
                    processes[i] = new Process()
                    {
                        Id = id,
                        SystemId = sysid,
                    };
                }

                return processes;
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId { get; private set; }

        /// <summary>
        /// Gets the current thread.
        /// </summary>
        public Thread CurrentThread
        {
            get
            {
                using (ProcessSwitcher process = new ProcessSwitcher(Id))
                {
                    uint currentId = Context.SystemObjects.GetCurrentThreadId();

                    return Threads.FirstOrDefault(t => t.Id == currentId);
                }
            }
        }

        /// <summary>
        /// Gets the threads.
        /// </summary>
        public Thread[] Threads
        {
            get
            {
                using (ProcessSwitcher process = new ProcessSwitcher(Id))
                {
                    uint threadCount = Context.SystemObjects.GetNumberThreads();
                    Thread[] threads = new Thread[threadCount];

                    for (uint i = 0; i < threadCount; i++)
                    {
                        uint id, sysid;

                        Context.SystemObjects.GetThreadIdsByIndex(i, 1, out id, out sysid);
                        threads[i] = new Thread()
                        {
                            Id = id,
                            SystemId = sysid,
                        };
                    }

                    return threads;
                }
            }
        }
    }
}
