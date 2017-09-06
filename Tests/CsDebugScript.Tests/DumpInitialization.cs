using CsDebugScript.DwarfSymbolProvider;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;
using Xunit;

namespace CsDebugScript.Tests
{
    public class DumpInitialization
    {
        public DumpInitialization(string dumpPath, string defaultModuleName, string symbolPath)
        {
            DumpPath = TestBase.GetAbsoluteBinPath(dumpPath);
            DefaultModuleName = defaultModuleName;
            SymbolPath = symbolPath;
        }

        public string DumpPath { get; private set; }

        public string DefaultModuleName { get; private set; }

        public string SymbolPath { get; private set; }

        public InteractiveExecution InteractiveExecution { get; private set; } = new InteractiveExecution();

        internal bool CodeGenExecuted { get; set; }
    }

    public class ElfCoreDumpInitialization : DumpInitialization
    {
        public ElfCoreDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath)
            : base(dumpPath, defaultModuleName, symbolPath)
        {
            IDebuggerEngine engine = new ElfCoreDumpDebuggingEngine(DumpPath);

            Context.InitializeDebugger(engine, engine.CreateDefaultSymbolProvider());
        }
    }

    public class DbgEngDumpInitialization : DumpInitialization
    {
        public DbgEngDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = @".\", bool addSymbolServer = true, bool useDia = true, bool useDwarf = false)
            : base(dumpPath, defaultModuleName, FixSymbolPath(symbolPath, addSymbolServer))
        {
            IDebugClient client = DebugClient.OpenDumpFile(DumpPath, symbolPath);

            DbgEngDll.InitializeContext(client);
            if (!useDia && !useDwarf)
            {
                Context.InitializeDebugger(Context.Debugger, Context.Debugger.CreateDefaultSymbolProvider());
            }
            else if (useDwarf)
            {
                Context.InitializeDebugger(Context.Debugger, new DwarfSymbolProvider.DwarfSymbolProvider());
            }
        }

        private static string FixSymbolPath(string symbolPath, bool addSymbolServer)
        {
            symbolPath = TestBase.GetAbsoluteBinPath(symbolPath);
            if (addSymbolServer)
            {
                symbolPath += ";srv*";
            }
            return symbolPath;
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.dmp")]
    public class NativeDumpTest_x64_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_dmp_Initialization>
    {
        public NativeDumpTest_x64_dmp_Initialization()
            : base("NativeDumpTest.x64.dmp", "NativeDumpTest_x64")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.dmp NoDia")]
    public class NativeDumpTest_x64_dmp_NoDia_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_dmp_NoDia_Initialization>
    {
        public NativeDumpTest_x64_dmp_NoDia_Initialization()
            : base("NativeDumpTest.x64.dmp", "NativeDumpTest_x64", useDia: false)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.Release.dmp")]
    public class NativeDumpTest_x64_Release_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_Release_dmp_Initialization>
    {
        public NativeDumpTest_x64_Release_dmp_Initialization()
            : base("NativeDumpTest.x64.Release.dmp", "NativeDumpTest_x64_Release")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x86.dmp")]
    public class NativeDumpTest_x86_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x86_dmp_Initialization>
    {
        public NativeDumpTest_x86_dmp_Initialization()
            : base("NativeDumpTest.x86.dmp", "NativeDumpTest_x86")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x86.Release.dmp")]
    public class NativeDumpTest_x86_Release_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x86_Release_dmp_Initialization>
    {
        public NativeDumpTest_x86_Release_dmp_Initialization()
            : base("NativeDumpTest.x86.Release.dmp", "NativeDumpTest_x86_Release")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.VS2013.mdmp")]
    public class NativeDumpTest_VS2013_mdmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_VS2013_mdmp_Initialization>
    {
        public NativeDumpTest_VS2013_mdmp_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.VS2013.mdmp", "NativeDumpTest_VS2013", @"..\..\..\dumps\")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.gcc.mdmp")]
    public class NativeDumpTest_gcc_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_gcc_dmp_Initialization>
    {
        public NativeDumpTest_gcc_dmp_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.gcc.mdmp", "NativeDumpTest_gcc", @"..\..\..\dumps\", useDwarf: true)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.clang.mdmp")]
    public class NativeDumpTest_x64_clang_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_clang_Initialization>
    {
        public NativeDumpTest_x64_clang_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.x64.clang.mdmp", "NativeDumpTest_x64_clang", @"..\..\..\dumps\")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.gcc.mdmp")]
    public class NativeDumpTest_x64_gcc_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_gcc_Initialization>
    {
        public NativeDumpTest_x64_gcc_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.x64.gcc.mdmp", "NativeDumpTest_x64_gcc", @"..\..\..\dumps\", useDwarf: true)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x86.gcc.coredump")]
    public class NativeDumpTest_linux_x86_gcc_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x86_gcc_Initialization>
    {
        public NativeDumpTest_linux_x86_gcc_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.linux.x86.gcc.coredump", "NativeDumpTest.linux.x86.gcc", @"..\..\..\dumps\")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x64.gcc.coredump")]
    public class NativeDumpTest_linux_x64_gcc_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x64_gcc_Initialization>
    {
        public NativeDumpTest_linux_x64_gcc_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.linux.x64.gcc.coredump", "NativeDumpTest.linux.x64.gcc", @"..\..\..\dumps\")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x64.clang.coredump")]
    public class NativeDumpTest_linux_x64_clang_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x64_clang_Initialization>
    {
        public NativeDumpTest_linux_x64_clang_Initialization()
            : base(@"..\..\..\dumps\NativeDumpTest.linux.x64.clang.coredump", "NativeDumpTest.linux.x64.clang", @"..\..\..\dumps\")
        {
        }
    }
}
