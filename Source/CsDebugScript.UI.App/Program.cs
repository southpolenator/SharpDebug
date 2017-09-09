using CommandLine;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;

namespace CsDebugScript.UI.App
{
    class Options
    {
        [Option('d', "dump", Required = true, HelpText = "Path to memory dump file that will be debugged")]
        public string DumpPath { get; set; }

        [Option('p', "symbol-path", Default = @"srv*;", HelpText = "Symbol path to be set in debugger")]
        public string SymbolPath { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Options options = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);

            if (options == null)
            {
                return;
            }

            try
            {
                IDebugClient debugClient = DebugClient.OpenDumpFile(options.DumpPath, options.SymbolPath);
                DbgEngDll.InitializeContext(debugClient);
                Context.InitializeDebugger(Context.Debugger, new DwarfSymbolProvider.DwarfSymbolProvider());
                Context.ClrProvider = new CLR.ClrMdProvider();
            }
            catch
            {
                IDebuggerEngine engine = new DwarfSymbolProvider.ElfCoreDumpDebuggingEngine(options.DumpPath);

                Context.InitializeDebugger(engine, engine.CreateDefaultSymbolProvider());
            }

            InteractiveWindow.ShowModalWindow();
        }
    }
}
