using CsDebugScript.Engine.Debuggers.DbgEngDllHelpers;
using CsDebugScript.Engine.Marshaling;
using CsDebugScript.Engine.Native;
using CsDebugScript.Engine.SymbolProviders;
using CsDebugScript.Engine.Utility;
using CsDebugScript.Exceptions;
using DbgEngManaged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CsDebugScript.Engine.Debuggers
{
    /// <summary>
    /// DbgEng.dll implementation of <see cref="IDebuggerEngine"/>.
    /// </summary>
    /// <seealso cref="CsDebugScript.Engine.IDebuggerEngine" />
    public class DbgEngDll : IDebuggerEngine
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
        /// Dictionary of all debugee flow controllers. Key is process id that is being debugged.
        /// </summary>
        private static DictionaryCache<uint, DebuggeeFlowController> debugeeFlowControllers;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DbgEngDll()
        {
            // Populate flow controllers lazily.
            //
            // NOTE: Client passed needs to be set to process with selected processId.
            debugeeFlowControllers =
                new DictionaryCache<uint, DebuggeeFlowController>(
                    (processId) => new DebuggeeFlowController(ThreadClient));
        }

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

        /// <summary>
        /// Gets the state cache.
        /// </summary>
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
        /// Initializes the context with the specified <see cref="IDebugClient"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        public static void InitializeContext(IDebugClient client)
        {
            IDebuggerEngine debugger = new DbgEngDll(client);
            ISymbolProvider symbolProvider = new DiaSymbolProvider(debugger.CreateDefaultSymbolProvider());

            Context.InitializeDebugger(debugger, symbolProvider);
        }

        #region Executing native commands
        /// <summary>
        /// Executes the specified command and captures its output.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Captured text</returns>
        public static string ExecuteAndCapture(string command, params object[] parameters)
        {
            DebugOutput captureFlags = DebugOutput.Normal | DebugOutput.Error | DebugOutput.Warning | DebugOutput.Verbose
                | DebugOutput.Prompt | DebugOutput.PromptRegisters | DebugOutput.ExtensionWarning | DebugOutput.Debuggee
                | DebugOutput.DebuggeePrompt | DebugOutput.Symbols | DebugOutput.Status;
            return ExecuteAndCapture(captureFlags, command, parameters);
        }

        /// <summary>
        /// Executes the specified command and captures its output.
        /// </summary>
        /// <param name="captureFlags">The capture flags.</param>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Captured text</returns>
        public static string ExecuteAndCapture(DebugOutput captureFlags, string command, params object[] parameters)
        {
            using (StringWriter writer = new StringWriter())
            {
                var callbacks = DebuggerOutputToTextWriter.Create(writer, captureFlags);
                using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
                {
                    Execute(command, parameters);
                    writer.Flush();
                    return writer.GetStringBuilder().ToString();
                }
            }
        }

        /// <summary>
        /// Executes the specified command, but leaves its output visible to the user.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Execute(string command, params object[] parameters)
        {
            ((DbgEngDll)Context.Debugger).ExecuteCommand(command, parameters);
        }
        #endregion

        /// <summary>
        /// Executes the specified command, but leaves its output visible to the user.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public void ExecuteCommand(string command, params object[] parameters)
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
        /// Gets the dump file memory reader of the specified process if it is debugged from a dump.
        /// </summary>
        /// <param name="process">The process.</param>
        public DumpFileMemoryReader GetDumpFileMemoryReader(Process process)
        {
            string dumpFilename = process.DumpFileName;

            if (File.Exists(dumpFilename))
            {
                return new WindowsDumpFileMemoryReader(dumpFilename);
            }

            return null;
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
            try
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
                        return new MemoryBuffer(bytes);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
            }
            catch (Exception)
            {
                throw new InvalidMemoryAddressException(address);
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
                if (modname == DebugModname.MappedImage && !File.Exists(sb.ToString()))
                {
                    uint imagePathSize;
                    Symbols.GetImagePathWide(null, 0, out imagePathSize);
                    StringBuilder imagePath = new StringBuilder((int)imagePathSize + 1);
                    Symbols.GetImagePathWide(imagePath, (uint)imagePath.Capacity, out imagePathSize);

                    uint symbolPathSize;
                    Symbols.GetSymbolPathWide(null, 0, out symbolPathSize);
                    StringBuilder symbolPath = new StringBuilder((int)symbolPathSize + 1);
                    Symbols.GetSymbolPathWide(symbolPath, (uint)symbolPath.Capacity, out symbolPathSize);

                    List<string> folders = new List<string>();

                    folders.AddRange(imagePath.ToString().Split(";".ToCharArray()));
                    folders.AddRange(symbolPath.ToString().Split(";".ToCharArray()));

                    foreach (string folder in folders)
                    {
                        string path = Path.Combine(folder, module.LoadedImageName);

                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
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
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(thread.Process, 1))
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

#if false
        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It is called when StackWalk64 needs to read memory from the address space of the process.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="lpBaseAddress">The base address of the memory to be read.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the memory to be read.</param>
        /// <param name="nSize">The size of the memory to be read, in bytes.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes actually read.</param>
        private static unsafe bool ReadMemory(IntPtr hProcess, ulong lpBaseAddress, IntPtr lpBuffer, uint nSize, out uint lpNumberOfBytesRead)
        {
            try
            {
                Process process = Process.All.Where(p => (IntPtr)p.SystemId == hProcess).First();
                MemoryBuffer memoryBuffer = Debugger.ReadMemory(process, lpBaseAddress, nSize);

                if (memoryBuffer.BytePointer != null)
                {
                    MemoryBuffer.MemCpy(lpBuffer.ToPointer(), memoryBuffer.BytePointer, Math.Min((uint)memoryBuffer.BytePointerLength, nSize));
                    lpNumberOfBytesRead = Math.Min((uint)memoryBuffer.BytePointerLength, nSize);
                    return true;
                }
                else
                {
                    Marshal.Copy(memoryBuffer.Bytes, 0, lpBuffer, Math.Min(memoryBuffer.Bytes.Length, (int)nSize));
                    lpNumberOfBytesRead = Math.Min((uint)memoryBuffer.Bytes.Length, nSize);
                    return true;
                }
            }
            catch (Exception)
            {
                lpNumberOfBytesRead = 0;
                return false;
            }
        }

        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It provides access to the run-time function table for the process.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="AddrBase">The address of the instruction to be located.</param>
        /// <returns>The function returns a pointer to the run-time function table. On an x86 computer, this is a pointer to an FPO_DATA structure. On an Alpha computer, this is a pointer to an IMAGE_FUNCTION_ENTRY structure.</returns>
        private static IntPtr GetFunctionTableAccess(IntPtr hProcess, ulong AddrBase)
        {
            return SymFunctionTableAccess64AccessRoutines(hProcess, AddrBase, ReadMemory, GetModuleBaseAddress);
        }

        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It is called when StackWalkEx needs a module base address for a given virtual address.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="Address">An address within the module image to be located.</param>
        /// <returns>The function returns the base address of the module.</returns>
        private static ulong GetModuleBaseAddress(IntPtr hProcess, ulong Address)
        {
            Process process = Process.All.Where(p => (IntPtr)p.SystemId == hProcess).First();
            var modules = process.Modules;
            ulong bestMatch = 0;
            ulong bestDistance = ulong.MaxValue;

            foreach (var module in modules)
            {
                if (module.Address < Address)
                {
                    ulong distance = Address - module.Address;

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestMatch = module.Address;
                    }
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// Reads the stack trace from context using StackWalkEx.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="contextAddress">The context address.</param>
        private StackTrace ReadStackTraceFromContext(Thread thread, IntPtr contextAddress)
        {
            List<_DEBUG_STACK_FRAME_EX> frames = new List<_DEBUG_STACK_FRAME_EX>();
            List<ThreadContext> contexts = new List<ThreadContext>();
            STACKFRAME_EX stackFrame = new STACKFRAME_EX();

            while (true)
            {
                if (!StackWalkEx(thread.Process.ActualProcessorType, (IntPtr)thread.Process.SystemId, (IntPtr)thread.SystemId, ref stackFrame, contextAddress, ReadMemory, GetFunctionTableAccess, GetModuleBaseAddress, null, 0))
                    break;

                frames.Add(new _DEBUG_STACK_FRAME_EX()
                {
                    FrameNumber = (uint)frames.Count,
                    FrameOffset = stackFrame.AddrFrame.Offset,
                    FuncTableEntry = (ulong)stackFrame.FuncTableEntry.ToInt64(),
                    InlineFrameContext = stackFrame.InlineFrameContext,
                    InstructionOffset = stackFrame.AddrPC.Offset,
                    Params = null,
                    Reserved = null,
                    Reserved1 = 0,
                    ReturnOffset = stackFrame.AddrReturn.Offset,
                    StackOffset = stackFrame.AddrStack.Offset,
                    Virtual = stackFrame.Virtual,
                });
                contexts.Add(ThreadContext.PtrToStructure(thread.Process, contextAddress));
            }
            return new StackTrace(thread, frames.ToArray(), contexts.ToArray());
        }
#endif

        /// <summary>
        /// Gets the stack trace from the specified context.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="contextAddress">The context address.</param>
        /// <param name="contextSize">Size of the context.</param>
        /// <returns></returns>
        private StackTrace GetStackTraceFromContext(Thread thread, IntPtr contextAddress, uint contextSize)
        {
#if false
            if (thread.Process.DumpFileMemoryReader != null)
            {
                if (contextAddress == IntPtr.Zero)
                {
                    using (ThreadSwitcher switcher = new ThreadSwitcher(StateCache, thread))
                    using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(thread.Process, 1))
                    {
                        Advanced.GetThreadContext(threadContextBuffer.Pointer, (uint)(threadContextBuffer.Count * threadContextBuffer.Size));
                        return ReadStackTraceFromContext(thread, threadContextBuffer.Pointer);
                    }
                }
                else
                {
                    return ReadStackTraceFromContext(thread, contextAddress);
                }
            }
#endif

            const int MaxCallStack = 10240;
            using (ThreadSwitcher switcher = new ThreadSwitcher(StateCache, thread))
            using (MarshalArrayReader<_DEBUG_STACK_FRAME_EX> frameBuffer = new RegularMarshalArrayReader<_DEBUG_STACK_FRAME_EX>(MaxCallStack))
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(thread.Process, MaxCallStack))
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
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadAnsiString(Process process, ulong address, int length = -1)
        {
            try
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
                {
                    bool readAll = true;

                    if (length < 0)
                    {
                        readAll = false;
                        length = (int)Constants.MaxStringReadLength;
                    }

                    uint byteLength;
                    StringBuilder sb = new StringBuilder(length);


                    if (readAll)
                    {
                        while (length > 0)
                        {
                            StringBuilder temp = new StringBuilder(length);

                            DataSpaces.ReadMultiByteStringVirtual(address, (uint)temp.Capacity, temp, (uint)temp.Capacity, out byteLength);
                            sb.Append(temp);
                            length -= (int)byteLength;
                            if (length > 0)
                            {
                                address += byteLength;
                                sb.Append('\0');
                            }
                        }
                    }
                    else
                    {
                        DataSpaces.ReadMultiByteStringVirtual(address, (uint)sb.Capacity, sb, (uint)sb.Capacity, out byteLength);
                    }

                    return sb.ToString();
                }
            }
            catch (Exception)
            {
                throw new InvalidMemoryAddressException(address);
            }
        }

        /// <summary>
        /// Reads the unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadUnicodeString(Process process, ulong address, int length = -1)
        {
            try
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
                {
                    bool readAll = true;

                    if (length < 0)
                    {
                        readAll = false;
                        length = (int)Constants.MaxStringReadLength;
                    }

                    uint byteLength = 0;
                    StringBuilder sb = new StringBuilder(length);

                    if (readAll)
                    {
                        while (length > 0)
                        {
                            StringBuilder temp = new StringBuilder(length);

                            DataSpaces.ReadUnicodeStringVirtualWide(address, (uint)temp.Capacity * 2, temp, (uint)temp.Capacity, out byteLength);
                            sb.Append(temp);
                            length -= (int)byteLength / 2;
                            if (length > 0)
                            {
                                address += byteLength;
                                sb.Append('\0');
                            }
                        }
                    }
                    else
                    {
                        DataSpaces.ReadUnicodeStringVirtualWide(address, (uint)sb.Capacity * 2, sb, (uint)sb.Capacity, out byteLength);
                    }

                    return sb.ToString();
                }
            }
            catch (Exception)
            {
                throw new InvalidMemoryAddressException(address);
            }
        }

        /// <summary>
        /// Reads the wide unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length.</param>
        public string ReadWideUnicodeString(Process process, ulong address, int length = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates new instance of default symbol provider.
        /// </summary>
        public ISymbolProvider CreateDefaultSymbolProvider()
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

        /// <summary>
        /// Finds memory range where the specified address belongs to.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="regionSize">Size of the region.</param>
        public void QueryVirtual(Process process, ulong address, out ulong baseAddress, out ulong regionSize)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                var memoryInfo = DataSpaces.QueryVirtual(address);

                baseAddress = memoryInfo.BaseAddress;
                regionSize = memoryInfo.RegionSize;
            }
        }

        /// <summary>
        /// Gets the all memory regions available in the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>Array of <see cref="MemoryRegion"/> objects available in the specified process</returns>
        public MemoryRegion[] GetMemoryRegions(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                List<MemoryRegion> regionsList = new List<MemoryRegion>();
                ulong currentAddress = 0, baseAddress, regionSize;

                try
                {
                    while (true)
                    {
                        currentAddress = DataSpaces.GetNextDifferentlyValidOffsetVirtual(currentAddress);
                        if (currentAddress == 0)
                        {
                            break;
                        }

                        QueryVirtual(process, currentAddress, out baseAddress, out regionSize);

                        // Merge consecutive regions
                        if (regionsList.Count > 0 && regionsList[regionsList.Count - 1].MemoryEnd == baseAddress)
                            regionsList[regionsList.Count - 1] = new MemoryRegion { BaseAddress = regionsList[regionsList.Count - 1].BaseAddress, MemoryEnd = baseAddress + regionSize };
                        else
                            regionsList.Add(new MemoryRegion { BaseAddress = baseAddress, RegionSize = regionSize });
                        currentAddress = baseAddress + regionSize - 1;
                    }
                }
                catch
                {
                }

                return regionsList.ToArray();
            }
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
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, module.Process))
            {
                uint index, bufferSize;
                ulong baseAddress;

                Symbols.GetModuleByOffset(module.Address, 0, out index, out baseAddress);
                Symbols.GetModuleVersionInformation(index, module.Address, "\\", IntPtr.Zero, 0, out bufferSize);

                IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);

                try
                {
                    Symbols.GetModuleVersionInformation(index, module.Address, "\\", buffer, bufferSize, out bufferSize);
                    minor = (ushort)Marshal.ReadInt16(buffer, 8);
                    major = (ushort)Marshal.ReadInt16(buffer, 10);
                    patch = (ushort)Marshal.ReadInt16(buffer, 12);
                    revision = (ushort)Marshal.ReadInt16(buffer, 14);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified process is being debugged as minidump without heap.
        /// </summary>
        /// <param name="process">The process.</param>
        public bool IsMinidump(Process process)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(StateCache, process))
            {
                uint cls, qual;

                Control.GetDebuggeeType(out cls, out qual);
                if (qual == (uint)Defines.DebugDumpSmall)
                {
                    uint flags = Control.GetDumpFormatFlags();

                    return (flags & (uint)Defines.DebugFormatUserSmallFullMemory) == 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets Lasts Event, most exception or event.
        /// </summary>
        /// <returns></returns>
        public DebugEventInfo GetLastEventInfo()
        {
            uint type;
            uint processId;
            uint threadId;
            uint extraInfoUsed;
            uint descriptionSize;

            //  Collect EventSize
            StringBuilder description = new StringBuilder();

            Control.GetLastEventInformation(
                out type,
                out processId,
                out threadId,
                IntPtr.Zero,
                0,
                out extraInfoUsed,
                description,
                0,
                out descriptionSize);

            //  Allocate structures are prepare string.
            description.Capacity = (int)descriptionSize;

            DEBUG_LAST_EVENT_INFO debugLastEventInfo = new DEBUG_LAST_EVENT_INFO();
            extraInfoUsed = Math.Min(extraInfoUsed, (uint)Marshal.SizeOf(typeof(DEBUG_LAST_EVENT_INFO)));
            GCHandle handle = GCHandle.Alloc(debugLastEventInfo, GCHandleType.Pinned);

            byte[] eventExtraInfo = new byte[extraInfoUsed];

            try
            {
                Control.GetLastEventInformation(
                    out type,
                    out processId,
                    out threadId,
                    handle.AddrOfPinnedObject(),
                    extraInfoUsed,
                    out extraInfoUsed,
                    description,
                    descriptionSize,
                    out descriptionSize);

                Marshal.Copy(handle.AddrOfPinnedObject(), eventExtraInfo, 0, (int)extraInfoUsed);

                return new DebugEventInfo
                {
                    Type = (DebugEvent)type,
                    Description = description.ToString(),
                    Process = Process.All.First(r => r.Id == processId),
                    Thread = Thread.All.First(r => r.Id == threadId),
                    EventExtraInfo = eventExtraInfo
                };
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Gets the timestamp and size of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        public Tuple<DateTime, ulong> GetModuleTimestampAndSize(Module module)
        {
            ulong baseAddress = module.Address;

            using (var moduleParameters = new RegularMarshalArrayReader<DEBUG_MODULE_PARAMETERS>(1))
            {
                DateTime timestamp;
                ulong size;

                Symbols.GetModuleParameters(1, ref baseAddress, 0, moduleParameters.Pointer);
                var mp = moduleParameters.Elements.First();
                size = mp.Size;
                timestamp = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(mp.TimeDateStamp);
                return Tuple.Create(timestamp, size);
            }
        }

        /// <summary>
        /// Releases the process that is being debugged.
        /// </summary>
        public void ContinueExecution(Process process)
        {
            using (var processSwitcher = new ProcessSwitcher(StateCache, process))
            {
                DebuggeeFlowController flowControler = debugeeFlowControllers[process.Id];
                flowControler.DebugStatusBreak.WaitOne();
                Control.Execute(0, "g", 0);
            }
        }

        /// <summary>
        /// Breaks the process that is being debugged.
        /// </summary>
        public void BreakExecution(Process process)
        {
            using (var processSwitcher = new ProcessSwitcher(StateCache, process))
            {
                DebuggeeFlowController flowControler = debugeeFlowControllers[process.Id];
                flowControler.DebugStatusBreak.Reset();
                Control.SetInterrupt(0);
                flowControler.DebugStatusBreak.WaitOne();

                // Drop the cache.
                // TODO: When support for debugging multiple processes is added.
                //       this needs to clear the cache of all the processes.
                //
                process.InvalidateProcessCache();
            }
        }

        /// <summary>
        /// Terminates the process that is being debugged and ends the session.
        /// </summary>
        public void Terminate(Process process)
        {
            Client.EndSession((uint)Defines.DebugEndActiveTerminate);

            DebuggeeFlowController flowControler;
            debugeeFlowControllers.RemoveEntry(process.Id, out flowControler);

            // Release any threads that are waiting.
            //
            flowControler.DebugStatusGo.Set();
            flowControler.DebugStatusBreak.Set();

            flowControler.WaitForDebuggerLoopToExit();
        }

        #region Native methods
#pragma warning disable CS0649
        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It is called when StackWalk64 needs to read memory from the address space of the process.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="lpBaseAddress">The base address of the memory to be read.</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the memory to be read.</param>
        /// <param name="nSize">The size of the memory to be read, in bytes.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes actually read.</param>
        /// <returns></returns>
        private delegate bool ReadProcessMemoryProc64(IntPtr hProcess, ulong lpBaseAddress, IntPtr lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It provides access to the run-time function table for the process.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="AddrBase">The address of the instruction to be located.</param>
        /// <returns>The function returns a pointer to the run-time function table. On an x86 computer, this is a pointer to an FPO_DATA structure. On an Alpha computer, this is a pointer to an IMAGE_FUNCTION_ENTRY structure.</returns>
        private delegate IntPtr FunctionTableAccessProc64(IntPtr hProcess, ulong AddrBase);

        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It is called when StackWalkEx needs a module base address for a given virtual address.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="Address">An address within the module image to be located.</param>
        /// <returns>The function returns the base address of the module.</returns>
        private delegate ulong GetModuleBaseProc64(IntPtr hProcess, ulong Address);

        /// <summary>
        /// An application-defined callback function used with the StackWalkEx function. It provides address translation for 16-bit addresses.
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated.</param>
        /// <param name="hThread">A handle to the thread for which the stack trace is generated.</param>
        /// <param name="lpaddr">An address to be translated.</param>
        /// <returns>The function returns the translated address.</returns>
        private delegate ulong TranslateAddressProc64(IntPtr hProcess, IntPtr hThread, IntPtr lpaddr);

        /// <summary>
        /// Represents an address. It is used in the STACKFRAME_EX structure.
        /// </summary>
        private struct ADDRESS64
        {
            /// <summary>
            /// The offset into the segment, or a 32-bit virtual address. The interpretation of this value depends on the value contained in the Mode member.
            /// </summary>
            public ulong Offset;

            /// <summary>
            /// The segment number. This value is used only for 16-bit addressing.
            /// </summary>
            public ushort Segment;

            /// <summary>
            /// The addressing mode. This member can be one of the following values.
            /// </summary>
            public uint Mode;
        }

        /// <summary>
        /// Represents an extended stack frame.
        /// </summary>
        private struct STACKFRAME_EX
        {
            /// <summary>
            /// An ADDRESS64 structure that specifies the program counter.
            /// x86:  The program counter is EIP.
            /// Intel Itanium:  The program counter is StIIP.
            /// x64:  The program counter is RIP.
            /// </summary>
            public ADDRESS64 AddrPC;

            /// <summary>
            /// An ADDRESS64 structure that specifies the return address.
            /// </summary>
            public ADDRESS64 AddrReturn;

            /// <summary>
            /// An ADDRESS64 structure that specifies the frame pointer.
            /// x86:  The frame pointer is EBP.
            /// Intel Itanium:  There is no frame pointer, but AddrBStore is used.
            /// x64:  The frame pointer is RBP or RDI.This value is not always used.
            /// </summary>
            public ADDRESS64 AddrFrame;

            /// <summary>
            /// An ADDRESS64 structure that specifies the stack pointer.
            /// x86:  The stack pointer is ESP.
            /// Intel Itanium:  The stack pointer is SP.
            /// x64:  The stack pointer is RSP.
            /// </summary>
            public ADDRESS64 AddrStack;

            /// <summary>
            /// Intel Itanium:  An ADDRESS64 structure that specifies the backing store (RsBSP).
            /// </summary>
            public ADDRESS64 AddrBStore;

            /// <summary>
            /// On x86 computers, this member is an FPO_DATA structure. If there is no function table entry, this member is NULL.
            /// </summary>
            public IntPtr FuncTableEntry;

            /// <summary>
            /// The possible arguments to the function.
            /// </summary>
            public ulong Params0;

            /// <summary>
            /// The possible arguments to the function.
            /// </summary>
            public ulong Params1;

            /// <summary>
            /// The possible arguments to the function.
            /// </summary>
            public ulong Params2;

            /// <summary>
            /// The possible arguments to the function.
            /// </summary>
            public ulong Params3;

            /// <summary>
            /// This member is TRUE if this is a WOW far call.
            /// </summary>
            public int Far;

            /// <summary>
            /// This member is TRUE if this is a virtual frame.
            /// </summary>
            public int Virtual;

            /// <summary>
            /// This member is used internally by the StackWalkEx function.
            /// </summary>
            public ulong Reserved0;

            /// <summary>
            /// This member is used internally by the StackWalkEx function.
            /// </summary>
            public ulong Reserved1;

            /// <summary>
            /// This member is used internally by the StackWalkEx function.
            /// </summary>
            public ulong Reserved2;

            /// <summary>
            /// A KDHELP64 structure that specifies helper data for walking kernel callback frames.
            /// </summary>
            public KDHELP64 KdHelp;

            /// <summary>
            /// Set to sizeof(STACKFRAME_EX).
            /// </summary>
            public uint StackFrameSize;

            /// <summary>
            /// Specifies the type of the inline frame context.
            /// INLINE_FRAME_CONTEXT_INIT(0)
            /// INLINE_FRAME_CONTEXT_IGNORE(0xffffffff)
            /// </summary>
            public uint InlineFrameContext;
        }

        /// <summary>
        /// Information that is used by kernel debuggers to trace through user-mode callbacks in a thread's kernel stack.
        /// </summary>
        private struct KDHELP64
        {
            /// <summary>
            /// The address of the kernel thread object, as provided in the WAIT_STATE_CHANGE packet.
            /// </summary>
            public ulong Thread;

            /// <summary>
            /// The offset in the thread object to the pointer to the current callback frame in the kernel stack.
            /// </summary>
            public uint ThCallbackStack;

            /// <summary>
            /// Intel Itanium:  The offset in the thread object to a pointer to the current callback backing store frame in the kernel stack.
            /// </summary>
            public uint ThCallbackBStore;

            /// <summary>
            /// The address of the next callback frame.
            /// </summary>
            public uint NextCallback;

            /// <summary>
            /// The address of the saved frame pointer, if applicable.
            /// </summary>
            public uint FramePointer;

            /// <summary>
            /// The address of the kernel function that calls out to user mode.
            /// </summary>
            public ulong KiCallUserMode;

            /// <summary>
            /// The address of the user-mode dispatcher function.
            /// </summary>
            public ulong KeUserCallbackDispatcher;

            /// <summary>
            /// The lowest kernel-mode address.
            /// </summary>
            public ulong SystemRangeStart;

            /// <summary>
            /// The address of the user-mode exception dispatcher function.
            /// </summary>
            public ulong KiUserExceptionDispatcher;

            /// <summary>
            /// The address of the stack base.
            /// </summary>
            public ulong StackBase;

            /// <summary>
            /// The stack limit.
            /// </summary>
            public ulong StackLimit;

            /// <summary>
            /// This member is reserved for use by the operating system.
            /// </summary>
            public ulong Reserved0;

            /// <summary>
            /// This member is reserved for use by the operating system.
            /// </summary>
            public ulong Reserved1;

            /// <summary>
            /// This member is reserved for use by the operating system.
            /// </summary>
            public ulong Reserved2;

            /// <summary>
            /// This member is reserved for use by the operating system.
            /// </summary>
            public ulong Reserved3;

            /// <summary>
            /// This member is reserved for use by the operating system.
            /// </summary>
            public ulong Reserved4;
        }

        /// <summary>
        /// Obtains a stack trace.
        /// </summary>
        /// <param name="MachineType">The architecture type of the computer for which the stack trace is generated.</param>
        /// <param name="hProcess">A handle to the process for which the stack trace is generated. If the caller supplies a valid callback pointer for the ReadMemoryRoutine parameter, then this value does not have to be a valid process handle. It can be a token that is unique and consistently the same for all calls to the StackWalkEx function. If the symbol handler is used with StackWalkEx, use the same process handles for the calls to each function.</param>
        /// <param name="hThread">A handle to the thread for which the stack trace is generated.If the caller supplies a valid callback pointer for the ReadMemoryRoutine parameter, then this value does not have to be a valid thread handle. It can be a token that is unique and consistently the same for all calls to the StackWalkEx function.</param>
        /// <param name="StackFrame">A pointer to a STACKFRAME_EX structure. This structure receives information for the next frame, if the function call succeeds.</param>
        /// <param name="ContextRecord">A pointer to a CONTEXT structure.This parameter is required only when the MachineType parameter is not IMAGE_FILE_MACHINE_I386. However, it is recommended that this parameter contain a valid context record. This allows StackWalkEx to handle a greater variety of situations.
        /// This context may be modified, so do not pass a context record that should not be modified.</param>
        /// <param name="ReadMemoryRoutine">A callback routine that provides memory read services.When the StackWalkEx function needs to read memory from the process's address space, the ReadProcessMemoryProc64 callback is used.
        /// If this parameter is NULL, then the function uses a default routine.In this case, the hProcess parameter must be a valid process handle.
        /// If this parameter is not NULL, the application should implement and register a symbol handler callback function that handles CBA_READ_MEMORY.</param>
        /// <param name="FunctionTableAccessRoutine">A callback routine that provides access to the run-time function table for the process.This parameter is required because the StackWalkEx function does not have access to the process's run-time function table. For more information, see FunctionTableAccessProc64.
        /// The symbol handler provides functions that load and access the run-time table. If these functions are used, then SymFunctionTableAccess64 can be passed as a valid parameter.</param>
        /// <param name="GetModuleBaseRoutine">A callback routine that provides a module base for any given virtual address.This parameter is required.For more information, see GetModuleBaseProc64.
        /// The symbol handler provides functions that load and maintain module information. If these functions are used, then SymGetModuleBase64 can be passed as a valid parameter.</param>
        /// <param name="TranslateAddress">A callback routine that provides address translation for 16-bit addresses.For more information, see TranslateAddressProc64.
        /// Most callers of StackWalkEx can safely pass NULL for this parameter.</param>
        /// <param name="Flags">A combination of zero or more flags.
        /// SYM_STKWALK_DEFAULT (0)
        /// SYM_STKWALK_FORCE_FRAMEPTR(1)
        /// </param>
        /// <returns>If the function succeeds, the return value is TRUE.</returns>
        [DllImport("dbghelp.dll", SetLastError = true)]
        private static extern bool StackWalkEx(
            ImageFileMachine MachineType,
            IntPtr hProcess,
            IntPtr hThread,
            ref STACKFRAME_EX StackFrame,
            IntPtr ContextRecord,
            ReadProcessMemoryProc64 ReadMemoryRoutine,
            FunctionTableAccessProc64 FunctionTableAccessRoutine,
            GetModuleBaseProc64 GetModuleBaseRoutine,
            TranslateAddressProc64 TranslateAddress,
            uint Flags);

        /// <summary>
        /// Retrieves the function table entry for the specified address.
        /// </summary>
        /// <param name="hProcess">A handle to the process that was originally passed to the SymInitialize function.</param>
        /// <param name="AddrBase">The base address for which function table information is required.</param>
        /// <param name="ReadMemoryRoutine">A callback routine that provides memory read services.When the StackWalkEx function needs to read memory from the process's address space, the ReadProcessMemoryProc64 callback is used.
        /// If this parameter is NULL, then the function uses a default routine.In this case, the hProcess parameter must be a valid process handle.
        /// If this parameter is not NULL, the application should implement and register a symbol handler callback function that handles CBA_READ_MEMORY.</param>
        /// <param name="GetModuleBaseRoutine">A callback routine that provides a module base for any given virtual address.This parameter is required.For more information, see GetModuleBaseProc64.
        /// The symbol handler provides functions that load and maintain module information. If these functions are used, then SymGetModuleBase64 can be passed as a valid parameter.</param>
        /// <returns>If the function succeeds, the return value is a pointer to the function table entry.</returns>
        [DllImport("dbghelp.dll", SetLastError = true)]
        private static extern IntPtr SymFunctionTableAccess64AccessRoutines(
            IntPtr hProcess,
            ulong AddrBase,
            ReadProcessMemoryProc64 ReadMemoryRoutine,
            GetModuleBaseProc64 GetModuleBaseRoutine);
#pragma warning restore CS0649
        #endregion
    }
}