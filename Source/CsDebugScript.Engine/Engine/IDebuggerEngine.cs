﻿using CsDebugScript.Engine.Utility;
using System;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Debugger engine interface that provides functionality for controlling debugger engine
    /// </summary>
    public interface IDebuggerEngine
    {
        /// <summary>
        /// Gets a value indicating whether debugger is currently in live debugging.
        /// </summary>
        /// <value>
        /// <c>true</c> if debugger is currently in live debugging; otherwise, <c>false</c>.
        /// </value>
        bool IsLiveDebugging { get; }

        /// <summary>
        /// Ends current debugging session and disposes used memory.
        /// This method is being called when new debugger engine is loaded with <see cref="Context.InitializeDebugger(IDebuggerEngine, ISymbolProvider)"/>.
        /// </summary>
        void EndSession();

        /// <summary>
        /// Gets instance of default symbol provider.
        /// </summary>
        ISymbolProvider GetDefaultSymbolProvider();

        /// <summary>
        /// Gets the dump file memory reader of the specified process if it is debugged from a dump.
        /// </summary>
        /// <param name="process">The process.</param>
        DumpFileMemoryReader GetDumpFileMemoryReader(Process process);

        /// <summary>
        /// Reads the memory from the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="size">The buffer size.</param>
        /// <returns>Buffer containing read memory</returns>
        MemoryBuffer ReadMemory(Process process, ulong address, uint size);

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
        ulong FindPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false);

        /// <summary>
        /// Gets the module name. This is usually just the file name without the extension. In a few cases,
        /// the module name differs significantly from the file name.
        /// </summary>
        /// <param name="module">The module.</param>
        string GetModuleName(Module module);

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        /// <param name="module">The module.</param>
        string GetModuleImageName(Module module);

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        /// <param name="module">The module.</param>
        string GetModuleLoadedImage(Module module);

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        /// <param name="module">The module.</param>
        string GetModuleSymbolFile(Module module);

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is null. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        /// <param name="module">The module.</param>
        string GetModuleMappedImage(Module module);

        /// <summary>
        /// Gets the current stack frame of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        StackFrame GetThreadCurrentStackFrame(Thread thread);

        /// <summary>
        /// Sets the current stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        void SetCurrentStackFrame(StackFrame stackFrame);

        /// <summary>
        /// Sets the current thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        void SetCurrentThread(Thread thread);

        /// <summary>
        /// Gets the environment block address of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        ulong GetThreadEnvironmentBlockAddress(Thread thread);

        /// <summary>
        /// Gets the thread context of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        ThreadContext GetThreadContext(Thread thread);

        /// <summary>
        /// Gets the stack trace of the specified thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        StackTrace GetThreadStackTrace(Thread thread);

        /// <summary>
        /// Gets the stack trace from the specified context.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="contextAddress">The context address.</param>
        /// <param name="contextSize">Size of the context. If 0 is specified, context size will be automatically calculated.</param>
        StackTrace GetStackTraceFromContext(Process process, ulong contextAddress, uint contextSize = 0);

        /// <summary>
        /// Gets the system identifier of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        uint GetProcessSystemId(Process process);

        /// <summary>
        /// Gets the up time of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        uint GetProcessUpTime(Process process);

        /// <summary>
        /// Gets the process environment block address of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        ulong GetProcessEnvironmentBlockAddress(Process process);

        /// <summary>
        /// Gets the executable name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        string GetProcessExecutableName(Process process);

        /// <summary>
        /// Gets the dump file name of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        string GetProcessDumpFileName(Process process);

        /// <summary>
        /// Gets the architecture type of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        ArchitectureType GetProcessArchitectureType(Process process);

        /// <summary>
        /// Gets all threads of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        Thread[] GetProcessThreads(Process process);

        /// <summary>
        /// Gets all modules of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        Module[] GetProcessModules(Process process);

        /// <summary>
        /// Gets the current process.
        /// </summary>
        Process GetCurrentProcess();

        /// <summary>
        /// Sets the current process.
        /// </summary>
        /// <param name="process">The process.</param>
        void SetCurrentProcess(Process process);

        /// <summary>
        /// Gets all processes currently being debugged.
        /// </summary>
        Process[] GetAllProcesses();

        /// <summary>
        /// Gets the current thread of the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        Thread GetProcessCurrentThread(Process process);

        /// <summary>
        /// Gets the address of the module loaded into specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="moduleName">Name of the module.</param>
        ulong GetModuleAddress(Process process, string moduleName);

        /// <summary>
        /// Gets the timestamp and size of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        Tuple<DateTime, ulong> GetModuleTimestampAndSize(Module module);

        /// <summary>
        /// Reads the ANSI string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        string ReadAnsiString(Process process, ulong address, int length = -1);

        /// <summary>
        /// Reads the unicode string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        string ReadUnicodeString(Process process, ulong address, int length = -1);

        /// <summary>
        /// Reads the wide unicode (4bytes) string.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        string ReadWideUnicodeString(Process process, ulong address, int length = -1);

        /// <summary>
        /// Finds memory range where the specified address belongs to.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="regionSize">Size of the region.</param>
        void QueryVirtual(Process process, ulong address, out ulong baseAddress, out ulong regionSize);

        /// <summary>
        /// Gets the all memory regions available in the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns>Array of <see cref="MemoryRegion"/> objects available in the specified process</returns>
        MemoryRegion[] GetMemoryRegions(Process process);

        /// <summary>
        /// Gets the module version.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="major">The version major number.</param>
        /// <param name="minor">The version minor number.</param>
        /// <param name="revision">The version revision number.</param>
        /// <param name="patch">The version patch number.</param>
        void GetModuleVersion(Module module, out int major, out int minor, out int revision, out int patch);

        /// <summary>
        /// Determines whether the specified process is being debugged as minidump without heap.
        /// </summary>
        /// <param name="process">The process.</param>
        bool IsMinidump(Process process);

        /// <summary>
        /// When doing live process debugging continues debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to be continued.</param>
        void ContinueExecution(Process process);

        /// <summary>
        /// When doing live process debugging breaks debugee execution of the specified process.
        /// </summary>
        /// <param name="process">Process to break.</param>
        void BreakExecution(Process process);

        /// <summary>
        /// Terminates given process.
        /// </summary>
        /// <param name="process">Process to Terminate.</param>
        void Terminate(Process process);

        /// <summary>
        /// Adds new breakpoint to the given process.
        /// </summary>
        /// <param name="process">Process.</param>
        /// <param name="breakpointSpec">Description of this breakpoint.</param>
        /// <returns>New breakpoint.</returns>
        IBreakpoint AddBreakpoint(Process process, BreakpointSpec breakpointSpec);
    }
}
