using CsDebugScript.CodeGen;
using CsDebugScript.DwarfSymbolProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace DbgEngTest
{
    [TestClass]
    [DeploymentItem("NativeDumpTest.x64.pdb")]
    [DeploymentItem("NativeDumpTest.x64.Release.pdb")]
    [DeploymentItem("NativeDumpTest.x86.pdb")]
    [DeploymentItem("NativeDumpTest.x86.Release.pdb")]
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
                MultiFileExport = false,
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
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.unordered_map<${TKey},${TValue}>",
                        OriginalType = "std::map<${TKey},${TValue},${hasher},${keyEquality},${allocator}>",
                    },
                    new XmlTypeTransformation()
                    {
                        Constructor = "${new}",
                        NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.pair<${TFirst},${TSecond}>",
                        OriginalType = "std::pair<${TFirst},${TSecond}>",
                    },
                },
            };
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64()
        {
            DoCodeGen("NativeDumpTest.x64.pdb");
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_NoTransformations()
        {
            DoCodeGen("NativeDumpTest.x64.pdb", transformations: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_NoSingle()
        {
            DoCodeGen("NativeDumpTest.x64.pdb", singleFileExport: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_NoRoslyn()
        {
            DoCodeGen("NativeDumpTest.x64.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_Release()
        {
            DoCodeGen("NativeDumpTest.x64.Release.pdb");
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_Release_NoTransformations()
        {
            DoCodeGen("NativeDumpTest.x64.Release.pdb", transformations: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_Release_NoSingle()
        {
            DoCodeGen("NativeDumpTest.x64.Release.pdb", singleFileExport: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x64_Release_NoRoslyn()
        {
            DoCodeGen("NativeDumpTest.x64.Release.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86()
        {
            DoCodeGen("NativeDumpTest.x86.pdb");
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_NoTransformations()
        {
            DoCodeGen("NativeDumpTest.x86.pdb", transformations: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_NoSingle()
        {
            DoCodeGen("NativeDumpTest.x86.pdb", singleFileExport: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_NoRoslyn()
        {
            DoCodeGen("NativeDumpTest.x86.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_Release()
        {
            DoCodeGen("NativeDumpTest.x86.Release.pdb");
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_Release_NoTransformations()
        {
            DoCodeGen("NativeDumpTest.x86.Release.pdb", transformations: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_Release_NoSingle()
        {
            DoCodeGen("NativeDumpTest.x86.Release.pdb", singleFileExport: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_x86_Release_NoRoslyn()
        {
            DoCodeGen("NativeDumpTest.x86.Release.pdb", compileWithRoslyn: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.gcc.exe", useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_NoTransformations()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.gcc.exe", transformations: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_NoSingle()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.gcc.exe", singleFileExport: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_NoRoslyn()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.gcc.exe", compileWithRoslyn: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_x64()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.x64.gcc.exe", useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_x64_NoTransformations()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.x64.gcc.exe", transformations: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_x64_NoSingle()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.x64.gcc.exe", singleFileExport: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_gcc_x64_NoRoslyn()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.x64.gcc.exe", compileWithRoslyn: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_linux_gcc_x64()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.linux.x64.gcc", useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_linux_gcc_x64_NoTransformations()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.linux.x64.gcc", transformations: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_linux_gcc_x64_NoSingle()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.linux.x64.gcc", singleFileExport: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_linux_gcc_x64_NoRoslyn()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.linux.x64.gcc", compileWithRoslyn: false, useDwarf: true);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_VS2013()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.VS2013.pdb");
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_VS2013_NoTransformations()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.VS2013.pdb", transformations: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_VS2013_NoSingle()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.VS2013.pdb", singleFileExport: false);
        }

        [TestMethod]
        [TestCategory("CodeGen")]
        public void NativeDumpTest_VS2013_NoRoslyn()
        {
            DoCodeGen(@"..\..\..\dumps\NativeDumpTest.VS2013.pdb", compileWithRoslyn: false);
        }

        private void DoCodeGen(string pdbFile, bool singleFileExport = true, bool compileWithRoslyn = true, bool transformations = true, bool useDwarf = false)
        {
            XmlConfig xmlConfig = GetXmlConfig(pdbFile);

            xmlConfig.MultiFileExport = !singleFileExport;
            xmlConfig.GenerateAssemblyWithRoslyn = compileWithRoslyn;
            xmlConfig.DontSaveGeneratedCodeFiles = true;
            if (!transformations)
            {
                xmlConfig.Transformations = new XmlTypeTransformation[0];
            }
            DoCodeGen(xmlConfig, useDwarf);
        }

        private void DoCodeGen(XmlConfig xmlConfig, bool useDwarf)
        {
            TextWriter error = Console.Error;

            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    Generator generator;

                    if (!useDwarf)
                    {
                        generator = new Generator();
                    }
                    else
                    {
                        generator = new Generator(new DwarfCodeGenModuleProvider());
                    }

                    Console.SetError(writer);
                    generator.Generate(xmlConfig);
                    writer.Flush();
                    string errorText = writer.GetStringBuilder().ToString();

                    if (!string.IsNullOrEmpty(errorText))
                    {
                        throw new Exception(errorText);
                    }
                }
            }
            finally
            {
                Console.SetError(error);
            }
        }
    }
}
