using SharpDebug.Engine;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Xunit;

namespace SharpDebug.Tests.CLR
{
    public class ClrDumpInitialization : DbgEngDumpInitialization
    {
        public ClrDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath, bool addSymbolServer = true, bool useDia = true, bool useDwarf = false, bool useILCodeGen = false)
            : base(dumpPath, defaultModuleName, FixSymbolPath(dumpPath, symbolPath), addSymbolServer, useDia, useDwarf, useILCodeGen)
        {
            Context.ClrProvider = new SharpDebug.CLR.ClrMdProvider();
        }

        private static string FixSymbolPath(string dumpPath, string symbolPath)
        {
            if (symbolPath != DefaultDumpPath)
                return symbolPath;
            symbolPath = Path.Combine(symbolPath, Path.GetDirectoryName(dumpPath));
            if (symbolPath.Last() != Path.DirectorySeparatorChar)
                symbolPath += Path.DirectorySeparatorChar;
            if (!Path.IsPathRooted(symbolPath))
                symbolPath = Path.GetFullPath(symbolPath);
            return symbolPath;
        }
    }

    public class ClrLinuxDumpInitialization : ElfCoreDumpInitialization
    {
        public ClrLinuxDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath)
            : base(dumpPath, defaultModuleName, symbolPath)
        {
            Context.ClrProvider = new SharpDebug.CLR.ClrMdProvider();
        }
    }

    public class ClrFrameworkDumpInitialization : ClrDumpInitialization
    {
        public ClrFrameworkDumpInitialization(string dumpPath, string defaultModuleName, string symbolPath = DefaultDumpPath, bool addSymbolServer = true, bool useDia = true, bool useDwarf = false, bool useILCodeGen = false)
            : base(FixDumpVersion(dumpPath), defaultModuleName, symbolPath, addSymbolServer, useDia, useDwarf, useILCodeGen)
        {
        }

        private static string FixDumpVersion(string dumpPath)
        {
            if (IntPtr.Size == 4)
                return dumpPath.Replace(".x64", ".x86");
            return dumpPath;
        }
    }

    [CollectionDefinition("CLR AppDomains")]
    public class ClrAppDomainsInitialization : ClrFrameworkDumpInitialization, ICollectionFixture<ClrAppDomainsInitialization>
    {
        public ClrAppDomainsInitialization()
            : base(Path.Combine("Clr.Windows.x64", "AppDomains.x64.mdmp"), "AppDomains")
        {
        }
    }

    [CollectionDefinition("CLR LocalVariables")]
    public class ClrLocalVariablesInitialization : ClrFrameworkDumpInitialization, ICollectionFixture<ClrLocalVariablesInitialization>
    {
        public ClrLocalVariablesInitialization()
            : base(Path.Combine("Clr.Windows.x64", "LocalVariables.x64.mdmp"), "LocalVariables")
        {
        }
    }

    [CollectionDefinition("CLR LocalVariables Windows Core")]
    public class ClrLocalVariablesWindowsCoreInitialization : ClrDumpInitialization, ICollectionFixture<ClrLocalVariablesWindowsCoreInitialization>
    {
        public ClrLocalVariablesWindowsCoreInitialization()
            : base(Path.Combine("Clr.Windows.Core", "LocalVariables.Core.mdmp"), "LocalVariables")
        {
        }
    }

    [CollectionDefinition("CLR LocalVariables Linux Core")]
    public class ClrLocalVariablesLinuxCoreInitialization : ClrLinuxDumpInitialization, ICollectionFixture<ClrLocalVariablesLinuxCoreInitialization>
    {
        public ClrLocalVariablesLinuxCoreInitialization()
            : base(Path.Combine("Clr.linux", "LocalVariables.linux.coredump"), "LocalVariables")
        {
        }
    }

    [CollectionDefinition("CLR NestedException")]
    public class ClrNestedExceptionInitialization : ClrFrameworkDumpInitialization, ICollectionFixture<ClrNestedExceptionInitialization>
    {
        public ClrNestedExceptionInitialization()
            : base(Path.Combine("Clr.Windows.x64", "NestedException.x64.mdmp"), "NestedException")
        {
        }
    }

    [CollectionDefinition("CLR NestedException Windows Core")]
    public class ClrNestedExceptionWindowsCoreInitialization : ClrDumpInitialization, ICollectionFixture<ClrNestedExceptionWindowsCoreInitialization>
    {
        public ClrNestedExceptionWindowsCoreInitialization()
            : base(Path.Combine("Clr.Windows.Core", "NestedException.Core.mdmp"), "NestedException")
        {
        }
    }

    [CollectionDefinition("CLR NestedException Linux Core")]
    public class ClrNestedExceptionLinuxCoreInitialization : ClrLinuxDumpInitialization, ICollectionFixture<ClrNestedExceptionLinuxCoreInitialization>
    {
        public ClrNestedExceptionLinuxCoreInitialization()
            : base(Path.Combine("Clr.linux", "NestedException.linux.coredump"), "NestedException")
        {
        }
    }

    [CollectionDefinition("CLR Types")]
    public class ClrTypesInitialization : ClrFrameworkDumpInitialization, ICollectionFixture<ClrTypesInitialization>
    {
        public ClrTypesInitialization()
            : base(Path.Combine("Clr.Windows.x64", "Types.x64.mdmp"), "Types")
        {
        }
    }

    [CollectionDefinition("CLR Types Windows Core")]
    public class ClrTypesWindowsCoreInitialization : ClrDumpInitialization, ICollectionFixture<ClrTypesWindowsCoreInitialization>
    {
        public ClrTypesWindowsCoreInitialization()
            : base(Path.Combine("Clr.Windows.Core", "Types.Core.mdmp"), "Types")
        {
        }
    }

    [CollectionDefinition("CLR Types Linux Core")]
    public class ClrTypesLinuxCoreInitialization : ClrLinuxDumpInitialization, ICollectionFixture<ClrTypesLinuxCoreInitialization>
    {
        public ClrTypesLinuxCoreInitialization()
            : base(Path.Combine("Clr.linux", "Types.linux.coredump"), "Types")
        {
        }
    }

    [CollectionDefinition("CLR Types Server")]
    public class ClrTypesServerInitialization : ClrFrameworkDumpInitialization, ICollectionFixture<ClrTypesServerInitialization>
    {
        public ClrTypesServerInitialization()
            : base(Path.Combine("Clr.Windows.x64", "Types.x64.ServerGC.mdmp"), "Types")
        {
        }
    }

    [CollectionDefinition("CLR Types Workstation")]
    public class ClrTypesWorkstationInitialization : ClrFrameworkDumpInitialization, ICollectionFixture<ClrTypesWorkstationInitialization>
    {
        public ClrTypesWorkstationInitialization()
            : base(Path.Combine("Clr.Windows.x64", "Types.x64.mdmp"), "Types")
        {
        }
    }
}
