using CsDebugScript.CodeGen;
using CsDebugScript.DwarfSymbolProvider;
using System;
using System.IO;
using Xunit;

namespace CsDebugScript.Tests
{
    [Trait("x64", "true")]
    public class CodeGenTests : TestBase
    {
        [Theory]
        [InlineData("NativeDumpTest.x64.pdb", true, true, true, false)]
        [InlineData("NativeDumpTest.x64.pdb", false, true, true, false)]
        [InlineData("NativeDumpTest.x64.pdb", true, false, true, false)]
        [InlineData("NativeDumpTest.x64.pdb", true, true, false, false)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, true, true, false)]
        [InlineData("NativeDumpTest.x64.Release.pdb", false, true, true, false)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, false, true, false)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, true, false, false)]
        [InlineData("NativeDumpTest.x86.pdb", true, true, true, false)]
        [InlineData("NativeDumpTest.x86.pdb", false, true, true, false)]
        [InlineData("NativeDumpTest.x86.pdb", true, false, true, false)]
        [InlineData("NativeDumpTest.x86.pdb", true, true, false, false)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, true, true, false)]
        [InlineData("NativeDumpTest.x86.Release.pdb", false, true, true, false)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, false, true, false)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, true, false, false)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, true, true, false)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", false, true, true, false)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, false, true, false)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, true, false, false)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, true, true, false)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", false, true, true, false)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, false, true, false)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, true, false, false)]
        [InlineData("NativeDumpTest.gcc.exe", true, true, true, true)]
        [InlineData("NativeDumpTest.gcc.exe", false, true, true, true)]
        [InlineData("NativeDumpTest.gcc.exe", true, false, true, true)]
        [InlineData("NativeDumpTest.gcc.exe", true, true, false, true)]
        [InlineData("NativeDumpTest.x64.gcc.exe", true, true, true, true)]
        [InlineData("NativeDumpTest.x64.gcc.exe", false, true, true, true)]
        [InlineData("NativeDumpTest.x64.gcc.exe", true, false, true, true)]
        [InlineData("NativeDumpTest.x64.gcc.exe", true, true, false, true)]
        [InlineData("NativeDumpTest.linux.x64.gcc", true, true, true, true)]
        [InlineData("NativeDumpTest.linux.x64.gcc", false, true, true, true)]
        //[InlineData("NativeDumpTest.linux.x64.gcc", true, false, true, true)] // TODO: Once ModuleGlobals are implemented for DwarfSymbolProvider, enable this test case...
        [InlineData("NativeDumpTest.linux.x64.gcc", true, true, false, true)]
        public void Generation(string pdbFile, bool singleFileExport, bool compileWithRoslyn, bool transformations, bool useDwarf)
        {
            // Generate CodeGen configuration
            XmlConfig xmlConfig = GetXmlConfig(Path.Combine(DumpInitialization.DefaultDumpPath, pdbFile));

            xmlConfig.MultiFileExport = !singleFileExport;
            xmlConfig.GenerateAssemblyWithRoslyn = compileWithRoslyn;
            xmlConfig.DontSaveGeneratedCodeFiles = true;
            if (!transformations)
            {
                xmlConfig.Transformations = new XmlTypeTransformation[0];
            }

            // Execute CodeGen
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
                    ExecuteMTA(() => { generator.Generate(xmlConfig); });
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

        private static XmlConfig GetXmlConfig(string pdbFile)
        {
            pdbFile = GetAbsoluteBinPath(pdbFile);

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
                        Path = GetAbsoluteBinPath("CsDebugScript.Engine.dll"),
                    },
                    new XmlReferencedAssembly()
                    {
                        Path = GetAbsoluteBinPath("CsDebugScript.CommonUserTypes.dll"),
                    },
                },
                Types = new XmlType[0],
                IncludedFiles = new XmlIncludedFile[0],
                Transformations = ScriptExecution.DefaultTransformations,
            };
        }
    }
}
