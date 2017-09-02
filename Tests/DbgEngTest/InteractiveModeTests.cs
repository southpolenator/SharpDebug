using CsDebugScript;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    [TestClass]
    public class InteractiveModeTests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;
        private InteractiveExecution interactiveExecution;

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
            interactiveExecution = new InteractiveExecution();
        }

        [TestMethod]
        [TestCategory("Scripting")]
        public void DynamicTest()
        {
            InterpretInteractive("dynamic a = 5");
            InterpretInteractive("Console.WriteLine(a);");
            InterpretInteractive("Dump(a);");
            InterpretInteractive("a");
        }

        [TestMethod]
        [TestCategory("Scripting")]
        public void ScriptBaseTest()
        {
            InterpretInteractive("ListCommands();");
            InterpretInteractive("ListAllCommands();");
            InterpretInteractive("ListVariables();");
            InterpretInteractive("ChangeBaseClass<InteractiveScriptBase>();");
            InterpretInteractive("exit");
        }

        [TestMethod]
        [TestCategory("Scripting")]
        public void DebuggerCommand()
        {
            InterpretInteractive("#dbg k");
        }

        private new void InterpretInteractive(string code)
        {
            DbgEngDll dbgEngDll = Context.Debugger as DbgEngDll;

            dbgEngDll.ExecuteAction(() => interactiveExecution.Interpret(code));
        }
    }
}
