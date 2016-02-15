using CsScriptManaged;
using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsScripts
{
    /// <summary>
    /// The process being debugged.
    /// </summary>
    public class Process
    {
        /// <summary>
        /// The executable name
        /// </summary>
        private SimpleCache<string> executableName;

        /// <summary>
        /// The dump file name
        /// </summary>
        private SimpleCache<string> dumpFileName;

        /// <summary>
        /// The system identifier
        /// </summary>
        private SimpleCache<uint> systemId;

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
        /// The user types
        /// </summary>
        private SimpleCache<List<UserTypeDescription>> userTypes;

        /// <summary>
        /// The actual processor type
        /// </summary>
        private SimpleCache<ImageFileMachine> actualProcessorType;

        /// <summary>
        /// The effective processor type
        /// </summary>
        private SimpleCache<ImageFileMachine> effectiveProcessorType;

        /// <summary>
        /// The dump file memory reader
        /// </summary>
        private SimpleCache<DumpFileMemoryReader> dumpFileMemoryReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="Process"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal Process(uint id)
        {
            Id = id;
            systemId = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessSystemId()));
            upTime = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessUpTime()));
            pebAddress = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessPeb()));
            executableName = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => Context.SystemObjects.GetCurrentProcessExecutableName()));
            dumpFileName = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, GetDumpFileName));
            actualProcessorType = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => (ImageFileMachine)Context.Control.GetActualProcessorType()));
            effectiveProcessorType = SimpleCache.Create(ProcessSwitcher.DelegateProtector(this, () => (ImageFileMachine)Context.Control.GetEffectiveProcessorType()));
            threads = SimpleCache.Create(GetThreads);
            modules = SimpleCache.Create(GetModules);
            userTypes = SimpleCache.Create(GetUserTypes);
            ModulesByName = new DictionaryCache<string, Module>(GetModuleByName);
            ModulesById = new DictionaryCache<ulong, Module>(GetModuleById);
            Variables = new DictionaryCache<Tuple<CodeType, ulong, string, string>, Variable>((tuple) => new Variable(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
            UserTypeCastedVariables = new DictionaryCache<Variable, Variable>((variable) => Variable.CastVariableToUserType(variable));
            GlobalCache.UserTypeCastedVariables.Add(UserTypeCastedVariables);
            dumpFileMemoryReader = SimpleCache.Create(() =>
            {
                try
                {
                    return string.IsNullOrEmpty(DumpFileName) ? null : new DumpFileMemoryReader(DumpFileName);
                }
                catch (Exception)
                {
                    return null;
                }
            });
            TypeToUserTypeDescription = new DictionaryCache<Type, UserTypeDescription>(GetUserTypeDescription);
        }

        /// <summary>
        /// Gets or sets the current process.
        /// </summary>
        public static Process Current
        {
            get
            {
                return Context.StateCache.CurrentProcess;
            }

            set
            {
                Context.StateCache.CurrentProcess = value;
            }
        }

        /// <summary>
        /// Gets the array of all processes.
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
                    processes[i] = GlobalCache.Processes[processIds[i]];
                    processes[i].systemId.Value = processSytemIds[i];
                }

                return processes;
            }
        }

        /// <summary>
        /// The modules by name
        /// </summary>
        internal DictionaryCache<string, Module> ModulesByName { get; private set; }

        /// <summary>
        /// The modules by identifier
        /// </summary>
        internal DictionaryCache<ulong, Module> ModulesById { get; private set; }

        /// <summary>
        /// Gets the variables by constructor key.
        /// </summary>
        internal DictionaryCache<Tuple<CodeType, ulong, string, string>, Variable> Variables { get; private set; }

        /// <summary>
        /// Gets the user type casted variables.
        /// </summary>
        private DictionaryCache<Variable, Variable> UserTypeCastedVariables { get; set; }

        /// <summary>
        /// Gets the type to user type description cache.
        /// </summary>
        internal DictionaryCache<Type, UserTypeDescription> TypeToUserTypeDescription { get; private set; }

        /// <summary>
        /// Gets the user types.
        /// </summary>
        internal List<UserTypeDescription> UserTypes
        {
            get
            {
                return userTypes.Value;
            }
        }

        /// <summary>
        /// Gets the dump file memory reader.
        /// </summary>
        internal DumpFileMemoryReader DumpFileMemoryReader
        {
            get
            {
                return dumpFileMemoryReader.Value;
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId
        {
            get
            {
                return systemId.Value;
            }
        }

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
        /// Gets the name of the dump file.
        /// </summary>
        public string DumpFileName
        {
            get
            {
                return dumpFileName.Value;
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
        /// Gets or sets the current thread.
        /// </summary>
        public Thread CurrentThread
        {
            get
            {
                return Context.StateCache.CurrentThread[this];
            }

            set
            {
                if (value.Process != this)
                {
                    throw new Exception("Cannot set current thread to be from different process");
                }

                Context.StateCache.SetCurrentThread(value);
            }
        }

        /// <summary>
        /// Gets the array of process threads.
        /// </summary>
        public Thread[] Threads
        {
            get
            {
                return threads.Value;
            }
        }

        /// <summary>
        /// Gets the array of process modules.
        /// </summary>
        public Module[] Modules
        {
            get
            {
                return modules.Value;
            }
        }

        /// <summary>
        /// Gets the actual type of the processor.
        /// </summary>
        public ImageFileMachine ActualProcessorType
        {
            get
            {
                return actualProcessorType.Value;
            }
        }

        /// <summary>
        /// Gets the effective type of the processor.
        /// </summary>
        public ImageFileMachine EffectiveProcessorType
        {
            get
            {
                return effectiveProcessorType.Value;
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
            int moduleIndex = name.IndexOf('!');

            if (moduleIndex > 0)
            {
                return ModulesByName[name.Substring(0, moduleIndex)].GetVariable(name.Substring(moduleIndex + 1));
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
        /// Clears the process metadata cache.
        /// </summary>
        internal void ClearMetadataCache()
        {
            userTypes.Cached = false;
        }

        /// <summary>
        /// Casts the specified variable to a user type.
        /// </summary>
        /// <param name="variable">The variable.</param>
        internal Variable CastVariableToUserType(Variable variable)
        {
            if (Context.EnableUserCastedVariableCaching)
            {
                return UserTypeCastedVariables[variable];
            }

            return Variable.CastVariableToUserType(variable);
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

        /// <summary>
        /// Gets the user types.
        /// </summary>
        private List<UserTypeDescription> GetUserTypes()
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(this))
            {
                if (Context.UserTypeMetadata != null && Context.UserTypeMetadata.Length > 0)
                {
                    List<UserTypeDescription> userTypes = new List<UserTypeDescription>(Context.UserTypeMetadata.Length);

                    for (int i = 0; i < userTypes.Count; i++)
                    {
                        userTypes.Add(Context.UserTypeMetadata[i].ConvertToDescription());
                    }

                    return userTypes;
                }

                return new List<UserTypeDescription>();
            }
        }

        /// <summary>
        /// Gets the name of the dump file.
        /// </summary>
        private string GetDumpFileName()
        {
            uint dumpsFiles = Context.Client.GetNumberDumpFiles();

            if (dumpsFiles > 1)
            {
                throw new Exception("Unexpected number of dump files");
            }

            if (dumpsFiles == 1)
            {
                StringBuilder sb = new StringBuilder(Constants.MaxFileName);
                uint nameSize, type;
                ulong handle;

                Context.Client.GetDumpFileWide(0, sb, (uint)sb.Capacity, out nameSize, out handle, out type);
                return sb.ToString();
            }

            return "";
        }

        /// <summary>
        /// Gets the user type description from the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private UserTypeDescription GetUserTypeDescription(Type type)
        {
            UserTypeMetadata metadata = UserTypeMetadata.ReadFromType(type);
            return metadata.ConvertToDescription(this);
        }
    }
}
