using CommandLine;
using CsScriptManaged;
using CsScripts;
using DbgEngManaged;
using System;
using System.Runtime.InteropServices;

namespace CsScriptManagedTest
{
    class Options
    {
        [Option('d', "dump", DefaultValue = "NativeDump1.dmp", HelpText = "Path to memory dump file that will be debugged")]
        public string DumpPath { get; set; }

        [Option('p', "symbol-path", DefaultValue = @"srv*;.\", HelpText = "Symbol path to be set in debugger")]
        public string SymbolPath { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArgumentsStrict(args, options);

            var client = OpenDumpFile(options.DumpPath, options.SymbolPath);

            Context.Initalize(client);
            //Context.Execute(@"..\..\..\..\samples\script.cs", new string[] { });

            Console.WriteLine("Threads: {0}", Thread.All.Length);
            Console.WriteLine("Current thread: {0}", Thread.Current.Id);
            var frames = Thread.Current.StackTrace.Frames;
            Console.WriteLine("Call stack:");
            foreach (var frame in frames)
                Console.WriteLine("  {0,3:x} {1}+0x{2:x}", frame.FrameNumber, frame.FunctionName, frame.FunctionDisplacement);
        }

        public static IDebugClient OpenDumpFile(string dumpFile, string symbolPath)
        {
            IDebugClient client;
            int hresult = DebugCreate(Marshal.GenerateGuidForType(typeof(IDebugClient)), out client);

            if (hresult > 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            client.OpenDumpFile(dumpFile);
            ((IDebugControl7)client).WaitForEvent(0, uint.MaxValue);
            ((IDebugSymbols5)client).SetSymbolPathWide(symbolPath);
            return client;
        }

        [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", SetLastError = false)]
        public static extern int DebugCreate(Guid iid, out IDebugClient client);
    }
}
