using CommandLine;
using Dia2Lib;
using GenerateUserTypesFromPdb.UserTypes;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    internal static class GlobalCache
    {
        internal static ConcurrentDictionary<string, IDiaSymbol> UdtTypesByName = new ConcurrentDictionary<string, IDiaSymbol>();

        internal static ConcurrentDictionary<string, UserType> UserTypesBySymbolName = new ConcurrentDictionary<string, UserType>();
    };

    class Options
    {
        [Option('p', "pdb", Required = true, HelpText = "Path to PDB which will be used to generate the code")]
        public string PdbPath { get; set; }

        [Option('t', "types", Separator = ',', Required = false, HelpText = "List of types to be exported", SetName = "cmdSettings")]
        public IList<string> Types { get; set; }

        [Option("no-type-info-comment", Default = false, HelpText = "Generate filed type info comment", Required = false, SetName = "cmdSettings")]
        public bool DontGenerateFieldTypeInfoComment { get; set; }

        [Option("multi-line-properties", Default = false, HelpText = "Generate properties as multi line", Required = false, SetName = "cmdSettings")]
        public bool MultiLineProperties { get; set; }

        [Option("use-dia-symbol-provider", Default = false, HelpText = "Use DIA symbol provider and access fields for specific type", Required = false, SetName = "cmdSettings")]
        public bool UseDiaSymbolProvider { get; set; }

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

        [Option("generated-assembly-name", Default = "", HelpText = "Name of the assembly that will be generated next to sources in output folder", Required = false, SetName = "cmdSettings")]
        public string GeneratedAssemblyName { get; set; }

        [Option("generated-props-file-name", Default = "", HelpText = "Name of the props file that will be generated next to sources in output folder. It can be later included into project that will be compiled", Required = false, SetName = "cmdSettings")]
        public string GeneratedPropsFileName { get; set; }

        [Option('x', "xml-config", HelpText = "Path to xml file with configuration", SetName = "xmlConfig")]
        public string XmlConfigPath { get; set; }
    }

    class Program
    {
        private static void OpenPdb(string path, out IDiaDataSource dia, out IDiaSession session)
        {
            dia = new DiaSource();
            dia.loadDataFromPdb(path);
            dia.openSession(out session);
        }

        private static void DumpSymbol(IDiaSymbol symbol)
        {
            Type type = typeof(IDiaSymbol);

            foreach (var property in type.GetProperties())
            {
                Console.WriteLine("{0} = {1}", property.Name, property.GetValue(symbol));
            }
        }

        static void Main(string[] args)
        {
            var error = Console.Error;
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
                    UseDiaSymbolProvider = options.UseDiaSymbolProvider,
                    GeneratedAssemblyName = options.GeneratedAssemblyName,
                    GeneratedPropsFileName = options.GeneratedPropsFileName,
                    CacheStaticUserTypeFields = options.CacheStaticUserTypeFields,
                    CacheUserTypeFields = options.CacheUserTypeFields,
                    LazyCacheUserTypeFields = options.LazyCacheUserTypeFields,
                    GeneratePhysicalMappingOfUserTypes = options.GeneratePhysicalMappingOfUserTypes,
                    Types = new XmlType[options.Types.Count],
                };

                for (int i = 0; i < options.Types.Count; i++)
                    config.Types[i] = new XmlType()
                    {
                        Name = options.Types[i],
                    };
            }

            string pdbPath = options.PdbPath;
            XmlType[] typeNames = config.Types;
            UserTypeGenerationFlags generationOptions = UserTypeGenerationFlags.None;

            if (!config.DontGenerateFieldTypeInfoComment)
                generationOptions |= UserTypeGenerationFlags.GenerateFieldTypeInfoComment;
            if (!config.MultiLineProperties)
                generationOptions |= UserTypeGenerationFlags.SingleLineProperty;
            if (config.UseDiaSymbolProvider)
                generationOptions |= UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider;
            if (config.ForceUserTypesToNewInsteadOfCasting)
                generationOptions |= UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting;
            if (config.CacheUserTypeFields)
                generationOptions |= UserTypeGenerationFlags.CacheUserTypeFields;
            if (config.CacheStaticUserTypeFields)
                generationOptions |= UserTypeGenerationFlags.CacheStaticUserTypeFields;
            if (config.LazyCacheUserTypeFields)
                generationOptions |= UserTypeGenerationFlags.LazyCacheUserTypeFields;
            if (config.GeneratePhysicalMappingOfUserTypes)
                generationOptions |= UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes;

            string moduleName = Path.GetFileNameWithoutExtension(pdbPath).ToLower();
            var factory = new UserTypeFactory(config.Transformations);
            IDiaDataSource dia;
            IDiaSession session;

            OpenPdb(pdbPath, out dia, out session);

            foreach (var type in typeNames)
            {
                IDiaSymbol[] symbols = session.globalScope.GetChildrenWildcard(type.NameWildcard, SymTagEnum.SymTagUDT).ToArray();

                if (symbols.Length == 0)
                {
                    error.WriteLine("Symbol not found: {0}", type.Name);
                }
                else
                {
                    factory.AddSymbol(session, symbols, type, moduleName, generationOptions);
                }
            }

            foreach (IDiaSymbol symbol in session.globalScope.GetChildren(SymTagEnum.SymTagUDT))
            {
                // #fixme, use helper to get right name
                string symbolName = symbol.name;
                if (symbolName.StartsWith("$") || symbolName.StartsWith("__vc_attributes") || symbolName.StartsWith("std::") || symbolName.StartsWith("`anonymous-namespace'"))
                {
                    continue;
                }

                if (symbolName.Contains("<"))
                {
                    // skip for now
                    continue;
                }

                GlobalCache.UdtTypesByName.TryAdd(symbolName, symbol);
            }
            
            foreach (IDiaSymbol symbol in GlobalCache.UdtTypesByName.Values)
            {
                string symbolName = symbol.name;

                if (symbolName == "SystemThreadPool")
                {
                }

                XmlType type = new XmlType()
                {
                    Name = symbolName
                };

                factory.AddSymbol(symbol, type, moduleName, generationOptions);
            }

            int index = 0;

            // Update 
            UserType[] userTypesInitialSet = factory.Symbols.ToArray();
            Parallel.ForEach(userTypesInitialSet,
                (userType) =>
                {
                    Interlocked.Increment(ref index);
                    Console.WriteLine("{0}/{1}", index++, factory.Symbols.Count());

                    userType.UpdateUserTypes(factory, generationOptions);
                });

            if (!string.IsNullOrEmpty(config.DefaultNamespace))
            {
                foreach (var userType in factory.Symbols)
                {
                    //userType.M.Namespace = config.DefaultNamespace;
                }
            }

            factory.ProcessTypes();
            factory.InserUserType(new GlobalsUserType(session, moduleName));

            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";
            Directory.CreateDirectory(outputDirectory);
            List<string> generatedFiles = new List<string>();

            string[] allUDTs = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).Select(s => s.name).Distinct().OrderBy(s => s).ToArray();

            File.WriteAllLines(outputDirectory + "symbols.txt", allUDTs);

            // Generate Code
            Parallel.ForEach(factory.Symbols,
                (symbolEntry) =>
                {
                    GenerateUseTypeCode(symbolEntry, factory, outputDirectory, error, generationOptions, generatedFiles);
                });
            
            /*
            foreach(var symbolEntry in factory.Symbols)
            {
                GenerateUseTypeCode(symbolEntry, factory, outputDirectory, error, generationOptions, generatedFiles);
            }
            */

            if (!string.IsNullOrEmpty(config.GeneratedPropsFileName))
            {
                using (TextWriter output = new StreamWriter(outputDirectory + config.GeneratedPropsFileName))
                {
                    output.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                    output.WriteLine(@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");
                    output.WriteLine(@"  <ItemGroup>");
                    foreach (var file in generatedFiles.Distinct())
                        output.WriteLine(@"    <Compile Include=""{0}"" />", file);
                    output.WriteLine(@" </ItemGroup>");
                    output.WriteLine(@"</Project>");
                }
            }
        }

        private static bool GenerateUseTypeCode(UserType userType, UserTypeFactory factory, string outputDirectory, TextWriter errorOutput, UserTypeGenerationFlags generationOptions, List<string> generatedFiles)
        {
            var symbol = userType.Symbol;

            Console.WriteLine(userType.XmlType.Name);
            if (userType.DeclaredInType != null)
            {
                return false;
            }

            string classOutputDirectory = outputDirectory;

            classOutputDirectory = Path.Combine(classOutputDirectory, userType.ModuleName);

            if (!string.IsNullOrEmpty(userType.Namespace))
                classOutputDirectory = Path.Combine(classOutputDirectory, userType.Namespace.Replace(".", "\\").Replace(":", "."));

            Directory.CreateDirectory(classOutputDirectory);

            bool isEnum = userType is EnumUserType;

            string filename = string.Format(@"{0}\{1}{2}.exported.cs", classOutputDirectory, userType.ConstructorName, isEnum ? "_enum" : "");

            try
            {
                using (TextWriter output = new StreamWriter(filename))
                {
                    userType.WriteCode(new IndentedWriter(output), errorOutput, factory, generationOptions);

                    lock(generatedFiles)
                    {
                        generatedFiles.Add(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}

/*
            // Check whether we should generate assembly
            if (!string.IsNullOrEmpty(config.GeneratedAssemblyName))
            {
                var codeProvider = new CSharpCodeProvider();
var compilerParameters = new CompilerParameters()
{
    IncludeDebugInformation = true,
    OutputAssembly = outputDirectory + config.GeneratedAssemblyName,
};

compilerParameters.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location).ToArray());
                //compilerParameters.ReferencedAssemblies.AddRange(referencedAssemblies);

                const string MicrosoftCSharpDll = "Microsoft.CSharp.dll";

                if (!compilerParameters.ReferencedAssemblies.Cast<string>().Where(a => a.Contains(MicrosoftCSharpDll)).Any())
                {
                    compilerParameters.ReferencedAssemblies.Add(MicrosoftCSharpDll);
                }

                string binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

compilerParameters.ReferencedAssemblies.Add(Path.Combine(binFolder, "CsScriptManaged.dll"));
                compilerParameters.ReferencedAssemblies.Add(Path.Combine(binFolder, "CsScripts.CommonUserTypes.dll"));

                var compileResult = codeProvider.CompileAssemblyFromFile(compilerParameters, generatedFiles.ToArray());

                if (compileResult.Errors.Count > 0)
                {
                    Console.Error.WriteLine("Compile errors:");
                    foreach (CompilerError err in compileResult.Errors)
                        Console.Error.WriteLine(err);
                }

    }
    }
}



*/