using CommandLine;

namespace SharpDebug.UI.App
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

            DebuggerInitialization.OpenDump(options.DumpPath, options.SymbolPath);
            InteractiveWindow.ShowModalWindow();
        }
    }
}
