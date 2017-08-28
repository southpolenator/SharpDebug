using CsDebugScript.Engine.Debuggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace DbgEngTest
{
    [TestClass]
    public class WinDbgExtensionTests : TestBase
    {
        internal const string DefaultDumpFile = "NativeDumpTest.x64.dmp";
        internal const string DefaultModuleName = "NativeDumpTest_x64";
        internal const string DefaultSymbolPath = @".\";

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            SyncStart();
            InitializeDump(DefaultDumpFile, DefaultSymbolPath);
            LoadExtension();
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        private static void LoadExtension()
        {
            string binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configuration = Environment.Is64BitProcess ? "x64" : "x86";
            string extensionPath = Path.Combine(binFolder, $"CsDebugScript.WinDbg.{configuration}.dll");
            string output = DbgEngDll.ExecuteAndCapture($".load {extensionPath}");

            Assert.AreEqual("", output?.Trim());
        }

        [TestMethod]
        [TestCategory("WinDbgExtension")]
        public void CheckSimpleInterpret()
        {
            string output = DbgEngDll.ExecuteAndCapture("!interpret 2+3");

            Assert.AreEqual("5", output?.Trim());
        }

        [TestMethod]
        [TestCategory("WinDbgExtension")]
        public void CheckInterpret()
        {
            string output = DbgEngDll.ExecuteAndCapture("!interpret Process.Current.GetGlobal(\"MyTestClass::staticVariable\")");

            Assert.AreEqual("1212121212", output?.Trim());
        }

        [TestMethod]
        [TestCategory("WinDbgExtension")]
        public void CheckExecuteFailure()
        {
            string output = DbgEngDll.ExecuteAndCapture("!execute invalid_path_to_script arg1 arg2");

            Assert.IsTrue(output.Contains("Compile error"));
        }
    }
}
