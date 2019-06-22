using CsDebugScript.DwarfSymbolProvider;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;
using System.IO;

namespace CsDebugScript.Tests
{
    public class DumpInitialization
    {
        public const string DefaultDumpPath = @"..\..\..\dumps\";

        private static CommonUserTypes.NativeTypes.std.vector vector = InitializeCommonUserTypes();

        public DumpInitialization(string dumpPath, string defaultModuleName, string symbolPath, bool useILCodeGen = false)
        {
            if (!Path.IsPathRooted(dumpPath))
            {
                DumpPath = TestBase.GetAbsoluteBinPath(Path.Combine(DefaultDumpPath, dumpPath));
            }
            else
            {
                DumpPath = dumpPath;
            }
            DefaultModuleName = defaultModuleName;
            SymbolPath = symbolPath;
            UseILCodeGen = useILCodeGen;
        }

        public string DumpPath { get; private set; }

        public string DefaultModuleName { get; private set; }

        public string SymbolPath { get; private set; }

        public bool UseILCodeGen { get; private set; }

        public InteractiveExecution InteractiveExecution { get; private set; } = new InteractiveExecution();

        internal bool CodeGenExecuted { get; set; }

        private static CommonUserTypes.NativeTypes.std.vector InitializeCommonUserTypes()
        {
            try
            {
                return new CommonUserTypes.NativeTypes.std.vector(null);
            }
            catch
            {
                return null;
            }
        }
    }

    public class ElfCoreDumpInitialization : DumpInitialization
    {
        public ElfCoreDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath)
            : base(dumpPath, defaultModuleName, TestBase.GetAbsoluteBinPath(symbolPath))
        {
            IDebuggerEngine engine = new ElfCoreDumpDebuggingEngine(DumpPath);

            Context.InitializeDebugger(engine);
        }
    }

    public class DbgEngDumpInitialization : DumpInitialization
    {
        public DbgEngDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath, bool addSymbolServer = true, bool useDia = true, bool useDwarf = false, bool useILCodeGen = false)
            : base(dumpPath, defaultModuleName, FixSymbolPath(symbolPath, addSymbolServer), useILCodeGen)
        {
            // Clear all processes being debugged with DbgEng.dll...
            (Context.Debugger as DbgEngDll)?.Client?.EndSession(DebugEnd.ActiveTerminate);

            IDebugClient client = DebugClient.OpenDumpFile(DumpPath, SymbolPath);

            DbgEngDll.InitializeContext(client);
            if (!useDia && !useDwarf)
            {
                Context.InitializeDebugger(Context.Debugger);
            }
            else if (useDwarf)
            {
                Context.InitializeDebugger(Context.Debugger, new DwarfSymbolProvider.DwarfSymbolProvider());
            }
        }

        private static string FixSymbolPath(string symbolPath, bool addSymbolServer)
        {
            symbolPath = TestBase.GetAbsoluteBinPath(symbolPath);
            if (addSymbolServer)
            {
                symbolPath += ";srv*";
            }
            return symbolPath;
        }
    }
}
