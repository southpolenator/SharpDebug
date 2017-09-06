using CsDebugScript.Engine;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Xunit;

namespace CsDebugScript.Tests.CLR
{
    public class ClrDumpInitialization : DbgEngDumpInitialization
    {
        public ClrDumpInitialization(string appName, string customDumpName = null, bool serverGC = false)
            : base(GetDumpPath(appName, customDumpName, serverGC), appName, GetDumpDirectory())
        {
            Context.ClrProvider = new CsDebugScript.CLR.ClrMdProvider();
        }

        private static string GetDumpPath(string appName, string customDumpName, bool serverGC)
        {
            Environment.SetEnvironmentVariable("COMPlus_BuildFlavor", serverGC ? "svr" : "");

            string programName = appName;

            if (!string.IsNullOrEmpty(customDumpName))
            {
                programName = Path.GetFileNameWithoutExtension(customDumpName);
            }

            string appPath = CompileApp(programName, appName + ".cs", "SharedLibrary.cs");
            string appDirectory = Path.GetDirectoryName(appPath);
            string dumpPath = Path.Combine(appDirectory, Path.GetFileNameWithoutExtension(appName) + ".mdmp");

            if (!string.IsNullOrEmpty(customDumpName))
            {
                dumpPath = Path.Combine(appDirectory, customDumpName);
            }

            if (!File.Exists(dumpPath) || File.GetLastWriteTimeUtc(appPath) > File.GetLastWriteTimeUtc(dumpPath))
            {
                ExceptionDumper.Dumper.RunAndDumpOnException(appPath, dumpPath, false);
            }
            return dumpPath;
        }

        private static string GetDumpDirectory()
        {
            return TestBase.GetAbsoluteBinPath(IntPtr.Size.ToString());
        }

        private static string CompileApp(string appName, params string[] files)
        {
            string directory = TestBase.GetBinFolder();
            string destinationDirectory = GetDumpDirectory();
            string destination = Path.Combine(destinationDirectory, appName + ".exe");
            string pdbPath = Path.Combine(destinationDirectory, appName + ".pdb");
            string[] fullPathFiles = files.Select(f => Path.Combine(directory, "CLR", "Apps", f)).ToArray();

            Directory.CreateDirectory(destinationDirectory);

            // Check if we need to compile at all
            if (File.Exists(destination) && File.Exists(pdbPath))
            {
                bool upToDate = true;

                foreach (var file in fullPathFiles)
                {
                    if (File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(destination))
                    {
                        upToDate = false;
                        break;
                    }
                }

                if (upToDate)
                {
                    return destination;
                }
            }

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("system.dll");
            cp.GenerateInMemory = false;
            cp.GenerateExecutable = true;
            cp.CompilerOptions = IntPtr.Size == 4 ? "/platform:x86" : "/platform:x64";
            cp.IncludeDebugInformation = true;
            cp.OutputAssembly = destination;
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, fullPathFiles);

            if (cr.Errors.Count > 0 && System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }

            Assert.Equal(0, cr.Errors.Count);

            return cr.PathToAssembly;
        }
    }

    [CollectionDefinition("CLR AppDomains")]
    public class ClrAppDomainsInitialization : ClrDumpInitialization, ICollectionFixture<ClrAppDomainsInitialization>
    {
        public ClrAppDomainsInitialization()
            : base(GetAppName())
        {
        }

        private static string GetAppName()
        {
            ClrNestedExceptionInitialization initialization = new ClrNestedExceptionInitialization();

            return "AppDomains";
        }
    }

    [CollectionDefinition("CLR LocalVariables")]
    public class ClrLocalVariablesInitialization : ClrDumpInitialization, ICollectionFixture<ClrLocalVariablesInitialization>
    {
        public ClrLocalVariablesInitialization()
            : base("LocalVariables")
        {
        }
    }

    [CollectionDefinition("CLR NestedException")]
    public class ClrNestedExceptionInitialization : ClrDumpInitialization, ICollectionFixture<ClrNestedExceptionInitialization>
    {
        public ClrNestedExceptionInitialization()
            : base("NestedException")
        {
        }
    }

    [CollectionDefinition("CLR Types")]
    public class ClrTypesInitialization : ClrDumpInitialization, ICollectionFixture<ClrTypesInitialization>
    {
        public ClrTypesInitialization()
            : base("Types")
        {
        }
    }

    [CollectionDefinition("CLR Types Server")]
    public class ClrTypesServerInitialization : ClrDumpInitialization, ICollectionFixture<ClrTypesServerInitialization>
    {
        public ClrTypesServerInitialization()
            : base("Types", "TypesServerGC.mdmp", serverGC: true)
        {
        }
    }

    [CollectionDefinition("CLR Types Workstation")]
    public class ClrTypesWorkstationInitialization : ClrDumpInitialization, ICollectionFixture<ClrTypesWorkstationInitialization>
    {
        public ClrTypesWorkstationInitialization()
            : base("Types", "TypesWorkstation.mdmp")
        {
        }
    }
}
