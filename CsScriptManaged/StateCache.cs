using CsScriptManaged.Utility;
using CsScripts;
using System.Collections.Generic;
using System.Linq;

namespace CsScriptManaged
{
    /// <summary>
    /// Cache for current process, thread and stack frame. Also introduces lazy state sync.
    /// </summary>
    internal class StateCache
    {
        /// <summary>
        /// The process we switched to.
        /// </summary>
        private Process switchedProcess = null;

        /// <summary>
        /// The current process.
        /// </summary>
        private Process currentProcess = null;

        /// <summary>
        /// The thread we switched to.
        /// </summary>
        private Dictionary<Process, Thread> switchedThread = new Dictionary<Process, Thread>();

        /// <summary>
        /// The stack frame we switched to.
        /// </summary>
        private Dictionary<Thread, StackFrame> switchedStackFrame = new Dictionary<Thread, StackFrame>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StateCache"/> class.
        /// </summary>
        public StateCache()
        {
            CurrentThread = new DictionaryCache<Process, Thread>(CacheCurrentThread);
            CurrentStackFrame = new DictionaryCache<Thread, StackFrame>(CacheCurrentStackFrame);
        }

        /// <summary>
        /// Gets the current process.
        /// </summary>
        public Process CurrentProcess
        {
            get
            {
                CacheCurrentProcess();
                return currentProcess;
            }
        }

        /// <summary>
        /// Gets the current thread.
        /// </summary>
        internal DictionaryCache<Process, Thread> CurrentThread { get; private set; }

        /// <summary>
        /// Gets the current stack frame.
        /// </summary>
        internal DictionaryCache<Thread, StackFrame> CurrentStackFrame { get; private set; }

        /// <summary>
        /// Synchronizes the state with DbgEng.dll.
        /// </summary>
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

        /// <summary>
        /// Switches the process.
        /// </summary>
        /// <param name="process">The process.</param>
        internal void SwitchProcess(Process process)
        {
            CacheCurrentProcess();
            if (switchedProcess != process)
            {
                switchedProcess = process;
                Context.SystemObjects.SetCurrentProcessId(process.Id);
            }
        }

        /// <summary>
        /// Switches the thread.
        /// </summary>
        /// <param name="thread">The thread.</param>
        internal void SwitchThread(Thread thread)
        {
            CacheCurrentThread();
            switchedThread[thread.Process] = thread;
            SwitchProcess(thread.Process);
            Context.SystemObjects.SetCurrentThreadId(thread.Id);
        }

        /// <summary>
        /// Switches the stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        internal void SwitchStackFrame(StackFrame stackFrame)
        {
            CacheCurrentStackFrame();
            switchedStackFrame[stackFrame.Thread] = stackFrame;
            SwitchThread(stackFrame.Thread);
            Context.Symbols.SetScopeFrameByIndex(stackFrame.FrameNumber);
        }

        /// <summary>
        /// Caches the current process.
        /// </summary>
        private void CacheCurrentProcess()
        {
            if (currentProcess == null)
            {
                uint currentId = Context.SystemObjects.GetCurrentProcessId();

                currentProcess = GlobalCache.Processes[currentId];
            }
        }

        /// <summary>
        /// Caches the current thread.
        /// </summary>
        /// <param name="process">The process.</param>
        private Thread CacheCurrentThread(Process process)
        {
            SwitchProcess(process);

            uint currentThreadId = Context.SystemObjects.GetCurrentThreadId();

            return process.Threads.FirstOrDefault(t => t.Id == currentThreadId);
        }

        /// <summary>
        /// Caches the current thread.
        /// </summary>
        private Thread CacheCurrentThread()
        {
            CacheCurrentProcess();
            return CurrentThread[currentProcess];
        }

        /// <summary>
        /// Caches the current stack frame.
        /// </summary>
        /// <param name="thread">The thread.</param>
        private StackFrame CacheCurrentStackFrame(Thread thread)
        {
            SwitchThread(thread);

            uint current = Context.Symbols.GetCurrentScopeFrameIndex();

            return thread.StackTrace.Frames.FirstOrDefault(f => f.FrameNumber == current);
        }

        /// <summary>
        /// Caches the current stack frame.
        /// </summary>
        private StackFrame CacheCurrentStackFrame()
        {
            Thread currentThread = CacheCurrentThread();

            return CurrentStackFrame[currentThread];
        }
    }
}
