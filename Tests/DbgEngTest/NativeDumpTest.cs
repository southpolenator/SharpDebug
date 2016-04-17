using CsScriptManaged;
using CsScripts;
using DbgEngManaged;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.exe.
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

            if (hresult < 0)
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

        private const string DefaultDumpFile = "NativeDumpTest.dmp";

        private const string DefaultSymbolPath = @"srv*;.\";

        private static IDebugClient client;

        [ClassInitialize]
        static public void TestSetup(TestContext context)
        {
            client = OpenDumpFile(DefaultDumpFile, DefaultSymbolPath);
            Context.Initalize(client);
        }

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestCpp()
        {
            foreach (var frame in Thread.Current.StackTrace.Frames)
            {
                try
                {
                    if (frame.SourceFileName.EndsWith("nativedumptest.cpp"))
                        return;
                }
                catch (Exception)
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            Assert.Fail("nativedumptest.cpp not found on the current thread stack trace");
        }

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            foreach (var frame in Thread.Current.StackTrace.Frames)
            {
                try
                {
                    if (frame.FunctionName == "NativeDumpTest!main")
                        return;
                }
                catch (Exception)
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            Assert.Fail("nativedumptest.cpp not found on the current thread stack trace");
        }

        [TestMethod]
        public void TestModuleExtraction()
        {
            Assert.IsTrue(Module.All.Any(module => module.ImageName.Contains("NativeDump")));
        }
    }
}
