using CommandLine;
using SharpDebug.DwarfSymbolProvider;
using SharpDebug.PdbSymbolProvider;
using System.Collections.Generic;
using System.IO;

namespace SharpDebug.CodeGen.App
{
    class Options
    {
        [Option('s', "symbols", Required = false, HelpText = "Path to symbols file which will be used to generate the code", SetName = "cmdSettings")]
        public string SymbolsPath { get; set; }

        [Option('t', "types", Separator = ',', Required = false, HelpText = "List of types to be exported", SetName = "cmdSettings")]
        public IList<string> Types { get; set; }

        [Option("no-type-info-comment", Default = false, HelpText = "Generate filed type info comment", Required = false, SetName = "cmdSettings")]
        public bool DontGenerateFieldTypeInfoComment { get; set; }

        [Option("multi-line-properties", Default = false, HelpText = "Generate properties as multi line", Required = false, SetName = "cmdSettings")]
        public bool MultiLineProperties { get; set; }

        [Option("use-direct-class-access", Default = false, HelpText = "Generated code that will use class members directly (not using GetField, but GetClassField).", Required = false, SetName = "cmdSettings")]
        public bool UseDirectClassAccess { get; set; }

        [Option("force-user-types-to-new-instead-of-casting", Default = false, HelpText = "Force using new during type casting instead of direct casting", Required = false, SetName = "cmdSettings")]
        public bool ForceUserTypesToNewInsteadOfCasting { get; set; }

        [Option("cache-user-type-fields", Default = false, HelpText = "Caches result of getting user type field when exporting user type", Required = false, SetName = "cmdSettings")]
        public bool CacheUserTypeFields { get; set; }

        [Option("cache-static-user-type-fields", Default = false, HelpText = "Caches result of getting static user type field when exporting user type", Required = false, SetName = "cmdSettings")]
        public bool CacheStaticUserTypeFields { get; set; }

        [Option("lazy-cache-user-type-fields", Default = false, HelpText = "Cache result of getting user type field inside UserMember when exporting user type", Required = false, SetName = "cmdSettings")]
        public bool LazyCacheUserTypeFields { get; set; }

        [Option("generate-physical-mapping-of-user-types", Default = false, HelpText = "Generate physical access to fields in exported user types (instead of symbolic/by name)", Required = false, SetName = "cmdSettings")]
        public bool GeneratePhysicalMappingOfUserTypes { get; set; }

        [Option("generate-assembly-with-il", Default = false, HelpText = "Generate assembly by emitting IL instead of compiling C# code.", Required = false, SetName = "cmdSettings")]
        public bool GenerateAssemblyWithIL { get; set; }

        [Option("generated-assembly-name", Default = "", HelpText = "Name of the assembly that will be generated next to sources in output folder", Required = false, SetName = "cmdSettings")]
        public string GeneratedAssemblyName { get; set; }

        [Option("generated-props-file-name", Default = "", HelpText = "Name of the props file that will be generated next to sources in output folder. It can be later included into project that will be compiled", Required = false, SetName = "cmdSettings")]
        public string GeneratedPropsFileName { get; set; }

        [Option('x', "xml-config", HelpText = "Path to xml file with configuration", SetName = "xmlConfig")]
        public string XmlConfigPath { get; set; }

        [Option("use-dwarf", Default = false, HelpText = "Use DWARF symbol provider")]
        public bool UseDwarfSymbolProvider { get; set; }

        [Option("use-pdb-reader", Default = false, HelpText = "Use PDB reader symbol provider")]
        public bool UsePDBReaderSymbolProvider { get; set; }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            Options options = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o);

            if (options == null)
                return;

            XmlConfig config;

            if (!string.IsNullOrEmpty(options.XmlConfigPath))
            {
                config = XmlConfig.Read(options.XmlConfigPath);
            }
            else
            {
                config = new XmlConfig()
                {
                    DontGenerateFieldTypeInfoComment = options.DontGenerateFieldTypeInfoComment,
                    ForceUserTypesToNewInsteadOfCasting = options.ForceUserTypesToNewInsteadOfCasting,
                    MultiLineProperties = options.MultiLineProperties,
                    UseDirectClassAccess = options.UseDirectClassAccess,
                    GeneratedAssemblyName = options.GeneratedAssemblyName,
                    GeneratedPropsFileName = options.GeneratedPropsFileName,
                    CacheStaticUserTypeFields = options.CacheStaticUserTypeFields,
                    CacheUserTypeFields = options.CacheUserTypeFields,
                    LazyCacheUserTypeFields = options.LazyCacheUserTypeFields,
                    GeneratePhysicalMappingOfUserTypes = options.GeneratePhysicalMappingOfUserTypes,
                    GenerateAssemblyWithILWriter = options.GenerateAssemblyWithIL,
                    Types = new XmlType[options.Types.Count],
                    Modules = new XmlModule[]
                    {
                        new XmlModule
                        {
                            SymbolsPath = options.SymbolsPath,
                            Name = Path.GetFileNameWithoutExtension(options.SymbolsPath),
                            Namespace = Path.GetFileNameWithoutExtension(options.SymbolsPath),
                        }
                    },
                };

                for (int i = 0; i < options.Types.Count; i++)
                    config.Types[i] = new XmlType()
                    {
                        Name = options.Types[i],
                    };
            }

            Generator generator;

            if (!options.UseDwarfSymbolProvider)
            {
                if (!options.UsePDBReaderSymbolProvider)
                    generator = new Generator();
                else
                    generator = new Generator(new PdbModuleProvider());
            }
            else
            {
                generator = new Generator(new DwarfCodeGenModuleProvider());
            }
            generator.Generate(config);
        }
    }
}
