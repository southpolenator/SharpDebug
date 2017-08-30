using CsDebugScript;
using CsDebugScript.ClrMdProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ClrException = CsDebugScript.CommonUserTypes.CLR.System.Exception;

namespace DbgEngTest.CLR
{
    [TestClass]
    [DeploymentItem(@"CLR\Apps\NestedException.cs", @"CLR\Apps")]
    [DeploymentItem(@"CLR\Apps\SharedLibrary.cs", @"CLR\Apps")]
    public class DataReaderTests : ClrTestBase
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
        public void CodeCoverageTest()
        {
            DataReader dataReader = new DataReader(Process.Current);
            Thread thread = Thread.Current;
            int contextLength = thread.ThreadContext.Bytes.Length;
            Microsoft.Diagnostics.Runtime.VirtualQueryData vq;

            Assert.IsFalse(dataReader.CanReadAsync);
            Assert.AreNotEqual(0, dataReader.EnumerateAllThreads().Count());
            Assert.IsTrue(dataReader.GetThreadContext(thread.SystemId, 0, (uint)contextLength, new byte[contextLength]));
            Assert.AreEqual(thread.TebAddress, dataReader.GetThreadTeb(thread.SystemId));
            Assert.AreEqual((uint)0, dataReader.ReadDwordUnsafe(0));
            Assert.AreEqual((ulong)0, dataReader.ReadPointerUnsafe(0));
            Assert.IsFalse(dataReader.VirtualQuery(0, out vq));

            Debugger.DontUseDumpReader = true;
            ClrException exception = Variable.CastAs<ClrException>(thread.ClrThread.LastThrownException);
            Assert.AreEqual("IOE Message", exception.Message);
            Debugger.DontUseDumpReader = false;

            dataReader.Flush();
            dataReader.Close();
        }
    }
}
