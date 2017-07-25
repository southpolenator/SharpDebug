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
        [TestCategory("NativeDumpTests")]
        public void TestModuleExtraction()
        {
            testRunner.TestModuleExtraction();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void ReadingFloatPointTypes()
        {
            testRunner.ReadingFloatPointTypes();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void GettingClassStaticMember()
        {
            testRunner.GettingClassStaticMember();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckProcess()
        {
            testRunner.CheckProcess();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckDebugger()
        {
            testRunner.CheckDebugger();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CurrentThreadContainsNativeDumpTestCpp()
        {
            testRunner.CurrentThreadContainsNativeDumpTestCpp();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            testRunner.CurrentThreadContainsNativeDumpTestMainFunction();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckMainArguments()
        {
            testRunner.CheckMainArguments();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckThread()
        {
            testRunner.CheckThread();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckCodeArray()
        {
            testRunner.CheckCodeArray();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckCodeFunction()
        {
            testRunner.CheckCodeFunction();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void CheckMainLocals()
        {
            testRunner.CheckDefaultTestCaseLocals();
        }

        [TestMethod]
        [TestCategory("NativeDumpTests")]
        public void TestBasicTemplateType()
        {
            testRunner.TestBasicTemplateType();
        }
    }
}
