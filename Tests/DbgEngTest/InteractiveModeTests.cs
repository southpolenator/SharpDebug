using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    [TestClass]
    public class InteractiveModeTests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;

        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            SyncStart();
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            InitializeDump(DefaultDumpFile, DefaultSymbolPath);
        }

        [TestMethod]
        public void DynamicTest()
        {
            Executor.InterpretInteractive("dynamic a = 5");
            Executor.InterpretInteractive("Console.WriteLine(a);");
            Executor.InterpretInteractive("Dump(a);");
            Executor.InterpretInteractive("a");
        }

        [TestMethod]
        public void ScriptBaseTest()
        {
            Executor.InterpretInteractive("ListCommands();");
            Executor.InterpretInteractive("ListAllCommands();");
            Executor.InterpretInteractive("ListVariables();");
            Executor.InterpretInteractive("ChangeBaseClass<InteractiveScriptBase>();");
            Executor.InterpretInteractive("exit");
        }

        [TestMethod]
        public void DebuggerCommand()
        {
            new Executor().Interpret("#dbg k");
        }
    }
}
