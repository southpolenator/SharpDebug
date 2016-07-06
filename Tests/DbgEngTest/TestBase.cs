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
        /// Creates absolute paths out of given file path and symbol path.
        /// <param name="dumpFile">The dump file.</param>
        /// <param name="symbolPath">The symbol path.</param>
        /// <param name="addSymbolServer">if set to <c>true</c> symbol server will be added to the symbol path.</param>
        /// </summary>
        private static void NormalizeDebugPaths(ref string file, ref string symbolPath, bool addSymbolServer)
        {
            if (!Path.IsPathRooted(file))
            {
                file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestBase).Assembly.Location), file));
            }

            if (!Path.IsPathRooted(symbolPath))
            {
                symbolPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestBase).Assembly.Location), symbolPath));
            }

            if (addSymbolServer)
            {
                symbolPath += ";srv*";
            }
        }

        /// <summary>
        /// Initializes the test class with the specified dump file.
        /// </summary>
        /// <param name="dumpFile">The dump file.</param>
        /// <param name="symbolPath">The symbol path.</param>
        /// <param name="addSymbolServer">if set to <c>true</c> symbol server will be added to the symbol path.</param>
        protected static void InitializeDump(string dumpFile, string symbolPath, bool addSymbolServer = true)
        {
            NormalizeDebugPaths(ref dumpFile, ref symbolPath, addSymbolServer);

            client = DebugClient.OpenDumpFile(dumpFile, symbolPath);
            Context.Initalize(client);
        }

        /// <summary>
        /// Initializes the test class with the specified process file.
        /// </summary>
        /// <param name="processPath"></param>
        /// <param name="symbolPath"></param>
        /// <param name="addSymbolServer"></param>
        /// <param name="debugEngineOptions"></param>
        protected static void InitializeProcess(string processPath, string processArguments, string symbolPath, bool addSymbolServer = true, uint debugEngineOptions = (uint)(Defines.DebugEngoptInitialBreak | Defines.DebugEngoptFinalBreak))
        {
            NormalizeDebugPaths(ref processPath, ref symbolPath, addSymbolServer);

            // Disable caching.
            //
            Context.EnableUserCastedVariableCaching = false;
            Context.EnableVariableCaching = false;

            client = DebugClient.OpenProcess(processPath, processArguments, symbolPath, debugEngineOptions);
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
