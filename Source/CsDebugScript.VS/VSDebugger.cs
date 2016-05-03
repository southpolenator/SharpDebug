using CsDebugScript.Engine.Marshaling;
using CsDebugScript.Engine.Utility;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Symbols;
using System;
using System.Linq;

namespace CsDebugScript.VS
{
    internal class VSDebugger : Engine.Debuggers.IDebuggerEngine
    {
        /// <summary>
        /// The cached DKM processes
        /// </summary>
        private SimpleCache<DkmProcess[]> dkmProcesses;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSDebugger"/> class.
        /// </summary>
        public VSDebugger()
        {
            dkmProcesses = SimpleCache.Create(() => DkmProcess.GetProcesses());
        }

        /// <summary>
        /// Gets the cached DKM processes.
        /// </summary>
        internal DkmProcess[] DkmProcesses
        {
            get
            {
                return dkmProcesses.Value;
            }
        }

        /// <summary>
        /// Converts Process into DkmProcess.
        /// </summary>
        /// <param name="process">The process.</param>
        private DkmProcess ConvertProcess(Process process)
        {
            return DkmProcesses[process.Id];
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        private static string GetModuleName(DkmModuleInstance module)
        {
            return System.IO.Path.GetFileNameWithoutExtension(module.Name);
        }

        /// <summary>
        /// Converts Module into DkmModuleInstance.
        /// </summary>
        /// <param name="module">The module.</param>
        private DkmModuleInstance ConvertModule(Module module)
        {
            return ConvertProcess(module.Process).GetRuntimeInstances().SelectMany(r => r.GetModuleInstances()).Where(m => m.BaseAddress == module.Address).Single();
        }

        /// <summary>
        /// Converts Thread into DkmThread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        private DkmThread ConvertThread(Thread thread)
        {
            DkmProcess process = ConvertProcess(thread.Process);

            return process.GetThreads().Where(t => t.SystemPart.Id == thread.SystemId).Single();
        }

        public bool IsLiveDebugging
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Engine.SymbolProviders.ISymbolProvider CreateDefaultSymbolProvider()
        {
            throw new NotImplementedException();
        }

        public Engine.SymbolProviders.ISymbolProviderModule CreateDefaultSymbolProviderModule()
        {
            throw new NotImplementedException();
        }

        public ulong FindPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all processes currently being debugged.
        /// </summary>
        public Process[] GetAllProcesses()
        {
            Process[] processes = new Process[DkmProcesses.Length];

            for (int i = 0; i < DkmProcesses.Length; i++)
            {
                processes[i] = Engine.GlobalCache.Processes[(uint)i];
                processes[i].systemId.Value = (uint)DkmProcesses[i].LivePart.Id;
            }

            return processes;
        }

        /// <summary>
        /// Gets the current process.
        /// </summary>
        public Process GetCurrentProcess()
        {
            return Process.All.Where(p => p.SystemId == VSContext.DTE.Debugger.CurrentProcess.ProcessID).Single();
        }

        /// <summary>
        /// Gets the current thread of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread GetProcessCurrentThread(Process process)
        {
            if (GetCurrentProcess() == process)
            {
                return process.Threads.Where(t => t.SystemId == VSContext.DTE.Debugger.CurrentThread.ID).Single();
            }
            else
            {
                return process.Threads[0];
            }
        }

        public StackFrame GetThreadCurrentStackFrame(Thread thread)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the address of the module loaded into specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="moduleName">Name of the module.</param>
        public ulong GetModuleAddress(Process process, string moduleName)
        {
            DkmProcess dkmProcess = ConvertProcess(process);

            moduleName = moduleName.ToLower();
            return dkmProcess.GetRuntimeInstances().SelectMany(r => r.GetModuleInstances()).Where(m => GetModuleName(m).ToLower() == moduleName).Single().BaseAddress;
        }

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleImageName(Module module)
        {
            DkmModuleInstance dkmModule = ConvertModule(module);

            return dkmModule.FullName;
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
            DkmModuleInstance dkmModule = ConvertModule(module);

            return GetModuleName(dkmModule);
        }

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        /// <param name="module">The module.</param>
        public string GetModuleSymbolFile(Module module)
        {
            DkmModuleInstance dkmModule = ConvertModule(module);
            DkmSymbolFileId symbolFileId = dkmModule.SymbolFileId;

            if (symbolFileId.TagValue == DkmSymbolFileId.Tag.PdbFileId)
            {
                DkmPdbFileId pdbFileId = (DkmPdbFileId)symbolFileId;

                return pdbFileId.PdbName;
            }

            return dkmModule.FullName;
        }

        /// <summary>
        /// Gets the timestamp and size of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        public Tuple<DateTime, ulong> GetModuleTimestampAndSize(Module module)
        {
            DkmModuleInstance dkmModule = ConvertModule(module);

            return Tuple.Create(DateTime.FromFileTimeUtc((long)dkmModule.TimeDateStamp), (ulong)dkmModule.Size);
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
            DkmModuleInstance dkmModule = ConvertModule(module);

            if (dkmModule.Version != null)
            {
                major = (int)(dkmModule.Version.ProductVersionMS / 65536);
                minor = (int)(dkmModule.Version.ProductVersionMS % 65536);
                revision = (int)(dkmModule.Version.ProductVersionLS / 65536);
                patch = (int)(dkmModule.Version.ProductVersionLS % 65536);
            }
            else
            {
                major = minor = revision = patch = 0;
            }
        }

        public Engine.Native.ImageFileMachine GetProcessActualProcessorType(Process process)
        {
            throw new NotImplementedException();
        }

        public string GetProcessDumpFileName(Process process)
        {
            throw new NotImplementedException();
        }

        public Engine.Native.ImageFileMachine GetProcessEffectiveProcessorType(Process process)
        {
            throw new NotImplementedException();
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
            DkmProcess dkmProcess = ConvertProcess(process);

            return dkmProcess.Path;
        }

        /// <summary>
        /// Gets all modules of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Module[] GetProcessModules(Process process)
        {
            DkmProcess dkmProcess = ConvertProcess(process);

            return dkmProcess.GetRuntimeInstances().SelectMany(r => r.GetModuleInstances()).Select(m => process.ModulesById[m.BaseAddress]).ToArray();
        }

        /// <summary>
        /// Gets the system identifier of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public uint GetProcessSystemId(Process process)
        {
            DkmProcess dkmProcess = ConvertProcess(process);

            return (uint)dkmProcess.LivePart.Id;
        }

        /// <summary>
        /// Gets all threads of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public Thread[] GetProcessThreads(Process process)
        {
            DkmProcess dkmProcess = ConvertProcess(process);
            DkmThread[] dkmThreads = dkmProcess.GetThreads();
            Thread[] threads = new Thread[dkmThreads.Length];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread((uint)i, (uint)dkmThreads[i].SystemPart.Id, process);
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
        public unsafe ThreadContext GetThreadContext(Thread thread)
        {
            DkmThread dkmThread = ConvertThread(thread);

            using (MarshalArrayReader<ThreadContext> threadContextBuffer = ThreadContext.CreateArrayMarshaler(thread.Process, 1))
            {
                dkmThread.GetContext(-1, threadContextBuffer.Pointer.ToPointer(), threadContextBuffer.Count * threadContextBuffer.Size);

                return threadContextBuffer.Elements.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the environment block address of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        public ulong GetThreadEnvironmentBlockAddress(Thread thread)
        {
            DkmThread dkmThread = ConvertThread(thread);

            return dkmThread.TebAddress;
        }

        public StackTrace GetThreadStackTrace(Thread thread)
        {
            DkmThread dkmThread = ConvertThread(thread);

            throw new NotImplementedException();
        }

        public bool IsMinidump(Process process)
        {
            throw new NotImplementedException();
        }

        public void QueryVirtual(ulong address, out ulong baseAddress, out ulong regionSize)
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
            DkmProcess dkmProcess = ConvertProcess(process);
            byte[] bytes = new byte[size];

            dkmProcess.ReadMemory(address, DkmReadMemoryFlags.None, bytes);
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
            DkmProcess dkmProcess = ConvertProcess(process);

            return System.Text.ASCIIEncoding.Default.GetString(dkmProcess.ReadMemoryString(address, DkmReadMemoryFlags.None, 1, length));
        }

        /// <summary>
        /// Reads the unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadUnicodeString(Process process, ulong address, int length = -1)
        {
            DkmProcess dkmProcess = ConvertProcess(process);

            return System.Text.UnicodeEncoding.Default.GetString(dkmProcess.ReadMemoryString(address, DkmReadMemoryFlags.None, 2, length));
        }

        /// <summary>
        /// Sets the current process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <exception cref="System.ArgumentException">Process wasn't found</exception>
        public void SetCurrentProcess(Process process)
        {
            if (VSContext.DTE.Debugger.CurrentProcess.ProcessID != process.SystemId)
            {
                foreach (EnvDTE.Process vsProcess in VSContext.DTE.Debugger.DebuggedProcesses)
                {
                    if (process.SystemId == vsProcess.ProcessID)
                    {
                        VSContext.DTE.Debugger.CurrentProcess = vsProcess;
                        return;
                    }
                }

                throw new ArgumentException("Process wasn't found", nameof(process));
            }
        }

        /// <summary>
        /// Sets the current thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <exception cref="System.ArgumentException">Thread wasn't found</exception>
        public void SetCurrentThread(Thread thread)
        {
            SetCurrentProcess(thread.Process);
            if (VSContext.DTE.Debugger.CurrentThread.ID != thread.SystemId)
            {
                foreach (EnvDTE.Program vsProgram in VSContext.DTE.Debugger.CurrentProcess.Programs)
                {
                    foreach (EnvDTE.Thread vsThread in vsProgram.Threads)
                    {
                        if (thread.SystemId == vsThread.ID)
                        {
                            VSContext.DTE.Debugger.CurrentThread = vsThread;
                            return;
                        }
                    }
                }

                throw new ArgumentException("Thread wasn't found", nameof(thread));
            }
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
            // TODO: This should update cache with new values. For now, just clear everything
        }

        #region Unsupported functionality
        /// <summary>
        /// The exception text for all functions that were intentionally not implemented.
        /// </summary>
        public const string NotImplementedExceptionText = "This function is not planned to be implemented for VS debugger.";

        /// <summary>
        /// Executes the specified command, but leaves its output visible to the user.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.NotImplementedException">This function is not planned to be implemented for VS debugger.</exception>
        public void Execute(string command, params object[] parameters)
        {
            throw new NotImplementedException(NotImplementedExceptionText);
        }

        /// <summary>
        /// Executes the action in redirected console output and error stream.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="System.NotImplementedException">This function is not planned to be implemented for VS debugger.</exception>
        public void ExecuteAction(Action action)
        {
            throw new NotImplementedException(NotImplementedExceptionText);
        }

        /// <summary>
        /// Reads the line from the debugger input.
        /// </summary>
        /// <exception cref="System.NotImplementedException">This function is not planned to be implemented for VS debugger.</exception>
        public string ReadInput()
        {
            throw new NotImplementedException(NotImplementedExceptionText);
        }
        #endregion
    }
}
