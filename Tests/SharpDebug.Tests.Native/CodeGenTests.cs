using SharpDebug.CodeGen;
using SharpDebug.DwarfSymbolProvider;
using SharpDebug.PdbSymbolProvider;
using System;
using System.IO;
using Xunit;

namespace SharpDebug.Tests.Native
{
    public enum CodeGenSymbolProvider
    {
        DIA,
        Dwarf,
        PdbReader,
    }

    public enum CodeGenCodeOutput
    {
        CSharp_SingleFile,
        CSharp_MultipleFiles,
        ILWriter,
    }

    [Trait("x64", "true")]
    public class CodeGenTests : TestBase
    {
        [Theory]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x86.Release.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2013.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.DIA)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.x64.VS2015.pdb", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.PdbReader)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.gcc.exe", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.x64.gcc.exe", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_SingleFile, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_MultipleFiles, true, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.CSharp_SingleFile, false, CodeGenSymbolProvider.Dwarf)]
        [InlineData("NativeDumpTest.linux.x64.gcc", CodeGenCodeOutput.ILWriter, false, CodeGenSymbolProvider.Dwarf)]
        public void Generation(string pdbFile, CodeGenCodeOutput codeOutput, bool transformations, CodeGenSymbolProvider symbolProvider)
        {
            // Generate CodeGen configuration
            XmlConfig xmlConfig = GetXmlConfig(Path.Combine(DumpInitialization.DefaultDumpPath, pdbFile));

            xmlConfig.MultiFileExport = codeOutput == CodeGenCodeOutput.CSharp_MultipleFiles;
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
                using (StringWriter logger = new StringWriter())
                using (StringWriter errorLogger = new StringWriter())
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

                    generator.Generate(xmlConfig, logger);
                    errorLogger.Flush();
                    string errorText = errorLogger.GetStringBuilder().ToString();

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
                        SymbolsPath = pdbFile,
                    }
                },
                ReferencedAssemblies = new[]
                {
                    new XmlReferencedAssembly()
                    {
                        Path = GetAbsoluteBinPath("SharpDebug.Engine.dll"),
                    },
                    new XmlReferencedAssembly()
                    {
                        Path = GetAbsoluteBinPath("SharpDebug.CommonUserTypes.dll"),
                    },
                },
                Types = new XmlType[0],
                IncludedFiles = new XmlIncludedFile[0],
                Transformations = ScriptExecution.DefaultTransformations,
            };
        }
    }
}
