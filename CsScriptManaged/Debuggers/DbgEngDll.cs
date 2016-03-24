using CsScriptManaged.Debuggers.DbgEngDllHelpers;
using CsScriptManaged.Marshaling;
using CsScriptManaged.Native;
using CsScriptManaged.SymbolProviders;
using CsScripts;
using DbgEngManaged;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CsScriptManaged.Debuggers
{
    internal class DbgEngDll : IDebuggerEngine
    {
        /// <summary>
        /// The original debug client interface
        /// </summary>
        private static IDebugClient originalClient;

        /// <summary>
        /// The thread debug client interface
        /// </summary>
        [ThreadStatic]
        private static IDebugClient threadClient;

        /// <summary>
        /// The state cache
        /// </summary>
        [ThreadStatic]
        private static StateCache stateCache;

        /// <summary>
        /// The DbgEng.dll Advanced interface
        /// </summary>
        [ThreadStatic]
        private static IDebugAdvanced3 advanced;

        /// <summary>
        /// The DbgEng.dll Client interface
        /// </summary>
        [ThreadStatic]
        private static IDebugClient7 client;

        /// <summary>
        /// The DbgEng.dll Control interface
        /// </summary>
        [ThreadStatic]
        private static IDebugControl7 control;

        /// <summary>
        /// The DbgEng.dll Data spaces interface
        /// </summary>
        [ThreadStatic]
        private static IDebugDataSpaces4 dataSpaces;

        /// <summary>
        /// The DbgEng.dll Registers interface
        /// </summary>
        [ThreadStatic]
        private static IDebugRegisters2 registers;

        /// <summary>
        /// The DbgEng.dll Symbols interface
        /// </summary>
        [ThreadStatic]
        private static IDebugSymbols5 symbols;

        /// <summary>
        /// The DbgEng.dll System objects interface
        /// </summary>
        [ThreadStatic]
        private static IDebugSystemObjects4 systemObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbgEngDll"/> class.
        /// </summary>
        /// <param name="client">The debugger client interface.</param>
        public DbgEngDll(IDebugClient client)
        {
            originalClient = client;
            threadClient = client;
            stateCache = new StateCache(this);
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
                try
                {
                    return Client.GetNumberDumpFiles() == 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal static StateCache StateCache
        {
            get
            {
                if (stateCache == null)
                {
                    stateCache = new StateCache((DbgEngDll)Context.Debugger);
                }

                return stateCache;
            }
        }

        /// <summary>
        /// Gets the thread debug client interface.
        /// </summary>
        private static IDebugClient ThreadClient
        {
            get
            {
                if (threadClient == null)
                {
                    threadClient = originalClient.CreateClient();
                }

                return threadClient;
            }
        }

        /// <summary>
        /// Gets the DbgEng.dll Advanced interface
        /// </summary>
        internal IDebugAdvanced3 Advanced
        {
            get
            {
                if (advanced == null)
                {
                    advanced = ThreadClient as IDebugAdvanced3;
                }

                return advanced;
            }
        }

        /// <summary>
        /// The DbgEng.dll Client interface
        /// </summary>
        internal IDebugClient7 Client
        {
            get
            {
                if (client == null)
                {
                    client = ThreadClient as IDebugClient7;
                }

                return client;
            }
        }

        /// <summary>
        /// The DbgEng.dll Control interface
        /// </summary>
        internal IDebugControl7 Control
        {
            get
            {
                if (control == null)
                {
                    control = ThreadClient as IDebugControl7;
                }

                return control;
            }
        }

        /// <summary>
        /// The DbgEng.dll Data spaces interface
        /// </summary>
        internal IDebugDataSpaces4 DataSpaces
        {
            get
            {
                if (dataSpaces == null)
                {
                    dataSpaces = ThreadClient as IDebugDataSpaces4;
                }

                return dataSpaces;
            }
        }

        /// <summary>
        /// The DbgEng.dll Registers interface
        /// </summary>
        internal IDebugRegisters2 Registers
        {
            get
            {
                if (registers == null)
                {
                    registers = ThreadClient as IDebugRegisters2;
                }

                return registers;
            }
        }

        /// <summary>
        /// The DbgEng.dll Symbols interface
        /// </summary>
        internal IDebugSymbols5 Symbols
        {
            get
            {
                if (symbols == null)
                {
                    symbols = ThreadClient as IDebugSymbols5;
                }

                return symbols;
            }
        }

        /// <summary>
        /// The DbgEng.dll System objects interface
        /// </summary>
        internal IDebugSystemObjects4 SystemObjects
        {
            get
            {
                if (systemObjects == null)
                {
                    systemObjects = ThreadClient as IDebugSystemObjects4;
                }

                return systemObjects;
            }
        }

        /// <summary>
        /// Executes the specified command, but leaves its output visible to the user.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public void Execute(string command, params object[] parameters)
        {
            command = string.Join(" ", command, string.Join(" ", parameters));
            StateCache.SyncState();
            Control.Execute((uint)DebugOutctl.ThisClient, command, (uint)(DebugExecute.NotLogged_ | DebugExecute.NoRepeat));
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
        /// <returns>Address of the successful match or 0 if patterns wasn't found.</returns>
        public ulong FindPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            if (memoryEnd <= memoryStart)
            {
                throw new ArgumentOutOfRangeException("memoryEnd", "less than memoryStart");
            }

            int patternSize = patternEnd - patternStart;
            IntPtr pointer = Marshal.AllocHGlobal(patternSize);

            try
            {
                Marshal.Copy(pattern, patternStart, pointer, patternSize);
                using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
                {
                    return DataSpaces.SearchVirtual2(memoryStart, memoryEnd - memoryStart, searchWritableMemoryOnly ? 1U : 0U, pointer, (uint)patternSize, searchAlignment);
                }
            }
            catch (COMException ex)
            {
                if ((uint)ex.HResult == 0x9000001A)
                {
                    return 0;
                }

                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
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
        public byte[] ReadMemory(Process process, ulong address, uint size)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                uint read;

                try
                {
                    DataSpaces.ReadVirtual(address, buffer, size, out read);

                    byte[] bytes = new byte[size];

                    Marshal.Copy(buffer, bytes, 0, (int)size);
                    return bytes;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        /// <summary>
        /// Gets the module name. This is usually just the file name without the extension. In a few cases,
        /// the module name differs significantly from the file name.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleName(Module module)
        {
            return GetModuleName(module, DebugModname.Module);
        }

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleImageName(Module module)
        {
            return GetModuleName(module, DebugModname.Image);
        }

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleLoadedImage(Module module)
        {
            return GetModuleName(module, DebugModname.LoadedImage);
        }

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleSymbolFile(Module module)
        {
            return GetModuleName(module, DebugModname.SymbolFile);
        }

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is null. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleMappedImage(Module module)
        {
            return GetModuleName(module, DebugModname.MappedImage);
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="modname">The type of module name.</param>
        /// <returns>Read name</returns>
        private string GetModuleName(Module module, DebugModname modname)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, module.Process))
            {
                uint nameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxFileName);

                Symbols.GetModuleNameStringWide((uint)modname, 0xffffffff, module.Address, sb, (uint)sb.Capacity, out nameSize);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the current stack frame of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public StackFrame GetThreadCurrentStackFrame(Thread thread)
        {
            return StateCache.CurrentStackFrame[thread];
        }

        /// <summary>
        /// Sets the current stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        public void SetCurrentStackFrame(StackFrame stackFrame)
        {
            StateCache.SetCurrentStackFrame(stackFrame);
        }

        /// <summary>
        /// Reads the line from the debugger input.
        /// </summary>
        public string ReadInput()
        {
            uint inputSize;
            StringBuilder sb = new StringBuilder(10240);

            Control.InputWide(sb, (uint)sb.Capacity, out inputSize);
            return sb.ToString();
        }

        /// <summary>
        /// Sets the current thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public void SetCurrentThread(Thread thread)
        {
            StateCache.SetCurrentThread(thread);
        }

        /// <summary>
        /// Gets the environment block address of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ulong GetThreadEnvironmentBlockAddress(Thread thread)
        {
            using (ThreadSwitcher switcher = new ThreadSwitcher(StateCache, thread))
            {
                return SystemObjects.GetCurrentThreadTeb();
            }
        }

        /// <summary>
        /// Gets the thread context of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ThreadContext GetThreadContext(Thread thread)
        {
            using (ThreadSwitcher switcher = new ThreadSwitcher(StateCache, thread))
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(1))
            {
                Advanced.GetThreadContext(threadContextBuffer.Pointer, (uint)(threadContextBuffer.Count * threadContextBuffer.Size));

                return threadContextBuffer.Elements.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the stack trace of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public StackTrace GetThreadStackTrace(Thread thread)
        {
            return GetStackTraceFromContext(thread, IntPtr.Zero, 0);
        }

        /// <summary>
        /// Gets the stack trace from the specified context.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="contextAddress">The context address.</param>
        /// <param name="contextSize">Size of the context. If 0 is specified, context size will be automatically calculated.</param>
        public StackTrace GetStackTraceFromContext(Process process, ulong contextAddress, uint contextSize = 0)
        {
            if (contextSize == 0)
            {
                contextSize = (uint)ThreadContext.GetContextSize(process);
            }

            Thread thread = new Thread(0, 0, Process.Current);
            IntPtr buffer = Marshal.AllocHGlobal((int)contextSize);

            try
            {
                uint read;

                DataSpaces.ReadVirtual(contextAddress, buffer, contextSize, out read);
                return GetStackTraceFromContext(thread, buffer, contextSize);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Gets the stack trace from the specified context.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="contextAddress">The context address.</param>
        /// <param name="contextSize">Size of the context.</param>
        /// <returns></returns>
        private StackTrace GetStackTraceFromContext(Thread thread, IntPtr contextAddress, uint contextSize)
        {
            const int MaxCallStack = 10240;
            using (ThreadSwitcher switcher = new ThreadSwitcher(StateCache, thread))
            using (MarshalArrayReader<_DEBUG_STACK_FRAME_EX> frameBuffer = new RegularMarshalArrayReader<_DEBUG_STACK_FRAME_EX>(MaxCallStack))
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(MaxCallStack))
            {
                uint framesCount;

                Control.GetContextStackTraceEx(contextAddress, contextSize, frameBuffer.Pointer, (uint)frameBuffer.Count, threadContextBuffer.Pointer, (uint)(threadContextBuffer.Size * threadContextBuffer.Count), (uint)threadContextBuffer.Size, out framesCount);
                return new StackTrace(thread, frameBuffer.Elements.Take((int)framesCount).ToArray(), threadContextBuffer.Elements.Take((int)framesCount).ToArray());
            }
        }

        /// <summary>
        /// Gets the system identifier of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public uint GetProcessSystemId(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                return SystemObjects.GetCurrentProcessSystemId();
            }
        }

        /// <summary>
        /// Gets the up time of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public uint GetProcessUpTime(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                return SystemObjects.GetCurrentProcessUpTime();
            }
        }

        /// <summary>
        /// Gets the process environment block address of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ulong GetProcessEnvironmentBlockAddress(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                return SystemObjects.GetCurrentProcessPeb();
            }
        }

        /// <summary>
        /// Gets the executable name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public string GetProcessExecutableName(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                return SystemObjects.GetCurrentProcessExecutableName();
            }
        }

        /// <summary>
        /// Gets the dump file name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public string GetProcessDumpFileName(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint dumpsFiles = Client.GetNumberDumpFiles();

                if (dumpsFiles > 1)
                {
                    throw new Exception("Unexpected number of dump files");
                }

                if (dumpsFiles == 1)
                {
                    StringBuilder sb = new StringBuilder(Constants.MaxFileName);
                    uint nameSize, type;
                    ulong handle;

                    Client.GetDumpFileWide(0, sb, (uint)sb.Capacity, out nameSize, out handle, out type);
                    return sb.ToString();
                }

                return "";
            }
        }

        /// <summary>
        /// Gets the actual processor type of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ImageFileMachine GetProcessActualProcessorType(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                return (ImageFileMachine)Control.GetActualProcessorType();
            }
        }

        /// <summary>
        /// Gets the effective processor type of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ImageFileMachine GetProcessEffectiveProcessorType(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                return (ImageFileMachine)Control.GetEffectiveProcessorType();
            }
        }

        /// <summary>
        /// Gets all threads of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread[] GetProcessThreads(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint threadCount = SystemObjects.GetNumberThreads();
                Thread[] threads = new Thread[threadCount];
                uint[] threadIds = new uint[threadCount];
                uint[] threadSytemIds = new uint[threadCount];

                unsafe
                {
                    fixed (uint* ids = &threadIds[0])
                    fixed (uint* systemIds = &threadSytemIds[0])
                    {
                        SystemObjects.GetThreadIdsByIndex(0, threadCount, out *ids, out *systemIds);
                    }
                }

                for (uint i = 0; i < threadCount; i++)
                {
                    threads[i] = new Thread(threadIds[i], threadSytemIds[i], process);
                }

                return threads;
            }
        }

        /// <summary>
        /// Gets all modules of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Module[] GetProcessModules(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint loaded, unloaded;

                Symbols.GetNumberModules(out loaded, out unloaded);
                Module[] modules = new Module[loaded + unloaded];

                for (int i = 0; i < modules.Length; i++)
                {
                    ulong moduleId = Symbols.GetModuleByIndex((uint)i);

                    modules[i] = process.ModulesById[moduleId];
                }

                return modules;
            }
        }

        /// <summary>
        /// Gets the current process.
        /// </summary>
        public Process GetCurrentProcess()
        {
            return StateCache.CurrentProcess;
        }

        /// <summary>
        /// Sets the current process.
        /// </summary>
        /// <param name="process">The process.</param>
        public void SetCurrentProcess(Process process)
        {
            StateCache.CurrentProcess = process;
        }

        /// <summary>
        /// Gets all processes currently being debugged.
        /// </summary>
        public Process[] GetAllProcesses()
        {
            uint processCount = SystemObjects.GetNumberProcesses();
            Process[] processes = new Process[processCount];
            uint[] processIds = new uint[processCount];
            uint[] processSytemIds = new uint[processCount];

            unsafe
            {
                fixed (uint* ids = &processIds[0])
                fixed (uint* systemIds = &processSytemIds[0])
                {
                    SystemObjects.GetProcessIdsByIndex(0, processCount, out *ids, out *systemIds);
                }
            }

            for (uint i = 0; i < processCount; i++)
            {
                processes[i] = GlobalCache.Processes[processIds[i]];
                processes[i].systemId.Value = processSytemIds[i];
            }

            return processes;
        }

        /// <summary>
        /// Gets the current thread of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread GetProcessCurrentThread(Process process)
        {
            return StateCache.CurrentThread[process];
        }

        /// <summary>
        /// Gets the address of the module loaded into specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="moduleName">Name of the module.</param>
        public ulong GetModuleAddress(Process process, string moduleName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint index;
                ulong moduleAddress;

                Symbols.GetModuleByModuleName2Wide(moduleName, 0, 0, out index, out moduleAddress);
                return moduleAddress;
            }
        }

        /// <summary>
        /// Reads the ANSI string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        public string ReadAnsiString(Process process, ulong address)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint stringLength;
                StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                DataSpaces.ReadMultiByteStringVirtual(address, Constants.MaxStringReadLength, sb, (uint)sb.Capacity, out stringLength);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Reads the unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        public string ReadUnicodeString(Process process, ulong address)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint stringLength;
                StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                DataSpaces.ReadUnicodeStringVirtualWide(address, Constants.MaxStringReadLength * 2, sb, (uint)sb.Capacity, out stringLength);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Creates new instance of default symbol provider.
        /// </summary>
        public ISymbolProvider CreateDefaultSymbolProvider()
        {
            return new DbgEngSymbolProvider(this);
        }

        /// <summary>
        /// Creates new instance of default symbol provider module.
        /// </summary>
        public ISymbolProviderModule CreateDefaultSymbolProviderModule()
        {
            return new DbgEngSymbolProvider(this);
        }

        /// <summary>
        /// Executes the action in redirected console output and error stream.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ExecuteAction(Action action)
        {
            TextWriter originalConsoleOut = Console.Out;
            TextWriter originalConsoleError = Console.Error;

            Console.SetOut(new DebuggerTextWriter(this, DebugOutput.Normal));
            Console.SetError(new DebuggerTextWriter(this, DebugOutput.Error));
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
                Console.SetError(originalConsoleError);
                StateCache.SyncState();
            }
        }
    }
}
