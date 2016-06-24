using CsDebugScript;
using CsDebugScript.CommonUserTypes;
using CsDebugScript.CommonUserTypes.NativeTypes.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DbgEngTest
{
    [TestClass]
    public class UserTypeTests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;
        private const string DefaultModuleName = NativeDumpTest64.DefaultModuleName;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            SyncStart();
            Initialize(DefaultDumpFile, DefaultSymbolPath);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        public void TestNakedPointer()
        {
            NakedPointer nakedPointer = new NakedPointer(0);

            Assert.IsTrue(nakedPointer.IsNull);
            Assert.IsTrue(nakedPointer.CastAs<int>().IsNull);
        }

        [TestMethod]
        public void TestPEB()
        {
            ProcessEnvironmentBlock peb = new ProcessEnvironmentBlock();

            try
            {
                Console.WriteLine(peb.BeingDebugged);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessHeap.GetPointerAddress());
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessHeaps.Length);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessParameters.CommandLine);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessParameters.EnvironmentVariables.Length);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessParameters.ImagePathName);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }
        }

        [TestMethod]
        public void TestTEB()
        {
            ThreadEnvironmentBlock teb = new ThreadEnvironmentBlock(Thread.Current.TEB);

            try
            {
                Console.WriteLine(teb.PEB.GetPointerAddress());
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }
        }
    }
}
