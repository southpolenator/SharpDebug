using DbgEng;
using System;

namespace ExceptionDumper
{
    public class Dumper : IDebugEventCallbacks, IDisposable
    {
        private IDebugClient7 client;
        private IDebugControl7 control;
        private string dumpPath;
        private bool miniDump;
        private volatile bool dumpTaken = false;

        private Dumper(string applicationPath, string dumpPath, bool miniDump)
        {
            this.dumpPath = dumpPath;
            this.miniDump = miniDump;

            // Create debugging client
            IDebugClient clientBase = DebugClient.DebugCreate();

            // Cast to upper clients
            client = (IDebugClient7)clientBase;
            control = (IDebugControl7)client;
            client.SetEventCallbacks(this);
            client.CreateProcessAndAttach(0, applicationPath, DebugCreateProcess.DebugOnlyThisProcess, 0, DebugAttach.Default);
        }

        public static void RunAndDumpOnException(string applicationPath, string dumpPath, bool miniDump)
        {
            using (Dumper dumper = new Dumper(applicationPath, dumpPath, miniDump))
            {
                while (!dumper.dumpTaken)
                {
                    dumper.control.WaitForEvent(0, uint.MaxValue);
                }

                dumper.client.EndSession(DebugEnd.ActiveTerminate);
            }
        }

        #region IDebugEventCallbacks
        public uint GetInterestMask()
        {
            return (uint)(DebugEvent.Breakpoint | DebugEvent.CreateProcess
                | DebugEvent.Exception | DebugEvent.ExitProcess
                | DebugEvent.CreateThread | DebugEvent.ExitThread
                | DebugEvent.LoadModule | DebugEvent.UnloadModule);
        }

        public void Breakpoint(IDebugBreakpoint Bp)
        {
            // Do nothing
        }

        public void Exception(ref _EXCEPTION_RECORD64 Exception, uint FirstChance)
        {
            if (FirstChance != 1)
            {
                // Save the dump
                client.WriteDumpFile2(dumpPath, DebugDump.Small, miniDump ? DebugFormat.Default : DebugFormat.UserSmallFullMemory | DebugFormat.UserSmallHandleData);
                dumpTaken = true;
            }
        }

        public void CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset)
        {
            // Do nothing
        }

        public void ExitThread(uint ExitCode)
        {
            // Do nothing
        }

        public void CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset)
        {
            // Do nothing
        }

        public void ExitProcess(uint ExitCode)
        {
            // Do nothing
        }

        public void LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp)
        {
            // Do nothing
        }

        public void UnloadModule(string ImageBaseName, ulong BaseOffset)
        {
            // Do nothing
        }

        public void SystemError(uint Error, uint Level)
        {
            // Do nothing
        }

        public void SessionStatus(uint Status)
        {
            // Do nothing
        }

        public void ChangeDebuggeeState(uint Flags, ulong Argument)
        {
            // Do nothing
        }

        public void ChangeEngineState(uint Flags, ulong Argument)
        {
            // Do nothing
        }

        public void ChangeSymbolState(uint Flags, ulong Argument)
        {
            // Do nothing
        }
        #endregion
        int IDebugEventCallbacks.Breakpoint(IDebugBreakpoint Bp)
        {
            // Do nothing. 
            return 0;
        }

        public void Dispose()
        {
            client.SetEventCallbacks(null);
        }
    }
}
