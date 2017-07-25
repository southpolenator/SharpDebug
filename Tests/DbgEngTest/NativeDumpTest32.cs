using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.x86.exe.
    /// </summary>
    [TestClass]
    [DeploymentItem(DefaultDumpFile)]
    public class NativeDumpTest32 : TestBase
    {
        private const string DefaultDumpFile = "NativeDumpTest.x86.dmp";
        private const string DefaultModuleName = "NativeDumpTest_x86";
        private const string DefaultSymbolPath = @".\";

        private static NativeDumpTest testRunner;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            SyncStart();
            testRunner = new NativeDumpTest(DefaultDumpFile, DefaultModuleName, DefaultSymbolPath);
            testRunner.TestSetup();
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
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

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestCpp()
        {
            testRunner.CurrentThreadContainsNativeDumpTestCpp();
        }

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            testRunner.CurrentThreadContainsNativeDumpTestMainFunction();
        }

        [TestMethod]
        public void CheckMainArguments()
        {
            testRunner.CheckMainArguments();
        }

        [TestMethod]
        public void CheckThread()
        {
            testRunner.CheckThread();
        }

        [TestMethod]
        public void CheckCodeArray()
        {
            testRunner.CheckCodeArray();
        }

        [TestMethod]
        public void CheckCodeFunction()
        {
            testRunner.CheckCodeFunction();
        }

        [TestMethod]
        public void CheckMainLocals()
        {
            testRunner.CheckDefaultTestCaseLocals();
        }

        [TestMethod]
        public void TestBasicTemplateType()
        {
            testRunner.TestBasicTemplateType();
        }
    }
}
