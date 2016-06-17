using CsDebugScript;
using CsDebugScript.Engine;
using DbgEngManaged;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace DbgEngTest
{
    public class TestBase
    {
        private static IDebugClient client;
        private static object synchronizationObject = new object();

        internal static void SyncStart()
        {
            System.Threading.Monitor.Enter(synchronizationObject);
        }

        internal static void SyncStop()
        {
            System.Threading.Monitor.Exit(synchronizationObject);
        }


        /// <summary>
        /// Initializes the test class with the specified dump file.
        /// </summary>
        /// <param name="dumpFile">The dump file.</param>
        /// <param name="symbolPath">The symbol path.</param>
        /// <param name="addSymbolServer">if set to <c>true</c> symbol server will be added to the symbol path.</param>
        protected static void Initialize(string dumpFile, string symbolPath, bool addSymbolServer = true)
        {
            if (!Path.IsPathRooted(dumpFile))
            {
                dumpFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestBase).Assembly.Location), dumpFile));
            }

            if (!Path.IsPathRooted(symbolPath))
            {
                symbolPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestBase).Assembly.Location), symbolPath));
            }

            if (addSymbolServer)
            {
                symbolPath += ";srv*";
            }

            client = DebugClient.OpenDumpFile(dumpFile, symbolPath);
            Context.Initalize(client);
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

            Assert.Fail($"Frame not found '{functionName}'");
            return null;
        }

        public static void CompareArrays<T>(T[] array1, T[] array2)
        {
            Assert.AreEqual(array1.Length, array2.Length);
            for (int i = 0; i < array1.Length; i++)
                Assert.AreEqual(array1[i], array2[i]);
        }
    }
}
