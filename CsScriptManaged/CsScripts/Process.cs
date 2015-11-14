using CsScriptManaged;
using System;
using System.Linq;
using System.Text;

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
        /// Gets the name of the executable.
        /// </summary>
        public string ExecutableName
        {
            get
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(this))
                {
                    uint exeSize;
                    StringBuilder sb = new StringBuilder(Constants.MaxFileName);

                    Context.SystemObjects.GetCurrentProcessExecutableNameWide(sb, (uint)sb.Capacity, out exeSize);
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets up time.
        /// </summary>
        public uint UpTime
        {
            get
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(this))
                {
                    return Context.SystemObjects.GetCurrentProcessUpTime();
                }
            }
        }

        /// <summary>
        /// Gets the PEB (Process environment block) address.
        /// </summary>
        public ulong PEB
        {
            get
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(this))
                {
                    return Context.SystemObjects.GetCurrentProcessPeb();
                }
            }
        }

        /// <summary>
        /// Gets the current thread.
        /// </summary>
        public Thread CurrentThread
        {
            get
            {
                using (ProcessSwitcher process = new ProcessSwitcher(this))
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
                using (ProcessSwitcher process = new ProcessSwitcher(this))
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
                            Process = this,
                        };
                    }

                    return threads;
                }
            }
        }

        /// <summary>
        /// Gets the modules.
        /// </summary>
        public Module[] Modules
        {
            get
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(this))
                {
                    uint loaded, unloaded;

                    Context.Symbols.GetNumberModules(out loaded, out unloaded);
                    Module[] modules = new Module[loaded + unloaded];

                    for (int i = 0; i < modules.Length; i++)
                    {
                        ulong moduleId = Context.Symbols.GetModuleByIndex((uint)i);

                        modules[i] = new Module(this, moduleId);
                    }

                    return modules;
                }
            }
        }

        /// <summary>
        /// Gets the global variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Global variable if found</returns>
        /// <exception cref="System.ArgumentException">Global variable wasn't found, name:</exception>
        public Variable GetGlobal(string name)
        {
            // Try global name
            try
            {
                return Variable.FromName(name);
            }
            catch (Exception)
            {
                if (name.Contains('!'))
                    throw;
            }

            // Try all modules since module name wasn't specified
            foreach (Module module in Module.All)
            {
                try
                {
                    return module.GetVariable(name);
                }
                catch (Exception)
                {
                }
            }

            throw new ArgumentException("Global variable wasn't found, name: " + name);
        }
    }
}
