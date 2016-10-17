using CsDebugScript.CodeGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DbgEngTest
{
    [TestClass]
    public class CodeGenTests : TestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            SyncStart();
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        public static XmlConfig GetXmlConfig(string pdbFile)
        {
            if (!Path.IsPathRooted(pdbFile))
            {
                pdbFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), pdbFile));
            }

            return new XmlConfig()
            {
                UseDiaSymbolProvider = true,
                SingleFileExport = true,
                GeneratedAssemblyName = "NativeDumpTestExported.dll",
                GeneratedPropsFileName = "NativeDumpTestExported.props",
                GenerateAssemblyWithRoslyn = true,
                DisablePdbGeneration = true,
                GeneratePhysicalMappingOfUserTypes = true,
                CommonTypesNamespace = "NativeDumpTest",
                DontSaveGeneratedCodeFiles = false,
                Modules = new[]
                {
                    new XmlModule()
                    {
                        Name = "NativeDumpTest",
                        Namespace = "NativeDumpTest",
                        PdbPath = pdbFile,
                    }
                },
                ReferencedAssemblies = new[]
                {
                    new XmlReferencedAssembly()
                    {
                        Path = "CsDebugScript.Engine.dll",
                    },
                    new XmlReferencedAssembly()
                    {
                        Path = "CsDebugScript.CommonUserTypes.dll",
                    },
                },
                Types = new XmlType[0],
                IncludedFiles = new XmlIncludedFile[0],
                Transformations = new[]
                {
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.wstring",
                        OriginalType = "std::basic_string<wchar_t,${char_traits},${allocator}>",
                    },
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.wstring",
                        OriginalType = "std::basic_string<unsigned short,${char_traits},${allocator}>",
                    },
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.@string",
                        OriginalType = "std::basic_string<char,${char_traits},${allocator}>",
                    },
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.vector<${T}>",
                        OriginalType = "std::vector<${T},${allocator}>",
                    },
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.list<${T}>",
                        OriginalType = "std::list<${T},${allocator}>",
                    },
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.map<${TKey},${TValue}>",
                        OriginalType = "std::map<${TKey},${TValue},${comparator},${allocator}>",
                    },
                },
            };
        }

        [TestMethod]
        public void NativeDumpTest_x64()
        {
            DoCodeGen("NativeDumpTest.x64.pdb");
            DoCodeGen("NativeDumpTest.x64.pdb", singleFileExport: false);
            DoCodeGen("NativeDumpTest.x64.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        public void NativeDumpTest_x64_Release()
        {
            DoCodeGen("NativeDumpTest.x64.Release.pdb");
            DoCodeGen("NativeDumpTest.x64.Release.pdb", singleFileExport: false);
            DoCodeGen("NativeDumpTest.x64.Release.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        public void NativeDumpTest_x86()
        {
            DoCodeGen("NativeDumpTest.x86.pdb");
            DoCodeGen("NativeDumpTest.x86.pdb", singleFileExport: false);
            DoCodeGen("NativeDumpTest.x86.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        public void NativeDumpTest_x86_Release()
        {
            DoCodeGen("NativeDumpTest.x86.Release.pdb");
            DoCodeGen("NativeDumpTest.x86.Release.pdb", singleFileExport: false);
            DoCodeGen("NativeDumpTest.x86.Release.pdb", compileWithRoslyn: false);
        }

        private void DoCodeGen(string pdbFile, bool singleFileExport = true, bool compileWithRoslyn = true)
        {
            XmlConfig xmlConfig = GetXmlConfig(pdbFile);

            xmlConfig.SingleFileExport = singleFileExport;
            xmlConfig.GenerateAssemblyWithRoslyn = compileWithRoslyn;
            DoCodeGen(xmlConfig);
        }

        private void DoCodeGen(XmlConfig xmlConfig)
        {
            TextWriter error = Console.Error;

            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    Console.SetError(writer);

                    Task mtaTask = new Task(() =>
                    {
                        Generator.Generate(xmlConfig);
                    });

                    mtaTask.Start();
                    mtaTask.Wait();

                    writer.Flush();
                    string errorText = writer.GetStringBuilder().ToString();

                    if (!string.IsNullOrEmpty(errorText))
                        throw new Exception(errorText);
                }
            }
            finally
            {
                Console.SetError(error);
            }
        }
    }
}
