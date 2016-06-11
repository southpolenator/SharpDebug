using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.exe.
    /// </summary>
    public class NativeDumpTest : TestBase
    {
        private readonly string DefaultDumpFile;
        private readonly string DefaultModuleName;
        private readonly string DefaultSymbolPath;

        public NativeDumpTest(string defaultDumpFile, string defaultModuleName, string defaultSymbolPath)
        {
            DefaultDumpFile = defaultDumpFile;
            DefaultModuleName = defaultModuleName;
            DefaultSymbolPath = defaultSymbolPath;
        }

        public void TestSetup()
        {
            Initialize(DefaultDumpFile, DefaultSymbolPath);
        }

        public void CurrentThreadContainsNativeDumpTestCpp()
        {
            const string sourceFileName = "nativedumptest.cpp";

            foreach (var frame in Thread.Current.StackTrace.Frames)
            {
                try
                {
                    if (frame.SourceFileName.ToLower().EndsWith(sourceFileName))
                        return;
                }
                catch (Exception)
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            Assert.Fail($"{sourceFileName} not found on the current thread stack trace");
        }

        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            Assert.AreNotEqual(GetFrame($"{DefaultModuleName}!main"), null);
        }

        public void TestModuleExtraction()
        {
            Assert.IsTrue(Module.All.Any(module => module.Name == DefaultModuleName));
        }
    }
}
