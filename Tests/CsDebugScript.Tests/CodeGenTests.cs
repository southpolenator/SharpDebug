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

    public enum CodeGenCodeOutput
    {
        CSharp_SingleFile_Roslyn,
        CSharp_SingleFile,
        CSharp_MultipleFiles_Roslyn,
        CSharp_MultipleFiles,
        ILWriter,
    }

    [Trait("x64", "true")]
    public class CodeGenTests : TestBase
    {
        [Theory]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_SingleFile_Roslyn, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.Dwarf)]
        public void Generation(string pdbFile, CodeGenCodeOutput codeOutput, bool transformations, CodeGenSymbolProvider symbolProvider)
        {
            // Generate CodeGen configuration
            XmlConfig xmlConfig = GetXmlConfig(Path.Combine(DumpInitialization.DefaultDumpPath, pdbFile));

            xmlConfig.MultiFileExport = codeOutput == CodeGenCodeOutput.CSharp_MultipleFiles || codeOutput == CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn;
            xmlConfig.GenerateAssemblyWithRoslyn = codeOutput == CodeGenCodeOutput.CSharp_MultipleFiles_Roslyn || codeOutput == CodeGenCodeOutput.CSharp_SingleFile_Roslyn;
            xmlConfig.GenerateAssemblyWithILWriter = codeOutput == CodeGenCodeOutput.ILWriter;
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
