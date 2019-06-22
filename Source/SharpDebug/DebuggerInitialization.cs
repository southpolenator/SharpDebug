using CsDebugScript.DwarfSymbolProvider;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;

namespace CsDebugScript
{
    /// <summary>
    /// Class for simplified debugger initialization.
    /// </summary>
    public static class DebuggerInitialization
    {
        /// <summary>
        /// Initializes debugger for processing the specified dump.
        /// </summary>
        /// <param name="dumpPath">Path to the dump.</param>
        /// <param name="symbolPaths">Array of paths where debugger will look for symbols.</param>
        public static void OpenDump(string dumpPath, params string[] symbolPaths)
        {
            try
            {
                IDebugClient debugClient = DebugClient.OpenDumpFile(dumpPath, string.Join(";", symbolPaths));

                InitializeDbgEng(debugClient);
            }
            catch
            {
                IDebuggerEngine engine = new ElfCoreDumpDebuggingEngine(dumpPath);

                Context.InitializeDebugger(engine);
            }
        }

        /// <summary>
        /// Attaches debugger to the already running specified process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        /// <param name="attachFlags">The attaching flags.</param>
        /// <param name="symbolPaths">Array of paths where debugger will look for symbols.</param>
        public static void AttachToProcess(uint processId, DebugAttach attachFlags = DebugAttach.Noninvasive, params string[] symbolPaths)
        {
            IDebugClient debugClient = DebugClient.DebugCreate();
            IDebugSymbols5 symbols = (IDebugSymbols5)debugClient;
            IDebugControl7 control = (IDebugControl7)debugClient;

            symbols.SetSymbolPathWide(string.Join(";", symbolPaths));
            debugClient.AttachProcess(0, processId, attachFlags);
            control.WaitForEvent(0, uint.MaxValue);
            InitializeDbgEng(debugClient);
        }

        /// <summary>
        /// Initializes debugger as DbgEng from the specified debug client interface.
        /// </summary>
        /// <param name="debugClient">The debug client interface that will initialize debugger.</param>
        private static void InitializeDbgEng(IDebugClient debugClient)
        {
            DbgEngDll.InitializeContext(debugClient);
            Context.InitializeDebugger(Context.Debugger, new DwarfSymbolProvider.DwarfSymbolProvider());
            Context.ClrProvider = new CLR.ClrMdProvider();
        }
    }
}
