using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.VS
{
    internal class VSDebugger : Engine.Debuggers.IDebuggerEngine
    {
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

        public void Execute(string command, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void ExecuteAction(Action action)
        {
            throw new NotImplementedException();
        }

        public ulong FindPatternInMemory(Process process, ulong memoryStart, ulong memoryEnd, byte[] pattern, int patternStart, int patternEnd, uint searchAlignment = 1, bool searchWritableMemoryOnly = false)
        {
            throw new NotImplementedException();
        }

        public Process[] GetAllProcesses()
        {
            throw new NotImplementedException();
        }

        public Process GetCurrentProcess()
        {
            throw new NotImplementedException();
        }

        public ulong GetModuleAddress(Process process, string moduleName)
        {
            throw new NotImplementedException();
        }

        public string GetModuleImageName(Module module)
        {
            throw new NotImplementedException();
        }

        public string GetModuleLoadedImage(Module module)
        {
            throw new NotImplementedException();
        }

        public string GetModuleMappedImage(Module module)
        {
            throw new NotImplementedException();
        }

        public string GetModuleName(Module module)
        {
            throw new NotImplementedException();
        }

        public string GetModuleSymbolFile(Module module)
        {
            throw new NotImplementedException();
        }

        public Tuple<DateTime, ulong> GetModuleTimestampAndSize(Module module)
        {
            throw new NotImplementedException();
        }

        public void GetModuleVersion(Module module, out int major, out int minor, out int revision, out int patch)
        {
            throw new NotImplementedException();
        }

        public Engine.Native.ImageFileMachine GetProcessActualProcessorType(Process process)
        {
            throw new NotImplementedException();
        }

        public Thread GetProcessCurrentThread(Process process)
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

        public string GetProcessExecutableName(Process process)
        {
            throw new NotImplementedException();
        }

        public Module[] GetProcessModules(Process process)
        {
            throw new NotImplementedException();
        }

        public uint GetProcessSystemId(Process process)
        {
            throw new NotImplementedException();
        }

        public Thread[] GetProcessThreads(Process process)
        {
            throw new NotImplementedException();
        }

        public uint GetProcessUpTime(Process process)
        {
            throw new NotImplementedException();
        }

        public StackTrace GetStackTraceFromContext(Process process, ulong contextAddress, uint contextSize = 0)
        {
            throw new NotImplementedException();
        }

        public ThreadContext GetThreadContext(Thread thread)
        {
            throw new NotImplementedException();
        }

        public StackFrame GetThreadCurrentStackFrame(Thread thread)
        {
            throw new NotImplementedException();
        }

        public ulong GetThreadEnvironmentBlockAddress(Thread thread)
        {
            throw new NotImplementedException();
        }

        public StackTrace GetThreadStackTrace(Thread thread)
        {
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

        public string ReadAnsiString(Process process, ulong address, int length = -1)
        {
            throw new NotImplementedException();
        }

        public string ReadInput()
        {
            throw new NotImplementedException();
        }

        public Engine.Utility.MemoryBuffer ReadMemory(Process process, ulong address, uint size)
        {
            throw new NotImplementedException();
        }

        public string ReadUnicodeString(Process process, ulong address, int length = -1)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentProcess(Process process)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStackFrame(StackFrame stackFrame)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentThread(Thread thread)
        {
            throw new NotImplementedException();
        }

        internal void UpdateCache()
        {
            // TODO: This should update cache with new values. For now, just clear everything
            throw new NotImplementedException();
        }
    }
}
