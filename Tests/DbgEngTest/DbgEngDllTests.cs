using CsDebugScript;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DbgEngTest
{
    [TestClass]
    [DeploymentItem(DefaultDumpFile)]
    public class DbgEngDllTests : TestBase
    {
        internal const string DefaultDumpFile = "NativeDumpTest.x64.dmp";
        internal const string DefaultModuleName = "NativeDumpTest_x64";
        internal const string DefaultSymbolPath = @".\";

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            SyncStart();
            InitializeDump(DefaultDumpFile, DefaultSymbolPath);
            string version = DbgEngDll.ExecuteAndCapture("version");
            Console.WriteLine("Debugger version: {0}", version);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void CheckLiveDebugging()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;

            Assert.IsNotNull(debugger);
            Assert.IsFalse(debugger.IsLiveDebugging);
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void CheckRegistersInterface()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;

            Assert.IsNotNull(debugger);
            Assert.IsNotNull(debugger.Registers);
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void GetModuleLoadedImage()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            Module module = StackFrame.Current.Module;

            Assert.IsNotNull(debugger);
            Assert.AreEqual(module.LoadedImageName, debugger.GetModuleLoadedImage(module));
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void GetModuleMappedImage()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            Module module = StackFrame.Current.Module;

            Assert.IsNotNull(debugger);
            Assert.AreEqual(module.MappedImageName, debugger.GetModuleMappedImage(module));
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void GetStackTraceFromContext()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestDbgEngDll");
            VariableCollection locals = frame.Locals;
            Variable context = locals["context"];

            Assert.IsNotNull(debugger);
            Assert.IsNotNull(debugger.GetStackTraceFromContext(context.GetCodeType().Module.Process, context.GetPointerAddress()));
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void ReadAnsiString()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestDbgEngDll");
            VariableCollection locals = frame.Locals;
            Variable testString = locals["testString"];

            Assert.IsNotNull(debugger);
            Assert.AreEqual("Testing...", debugger.ReadAnsiString(testString.GetCodeType().Module.Process, testString.GetPointerAddress()));
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void ReadUnicodeString()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestDbgEngDll");
            VariableCollection locals = frame.Locals;
            Variable testString = locals["testWString"];

            Assert.IsNotNull(debugger);
            Assert.AreEqual("Testing...", debugger.ReadUnicodeString(testString.GetCodeType().Module.Process, testString.GetPointerAddress()));
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void GetMemoryRegions()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;

            Assert.IsNotNull(debugger);
            Assert.IsTrue(debugger.GetMemoryRegions(Process.Current).Length > 0);
        }

        [TestMethod]
        [TestCategory("DbgEngDll")]
        public void GetLastEventInfo()
        {
            DebugEventInfo lastEvent = DebugEventInfo.LastEvent;

            Assert.IsNotNull(lastEvent);
            Assert.IsTrue(lastEvent.Type.HasFlag(DebugEvent.Exception));
            Assert.AreEqual("Access violation - code c0000005 (first/second chance not available)", lastEvent.Description);
            Assert.AreEqual(Process.Current, lastEvent.Process);
            Assert.AreEqual(Thread.Current, lastEvent.Thread);
        }
    }
}
