using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace DbgEngTest
{
    [TestClass]
    public class ScriptExecutionTests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;
        private const string DefaultModuleName = NativeDumpTest64.DefaultModuleName;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            SyncStart();
            InitializeDump(DefaultDumpFile, DefaultSymbolPath);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        public void TestExecutor()
        {
            ExecuteScript((fileName, arguments) => new Executor().ExecuteScript(fileName),
                @"writeln(1 + 2);");
            ExecuteScript((fileName, arguments) => new Executor().ExecuteScript(fileName, arguments),
                @"writeln(args[0]);writeln(args[1]);", "First argument", "Second \" argument");
        }

        [TestMethod]
        public void SimpleScript()
        {
            string[] lines = ExecuteScript(@"writeln(1 + 2);");

            CompareArrays(lines, new[] { "3" });
        }

        [TestMethod]
        public void ScriptArguments()
        {
            string[] arguments = new[] { "First argument", "Second \" argument" };
            string[] lines = ExecuteScript(@"writeln(args[0]);writeln(args[1]);", arguments);

            CompareArrays(lines, arguments);
        }

        [TestMethod]
        public void DynamicTest()
        {
            string[] lines = ExecuteScript(@"dynamic a = new [] { 1, 2, 3, 4, 5, 6, 7 }; writeln(a.Length);");

            CompareArrays(lines, new[] { "7" });
        }

        [TestMethod]
        public void ScriptBase()
        {
            string[] lines = ExecuteScript($@"
var module = Modules.{DefaultModuleName};
var moduleName = module.Name;
var doubleTest = module.doubleTest;
var doubleTest2 = Globals.doubleTest;

WriteLine(moduleName);
write(doubleTest.GetPointerAddress() == doubleTest2.GetPointerAddress());
write(Processes.Length > 0);
Write(Threads.Length > 0);
");

            CompareArrays(lines, new[] { DefaultModuleName, "TrueTrueTrue" });
        }

        private static string[] ExecuteScript(string code, params string[] arguments)
        {
            TextWriter output = Console.Out;
            TextWriter error = Console.Error;

            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    Console.SetError(writer);
                    Console.SetOut(writer);
                    ExecuteScript((fileName, args) => ScriptExecution.Execute(fileName, args), code, arguments);
                    writer.Flush();

                    string text = writer.GetStringBuilder().ToString();

                    return text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                }
            }
            finally
            {
                Console.SetOut(output);
                Console.SetError(error);
            }
        }

        private static void ExecuteScript(Action<string, string[]> executor, string code, params string[] arguments)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                File.WriteAllText(fileName, code);
                executor(fileName, arguments);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
