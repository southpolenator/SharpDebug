using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    [TestClass]
    public class InteractiveModeTests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;

        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(DefaultDumpFile, DefaultSymbolPath);
        }

        [TestMethod]
        public void DynamicTest()
        {
            Executor.InterpretInteractive("dynamic a = 5");
            new Executor().Interpret("Console.WriteLine(a);");
        }

        [TestMethod]
        public void DebuggerCommand()
        {
            Executor.InterpretInteractive("#dbg k");
        }
    }
}
