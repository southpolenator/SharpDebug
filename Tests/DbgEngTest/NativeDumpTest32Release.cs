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
        public void CheckDebugger()
        {
            testRunner.CheckDebugger();
        }

        // TODO: Fix ExceptionDumper to catch correct exception
        #region Enable tests when ExceptionDumper is fixed for 32bit apps
        //[TestMethod]
        //public void CurrentThreadContainsNativeDumpTestCpp()
        //{
        //    testRunner.CurrentThreadContainsNativeDumpTestCpp();
        //}

        //[TestMethod]
        //public void CurrentThreadContainsNativeDumpTestMainFunction()
        //{
        //    testRunner.CurrentThreadContainsNativeDumpTestMainFunction();
        //}

        //[TestMethod]
        //public void CheckThread()
        //{
        //    testRunner.CheckThread();
        //}

        //[TestMethod]
        //public void CheckCodeFunction()
        //{
        //    testRunner.CheckCodeFunction();
        //}
        #endregion
    }
}
