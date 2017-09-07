using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;
using Xunit;

namespace CsDebugScript.Tests
{
    [Collection("NativeDumpTest.x64.dmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class DbgEngDllTests : DumpTestBase
    {
        public DbgEngDllTests(NativeDumpTest_x64_dmp_Initialization initialization)
            : base(initialization)
        {
        }

        [Fact]
        public void CheckLiveDebugging()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;

            Assert.NotNull(debugger);
            Assert.False(debugger.IsLiveDebugging);
        }

        [Fact]
        public void CheckRegistersInterface()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;

            Assert.NotNull(debugger);
            Assert.NotNull(debugger.Registers);
        }

        [Fact]
        public void GetModuleLoadedImage()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            Module module = StackFrame.Current.Module;

            Assert.NotNull(debugger);
            Assert.Equal(module.LoadedImageName, debugger.GetModuleLoadedImage(module));
        }

        [Fact]
        public void GetModuleMappedImage()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            Module module = StackFrame.Current.Module;

            Assert.NotNull(debugger);
            Assert.Equal(module.MappedImageName, debugger.GetModuleMappedImage(module));
        }

        [Fact]
        public void GetStackTraceFromContext()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestDbgEngDll");
            VariableCollection locals = frame.Locals;
            Variable context = locals["context"];

            Assert.NotNull(debugger);
            Assert.NotNull(debugger.GetStackTraceFromContext(context.GetCodeType().Module.Process, context.GetPointerAddress()));
        }

        [Fact]
        public void ReadAnsiString()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestDbgEngDll");
            VariableCollection locals = frame.Locals;
            Variable testString = locals["testString"];

            Assert.NotNull(debugger);
            Assert.Equal("Testing...", debugger.ReadAnsiString(testString.GetCodeType().Module.Process, testString.GetPointerAddress()));
        }

        [Fact]
        public void ReadUnicodeString()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestDbgEngDll");
            VariableCollection locals = frame.Locals;
            Variable testString = locals["testWString"];

            Assert.NotNull(debugger);
            Assert.Equal("Testing...", debugger.ReadUnicodeString(testString.GetCodeType().Module.Process, testString.GetPointerAddress()));
        }

        [Fact]
        public void GetMemoryRegions()
        {
            DbgEngDll debugger = Context.Debugger as DbgEngDll;

            Assert.NotNull(debugger);
            Assert.True(debugger.GetMemoryRegions(Process.Current).Length > 0);
        }

        [Fact]
        public void GetLastEventInfo()
        {
            DebugEventInfo lastEvent = DebugEventInfo.LastEvent;

            Assert.NotNull(lastEvent);
            Assert.True(lastEvent.Type.HasFlag(DebugEvent.Exception));
            Assert.Equal("Access violation - code c0000005 (first/second chance not available)", lastEvent.Description);
            Assert.Equal(Process.Current, lastEvent.Process);
            Assert.Equal(Thread.Current, lastEvent.Thread);
        }
    }
}
