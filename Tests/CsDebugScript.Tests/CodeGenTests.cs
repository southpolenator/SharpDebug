using CsDebugScript.CodeGen;
using CsDebugScript.DwarfSymbolProvider;
using CsDebugScript.PdbSymbolProvider;
using System;
using System.IO;
using Xunit;

namespace CsDebugScript.Tests
{
    public enum CodeGenSymbolProvider
    {
        DIA,
        Dwarf,
        PdbReader,
    }

    [Trait("x64", "true")]
    public class CodeGenTests : TestBase
    {
        [Theory]
        [InlineData("NativeDumpTest.x64.pdb", true, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", false, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", true, false, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", true, true, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", true, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", false, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", true, false, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", true, true, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", false, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, false, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, true, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", false, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, false, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", true, true, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", true, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", false, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", true, false, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", true, true, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", true, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", false, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", true, false, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", true, true, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", false, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, false, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, true, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", false, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, false, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", true, true, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", false, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, false, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, true, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", false, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, false, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", true, true, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", false, true, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, false, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, true, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", false, true, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, false, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", true, true, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.gcc.exe", true, true, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", false, true, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", true, false, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", true, true, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", true, true, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", false, true, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", true, false, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", true, true, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", true, true, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", false, true, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", true, false, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", true, true, false, CodeGenSymbolProvider.Dwarf)]
        public void Generation(string pdbFile, bool singleFileExport, bool compileWithRoslyn, bool transformations, CodeGenSymbolProvider symbolProvider)
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

                    switch (symbolProvider)
                    {
                        case CodeGenSymbolProvider.DIA:
                            generator = new Generator();
                            break;
                        case CodeGenSymbolProvider.Dwarf:
                            generator = new Generator(new DwarfCodeGenModuleProvider());
                            break;
                        case CodeGenSymbolProvider.PdbReader:
                            generator = new Generator(new PdbModuleProvider());
                            break;
                        default:
                            throw new NotImplementedException();
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

        private static XmlConfig GetXmlConfig(string pdbFile)
        {
            pdbFile = GetAbsoluteBinPath(pdbFile);

            return new XmlConfig()
            {
                UseDirectClassAccess = true,
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
