using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.x86.exe.
    /// </summary>
    [TestClass]
    public class NativeDumpTest32
    {
        private const string DefaultDumpFile = "NativeDumpTest.x86.dmp";
        private const string DefaultModuleName = "NativeDumpTest_x86";
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
        public void ReadingFloatPointTypes()
        {
            testRunner.ReadingFloatPointTypes();
        }

        [TestMethod]
        public void GettingClassStaticMember()
        {
            testRunner.GettingClassStaticMember();
        }

        [TestMethod]
        public void CheckMainArguments()
        {
            // TODO: Fix ExceptionDumper to catch correct exception
            //testRunner.CheckMainArguments();
        }

        [TestMethod]
        public void CheckProcess()
        {
            testRunner.CheckProcess();
        }

        [TestMethod]
        public void CheckThread()
        {
            testRunner.CheckThread();
        }
    }
}
