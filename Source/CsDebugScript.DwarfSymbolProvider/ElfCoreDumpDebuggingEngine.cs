using CsDebugScript.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using CsDebugScript.Engine.Native;
using CsDebugScript.Engine.Utility;
using System.IO;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Debugging engine that allows opening and reading ELF core dumps.
    /// </summary>
    /// <seealso cref="IDebuggerEngine" />
    public class ElfCoreDumpDebuggingEngine : IDebuggerEngine
    {
        /// <summary>
        /// Dictionary of opened dumps by virtual process ID.
        /// </summary>
        private Dictionary<uint, ElfCoreDump> openedDumps = new Dictionary<uint, ElfCoreDump>();

        /// <summary>
        /// The next dump identifier
        /// </summary>
        private int nextDumpId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElfCoreDumpDebuggingEngine"/> class.
        /// </summary>
        public ElfCoreDumpDebuggingEngine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElfCoreDumpDebuggingEngine"/> class.
        /// </summary>
        /// <param name="coreDumpPath">The core dump path.</param>
        public ElfCoreDumpDebuggingEngine(string coreDumpPath)
            : this()
        {
            OpenDump(coreDumpPath);
        }

        /// <summary>
        /// Gets a value indicating whether debugger is currently in live debugging.
        /// </summary>
        /// <value>
        /// <c>true</c> if debugger is currently in live debugging; otherwise, <c>false</c>.
        /// </value>
        public bool IsLiveDebugging
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Opens the specified core dump.
        /// </summary>
        /// <param name="coreDumpPath">The core dump path.</param>
        public void OpenDump(string coreDumpPath)
        {
            ElfCoreDump dump = new ElfCoreDump(coreDumpPath);
            uint dumpId = (uint)System.Threading.Interlocked.Increment(ref nextDumpId);

            lock (openedDumps)
            {
                openedDumps.Add(dumpId, dump);
            }
        }

        /// <summary>
        /// Gets the ELF core dump associated with the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        private ElfCoreDump GetDump(Process process)
        {
            lock (openedDumps)
            {
                return openedDumps[process.Id];
            }
        }

        /// <summary>
        /// Creates new instance of default symbol provider.
        /// </summary>
        public ISymbolProvider CreateDefaultSymbolProvider()
        {
            return new DwarfSymbolProvider();
        }

        /// <summary>
        /// Gets all processes currently being debugged.
        /// </summary>
        public Process[] GetAllProcesses()
        {
            lock (openedDumps)
            {
                Process[] processes = new Process[openedDumps.Count];
                int i = 0;

                foreach (var kvp in openedDumps)
                {
                    Process process = GlobalCache.Processes[kvp.Key];

                    process.systemId.Value = kvp.Value.ProcessId;
                    processes[i] = process;
                }

                return processes;
            }
        }

        /// <summary>
        /// Gets the memory regions.
        /// </summary>
        /// <param name="process">The process.</param>
        public MemoryRegion[] GetMemoryRegions(Process process)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.DumpFileMemoryReader.GetMemoryRanges();
        }

        /// <summary>
        /// Gets the address of the module loaded into specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="moduleName">Name of the module.</param>
        public ulong GetModuleAddress(Process process, string moduleName)
        {
            string originalModuleName = moduleName;

            moduleName = moduleName.ToLower();
            foreach (Module module in process.Modules)
            {
                if (module.Name.ToLower() == moduleName)
                {
                    return module.Address;
                }
            }

            throw new Exception($"Module not found: {originalModuleName}");
        }

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleImageName(Module module)
        {
            ElfCoreDump dump = GetDump(module.Process);
            string name = dump.GetModuleOriginalPath(module.Address);
            string fileName = Path.GetFileName(name);

            return fileName;
        }

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleLoadedImage(Module module)
        {
            ElfCoreDump dump = GetDump(module.Process);

            return dump.GetModuleOriginalPath(module.Address);
        }

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is null. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleMappedImage(Module module)
        {
            ElfCoreDump dump = GetDump(module.Process);
            string name = dump.GetModuleOriginalPath(module.Address);

            if (File.Exists(name))
            {
                // TODO: Verify that it is correct file
                return name;
            }

            string fileName = Path.GetFileName(name);

            name = Path.Combine(Path.GetDirectoryName(dump.Path), fileName);

            if (File.Exists(name))
            {
                // TODO: Verify that it is correct file
                return name;
            }

            return fileName;
        }

        /// <summary>
        /// Gets the dump file memory reader.
        /// </summary>
        /// <param name="process">The process.</param>
        public DumpFileMemoryReader GetDumpFileMemoryReader(Process process)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.DumpFileMemoryReader;
        }

        /// <summary>
        /// Gets the module name. This is usually just the file name without the extension. In a few cases,
        /// the module name differs significantly from the file name.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleName(Module module)
        {
            return GetModuleImageName(module);
        }

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleSymbolFile(Module module)
        {
            return GetModuleMappedImage(module);
        }

        /// <summary>
        /// Gets the size of the module timestamp and.
        /// </summary>
        /// <param name="module">The module.</param>
        public Tuple<DateTime, ulong> GetModuleTimestampAndSize(Module module)
        {
            ElfCoreDump dump = GetDump(module.Process);

            // TODO: Get module timestamp
            return Tuple.Create(DateTime.MinValue, dump.GetModuleSize(module.Address));
        }

        /// <summary>
        /// Gets the current process.
        /// </summary>
        public Process GetCurrentProcess()
        {
            // TODO: Utilize StateCache
            return GlobalCache.Processes[openedDumps.Keys.First()];
        }

        /// <summary>
        /// Gets the current thread of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread GetProcessCurrentThread(Process process)
        {
            // TODO: Utilize StateCache
            return process.Threads[0];
        }

        /// <summary>
        /// Gets the current stack frame of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public StackFrame GetThreadCurrentStackFrame(Thread thread)
        {
            // TODO: Utilize StateCache
            return thread.StackTrace.Frames[0];
        }

        /// <summary>
        /// Sets the current process.
        /// </summary>
        /// <param name="process">The process.</param>
        public void SetCurrentProcess(Process process)
        {
            // TODO: Utilize StateCache
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the current stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        public void SetCurrentStackFrame(StackFrame stackFrame)
        {
            // TODO: Utilize StateCache
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the current thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public void SetCurrentThread(Thread thread)
        {
            // TODO: Utilize StateCache
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the actual processor type of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ImageFileMachine GetProcessActualProcessorType(Process process)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.GetActualProcessorType();
        }

        /// <summary>
        /// Gets the effective processor type of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ImageFileMachine GetProcessEffectiveProcessorType(Process process)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.GetEffectiveProcessorType();
        }

        /// <summary>
        /// Gets the dump file name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public string GetProcessDumpFileName(Process process)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.Path;
        }

        /// <summary>
        /// Gets all modules of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Module[] GetProcessModules(Process process)
        {
            ElfCoreDump dump = GetDump(process);
            ulong[] moduleBaseAddresses = dump.GetModulesBaseAddresses();
            Module[] modules = new Module[moduleBaseAddresses.Length];

            for (int i = 0; i < modules.Length; i++)
            {
                modules[i] = process.ModulesById[moduleBaseAddresses[i]];
                modules[i].Id = (uint)i;
            }

            return modules;
        }

        /// <summary>
        /// Gets the process system identifier.
        /// </summary>
        /// <param name="process">The process.</param>
        public uint GetProcessSystemId(Process process)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.ProcessId;
        }

        /// <summary>
        /// Gets all threads of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread[] GetProcessThreads(Process process)
        {
            ElfCoreDump dump = GetDump(process);
            int[] threadIds = dump.GetThreadIds();
            Thread[] threads = new Thread[threadIds.Length];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread((uint)i, (uint)threadIds[i], process);
            }

            return threads;
        }

        /// <summary>
        /// Gets the stack trace of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public StackTrace GetThreadStackTrace(Thread thread)
        {
            ElfCoreDump dump = GetDump(thread.Process);
            Tuple<ulong, ulong, ulong>[] framesData = dump.GetThreadStackTrace((int)thread.Id);
            StackTrace stackTrace = new StackTrace(thread);
            StackFrame[] frames = new StackFrame[framesData.Length];

            stackTrace.Frames = frames;
            for (int i = 0; i < frames.Length; i++)
            {
                ulong instructionOffset = framesData[i].Item1, stackOffset = framesData[i].Item2, frameOffset = framesData[i].Item3;

                ThreadContext threadContext = new ThreadContext(instructionOffset, stackOffset, frameOffset);
                frames[i] = new StackFrame(stackTrace, threadContext)
                {
                    FrameNumber = (uint)i,
                    FrameOffset = frameOffset,
                    StackOffset = stackOffset,
                    InstructionOffset = instructionOffset,
                    ReturnOffset = 0, // Populated in the loop after this one
                    Virtual = false, // TODO:
                };
            }

            for (int i = frames.Length - 2; i >= 0; i--)
            {
                frames[i].ReturnOffset = frames[i + 1].InstructionOffset;
            }

            return stackTrace;
        }

        /// <summary>
        /// Determines whether the specified process is being debugged as minidump without heap.
        /// </summary>
        /// <param name="process">The process.</param>
        public bool IsMinidump(Process process)
        {
            return false;
        }

        /// <summary>
        /// Queries the virtual.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="regionSize">Size of the region.</param>
        public void QueryVirtual(Process process, ulong address, out ulong baseAddress, out ulong regionSize)
        {
            ElfCoreDump dump = GetDump(process);

            dump.DumpFileMemoryReader.GetMemoryRange(address, out baseAddress, out regionSize);
        }

        /// <summary>
        /// Reads the memory from the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="size">The buffer size.</param>
        /// <returns>
        /// Buffer containing read memory
        /// </returns>
        public MemoryBuffer ReadMemory(Process process, ulong address, uint size)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.DumpFileMemoryReader.ReadMemory(address, (int)size);
        }

        /// <summary>
        /// Reads the ANSI string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadAnsiString(Process process, ulong address, int length = -1)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.DumpFileMemoryReader.ReadAnsiString(address, length);
        }

        /// <summary>
        /// Reads the unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length.</param>
        public string ReadUnicodeString(Process process, ulong address, int length = -1)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.DumpFileMemoryReader.ReadWideString(address, length);
        }

        /// <summary>
        /// Reads the wide unicode (4bytes) string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadWideUnicodeString(Process process, ulong address, int length = -1)
        {
            ElfCoreDump dump = GetDump(process);

            return dump.DumpFileMemoryReader.ReadWideUnicodeString(address, length);
        }

        /// <summary>
        /// Gets the up time of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public uint GetProcessUpTime(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the process environment block address of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ulong GetProcessEnvironmentBlockAddress(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the executable name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public string GetProcessExecutableName(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the module version.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="major">The version major number.</param>
        /// <param name="minor">The version minor number.</param>
        /// <param name="revision">The version revision number.</param>
        /// <param name="patch">The version patch number.</param>
        public void GetModuleVersion(Module module, out int major, out int minor, out int revision, out int patch)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the stack trace from the specified context.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="contextAddress">The context address.</param>
        /// <param name="contextSize">Size of the context. If 0 is specified, context size will be automatically calculated.</param>
        public StackTrace GetStackTraceFromContext(Process process, ulong contextAddress, uint contextSize = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the thread context of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ThreadContext GetThreadContext(Thread thread)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the environment block address of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ulong GetThreadEnvironmentBlockAddress(Thread thread)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets last event info.
        /// </summary>
        public DebugEventInfo GetLastEventInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the pattern in memory of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="memoryStart">The memory start.</param>
        /// <param name="memoryEnd">The memory end.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="patternStart">The pattern start.</param>
        /// <param name="patternEnd">The pattern end.</param>
        /// <param name="searchAlignment">The search alignment in number of bytes. For a successful match, the difference between the location of the found pattern and memoryStart must be a multiple of searchAlignment.</param>
        /// <param name="searchWritableMemoryOnly">if set to <c>true</c> search through writable memory only.</param>
        /// <returns>
        /// Address of the successful match or 0 if patterns wasn't found.
        /// </returns>
        public ulong FindPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            throw new NotImplementedException();
        }

        #region Unsupported functionality
        /// <summary>
        /// When doing live process debugging breaks debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to break.</param>
        public void BreakExecution(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When doing live process debugging continues debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to be continued.</param>
        public void ContinueExecution(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Terminates given process.
        /// </summary>
        /// <param name="process">Process to Terminate.</param>
        public void Terminate(Process process)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
