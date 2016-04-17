using CommandLine;

namespace ExceptionDumper
{
    class Options
    {
        [Option('a', "app", Default = "NativeDumpTest.exe", HelpText = "Path to application will be debugged and dumped on exception")]
        public string AppPath { get; set; }

        [Option('d', "dump", Default = "NativeDumpTest.mdmp", HelpText = "Path to dump file to be generated")]
        public string DumpPath { get; set; }

        [Option("mini-dump", Default = false, HelpText = "Generate mini dump instead of full dump", Required = false)]
        public bool MiniDump { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Options options = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);

            if (options == null)
                return;

            Dumper.RunAndDumpOnException(options.AppPath, options.DumpPath, options.MiniDump);
        }
    }
}
