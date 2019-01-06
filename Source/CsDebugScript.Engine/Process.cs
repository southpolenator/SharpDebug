using CsDebugScript.CLR;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript
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
        /// The architecture type
        /// </summary>
        private SimpleCache<ArchitectureType> architectureType;

        /// <summary>
        /// The dump file memory reader
        /// </summary>
        private SimpleCache<DumpFileMemoryReader> dumpFileMemoryReader;

        /// <summary>
        /// The CLR runtimes running in the process
        /// </summary>
        private SimpleCache<IClrRuntime[]> clrRuntimes;

        /// <summary>
        /// The current application domain
        /// </summary>
        private SimpleCache<IClrAppDomain> currentAppDomain;

        /// <summary>
        /// The cache of memory regions
        /// </summary>
        private SimpleCache<MemoryRegion[]> memoryRegions;

        /// <summary>
        /// The cache of memory region finder
        /// </summary>
        private SimpleCache<MemoryRegionFinder> memoryRegionFinder;

        /// <summary>
        /// The ANSI string cache
        /// </summary>
        private DictionaryCache<Tuple<ulong, int>, string> ansiStringCache;

        /// <summary>
        /// The unicode string cache
        /// </summary>
        private DictionaryCache<Tuple<ulong, int>, string> unicodeStringCache;

        /// <summary>
        /// The wide unicode string cache
        /// </summary>
        private DictionaryCache<Tuple<ulong, int>, string> wideUnicodeStringCache;

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
            architectureType = SimpleCache.Create(() => Context.Debugger.GetProcessArchitectureType(this));
            threads = SimpleCache.Create(() => Context.Debugger.GetProcessThreads(this));
            modules = SimpleCache.Create(() => Context.Debugger.GetProcessModules(this));
            clrRuntimes = SimpleCache.Create(() =>
            {
                try
                {
                    if (Context.ClrProvider != null)
                    {
                        return Context.ClrProvider.GetClrRuntimes(this);
                    }
                }
                catch
                {
                }
                return new IClrRuntime[0];
            });
            currentAppDomain = SimpleCache.Create(() => ClrRuntimes.SelectMany(r => r.AppDomains).FirstOrDefault());
            ModulesByName = new DictionaryCache<string, Module>(GetModuleByName);
            ModulesById = new DictionaryCache<ulong, Module>(GetModuleByAddress);
            Variables = new DictionaryCache<Tuple<CodeType, ulong, string, string>, Variable>((tuple) => new Variable(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
            UserTypeCastedVariables = Context.UserTypeMetadataCaches.CreateDictionaryCache<Variable, Variable>((variable) => Variable.CastVariableToUserType(variable));
            ClrModuleCache = new DictionaryCache<IClrModule, Module>((clrModule) =>
            {
                // TODO: This needs to change when ClrModule starts to be child of Module
                Module module = ModulesById[clrModule.ImageBase];

                module.ClrModule = clrModule;
                module.ImageName = clrModule.Name;
                if (!string.IsNullOrEmpty(clrModule.PdbFileName))
                {
                    try
                    {
                        if (!module.SymbolFileName.ToLowerInvariant().EndsWith(".pdb"))
                        {
                            module.SymbolFileName = clrModule.PdbFileName;
                        }
                    }
                    catch
                    {
                        module.SymbolFileName = clrModule.PdbFileName;
                    }
                }
                module.Name = Path.GetFileNameWithoutExtension(clrModule.Name);
                module.LoadedImageName = clrModule.Name;
                module.Size = clrModule.Size;
                return module;
            });
            dumpFileMemoryReader = SimpleCache.Create(() =>
            {
                try
                {
                    return Context.Debugger.GetDumpFileMemoryReader(this);
                }
                catch (Exception)
                {
                    return null;
                }
            });
            memoryRegions = SimpleCache.Create(() =>
            {
                if (DumpFileMemoryReader != null)
                {
                    return DumpFileMemoryReader.GetMemoryRanges();
                }
                else
                {
                    return Context.Debugger.GetMemoryRegions(this);
                }
            });
            memoryRegionFinder = SimpleCache.Create(() => new MemoryRegionFinder(MemoryRegions));
            ansiStringCache = new DictionaryCache<Tuple<ulong, int>, string>(DoReadAnsiString);
            unicodeStringCache = new DictionaryCache<Tuple<ulong, int>, string>(DoReadUnicodeString);
            wideUnicodeStringCache = new DictionaryCache<Tuple<ulong, int>, string>(DoReadWideUnicodeString);
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
        /// The cache of CLR module to Module.
        /// </summary>
        internal DictionaryCache<IClrModule, Module> ClrModuleCache { get; private set; }

        /// <summary>
        /// Gets the user type casted variables.
        /// </summary>
        private DictionaryCache<Variable, Variable> UserTypeCastedVariables { get; set; }

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
        public ulong PebAddress
        {
            get
            {
                return pebAddress.Value;
            }
        }

        /// <summary>
        /// Gets the variable that represents PEB (Process environment block).
        /// </summary>
        public Variable PEB
        {
            get
            {
                try
                {
                    List<string> searchModulesOrder = new List<string> { Modules[0].Name.ToLower(), "wow64", "ntdll", "nt" };
                    IEnumerable<Module> modules = Modules.OrderByDescending(m => searchModulesOrder.IndexOf(m.Name.ToLower()));

                    foreach (Module module in modules)
                    {
                        try
                        {
                            CodeType pebCodeType = CodeType.Create("_PEB", module);

                            return Variable.Create(pebCodeType, PebAddress, "PEB", "Process.PEB");
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }

                return new NakedPointer(this, PebAddress);
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
        /// Gets the architecture type.
        /// </summary>
        public ArchitectureType ArchitectureType
        {
            get
            {
                return architectureType.Value;
            }
        }

        /// <summary>
        /// Gets the all memory regions available in the this process.
        /// </summary>
        public MemoryRegion[] MemoryRegions
        {
            get
            {
                return memoryRegions.Value;
            }
        }

        /// <summary>
        /// Gets the array of CLR runtimes running in the process.
        /// </summary>
        public IClrRuntime[] ClrRuntimes
        {
            get
            {
                return clrRuntimes.Value;
            }
        }

        /// <summary>
        /// Gets or sets the current CLR application domain. If not set, if will be first AppDomain from first Runtime.
        /// </summary>
        public IClrAppDomain CurrentCLRAppDomain
        {
            get
            {
                return currentAppDomain.Value;
            }

            set
            {
                currentAppDomain.Value = value;
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
            try
            {
                return $"({Id}:{SystemId}) - {ExecutableName}";
            }
            catch
            {
                return $"({Id}:{SystemId})";
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
        /// Finds the index of memory region where the specified address is located or -1 if not found.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The index of memory region where the specified address is located or -1 if not found.</returns>
        public int FindMemoryRegion(ulong address)
        {
            int index = memoryRegionFinder.Value.Find(address);

            if (index >= 0)
            {
                MemoryRegion region = MemoryRegions[index];

                if (address < region.MemoryStart || address >= region.MemoryEnd)
                {
                    index = -1;
                }
            }

            return index;
        }

        /// <summary>
        /// Invalidates cache structures.
        /// Use when memory state changes (e.g. during live debugging).
        /// </summary>
        public void InvalidateProcessCache()
        {
            CacheInvalidator.InvalidateCaches(this);
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
        /// Creates CodeType from the CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type.</param>
        internal CodeType FromClrType(IClrType clrType)
        {
            return ClrModuleCache[clrType.Module].ClrTypes[clrType];
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
        /// Gets the module that contains specified address in its address space.
        /// </summary>
        /// <param name="address">The address.</param>
        internal Module GetModuleByInnerAddress(ulong address)
        {
            foreach (var module in Modules)
            {
                if (module.Address <= address && module.Address + module.Size > address)
                {
                    return module;
                }
            }

            return null;
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
        /// Gets the size of the pointer.
        /// </summary>
        public uint GetPointerSize()
        {
            switch (ArchitectureType)
            {
                case ArchitectureType.X86:
                case ArchitectureType.X86OverAmd64:
                    return 4U;
                case ArchitectureType.Amd64:
                    return 8U;
                default:
                    throw new Exception($"Unsupported architecture type: {ArchitectureType}");
            }
        }

        #region Read functions
        /// <summary>
        /// Reads pointer from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Pointer read from the specified address.</returns>
        public ulong ReadPointer(ulong address)
        {
            uint pointerSize = GetPointerSize();
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, pointerSize);

            return UserType.ReadPointer(buffer, 0, (int)pointerSize);
        }

        /// <summary>
        /// Reads byte from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Byte read from the specified address.</returns>
        public byte ReadByte(ulong address, int bits = 8, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 1);

            return UserType.ReadByte(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Reads signed byte from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Signed byte read from the specified address.</returns>
        public sbyte ReadSbyte(ulong address)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 1);

            return UserType.ReadSbyte(buffer, 0);
        }

        /// <summary>
        /// Reads short from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Short read from the specified address.</returns>
        public short ReadShort(ulong address, int bits = 16, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 2);

            return UserType.ReadShort(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Read unsigned short from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Unsigned short read from the specified address.</returns>
        public ushort ReadUshort(ulong address, int bits = 16, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 2);

            return UserType.ReadUshort(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Read integer from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Integer read from the specified address.</returns>
        public int ReadInt(ulong address, int bits = 32, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 4);

            return UserType.ReadInt(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Read unsigned integer from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Unsigned integer read from the specified address.</returns>
        public uint ReadUint(ulong address, int bits = 32, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 4);

            return UserType.ReadUint(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Read long integer from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Long integer read from the specified address.</returns>
        public long ReadLong(ulong address, int bits = 64, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 8);

            return UserType.ReadLong(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Read unsigned long integer from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        /// <returns>Unsigned long integer read from the specified address.</returns>
        public ulong ReadUlong(ulong address, int bits = 64, int bitsOffset = 0)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 8);

            return UserType.ReadUlong(buffer, 0, bits, bitsOffset);
        }

        /// <summary>
        /// Read float from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Float from the specified address.</returns>
        public float ReadFloat(ulong address)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 4);

            return UserType.ReadFloat(buffer, 0);
        }

        /// <summary>
        /// Read double from the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>Double read from the specified address.</returns>
        public double ReadDouble(ulong address)
        {
            MemoryBuffer buffer = Debugger.ReadMemory(this, address, 8);

            return UserType.ReadDouble(buffer, 0);
        }

        /// <summary>
        /// Gets function that can be used to read integer from the specified address of the structure containing the specified field by dot path.
        /// This function handles all built-in types and converts result to integer for the user.
        /// </summary>
        /// <param name="codeType">The code type containing field that should be read.</param>
        /// <param name="fieldPath">Field dot path. Example: MyType.MyInnerType.MyField</param>
        /// <param name="baseOffset">Base offset that will be applied to the specified address. This offset will be added with field offset.</param>
        /// <param name="readingCodeType">Code type of the field to be read. If not specified, field code type will be used.</param>
        /// <returns>Function that reads integer for the speicifed address.</returns>
        public Func<ulong, int> GetReadInt(CodeType codeType, string fieldPath = null, int baseOffset = 0, CodeType readingCodeType = null)
        {
            CodeType fieldCodeType = codeType;
            int offset = baseOffset;

            if (!string.IsNullOrEmpty(fieldPath))
            {
                string[] fields = fieldPath.Split(".".ToCharArray());

                for (int i = 0; i < fields.Length; i++)
                {
                    offset += fieldCodeType.GetFieldOffset(fields[i]);
                    fieldCodeType = fieldCodeType.GetFieldType(fields[i]);
                }
            }

            if (readingCodeType == null)
                readingCodeType = fieldCodeType;
            switch (readingCodeType.BuiltinType)
            {
                case BuiltinType.Bool:
                    return (address) => ReadByte(address + (ulong)offset) != 0 ? 1 : 0;
                case BuiltinType.Char8:
                case BuiltinType.Int8:
                    return (address) => ReadSbyte(address + (ulong)offset);
                case BuiltinType.Char16:
                case BuiltinType.UInt16:
                    return (address) => ReadUshort(address + (ulong)offset);
                case BuiltinType.Char32:
                case BuiltinType.UInt32:
                    return (address) => (int)ReadUint(address + (ulong)offset);
                case BuiltinType.Float32:
                    return (address) => (int)ReadFloat(address + (ulong)offset);
                case BuiltinType.Float64:
                    return (address) => (int)ReadDouble(address + (ulong)offset);
                case BuiltinType.Int16:
                    return (address) => ReadShort(address + (ulong)offset);
                case BuiltinType.Int32:
                    return (address) => ReadInt(address + (ulong)offset);
                case BuiltinType.Int64:
                    return (address) => (int)ReadLong(address + (ulong)offset);
                case BuiltinType.UInt8:
                    return (address) => ReadByte(address + (ulong)offset);
                case BuiltinType.UInt64:
                    return (address) => (int)ReadUlong(address + (ulong)offset);
            }

            throw new NotImplementedException($"Unexpected build-in type: {readingCodeType.BuiltinType}");
        }

        /// <summary>
        /// Reads the string and caches it inside this object.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        internal string ReadString(ulong address, int charSize, int length = -1)
        {
            if (address == 0)
            {
                return null;
            }

            if (charSize == 1)
            {
                return ansiStringCache[Tuple.Create(address, length)];
            }
            else if (charSize == 2)
            {
                return unicodeStringCache[Tuple.Create(address, length)];
            }
            else if (charSize == 4)
            {
                return wideUnicodeStringCache[Tuple.Create(address, length)];
            }
            else
            {
                throw new Exception("Unsupported char size");
            }
        }

        /// <summary>
        /// Does the actual ANSI string read.
        /// </summary>
        /// <param name="tuple">Address and length tuple.</param>
        private string DoReadAnsiString(Tuple<ulong, int> tuple)
        {
            ulong address = tuple.Item1;
            int length = tuple.Item2;

            if (address == 0)
            {
                return null;
            }

            var dumpReader = DumpFileMemoryReader;

            if (dumpReader != null)
            {
                return dumpReader.ReadAnsiString(address, length);
            }
            else
            {
                return Context.Debugger.ReadAnsiString(this, address, length);
            }
        }

        /// <summary>
        /// Does the actual unicode string read.
        /// </summary>
        /// <param name="tuple">Address and length tuple.</param>
        private string DoReadUnicodeString(Tuple<ulong, int> tuple)
        {
            ulong address = tuple.Item1;
            int length = tuple.Item2;

            if (address == 0)
            {
                return null;
            }

            var dumpReader = DumpFileMemoryReader;

            if (dumpReader != null)
            {
                return dumpReader.ReadWideString(address, length);
            }
            else
            {
                return Context.Debugger.ReadUnicodeString(this, address, length);
            }
        }

        /// <summary>
        /// Does the actual wide unicode (4bytes) string read.
        /// </summary>
        /// <param name="tuple">Address and length tuple.</param>
        private string DoReadWideUnicodeString(Tuple<ulong, int> tuple)
        {
            ulong address = tuple.Item1;
            int length = tuple.Item2;

            if (address == 0)
            {
                return null;
            }

            var dumpReader = DumpFileMemoryReader;

            if (dumpReader != null)
            {
                return dumpReader.ReadWideUnicodeString(address, length);
            }
            else
            {
                return Context.Debugger.ReadWideUnicodeString(this, address, length);
            }
        }
        #endregion
    }
}
