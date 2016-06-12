using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DbgEngTest
{
    [TestClass]
    public class ScriptExecutionTests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;
        private const string DefaultModuleName = NativeDumpTest64.DefaultModuleName;

        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(DefaultDumpFile, DefaultSymbolPath);
        }

        [TestMethod]
        public void SimpleScript()
        {
            ExecuteScript(@"writeln(1 + 2);");
        }

        [TestMethod]
        public void ScriptArguments()
        {
            ExecuteScript(@"writeln(args[0]);writeln(args[1]);", "First argument", "Second \" argument");
        }

        [TestMethod]
        public void DynamicTest()
        {
            ExecuteScript(@"dynamic a = new [] { 1, 2, 3, 4, 5, 6, 7 }; writeln(a.Length);");
        }

        [TestMethod]
        public void ScriptBase()
        {
            ExecuteScript($@"
var module = Modules.{DefaultModuleName};
var moduleName = module.Name;
var doubleTest = module.doubleTest;
var doubleTest2 = Globals.doubleTest;

write(Processes.Length);
Write(Threads.Length);
");
        }

        private static void ExecuteScript(string code, params string[] arguments)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                File.WriteAllText(fileName, code);
                if (arguments.Length == 0)
                    new Executor().ExecuteScript(fileName);
                else
                    new Executor().ExecuteScript(fileName, arguments);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
