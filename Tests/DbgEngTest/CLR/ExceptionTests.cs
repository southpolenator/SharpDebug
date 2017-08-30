using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using ClrException = CsDebugScript.CommonUserTypes.CLR.System.Exception;

namespace DbgEngTest.CLR
{
    [TestClass]
    [DeploymentItem(@"CLR\Apps\NestedException.cs", @"CLR\Apps")]
    [DeploymentItem(@"CLR\Apps\SharedLibrary.cs", @"CLR\Apps")]
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
        [TestCategory("CLR")]
        public void ExceptionPropertyTest()
        {
            IClrThread thread = Thread.Current.ClrThread;

            Assert.IsFalse(thread.IsFinalizerThread);

            ClrException exception = Variable.CastAs<ClrException>(thread.LastThrownException);

            Assert.IsNotNull(exception);
            Assert.AreEqual("IOE Message", exception.Message);
            Assert.AreEqual("System.InvalidOperationException", exception.GetCodeType().Name);
            Assert.IsNotNull(exception.InnerException);
            Assert.AreEqual("FNF Message", exception.InnerException.Message);
            Assert.AreEqual("System.IO.FileNotFoundException", exception.InnerException.GetCodeType().Name);

            CodeFunction[] stackTrace = exception.StackTrace;

            Assert.AreEqual(4, stackTrace.Length);
            Assert.AreEqual("Program.Inner()", stackTrace[0].FunctionNameWithoutModule);
            Assert.AreEqual("Program.Middle()", stackTrace[1].FunctionNameWithoutModule);
            Assert.AreEqual("Program.Outer()", stackTrace[2].FunctionNameWithoutModule);
            Assert.AreEqual("Program.Main(System.String[])", stackTrace[3].FunctionNameWithoutModule);
            Assert.AreEqual((uint)11, stackTrace[3].SourceFileLine);
            Assert.AreEqual("nestedexception.cs", Path.GetFileName(stackTrace[3].SourceFileName).ToLowerInvariant());

            Assert.AreEqual(0x80131509, (uint)exception.HResult);
            Assert.IsNull(exception.HelpLink);
            Assert.IsNull(exception.Source);
            Assert.IsNull(exception.StackTraceString);

            try
            {
                IDictionary data = exception.Data;
                Assert.Fail();
            }
            catch (NotImplementedException)
            {
            }
        }
    }
}
