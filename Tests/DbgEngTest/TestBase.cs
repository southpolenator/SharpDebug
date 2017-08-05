using CsDebugScript;
using CsDebugScript.Engine;
using DbgEngManaged;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DbgEngTest
{
    public class TestBase
    {
        private static IDebugClient client;
        private InteractiveExecution interactiveExecution = new InteractiveExecution();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, int dwFlags);
        private const int LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100;

        static TestBase()
        {
            var sysdir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var res = LoadLibraryEx(Path.Combine(sysdir, "dbgeng.dll"), IntPtr.Zero, LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR);
            if (res == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        internal static void SyncStart()
        {
        }

        internal static void SyncStop()
        {
        }

        /// <summary>
        /// Creates absolute paths out of given file path and symbol path.
        /// <param name="file">Path to dump or process.</param>
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
        /// <param name="processPath">Path to the process to be started.</param>
        /// <param name="symbolPath">Symbol path.</param>
        /// <param name="addSymbolServer">if set to <c>true</c> symbol server will be added to the symbol path.</param>
        /// <param name="debugEngineOptions">Debug create options. Default is to start in break mode, and break on process exit.</param>
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

        protected static StackFrame GetFrame(string functionName)
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
            {
                Assert.IsTrue(array2.Contains(array1[i]));
            }
        }

        public void InterpretInteractive(string code)
        {
            code = @"
StackFrame GetFrame(string functionName)
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

    throw new Exception($""Frame not found '{functionName}'"");
}

void AreEqual<T>(T value1, T value2)
    where T : IEquatable<T>
{
    if (!value1.Equals(value2))
    {
        throw new Exception($""Not equal. value1 = {value1}, value2 = {value2}"");
    }
}
                " + code;

            interactiveExecution.UnsafeInterpret(code);
        }
    }
}
