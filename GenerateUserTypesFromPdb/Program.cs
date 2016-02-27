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
        internal static ConcurrentDictionary<string, IDiaSymbol> DiaSymbolsByName = new ConcurrentDictionary<string, IDiaSymbol>();

        internal static ConcurrentDictionary<string, UserType> UserTypesBySymbolName = new ConcurrentDictionary<string, UserType>();

        internal static ConcurrentDictionary<string, bool> InstantiableTemplateUserTypes = new ConcurrentDictionary<string, bool>();
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
                    factory.AddSymbols(session, symbols, type, moduleName, generationOptions);
                }
            }

            ConcurrentDictionary<string, List<IDiaSymbol>> templateSymbols = new ConcurrentDictionary<string, List<IDiaSymbol>>();
            ConcurrentDictionary<string, IDiaSymbol> specializedClassWithParentSymbol = new ConcurrentDictionary<string, IDiaSymbol>();

            foreach (IDiaSymbol symbol in session.globalScope.GetChildren(SymTagEnum.SymTagUDT))
            {
                //  TODO add configurable filter
                //
                string symbolName = symbol.name;
                if (symbolName.StartsWith("$") || symbolName.StartsWith("__vc_attributes") || /*symbolName.StartsWith("ATL::") ||*/ symbolName.StartsWith("`anonymous-namespace'"))
                {
                    continue;
                }

                // Do not handle template referenced arguments 
                if (symbolName.Contains("&"))
                {
                    continue;
                }

                // Skip symbols with large names (filepath issue)
                if (symbolName.Length > 160)
                {
                    continue;
                }

                var namespaces = NameHelper.GetFullSymbolNamespaces(symbolName);

                string scopedClassName = NameHelper.GetSymbolScopedClassName(symbol.name);

                if (scopedClassName == "<>")
                {
                    // TODO
                    // for now remove all unnamed-type symbols
                    //
                    continue;
                }

                // Parent Class is Template, Nested is Physical
                // Check if dealing template type.
                if (NameHelper.ContainsTemplateType(symbol.name))
                {
                    if (!NameHelper.IsTemplateType(scopedClassName))
                    {
                        // Parent is template but class itself is not,
                        // Class needs to be aware of parent context (UserTypeFactory).
                        //
                        // TODO
                        symbolName = string.Format("{0}:{1}", NameHelper.GetLookupNameForSymbol(symbolName), scopedClassName);
                        //specializedClassWithParentSymbol.TryAdd(symbolName, symbol);
                        continue;
                    }
                    else
                    {
                        try
                        {
                            string className = namespaces.Last();

                            List<string> templateSpecializationArgs = NameHelper.GetTemplateSpecializationArguments(className);

                            //
                            // TODO
                            // Inspect Template
                            //
                            TemplateUserType templateType = new TemplateUserType(session, symbol, new XmlType() { Name = symbolName }, moduleName, factory);

                            int templateArgs = templateType.GenericsArguments;
                            if (templateSpecializationArgs.Any(r => r == "void" || r == "void const"))
                            {
                                GlobalCache.DiaSymbolsByName.TryAdd(symbolName, symbol);
                            }

                            symbolName = NameHelper.GetLookupNameForSymbol(symbol);

                            if (templateSymbols.ContainsKey(symbolName) == false)
                            {
                                templateSymbols[symbolName] = new List<IDiaSymbol>() { symbol };
                            }
                            else
                            {
                                templateSymbols[symbolName].Add(symbol);
                            }

                            //
                            // TO DO
                            // Do not add physical types for template specialization (not now)
                            // do if types contains static fields
                            // nested in templates
                            continue;
                        }
                        catch(Exception ex)
                        {
                            continue;
                        }
                    }
                }

                GlobalCache.DiaSymbolsByName.TryAdd(symbolName, symbol);
            }

            // Populate specialization first
            //
            foreach (IDiaSymbol symbol in specializedClassWithParentSymbol.Values)
            {
                string symbolName = symbol.name;

                XmlType type = new XmlType()
                {
                    Name = symbolName
                };

                factory.AddSymbol(symbol, type, moduleName, generationOptions);
            }

            // Populate Templates
            //
            foreach (List<IDiaSymbol> symbols in templateSymbols.Values)
            {
                string symbolName = NameHelper.GetLookupNameForSymbol(symbols.First());

                //
                //  TODO
                //  consider adding physical type when dealing with single specialization
                //  revisit after adding multiple pdb support
                //
                try
                {

                    XmlType type = new XmlType()
                    {
                        Name = symbolName
                    };

                    factory.AddSymbols(session, symbols, type, moduleName, generationOptions);
                }
                catch(Exception)
                {
                    //  failed to add template type
                    //
                    //  TODO
                    //  consider adding specialized types
                    //
                }
            }

            //   Specialized class
            //
            foreach (IDiaSymbol symbol in GlobalCache.DiaSymbolsByName.Values)
            {
                string symbolName = symbol.name;

                XmlType type = new XmlType()
                {
                    Name = symbolName
                };

                factory.AddSymbol(symbol, type, moduleName, generationOptions);
            }


            //  To solve template dependencies.
            //  Update specialization arguments once all the templates has been populated.
            //
            foreach (TemplateUserType templateUserType in GlobalCache.UserTypesBySymbolName.Values.OfType<TemplateUserType>())
            {
                templateUserType.UpdateArguments(factory);
            }

            int index = 0;

            // Update 
            UserType[] userTypesInitialSet = factory.Symbols.ToArray();
            foreach (UserType userType in userTypesInitialSet)
            {
                Console.WriteLine("{0}/{1}", index++, factory.Symbols.Count());

                userType.UpdateUserTypes(factory, generationOptions);
            }

            factory.ProcessTypes();
            factory.InserUserType(new GlobalsUserType(session, moduleName));

            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";
            Directory.CreateDirectory(outputDirectory);


            ConcurrentDictionary<string, string> generatedFiles = new ConcurrentDictionary<string, string>();

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
                    foreach (var file in generatedFiles.Values)
                        output.WriteLine(@"    <Compile Include=""{0}"" />", file);
                    output.WriteLine(@" </ItemGroup>");
                    output.WriteLine(@"</Project>");
                }
            }
        }

        private static bool GenerateUseTypeCode(UserType userType, UserTypeFactory factory, string outputDirectory, TextWriter errorOutput, UserTypeGenerationFlags generationOptions, ConcurrentDictionary<string, string> generatedFiles)
        {
            var symbol = userType.Symbol;

            Console.WriteLine(userType.XmlType.Name);
            if (userType.DeclaredInType != null)
            {
                return false;
            }

            try
            {
                string classOutputDirectory = outputDirectory;

                classOutputDirectory = Path.Combine(classOutputDirectory, userType.ModuleName);

                if (!string.IsNullOrEmpty(userType.Namespace))
                    classOutputDirectory = Path.Combine(classOutputDirectory, UserType.NormalizeSymbolName(UserType.NormalizeSymbolName(userType.Namespace).Replace(".", "\\").Replace(":", ".")));

                Directory.CreateDirectory(classOutputDirectory);

                bool isEnum = userType is EnumUserType;

                string filename = string.Format(@"{0}\{1}{2}.exported.cs", classOutputDirectory, userType.ConstructorName, isEnum ? "_enum" : "");

                int index = 1;
                while (true)
                {
                    if (generatedFiles.TryAdd(filename.ToLowerInvariant(), filename))
                    {
                        break;
                    }

                    filename = string.Format(@"{0}\{1}_{2}.exported.cs", classOutputDirectory, userType.ConstructorName, index++);
                }

                using (TextWriter output = new StreamWriter(filename))
                {
                    userType.WriteCode(new IndentedWriter(output), errorOutput, factory, generationOptions);
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