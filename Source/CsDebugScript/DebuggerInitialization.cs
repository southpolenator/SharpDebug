using CsDebugScript.DwarfSymbolProvider;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;

namespace CsDebugScript
{
    /// <summary>
    /// Class for simplified debugger initializion.
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
                DbgEngDll.InitializeContext(debugClient);
                Context.InitializeDebugger(Context.Debugger, new DwarfSymbolProvider.DwarfSymbolProvider());
                Context.ClrProvider = new CLR.ClrMdProvider();
            }
            catch
            {
                IDebuggerEngine engine = new ElfCoreDumpDebuggingEngine(dumpPath);

                Context.InitializeDebugger(engine, engine.CreateDefaultSymbolProvider());
            }
        }
    }
}
