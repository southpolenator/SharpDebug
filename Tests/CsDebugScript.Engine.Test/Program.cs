using CommandLine;
using CsDebugScript.Engine.Utility;
using DbgEngManaged;
using System;

namespace CsDebugScript.Engine.Test
{
    class Options
    {
        [Option('d', "dump", Default = "NativeDumpTest.dmp", HelpText = "Path to memory dump file that will be debugged")]
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

            Context.Initalize(DebugClient.OpenDumpFile(options.DumpPath, options.SymbolPath));

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
            var callbacks = DebuggerOutputToTextWriter.Create(Console.Out, captureFlags);

            using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
            {
                Executor.Execute(@"..\..\..\..\samples\script.csx");
            }
        }
    }
}
