using CsScriptManaged;
using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using System;
using System.Collections.Generic;

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
        internal SimpleCache<uint> systemId;

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
        /// The ANSI string cache
        /// </summary>
        private DictionaryCache<ulong, string> ansiStringCache;

        /// <summary>
        /// The unicode string cache
        /// </summary>
        private DictionaryCache<ulong, string> unicodeStringCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Process"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal Process(uint id)
        {
            Id = id;
            systemId = SimpleCache.Create(() => Context.Debugger.GetProcessSystemId(this));
            upTime = SimpleCache.Create(() => Context.Debugger.GetProcessUpTime(this));
            pebAddress = SimpleCache.Create(() => Context.Debugger.GetProcessEnvironmentBlockAddress(this));
            executableName = SimpleCache.Create(() => Context.Debugger.GetProcessExecutableName(this));
            dumpFileName = SimpleCache.Create(() => Context.Debugger.GetProcessDumpFileName(this));
            actualProcessorType = SimpleCache.Create(() => Context.Debugger.GetProcessActualProcessorType(this));
            effectiveProcessorType = SimpleCache.Create(() => Context.Debugger.GetProcessEffectiveProcessorType(this));
            threads = SimpleCache.Create(() => Context.Debugger.GetProcessThreads(this));
            modules = SimpleCache.Create(() => Context.Debugger.GetProcessModules(this));
            userTypes = SimpleCache.Create(GetUserTypes);
            ModulesByName = new DictionaryCache<string, Module>(GetModuleByName);
            ModulesById = new DictionaryCache<ulong, Module>(GetModuleByAddress);
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
            ansiStringCache = new DictionaryCache<ulong, string>(DoReadAnsiString);
            unicodeStringCache = new DictionaryCache<ulong, string>(DoReadUnicodeString);
        }

        /// <summary>
        /// Gets or sets the current process.
        /// </summary>
        public static Process Current
        {
            get
            {
                return Context.Debugger.GetCurrentProcess();
            }

            set
            {
                Context.Debugger.SetCurrentProcess(value);
            }
        }

        /// <summary>
        /// Gets the array of all processes.
        /// </summary>
        public static Process[] All
        {
            get
            {
                return Context.Debugger.GetAllProcesses();
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
                return Context.Debugger.GetProcessCurrentThread(this);
            }

            set
            {
                if (value.Process != this)
                {
                    throw new Exception("Cannot set current thread to be from different process");
                }

                Context.Debugger.SetCurrentThread(value);
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
        /// Gets the module with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        private Module GetModuleByName(string name)
        {
            ulong moduleAddress = Context.Debugger.GetModuleAddress(this, name);
            Module module = ModulesById[moduleAddress];

            module.Name = name;
            return module;
        }

        /// <summary>
        /// Gets the module located at the specified module address.
        /// </summary>
        /// <param name="moduleAddress">The module address.</param>
        private Module GetModuleByAddress(ulong moduleAddress)
        {
            return new Module(this, moduleAddress);
        }

        /// <summary>
        /// Gets the user types.
        /// </summary>
        private List<UserTypeDescription> GetUserTypes()
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

        /// <summary>
        /// Gets the user type description from the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private UserTypeDescription GetUserTypeDescription(Type type)
        {
            UserTypeMetadata metadata = UserTypeMetadata.ReadFromType(type);
            return metadata.ConvertToDescription(this);
        }

        /// <summary>
        /// Gets the size of the pointer.
        /// </summary>
        /// <returns></returns>
        public uint GetPointerSize()
        {
            return ActualProcessorType == ImageFileMachine.I386 || EffectiveProcessorType == ImageFileMachine.I386 ? 4U : 8U;
        }

        /// <summary>
        /// Reads the string and caches it inside this object.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="charSize">Size of the character.</param>
        internal string ReadString(ulong address, int charSize)
        {
            if (address == 0)
            {
                return null;
            }

            if (charSize == 1)
            {
                return ansiStringCache[address];
            }
            else if (charSize == 2)
            {
                return unicodeStringCache[address];
            }
            else
            {
                throw new Exception("Unsupported char size");
            }
        }

        /// <summary>
        /// Does the actual ANSI string read.
        /// </summary>
        /// <param name="address">The address.</param>
        private string DoReadAnsiString(ulong address)
        {
            if (address == 0)
            {
                return null;
            }

            var dumpReader = DumpFileMemoryReader;

            if (dumpReader != null)
            {
                return dumpReader.ReadAnsiString(address);
            }
            else
            {
                return Context.Debugger.ReadAnsiString(this, address);
            }
        }

        /// <summary>
        /// Does the actual unicode string read.
        /// </summary>
        /// <param name="address">The address.</param>
        private string DoReadUnicodeString(ulong address)
        {
            if (address == 0)
            {
                return null;
            }

            var dumpReader = DumpFileMemoryReader;

            if (dumpReader != null)
            {
                return dumpReader.ReadWideString(address);
            }
            else
            {
                return Context.Debugger.ReadUnicodeString(this, address);
            }
        }
    }
}
