using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using System;
using System.IO;
using Xunit;

namespace CsDebugScript.Tests.Native
{
    [Collection("NativeDumpTest.x64.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class WinDbgExtensionTests : DumpTestBase
    {
        public bool ExtensionExists { get; private set; }

        public WinDbgExtensionTests(NativeDumpTest_x64_dmp_Initialization initialization)
            : base(initialization)
        {
            string configuration = Environment.Is64BitProcess ? "x64" : "x86";
            string extensionPath = GetAbsoluteBinPathRecursive1($"CsDebugScript.WinDbg.{configuration}.dll");
            ExtensionExists = File.Exists(extensionPath);
            string output = DbgEngDll.ExecuteAndCapture($".load {extensionPath}");

            Assert.Equal("", output?.Trim());
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(ExtensionExists))]
        public void CheckSimpleInterpret()
        {
            string output = Execute("!interpret 2+3");

            Assert.Equal("5", output?.Trim());
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(ExtensionExists))]
        public void CheckInterpret()
        {
            string output = Execute("!interpret Process.Current.GetGlobal(\"MyTestClass::staticVariable\")");

            Assert.Equal("1212121212", output?.Trim());
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(ExtensionExists))]
        public void CheckExecuteFailure()
        {
            string output = Execute("!execute invalid_path_to_script arg1 arg2");

            Assert.Contains("Compile error", output);
        }

        private string Execute(string command, params string[] parameters)
        {
            IDebuggerEngine debugger = Context.Debugger;
            ISymbolProvider symbolProvider = Context.SymbolProvider;
            string output = DbgEngDll.ExecuteAndCapture(command, parameters);

            Context.InitializeDebugger(debugger, symbolProvider);
            return output;
        }
    }
}
