using CsScriptManaged;
using System;
using System.Linq;

namespace CsScripts
{
    public class Process
    {
        /// <summary>
        /// The executable name
        /// </summary>
        private SimpleCache<string> executableName;

        /// <summary>
        /// Up time
        /// </summary>
        private SimpleCache<uint> upTime;

        /// <summary>
        /// The PEB address
        /// </summary>
        private SimpleCache<ulong> pebAddress;

        /// <summary>
        /// The threads
        /// </summary>
        private SimpleCache<Thread[]> threads;

        /// <summary>
        /// The modules
        /// </summary>
        private SimpleCache<Module[]> modules;

        /// <summary>
        /// Initializes a new instance of the <see cref="Process"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="systemId">The system identifier.</param>
        public Process(uint id, uint systemId)
        {
            Id = id;
            SystemId = systemId;
            upTime = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessUpTime()));
            pebAddress = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessPeb()));
            executableName = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessExecutableName()));
            threads = SimpleCache.Create(GetThreads);
            modules = SimpleCache.Create(GetModules);
            ModulesByName = new GlobalCache<string, Module>(GetModuleByName);
            ModulesById = new GlobalCache<ulong, Module>(GetModuleById);
        }

        /// <summary>
        /// Gets the current process.
        /// </summary>
        public static Process Current
        {
            get
            {
                uint currentId = Context.SystemObjects.GetCurrentProcessId();
                uint currentSystemId = Context.SystemObjects.GetCurrentProcessSystemId();

                return GlobalCache.Processes[Tuple.Create(currentId, currentSystemId)];
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
                uint[] processIds = new uint[processCount];
                uint[] processSytemIds = new uint[processCount];

                unsafe
                {
                    fixed (uint* ids = &processIds[0])
                    fixed (uint* systemIds = &processSytemIds[0])
                    {
                        Context.SystemObjects.GetProcessIdsByIndex(0, processCount, out *ids, out *systemIds);
                    }
                }

                for (uint i = 0; i < processCount; i++)
                {
                    processes[i] = GlobalCache.Processes[Tuple.Create(processIds[i], processSytemIds[i])];
                }

                return processes;
            }
        }

        /// <summary>
        /// The modules by name
        /// </summary>
        internal GlobalCache<string, Module> ModulesByName { get; private set; }

        /// <summary>
        /// The modules by identifier
        /// </summary>
        internal GlobalCache<ulong, Module> ModulesById { get; private set; }

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
                return executableName.Value;
            }
        }

        /// <summary>
        /// Gets up time.
        /// </summary>
        public uint UpTime
        {
            get
            {
                return upTime.Value;
            }
        }

        /// <summary>
        /// Gets the PEB (Process environment block) address.
        /// </summary>
        public ulong PEB
        {
            get
            {
                return pebAddress.Value;
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
                return threads.Value;
            }
        }

        /// <summary>
        /// Gets the modules.
        /// </summary>
        public Module[] Modules
        {
            get
            {
                return modules.Value;
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
            foreach (Module module in Modules)
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

        /// <summary>
        /// Updates the cache of modules specified by the name.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="name">The name.</param>
        internal void UpdateModuleByNameCache(Module module, string name)
        {
            ModulesByName[name] = module;
        }

        /// <summary>
        /// Gets the threads.
        /// </summary>
        private Thread[] GetThreads()
        {
            using (ProcessSwitcher process = new ProcessSwitcher(this))
            {
                uint threadCount = Context.SystemObjects.GetNumberThreads();
                Thread[] threads = new Thread[threadCount];
                uint[] threadIds = new uint[threadCount];
                uint[] threadSytemIds = new uint[threadCount];

                unsafe
                {
                    fixed (uint* ids = &threadIds[0])
                    fixed (uint* systemIds = &threadSytemIds[0])
                    {
                        Context.SystemObjects.GetThreadIdsByIndex(0, threadCount, out *ids, out *systemIds);
                    }
                }

                for (uint i = 0; i < threadCount; i++)
                {
                    threads[i] = new Thread(threadIds[i], threadSytemIds[i], this);
                }

                return threads;
            }
        }

        /// <summary>
        /// Gets the modules.
        /// </summary>
        private Module[] GetModules()
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(this))
            {
                uint loaded, unloaded;

                Context.Symbols.GetNumberModules(out loaded, out unloaded);
                Module[] modules = new Module[loaded + unloaded];

                for (int i = 0; i < modules.Length; i++)
                {
                    ulong moduleId = Context.Symbols.GetModuleByIndex((uint)i);

                    modules[i] = ModulesById[moduleId];
                }

                return modules;
            }
        }

        /// <summary>
        /// Gets the module with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        private Module GetModuleByName(string name)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(this))
            {
                uint index;
                ulong moduleId;

                Context.Symbols.GetModuleByModuleName2Wide(name, 0, 0, out index, out moduleId);
                Module module = ModulesById[moduleId];

                module.Name = name;
                return module;
            }
        }

        /// <summary>
        /// Gets the module with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private Module GetModuleById(ulong id)
        {
            return new Module(this, id);
        }
    }
}
