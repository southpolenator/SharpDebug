using CsDebugScript.CLR;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Marshaling;
using CsDebugScript.Engine.SymbolProviders;
using CsDebugScript.Engine.Utility;
using CsDebugScript.VS.CLR;
using DIA;
using SharpUtilities;
using System;
using System.IO;
using System.Linq;

namespace CsDebugScript.VS
{
    internal class VSDebugger : IDebuggerEngine, IDiaSessionProvider, IClrProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSDebugger"/> class.
        /// </summary>
        /// <param name="proxy">The Visual Studio debugger proxy running in Default AppDomain.</param>
        public VSDebugger(VSDebuggerProxy proxy)
        {
            Proxy = proxy;
            Context.SetUserTypeMetadata(ScriptCompiler.ExtractMetadata(new[]
                {
                    typeof(CsDebugScript.CommonUserTypes.NativeTypes.cv.Mat).Assembly, // CsDebugScript.CommonUserTypes.dll
                }));
        }

        /// <summary>
        /// The Visual Studio debugger proxy running in Default AppDomain.
        /// </summary>
        internal VSDebuggerProxy Proxy { get; private set; }

        /// <summary>
        /// Gets a value indicating whether debugger is currently in live debugging.
        /// </summary>
        /// <value>
        /// <c>true</c> if debugger is currently in live debugging; otherwise, <c>false</c>.
        /// </value>
        public bool IsLiveDebugging => Proxy.IsProcessLiveDebugging(Process.Current.Id);

        /// <summary>
        /// Ends current debugging session and disposes used memory.
        /// This method is being called when new debugger engine is loaded with <see cref="Context.InitializeDebugger(IDebuggerEngine, ISymbolProvider)"/>.
        /// </summary>
        public void EndSession()
        {
            // Do nothing
        }

        public Engine.ISymbolProvider GetDefaultSymbolProvider()
        {
            throw new NotImplementedException();
        }

        public ulong FindPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the dump file memory reader of the specified process if it is debugged from a dump.
        /// </summary>
        /// <param name="process">The process.</param>
        public DumpFileMemoryReader GetDumpFileMemoryReader(Process process)
        {
            string dumpFilename = process.DumpFileName;

            if (File.Exists(dumpFilename))
                return new WindowsDumpFileMemoryReader(dumpFilename);
            return null;
        }

        /// <summary>
        /// Gets all processes currently being debugged.
        /// </summary>
        public Process[] GetAllProcesses()
        {
            int[] processSystemIds = Proxy.GetAllProcesses();
            Process[] processes = new Process[processSystemIds.Length];

            for (int i = 0; i < processes.Length; i++)
            {
                processes[i] = Engine.GlobalCache.Processes[(uint)i];
                processes[i].systemId.Value = (uint)processSystemIds[i];
            }

            return processes;
        }

        /// <summary>
        /// Gets the current process.
        /// </summary>
        public Process GetCurrentProcess()
        {
            int currentProcessSystemId = Proxy.GetCurrentProcessSystemId();

            return Process.All.Where(p => p.SystemId == currentProcessSystemId).Single();
        }

        /// <summary>
        /// Gets the current thread of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread GetProcessCurrentThread(Process process)
        {
            if (GetCurrentProcess() == process)
            {
                int currentThreadSystemId = Proxy.GetCurrentThreadSystemId();

                return process.Threads.Where(t => t.SystemId == currentThreadSystemId).Single();
            }
            else
            {
                return process.Threads[0];
            }
        }

        /// <summary>
        /// Gets the current stack frame of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public StackFrame GetThreadCurrentStackFrame(Thread thread)
        {
            if (GetProcessCurrentThread(thread.Process) == thread)
            {
                StackTrace stackTrace = thread.StackTrace;
                int currentStackFrameNumber = Proxy.GetCurrentStackFrameNumber((int)thread.Id);

                return thread.StackTrace.Frames[currentStackFrameNumber];
            }
            else
            {
                return thread.StackTrace.Frames[0];
            }
        }

        /// <summary>
        /// Gets the address of the module loaded into specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="moduleName">Name of the module.</param>
        public ulong GetModuleAddress(Process process, string moduleName)
        {
            return Proxy.GetModuleAddress(process.Id, moduleName);
        }

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleImageName(Module module)
        {
            InitializeModuleId(module);
            return Proxy.GetModuleImageName(module.Id);
        }

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleLoadedImage(Module module)
        {
            return GetModuleImageName(module);
        }

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is null. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleMappedImage(Module module)
        {
            return GetModuleImageName(module);
        }

        /// <summary>
        /// Gets the module name. This is usually just the file name without the extension. In a few cases,
        /// the module name differs significantly from the file name.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleName(Module module)
        {
            InitializeModuleId(module);
            return Proxy.GetModuleName(module.Id);
        }

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleSymbolFile(Module module)
        {
            InitializeModuleId(module);
            return Proxy.GetModuleSymbolName(module.Id);
        }

        /// <summary>
        /// Gets the DIA session for the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>
        ///   <see cref="IDiaSession" /> if available, null otherwise.
        /// </returns>
        public IDiaSession GetModuleDiaSession(Module module)
        {
            InitializeModuleId(module);
            return Proxy.GetModuleDiaSession(module.Id) as IDiaSession;
        }

        /// <summary>
        /// Gets the timestamp and size of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        public Tuple<DateTime, ulong> GetModuleTimestampAndSize(Module module)
        {
            InitializeModuleId(module);
            return Proxy.GetModuleTimestampAndSize(module.Id);
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
            InitializeModuleId(module);
            Proxy.GetModuleVersion(module.Id, out major, out minor, out revision, out patch);
        }

        /// <summary>
        /// Gets the architecture type of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public ArchitectureType GetProcessArchitectureType(Process process)
        {
            return Proxy.GetProcessArchitectureType(process.Id);
        }

        /// <summary>
        /// Gets the dump file name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public string GetProcessDumpFileName(Process process)
        {
            return Proxy.GetProcessDumpFileName(process.Id);
        }

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
            return Proxy.GetProcessExecutableName(process.Id);
        }

        /// <summary>
        /// Gets all modules of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Module[] GetProcessModules(Process process)
        {
            Tuple<uint, ulong>[] moduleIdsAndBaseAddresses = Proxy.GetProcessModules(process.Id);
            Module[] modules = new Module[moduleIdsAndBaseAddresses.Length];

            for (int i = 0; i < modules.Length; i++)
            {
                modules[i] = process.ModulesById[moduleIdsAndBaseAddresses[i].Item2];
                modules[i].Id = moduleIdsAndBaseAddresses[i].Item1;
            }

            return modules;
        }

        /// <summary>
        /// Gets the system identifier of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public uint GetProcessSystemId(Process process)
        {
            return Proxy.GetProcessSystemId(process.Id);
        }

        /// <summary>
        /// Gets all threads of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread[] GetProcessThreads(Process process)
        {
            Tuple<uint, uint>[] threadIdsAndSystemIds = Proxy.GetProcessThreads(process.Id);
            Thread[] threads = new Thread[threadIdsAndSystemIds.Length];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(threadIdsAndSystemIds[i].Item1, threadIdsAndSystemIds[i].Item2, process);
            }

            return threads;
        }

        public uint GetProcessUpTime(Process process)
        {
            throw new NotImplementedException();
        }

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
            using (MarshalArrayReader<ThreadContext> threadContextBuffer = WindowsThreadContext.CreateArrayMarshaler(thread.Process, 1))
            {
                Proxy.GetThreadContext(thread.Id, threadContextBuffer.Pointer, threadContextBuffer.Count * threadContextBuffer.Size);

                return threadContextBuffer.Elements.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the environment block address of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ulong GetThreadEnvironmentBlockAddress(Thread thread)
        {
            return Proxy.GetThreadEnvironmentBlockAddress(thread.Id);
        }

        /// <summary>
        /// Gets the stack trace of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public StackTrace GetThreadStackTrace(Thread thread)
        {
            Tuple<ulong, ulong, ulong>[] framesData = Proxy.GetThreadStackTrace(thread.Id, thread.ThreadContext.Bytes);
            StackTrace stackTrace = new StackTrace(thread);
            StackFrame[] frames = new StackFrame[framesData.Length];

            stackTrace.Frames = frames;
            for (int i = 0; i < frames.Length; i++)
            {
                ulong instructionOffset = framesData[i].Item1, stackOffset = framesData[i].Item2, frameOffset = framesData[i].Item3;

                ThreadContext threadContext = new ThreadContext(instructionOffset, stackOffset, frameOffset, null);
                frames[i] = new StackFrame(stackTrace, threadContext)
                {
                    FrameNumber = (uint)i,
                    FrameOffset = frameOffset,
                    StackOffset = stackOffset,
                    InstructionOffset = instructionOffset,
                    ReturnOffset = 0, // Populated in the loop after this one
                    Virtual = false, // TODO:
                };
                threadContext.Registers = new RegistersAccess(thread.Id, frames[i].FrameNumber, Proxy);
            }

            for (int i = frames.Length - 2; i >= 0; i--)
            {
                frames[i].ReturnOffset = frames[i + 1].InstructionOffset;
            }

            return stackTrace;
        }

        /// <summary>
        /// Helper class for accessing register values
        /// </summary>
        private class RegistersAccess : IRegistersAccess
        {
            /// <summary>
            /// Thread identifier.
            /// </summary>
            private uint threadId;

            /// <summary>
            /// Stack frame identifier.
            /// </summary>
            private uint frameId;

            /// <summary>
            /// VS debugger proxy.
            /// </summary>
            private VSDebuggerProxy proxy;

            /// <summary>
            /// Initializes a new instance of the <see cref="RegisterAccess"/> class.
            /// </summary>
            /// <param name="threadId">Thread identifier.</param>
            /// <param name="frameId">Stack frame identifier.</param>
            /// <param name="proxy">VS debugger proxy.</param>
            public RegistersAccess(uint threadId, uint frameId, VSDebuggerProxy proxy)
            {
                this.threadId = threadId;
                this.frameId = frameId;
                this.proxy = proxy;
            }

            /// <summary>
            /// Gets register value.
            /// </summary>
            /// <param name="registerId">Register index.</param>
            /// <returns>Register value.</returns>
            public ulong GetRegisterValue(CV_HREG_e registerId)
            {
                return proxy.GetRegisterValue(threadId, frameId, (uint)registerId);
            }
        }

        public bool IsMinidump(Process process)
        {
            throw new NotImplementedException();
        }

        public void QueryVirtual(Process process, ulong address, out ulong baseAddress, out ulong regionSize)
        {
            throw new NotImplementedException();
        }

        public MemoryRegion[] GetMemoryRegions(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the memory from the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="size">The buffer size.</param>
        /// <returns>Buffer containing read memory</returns>
        public MemoryBuffer ReadMemory(Process process, ulong address, uint size)
        {
            byte[] bytes = Proxy.ReadMemory(process.Id, address, size);

            return new MemoryBuffer(bytes);
        }

        /// <summary>
        /// Reads the ANSI string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadAnsiString(Process process, ulong address, int length = -1)
        {
            return Proxy.ReadAnsiString(process.Id, address, length);
        }

        /// <summary>
        /// Reads the unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadUnicodeString(Process process, ulong address, int length = -1)
        {
            return Proxy.ReadUnicodeString(process.Id, address, length);
        }

        /// <summary>
        /// Reads the wide unicode (4bytes) string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadWideUnicodeString(Process process, ulong address, int length = -1)
        {
            return Proxy.ReadWideUnicodeString(process.Id, address, length);
        }

        /// <summary>
        /// Sets the current process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <exception cref="System.ArgumentException">Process wasn't found</exception>
        public void SetCurrentProcess(Process process)
        {
            Proxy.SetCurrentProcess(process.Id);
        }

        /// <summary>
        /// Sets the current thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <exception cref="System.ArgumentException">Thread wasn't found</exception>
        public void SetCurrentThread(Thread thread)
        {
            SetCurrentProcess(thread.Process);
            Proxy.SetCurrentThread(thread.Id);
        }

        public void SetCurrentStackFrame(StackFrame stackFrame)
        {
            SetCurrentThread(stackFrame.Thread);

            // TODO: Convert StackFrame to DTE.Frame
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the cache since something changed in debugger state.
        /// </summary>
        internal void UpdateCache()
        {
            // This should update cache with new values. For now, just clear everything
            Proxy.ClearCache();
            Engine.Context.ClearCache();
        }

        /// <summary>
        /// Initializes module id if it wasn't already.
        /// </summary>
        /// <param name="module">Module to be initialized.</param>
        private void InitializeModuleId(Module module)
        {
            if (module == null || module.Id != uint.MaxValue)
            {
                return;
            }

            module.Id = Proxy.GetModuleId(module.Process.Id, module.Address);
        }

        #region Unsupported functionality
        /// <summary>
        /// The exception text for all functions that were intentionally not implemented.
        /// </summary>
        public const string NotImplementedExceptionText = "This function is not planned to be implemented for VS debugger.";

        /// <summary>
        /// When doing live process debugging continues debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to be continued.</param>
        public void ContinueExecution(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When doing live process debugging breaks debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to break.</param>
        public void BreakExecution(Process process)
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

        /// <summary>
        /// Adds new breakpoint to the given process.
        /// </summary>
        /// <param name="process">Process.</param>
        /// <param name="breakpointSpec">Description of this breakpoint.</param>
        /// <returns>New breakpoint.</returns>
        public IBreakpoint AddBreakpoint(Process process, BreakpointSpec breakpointSpec)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IClrProvider
        /// <summary>
        /// Gets the CLR runtimes running in the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public IClrRuntime[] GetClrRuntimes(Process process)
        {
            Tuple<int, int, int, int, int>[] runtimeTuples = Proxy.GetClrRuntimes(process.Id);

            if (runtimeTuples == null)
                return new IClrRuntime[0];

            IClrRuntime[] runtimes = new IClrRuntime[runtimeTuples.Length];

            for (int i = 0; i < runtimeTuples.Length; i++)
                runtimes[i] = new VSClrRuntime(this, process, runtimeTuples[i].Item1, new ModuleVersion
                {
                    Major = runtimeTuples[i].Item2,
                    Minor = runtimeTuples[i].Item3,
                    Revision = runtimeTuples[i].Item4,
                    Patch = runtimeTuples[i].Item5,
                });
            return runtimes;
        }
        #endregion
    }
}
