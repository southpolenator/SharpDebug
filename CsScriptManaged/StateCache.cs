using CsScriptManaged.Utility;
using CsScripts;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CsScriptManaged
{
    internal class StateCache
    {
        private Process switchedProcess = null;
        private Process currentProcess = null;
        private Dictionary<Process, Thread> switchedThread = new Dictionary<Process, Thread>();
        private Dictionary<Thread, StackFrame> switchedStackFrame = new Dictionary<Thread, StackFrame>();

        public StateCache()
        {
            CurrentThread = new DictionaryCache<Process, Thread>(CacheCurrentThread);
            CurrentStackFrame = new DictionaryCache<Thread, StackFrame>(CacheCurrentStackFrame);
        }

        public Process CurrentProcess
        {
            get
            {
                CacheCurrentProcess();
                return currentProcess;
            }
        }

        internal DictionaryCache<Process, Thread> CurrentThread { get; private set; }

        internal DictionaryCache<Thread, StackFrame> CurrentStackFrame { get; private set; }

        internal void SyncState()
        {
            if (currentProcess == null)
            {
                return;
            }

            if (switchedProcess != null && switchedProcess != currentProcess)
            {
                SwitchProcess(currentProcess);
            }

            if (!switchedThread.ContainsKey(currentProcess))
            {
                return;
            }

            if (switchedThread[currentProcess] != CurrentThread[currentProcess])
            {
                SwitchThread(CurrentThread[currentProcess]);
            }
        }

        internal void SwitchProcess(Process process)
        {
            CacheCurrentProcess();
            if (switchedProcess != process)
            {
                switchedProcess = process;
                Context.SystemObjects.SetCurrentProcessId(process.Id);
            }
        }

        internal void SwitchThread(Thread thread)
        {
            CacheCurrentThread();
            switchedThread[thread.Process] = thread;
            SwitchProcess(thread.Process);
            Context.SystemObjects.SetCurrentThreadId(thread.Id);
        }

        internal void SwitchStackFrame(StackFrame stackFrame)
        {
            CacheCurrentStackFrame();
            switchedStackFrame[stackFrame.Thread] = stackFrame;
            SwitchThread(stackFrame.Thread);
            Context.Symbols.SetScopeFrameByIndex(stackFrame.FrameNumber);
        }

        private void CacheCurrentProcess()
        {
            if (currentProcess == null)
            {
                uint currentId = Context.SystemObjects.GetCurrentProcessId();

                currentProcess = GlobalCache.Processes[currentId];
            }
        }

        private Thread CacheCurrentThread(Process process)
        {
            SwitchProcess(process);

            uint currentThreadId = Context.SystemObjects.GetCurrentThreadId();

            return process.Threads.FirstOrDefault(t => t.Id == currentThreadId);
        }

        private Thread CacheCurrentThread()
        {
            CacheCurrentProcess();
            return CurrentThread[currentProcess];
        }

        private StackFrame CacheCurrentStackFrame(Thread thread)
        {
            SwitchThread(thread);

            uint current = Context.Symbols.GetCurrentScopeFrameIndex();

            return thread.StackTrace.Frames.FirstOrDefault(f => f.FrameNumber == current);
        }

        private StackFrame CacheCurrentStackFrame()
        {
            Thread currentThread = CacheCurrentThread();

            return CurrentStackFrame[currentThread];
        }
    }
}
