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

        private static void ExecuteScript(string code, params string[] arguments)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                File.WriteAllText(fileName, code);
                Executor.Execute(fileName, arguments);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
