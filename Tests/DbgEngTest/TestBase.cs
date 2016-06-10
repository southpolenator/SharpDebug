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

        /// <summary>
        /// Initializes the test class with the specified dump file.
        /// </summary>
        /// <param name="dumpFile">The dump file.</param>
        /// <param name="symbolPath">The symbol path.</param>
        protected static void Initialize(string dumpFile, string symbolPath)
        {
            if (!Path.IsPathRooted(dumpFile))
            {
                dumpFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dumpFile));
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

            Assert.Fail(string.Format("Frame not found '{0}'", functionName));
            return null;
        }
    }
}
