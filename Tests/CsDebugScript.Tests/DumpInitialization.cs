﻿using CsDebugScript.DwarfSymbolProvider;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers;
using DbgEng;
using System.IO;
using Xunit;

namespace CsDebugScript.Tests
{
    public class DumpInitialization
    {
        public const string DefaultDumpPath = @"..\..\..\dumps\";

        private static CommonUserTypes.NativeTypes.std.vector vector = InitializeCommonUserTypes();

        public DumpInitialization(string dumpPath, string defaultModuleName, string symbolPath, bool useILCodeGen = false)
        {
            if (!Path.IsPathRooted(dumpPath))
            {
                DumpPath = TestBase.GetAbsoluteBinPath(Path.Combine(DefaultDumpPath, dumpPath));
            }
            else
            {
                DumpPath = dumpPath;
            }
            DefaultModuleName = defaultModuleName;
            SymbolPath = symbolPath;
            UseILCodeGen = useILCodeGen;
        }

        public string DumpPath { get; private set; }

        public string DefaultModuleName { get; private set; }

        public string SymbolPath { get; private set; }

        public bool UseILCodeGen { get; private set; }

        public InteractiveExecution InteractiveExecution { get; private set; } = new InteractiveExecution();

        internal bool CodeGenExecuted { get; set; }

        private static CommonUserTypes.NativeTypes.std.vector InitializeCommonUserTypes()
        {
            try
            {
                return new CommonUserTypes.NativeTypes.std.vector(null);
            }
            catch
            {
                return null;
            }
        }
    }

    public class ElfCoreDumpInitialization : DumpInitialization
    {
        public ElfCoreDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath)
            : base(dumpPath, defaultModuleName, TestBase.GetAbsoluteBinPath(symbolPath))
        {
            IDebuggerEngine engine = new ElfCoreDumpDebuggingEngine(DumpPath);

            Context.InitializeDebugger(engine);
        }
    }

    public class DbgEngDumpInitialization : DumpInitialization
    {
        public DbgEngDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath, bool addSymbolServer = true, bool useDia = true, bool useDwarf = false, bool useILCodeGen = false)
            : base(dumpPath, defaultModuleName, FixSymbolPath(symbolPath, addSymbolServer), useILCodeGen)
        {
            // Clear all processes being debugged with DbgEng.dll...
            (Context.Debugger as DbgEngDll)?.Client?.EndSession(DebugEnd.ActiveTerminate);

            IDebugClient client = DebugClient.OpenDumpFile(DumpPath, SymbolPath);

            DbgEngDll.InitializeContext(client);
            if (!useDia && !useDwarf)
            {
                Context.InitializeDebugger(Context.Debugger);
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

    #region NativeDumpTest
    [CollectionDefinition("NativeDumpTest.x64.mdmp")]
    public class NativeDumpTest_x64_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_dmp_Initialization>
    {
        public NativeDumpTest_x64_dmp_Initialization()
            : base("NativeDumpTest.x64.mdmp", "NativeDumpTest_x64")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.mdmp NoDia")]
    public class NativeDumpTest_x64_dmp_NoDia_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_dmp_NoDia_Initialization>
    {
        public NativeDumpTest_x64_dmp_NoDia_Initialization()
            : base("NativeDumpTest.x64.mdmp", "NativeDumpTest_x64", useDia: false)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.mdmp ILCodeGen")]
    public class NativeDumpTest_x64_dmp_IL_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_dmp_IL_Initialization>
    {
        public NativeDumpTest_x64_dmp_IL_Initialization()
            : base("NativeDumpTest.x64.mdmp", "NativeDumpTest_x64", useILCodeGen: true)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.Release.mdmp")]
    public class NativeDumpTest_x64_Release_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_Release_dmp_Initialization>
    {
        public NativeDumpTest_x64_Release_dmp_Initialization()
            : base("NativeDumpTest.x64.Release.mdmp", "NativeDumpTest_x64_Release")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x86.mdmp")]
    public class NativeDumpTest_x86_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x86_dmp_Initialization>
    {
        public NativeDumpTest_x86_dmp_Initialization()
            : base("NativeDumpTest.x86.mdmp", "NativeDumpTest_x86")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x86.Release.mdmp")]
    public class NativeDumpTest_x86_Release_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x86_Release_dmp_Initialization>
    {
        public NativeDumpTest_x86_Release_dmp_Initialization()
            : base("NativeDumpTest.x86.Release.mdmp", "NativeDumpTest_x86_Release")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.VS2013.mdmp")]
    public class NativeDumpTest_x64_VS2013_mdmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_VS2013_mdmp_Initialization>
    {
        public NativeDumpTest_x64_VS2013_mdmp_Initialization()
            : base("NativeDumpTest.x64.VS2013.mdmp", "NativeDumpTest_x64_VS2013")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.VS2015.mdmp")]
    public class NativeDumpTest_x64_VS2015_mdmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_VS2015_mdmp_Initialization>
    {
        public NativeDumpTest_x64_VS2015_mdmp_Initialization()
            : base("NativeDumpTest.x64.VS2015.mdmp", "NativeDumpTest_x64_VS2015")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.gcc.mdmp")]
    public class NativeDumpTest_gcc_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_gcc_dmp_Initialization>
    {
        public NativeDumpTest_gcc_dmp_Initialization()
            : base("NativeDumpTest.gcc.mdmp", "NativeDumpTest_gcc", useDwarf: true)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.x64.gcc.mdmp")]
    public class NativeDumpTest_x64_gcc_Initialization : DbgEngDumpInitialization, ICollectionFixture<NativeDumpTest_x64_gcc_Initialization>
    {
        public NativeDumpTest_x64_gcc_Initialization()
            : base("NativeDumpTest.x64.gcc.mdmp", "NativeDumpTest_x64_gcc", useDwarf: true)
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x86.gcc.coredump")]
    public class NativeDumpTest_linux_x86_gcc_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x86_gcc_Initialization>
    {
        public NativeDumpTest_linux_x86_gcc_Initialization()
            : base("NativeDumpTest.linux.x86.gcc.coredump", "NativeDumpTest.linux.x86.gcc")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x64.gcc.coredump")]
    public class NativeDumpTest_linux_x64_gcc_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x64_gcc_Initialization>
    {
        public NativeDumpTest_linux_x64_gcc_Initialization()
            : base("NativeDumpTest.linux.x64.gcc.coredump", "NativeDumpTest.linux.x64.gcc")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x64.clang.coredump")]
    public class NativeDumpTest_linux_x64_clang_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x64_clang_Initialization>
    {
        public NativeDumpTest_linux_x64_clang_Initialization()
            : base("NativeDumpTest.linux.x64.clang.coredump", "NativeDumpTest.linux.x64.clang")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x86.gcc.nortti.coredump")]
    public class NativeDumpTest_linux_x86_gcc_nortti_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x86_gcc_nortti_Initialization>
    {
        public NativeDumpTest_linux_x86_gcc_nortti_Initialization()
            : base("NativeDumpTest.linux.x86.gcc.nortti.coredump", "NativeDumpTest.linux.x86.gcc.nortti")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x64.gcc.nortti.coredump")]
    public class NativeDumpTest_linux_x64_gcc_nortti_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x64_gcc_nortti_Initialization>
    {
        public NativeDumpTest_linux_x64_gcc_nortti_Initialization()
            : base("NativeDumpTest.linux.x64.gcc.nortti.coredump", "NativeDumpTest.linux.x64.gcc.nortti")
        {
        }
    }

    [CollectionDefinition("NativeDumpTest.linux.x64.clang.nortti.coredump")]
    public class NativeDumpTest_linux_x64_clang_nortti_Initialization : ElfCoreDumpInitialization, ICollectionFixture<NativeDumpTest_linux_x64_clang_nortti_Initialization>
    {
        public NativeDumpTest_linux_x64_clang_nortti_Initialization()
            : base("NativeDumpTest.linux.x64.clang.nortti.coredump", "NativeDumpTest.linux.x64.clang.nortti")
        {
        }
    }
    #endregion

    #region Cpp17Tests
    [CollectionDefinition("Cpp17.x64.mdmp")]
    public class Cpp17Tests_x64_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<Cpp17Tests_x64_dmp_Initialization>
    {
        public Cpp17Tests_x64_dmp_Initialization()
            : base("Cpp17.x64.mdmp", "Cpp17_x64")
        {
        }
    }

    [CollectionDefinition("Cpp17.x64.Release.mdmp")]
    public class Cpp17Tests_x64_Release_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<Cpp17Tests_x64_Release_dmp_Initialization>
    {
        public Cpp17Tests_x64_Release_dmp_Initialization()
            : base("Cpp17.x64.Release.mdmp", "Cpp17_x64_Release")
        {
        }
    }

    [CollectionDefinition("Cpp17.x86.mdmp")]
    public class Cpp17Tests_x86_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<Cpp17Tests_x86_dmp_Initialization>
    {
        public Cpp17Tests_x86_dmp_Initialization()
            : base("Cpp17.x86.mdmp", "Cpp17_x86")
        {
        }
    }

    [CollectionDefinition("Cpp17.x86.Release.mdmp")]
    public class Cpp17Tests_x86_Release_dmp_Initialization : DbgEngDumpInitialization, ICollectionFixture<Cpp17Tests_x86_Release_dmp_Initialization>
    {
        public Cpp17Tests_x86_Release_dmp_Initialization()
            : base("Cpp17.x86.Release.mdmp", "Cpp17_x86_Release")
        {
        }
    }

    [CollectionDefinition("Cpp17.linux.x86.gcc.coredump")]
    public class Cpp17Tests_linux_x86_gcc_Initialization : ElfCoreDumpInitialization, ICollectionFixture<Cpp17Tests_linux_x86_gcc_Initialization>
    {
        public Cpp17Tests_linux_x86_gcc_Initialization()
            : base("cpp17.linux.x86.gcc.coredump", "cpp17.linux.x86.gcc")
        {
        }
    }

    [CollectionDefinition("Cpp17.linux.x64.gcc.coredump")]
    public class Cpp17Tests_linux_x64_gcc_Initialization : ElfCoreDumpInitialization, ICollectionFixture<Cpp17Tests_linux_x64_gcc_Initialization>
    {
        public Cpp17Tests_linux_x64_gcc_Initialization()
            : base("cpp17.linux.x64.gcc.coredump", "cpp17.linux.x64.gcc")
        {
        }
    }

    [CollectionDefinition("Cpp17.linux.x64.clang.coredump")]
    public class Cpp17Tests_linux_x64_clang_Initialization : ElfCoreDumpInitialization, ICollectionFixture<Cpp17Tests_linux_x64_clang_Initialization>
    {
        public Cpp17Tests_linux_x64_clang_Initialization()
            : base("cpp17.linux.x64.clang.coredump", "cpp17.linux.x64.clang")
        {
        }
    }

    [CollectionDefinition("Cpp17.linux.x86.gcc.nortti.coredump")]
    public class Cpp17Tests_linux_x86_gcc_nortti_Initialization : ElfCoreDumpInitialization, ICollectionFixture<Cpp17Tests_linux_x86_gcc_nortti_Initialization>
    {
        public Cpp17Tests_linux_x86_gcc_nortti_Initialization()
            : base("cpp17.linux.x86.gcc.nortti.coredump", "cpp17.linux.x86.gcc.nortti")
        {
        }
    }

    [CollectionDefinition("Cpp17.linux.x64.gcc.nortti.coredump")]
    public class Cpp17Tests_linux_x64_gcc_nortti_Initialization : ElfCoreDumpInitialization, ICollectionFixture<Cpp17Tests_linux_x64_gcc_nortti_Initialization>
    {
        public Cpp17Tests_linux_x64_gcc_nortti_Initialization()
            : base("cpp17.linux.x64.gcc.nortti.coredump", "cpp17.linux.x64.gcc.nortti")
        {
        }
    }

    [CollectionDefinition("Cpp17.linux.x64.clang.nortti.coredump")]
    public class Cpp17Tests_linux_x64_clang_nortti_Initialization : ElfCoreDumpInitialization, ICollectionFixture<Cpp17Tests_linux_x64_clang_nortti_Initialization>
    {
        public Cpp17Tests_linux_x64_clang_nortti_Initialization()
            : base("cpp17.linux.x64.clang.nortti.coredump", "cpp17.linux.x64.clang.nortti")
        {
        }
    }
    #endregion
}
