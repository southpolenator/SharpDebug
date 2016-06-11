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
        //public void CheckMainArguments()
        //{
        //    testRunner.CheckMainArguments();
        //}

        //[TestMethod]
        //public void CheckThread()
        //{
        //    testRunner.CheckThread();
        //}

        //[TestMethod]
        //public void CheckCodeArray()
        //{
        //    testRunner.CheckCodeArray();
        //}

        //[TestMethod]
        //public void CheckCodeFunction()
        //{
        //    testRunner.CheckCodeFunction();
        //}

        //[TestMethod]
        //public void CheckMainLocals()
        //{
        //    testRunner.CheckMainLocals();
        //}
        #endregion
    }
}
