using CsScriptManaged;
using CsScripts;
using DbgEngManaged;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying varios functionalities of CsScript against NativeDumpTest.exe.
    /// </summary>
    [TestClass]
    public class NativeDumpTest
    {
        /// <summary>
        /// Opens dump file.
        /// </summary>
        /// <param name="dumpFile">Path to dump file.</param>
        /// <param name="symbolPath">Symbol path.</param>
        /// <returns></returns>
        private static IDebugClient OpenDumpFile(string dumpFile, string symbolPath)
        {
            IDebugClient client;
            int hresult = DebugCreate(Marshal.GenerateGuidForType(typeof(IDebugClient)), out client);

            if (hresult > 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            client.OpenDumpFile(dumpFile);
            ((IDebugControl7)client).WaitForEvent(0, uint.MaxValue);
            ((IDebugSymbols5)client).SetSymbolPathWide(symbolPath);
            return client;
        }

        /// <summary>
        /// PInvoke to DebugCreate in dbgeng.dll.
        /// </summary>
        /// <param name="iid"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", SetLastError = false)]
        private static extern int DebugCreate(Guid iid, out IDebugClient client);

        private const string DefaultDumpFile = "NativeDump1.dmp";

        private const string DefaultSymbolPath = @"srv*;.\";

        private static IDebugClient client;

        [ClassInitialize]
        static public void TestSetup(TestContext context)
        {
            client = OpenDumpFile(DefaultDumpFile, DefaultSymbolPath);
            Context.Initalize(client);
        }

        [TestMethod]
        public void TestThreadCount()
        {
            Assert.AreEqual(Thread.All.Length, 1, "Thread count should equal 1");
        }

        [TestMethod]
        public void TestStackLength()
        {
            Assert.AreEqual(Thread.Current.StackTrace.Frames.Length, 8, "Stack length should equal 1");
        }

        [TestMethod]
        public void TestCurrentFrame()
        {
            Assert.IsTrue(Thread.Current.StackTrace.CurrentFrame.SourceFileName.Contains("nativedumptest.cpp"));
        }

        [TestMethod]
        public void TestModuleExtraction()
        {
            Assert.AreEqual(Module.All.Length, 11, "Invalid number of open modules.");
            Assert.IsTrue(Module.All.Any(module => module.ImageName.Contains("NativeDump")));
        }
    }
}
