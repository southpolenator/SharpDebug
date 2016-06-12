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

        [TestInitialize]
        public void TestInitialize()
        {
            Initialize(DefaultDumpFile, DefaultSymbolPath);
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
                Console.WriteLine(peb.ProcessHeap.GetPointerAddress());
                Console.WriteLine(peb.ProcessHeaps.Length);
                Console.WriteLine(peb.ProcessParameters.CommandLine);
                Console.WriteLine(peb.ProcessParameters.EnvironmentVariables.Length);
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
