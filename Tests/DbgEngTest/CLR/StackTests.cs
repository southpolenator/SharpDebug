using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ClrString = CsDebugScript.CommonUserTypes.CLR.System.String;

namespace DbgEngTest.CLR
{
    [TestClass]
    [DeploymentItem(@"CLR\Apps\NestedException.cs", @"CLR\Apps")]
    [DeploymentItem(@"CLR\Apps\SharedLibrary.cs", @"CLR\Apps")]
    public class StackTests : ClrTestBase
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
        public void ObjectArgumentAndLocalTest()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable args = frame.Arguments.Single();

            Assert.IsNotNull(clrThread.Runtime);
            Assert.IsNotNull(clrThread.AppDomain);
            Assert.IsNotNull(args);
            Assert.AreEqual("System.String[]", args.GetCodeType().Name);
            Assert.AreEqual("args", args.GetName());

            Variable foo = frame.Locals.Single();

            Assert.IsNotNull(foo);
            Assert.IsFalse(foo.IsNullPointer());
            Assert.AreEqual("Foo", foo.GetCodeType().Name);
            Assert.AreEqual("foo", foo.GetName());

            Assert.AreEqual(8.4, (double)foo.GetField("d"));
            Assert.AreEqual("Foo string", new ClrString(foo.GetField("FooString")).Text);
        }
    }
}
