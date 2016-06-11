﻿using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.exe.
    /// </summary>
    [TestClass]
    public class NativeDumpTest : TestBase
    {
        private const string DefaultDumpFile = "NativeDumpTest.x64.dmp";

        private const string DefaultModuleName = "NativeDumpTest_x64";

        private const string DefaultSymbolPath = @".\";

        [TestInitialize]
        public void TestSetup()
        {
            Initialize(DefaultDumpFile, DefaultSymbolPath);
        }

        [TestMethod]
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

        [TestMethod]
        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            Assert.AreNotEqual(GetFrame($"{DefaultModuleName}!main"), null);
        }

        [TestMethod]
        public void TestModuleExtraction()
        {
            Assert.IsTrue(Module.All.Any(module => module.Name == DefaultModuleName));
        }
    }
}
