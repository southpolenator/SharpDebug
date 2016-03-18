using CommandLine;
using CsScriptManaged;
using CsScriptManaged.Utility;
using CsScripts;
using DbgEngManaged;
using System;
using System.Runtime.InteropServices;

namespace CsScriptManagedTest
{
    class Options
    {
        [Option('d', "dump", Default = "NativeDump1.dmp", HelpText = "Path to memory dump file that will be debugged")]
        public string DumpPath { get; set; }

        [Option('p', "symbol-path", Default = @"srv*;.\", HelpText = "Symbol path to be set in debugger")]
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
                return;

            var client = OpenDumpFile(options.DumpPath, options.SymbolPath);

            Context.Initalize(client);

            Console.WriteLine("Threads: {0}", Thread.All.Length);
            Console.WriteLine("Current thread: {0}", Thread.Current.Id);
            var frames = Thread.Current.StackTrace.Frames;
            Console.WriteLine("Call stack:");
            foreach (var frame in frames)
                try
                {
                    Console.WriteLine("  {0,3:x} {1}+0x{2:x}   ({3}:{4})", frame.FrameNumber, frame.FunctionName, frame.FunctionDisplacement, frame.SourceFileName, frame.SourceFileLine);
                }
                catch (Exception)
                {
                    Console.WriteLine("  {0,3:x} {1}+0x{2:x}", frame.FrameNumber, frame.FunctionName, frame.FunctionDisplacement);
                }

            // In order to use console output and error in scripts, we must set it to debug client
            DebugOutput captureFlags = DebugOutput.Normal | DebugOutput.Error | DebugOutput.Warning | DebugOutput.Verbose
                | DebugOutput.Prompt | DebugOutput.PromptRegisters | DebugOutput.ExtensionWarning | DebugOutput.Debuggee
                | DebugOutput.DebuggeePrompt | DebugOutput.Symbols | DebugOutput.Status;
            //var callbacks = new DebuggerOutputToTextWriter(Console.Out, captureFlags);

            //using (OutputCallbacksSwitcher switcher = new OutputCallbacksSwitcher(callbacks))
            //{
            //    Context.Execute(@"..\..\..\samples\script.cs", new string[] { });
            //}
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
