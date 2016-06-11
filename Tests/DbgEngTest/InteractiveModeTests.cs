using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    [TestClass]
    public class InteractiveModeTests : TestBase
    {
        private const string DefaultDumpFile = "NativeDumpTest.x64.dmp";
        private const string DefaultSymbolPath = @".\";

        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(DefaultDumpFile, DefaultSymbolPath);
        }

        [TestMethod]
        public void DynamicTest()
        {
            Executor.InterpretInteractive("dynamic a = 5");
            Executor.InterpretInteractive("Console.WriteLine(a);");
        }
    }
}
