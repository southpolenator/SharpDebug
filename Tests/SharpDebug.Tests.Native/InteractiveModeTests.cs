﻿using SharpDebug.Engine;
using SharpDebug.Engine.Debuggers;
using Xunit;

namespace SharpDebug.Tests.Native
{
    [Collection("NativeDumpTest.x64.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class InteractiveModeTests : DumpTestBase
    {
        public InteractiveModeTests(NativeDumpTest_x64_dmp_Initialization initialization)
            : base(initialization)
        {
        }

        [Fact]
        public void DynamicTest()
        {
            InterpretInteractive("dynamic a = 5");
            InterpretInteractive("Console.WriteLine(a);");
            InterpretInteractive("Dump(a);");
            InterpretInteractive("a");
        }

        [Fact]
        public void ScriptBaseTest()
        {
            InterpretInteractive("ListCommands();");
            InterpretInteractive("ListAllCommands();");
            InterpretInteractive("ListVariables();");
            InterpretInteractive("ChangeBaseClass<InteractiveScriptBase>();");
            InterpretInteractive("exit");
        }

        [Fact]
        public void DebuggerCommand()
        {
            InterpretInteractive("#dbg k");
        }

        private void InterpretInteractive(string code)
        {
            DbgEngDll dbgEngDll = Context.Debugger as DbgEngDll;

            dbgEngDll.ExecuteAction(() => DumpInitialization.InteractiveExecution.Interpret(code));
        }
    }
}
