using CsDebugScript;
using CsScripts;
using DbgEngManaged;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace DbgEngTest
{
    public class TestBase
    {
        /// <summary>
        /// Opens dump file.
        /// </summary>
        /// <param name="dumpFile">Path to dump file.</param>
        /// <param name="symbolPath">Symbol path.</param>
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

        private static IDebugClient client;

        /// <summary>
        /// Initializes the test class with the specified dump file.
        /// </summary>
        /// <param name="dumpFile">The dump file.</param>
        /// <param name="symbolPath">The symbol path.</param>
        protected static void Initialize(string dumpFile, string symbolPath)
        {
            client = OpenDumpFile(dumpFile, symbolPath);
            EngineContext.Initalize(client);
        }

        protected StackFrame GetFrame(string functionName)
        {
            foreach (var frame in Thread.Current.StackTrace.Frames)
            {
                try
                {
                    if (frame.FunctionName == functionName)
                    {
                        return frame;
                    }
                }
                catch (Exception)
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            Assert.Fail(string.Format("Frame not found '{0}'", functionName));
            return null;
        }
    }
}
