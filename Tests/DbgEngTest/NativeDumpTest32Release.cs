using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.x86.exe.
    /// </summary>
    [TestClass]
    public class NativeDumpTest32Release
    {
        private const string DefaultDumpFile = "NativeDumpTest.x86.Release.dmp";
        private const string DefaultModuleName = "NativeDumpTest_x86_Release";
        private const string DefaultSymbolPath = @".\";

        private static NativeDumpTest testRunner;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            testRunner = new NativeDumpTest(DefaultDumpFile, DefaultModuleName, DefaultSymbolPath);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            testRunner.TestSetup();
        }

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestCpp()
        {
            // TODO: Fix ExceptionDumper to catch correct exception
            //testRunner.CurrentThreadContainsNativeDumpTestCpp();
        }

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            // TODO: Fix ExceptionDumper to catch correct exception
            //testRunner.CurrentThreadContainsNativeDumpTestMainFunction();
        }

        [TestMethod]
        public void TestModuleExtraction()
        {
            testRunner.TestModuleExtraction();
        }

        [TestMethod]
        public void CheckProcess()
        {
            testRunner.CheckProcess();
        }

        [TestMethod]
        public void CheckThread()
        {
            // TODO: Fix ExceptionDumper to catch correct exception
            //testRunner.CheckThread();
        }

        [TestMethod]
        public void CheckCodeFunction()
        {
            // TODO: Fix ExceptionDumper to catch correct exception
            //testRunner.CheckCodeFunction();
        }
    }
}
