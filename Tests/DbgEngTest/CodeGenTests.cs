using CsDebugScript.CodeGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DbgEngTest
{
    [TestClass]
    public class CodeGenTests
    {
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
                },
            };
        }

        [TestMethod]
        public void NativeDumpTest_x64()
        {
            DoCodeGen("NativeDumpTest.x64.pdb");
        }

        [TestMethod]
        public void NativeDumpTest_x64_Release()
        {
            DoCodeGen("NativeDumpTest.x64.Release.pdb");
        }

        [TestMethod]
        public void NativeDumpTest_x86()
        {
            DoCodeGen("NativeDumpTest.x86.pdb");
        }

        [TestMethod]
        public void NativeDumpTest_x86_Release()
        {
            DoCodeGen("NativeDumpTest.x86.Release.pdb");
        }

        private void DoCodeGen(string pdbFile)
        {
            Task mtaTask = new Task(() =>
            {
                Generator.Generate(GetXmlConfig(pdbFile));
            });

            mtaTask.Start();
            mtaTask.Wait();
        }
    }
}
