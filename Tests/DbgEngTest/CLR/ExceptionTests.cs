using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbgEngTest.CLR
{
    [TestClass]
    public class ExceptionTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            SyncStart();
            CompileAndInitialize(ClrTestApps.NestedException);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        public void ExceptionPropertyTest()
        {
            ClrThread thread = Thread.Current.FindClrThread();

            Assert.IsFalse(thread.IsFinalizerThread);

            ClrException exception = thread.LastThrownException;

            Assert.IsNotNull(exception);
            Assert.AreEqual("IOE Message", exception.Message);
            Assert.AreEqual("System.InvalidOperationException", exception.GetCodeType().Name);
            Assert.IsNotNull(exception.InnerException);
            Assert.AreEqual("FNF Message", exception.InnerException.Message);
            Assert.AreEqual("System.IO.FileNotFoundException", exception.InnerException.GetCodeType().Name);

            // TODO: Check the call stack
        }
    }
}
