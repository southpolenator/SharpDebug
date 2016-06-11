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

        private Module DefaultModule
        {
            get
            {
                return Module.All.Single(module => module.Name == DefaultModuleName);
            }
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
            Assert.IsNotNull(GetFrame($"{DefaultModuleName}!main"));
        }

        public void TestModuleExtraction()
        {
            Assert.IsTrue(Module.All.Any(module => module.Name == DefaultModuleName));
        }

        public void ReadingFloatPointTypes()
        {
            Variable doubleTest = DefaultModule.GetVariable("doubleTest");

            Assert.AreEqual(3.5, (double)doubleTest.GetField("d"));
            Assert.AreEqual(2.5, (float)doubleTest.GetField("f"));
            Assert.AreEqual(5, (int)doubleTest.GetField("i"));

            Variable doubleTest2 = Process.Current.GetGlobal($"{DefaultModuleName}!doubleTest");

            Assert.AreEqual(doubleTest.GetPointerAddress(), doubleTest2.GetPointerAddress());
            Assert.AreEqual(doubleTest.GetCodeType(), doubleTest2.GetCodeType());
        }

        public void GettingClassStaticMember()
        {
            Variable staticVariable = DefaultModule.GetVariable("MyTestClass::staticVariable");

            Assert.AreEqual((int)staticVariable, 1212121212);
        }

        public void CheckMainArguments()
        {
            StackFrame mainFrame = GetFrame($"{DefaultModuleName}!main");
            VariableCollection arguments = mainFrame.Arguments;

            Assert.IsTrue(arguments.ContainsName("argc"));
            Assert.IsTrue(arguments.ContainsName("argv"));
            Assert.AreEqual(1, (int)arguments["argc"]);

            Assert.AreEqual(2, arguments.Count);
            for (int i = 0; i < arguments.Count; i++)
            {
                Variable argument = arguments[i];

                Assert.IsFalse(argument.IsNullPointer());
            }

            Variable p;
            Assert.IsFalse(arguments.TryGetValue("p", out p));
            Assert.IsNull(arguments.Names.FirstOrDefault(n => n == "p"));
            Assert.IsNull(arguments.FirstOrDefault(a => a.GetName() == "p"));
        }

        public void CheckProcess()
        {
            Process process = Process.Current;

            Console.WriteLine("Actual processor type: {0}", process.ActualProcessorType);
            Console.WriteLine("SystemId: {0}", process.SystemId);
            Assert.AreNotSame(0, process.SystemId);
            Assert.AreNotSame(0, Process.All.Length);
            Assert.AreNotEqual(-1, process.FindMemoryRegion(DefaultModule.Address));
            Assert.AreEqual(DefaultModule.ImageName, process.ExecutableName);
            Assert.IsNotNull(process.PEB);
            Assert.IsNull(process.CurrentCLRAppDomain);
        }
    }
}
