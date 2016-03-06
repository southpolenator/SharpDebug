using CommandLine;
using Dia2Lib;
using GenerateUserTypesFromPdb.UserTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    class Options
    {
        [Option('p', "pdb", Required = false, HelpText = "Path to PDB which will be used to generate the code", SetName = "cmdSettings")]
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
        private static Module OpenPdb(XmlModule module)
        {
            IDiaDataSource dia = new DiaSource();
            IDiaSession session;
            string moduleName = !string.IsNullOrEmpty(module.Name) ? module.Name : Path.GetFileNameWithoutExtension(module.PdbPath).ToLower();

            module.Name = moduleName;
            dia.loadDataFromPdb(module.PdbPath);
            dia.openSession(out session);
            return new Module(module.Name, dia, session);
        }

        static void Main(string[] args)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
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
                    Modules = new XmlModule[] { new XmlModule { PdbPath = options.PdbPath } },
                };

                for (int i = 0; i < options.Types.Count; i++)
                    config.Types[i] = new XmlType()
                    {
                        Name = options.Types[i],
                    };
            }

            XmlModule[] xmlModules = config.Modules;
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
            if (config.SingleFileExport)
                generationOptions |= UserTypeGenerationFlags.SingleFileExport;

            ConcurrentDictionary<string, string> generatedFiles = new ConcurrentDictionary<string, string>();
            var syntaxTrees = new List<SyntaxTree>();

            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";
            Directory.CreateDirectory(outputDirectory);

            // Load modules
            ConcurrentDictionary<Module, XmlModule> modules = new ConcurrentDictionary<Module, XmlModule>();
            ConcurrentDictionary<XmlModule, Symbol[]> globalTypesPerModule = new ConcurrentDictionary<XmlModule, Symbol[]>();

            Console.Write("Loading modules...");
            Parallel.ForEach(xmlModules, (xmlModule) =>
            {
                Module module = OpenPdb(xmlModule);

                modules.TryAdd(module, xmlModule);
            });

            Console.WriteLine(" {0}", sw.Elapsed);

            Console.Write("Enumerating symbols...");
            Parallel.ForEach(modules, (mm) =>
            {
                XmlModule xmlModule = mm.Value;
                Module module = mm.Key;
                string moduleName = xmlModule.Name;
                string nameSpace = xmlModule.Namespace;
                List<Symbol> allSymbols = new List<Symbol>();

                foreach (var type in typeNames)
                {
                    Symbol[] symbols = module.FindGlobalTypeWildcard(type.NameWildcard);

                    if (symbols.Length == 0)
                        error.WriteLine("Symbol not found: {0}", type.Name);
                    else
                        allSymbols.AddRange(symbols);
                }

                allSymbols.AddRange(module.GetAllTypes());
                globalTypesPerModule.TryAdd(xmlModule, allSymbols.ToArray());
            });

            Console.WriteLine(" {0}", sw.Elapsed);

            Console.Write("Deduplicating symbols...");

            // Group duplicated symbols
            Dictionary<string, List<Symbol>> symbolsByName = new Dictionary<string, List<Symbol>>();
            Dictionary<Symbol, List<Symbol>> duplicatedSymbols = new Dictionary<Symbol, List<Symbol>>();

            foreach (var symbol in globalTypesPerModule.SelectMany(gt => gt.Value))
            {
                List<Symbol> symbols;

                if (!symbolsByName.TryGetValue(symbol.Name, out symbols))
                    symbolsByName.Add(symbol.Name, symbols = new List<Symbol>());

                bool found = false;

                foreach (var s in symbols.ToArray())
                {
                    if (s.Size != 0 && symbol.Size != 0 && s.Size != symbol.Size)
                    {
#if DEBUG
                        Console.WriteLine("{0}!{1} ({2}) {3}!{4} ({5})", s.Module.Name, s.Name, s.Size, symbol.Module.Name, symbol.Name, symbol.Size);
#endif
                        continue;
                    }

                    if (s.Size == 0 && symbol.Size != 0)
                    {
                        List<Symbol> duplicates;

                        if (!duplicatedSymbols.TryGetValue(s, out duplicates))
                            duplicatedSymbols.Add(s, duplicates = new List<Symbol>());

                        duplicatedSymbols.Remove(s);
                        duplicates.Add(s);
                        duplicatedSymbols.Add(symbol, duplicates);
                        symbols.Remove(s);
                        symbols.Add(symbol);
                    }
                    else
                    {
                        List<Symbol> duplicates;

                        if (!duplicatedSymbols.TryGetValue(s, out duplicates))
                            duplicatedSymbols.Add(s, duplicates = new List<Symbol>());
                        duplicates.Add(symbol);
                    }

                    found = true;
                    break;
                }

                if (!found)
                    symbols.Add(symbol);
            }

            // Unlink duplicated symbols if two or more are named the same
            foreach (var symbols in symbolsByName.Values)
            {
                if (symbols.Count <= 1)
                    continue;

                foreach (var s in symbols.ToArray())
                {
                    List<Symbol> duplicates;

                    if (!duplicatedSymbols.TryGetValue(s, out duplicates))
                        continue;

                    symbols.AddRange(duplicates);
                    duplicatedSymbols.Remove(s);
                }
            }

            // Extracting deduplicated symbols
            Dictionary<string, Symbol[]> deduplicatedSymbols = new Dictionary<string, Symbol[]>();
            Dictionary<Symbol, string> symbolNamespaces = new Dictionary<Symbol, string>();

            foreach (var symbols in symbolsByName.Values)
            {
                if (symbols.Count != 1)
                    foreach (var s in symbols)
                        symbolNamespaces.Add(s, modules[s.Module].Namespace);
                else
                {
                    Symbol symbol = symbols.First();
                    List<Symbol> duplicates;

                    if (!duplicatedSymbols.TryGetValue(symbol, out duplicates))
                        duplicates = new List<Symbol>();
                    duplicates.Insert(0, symbol);
                    deduplicatedSymbols.Add(symbol.Name, duplicates.ToArray());

                    foreach (var s in duplicates)
                        symbolNamespaces.Add(s, config.CommonTypesNamespace);
                }
            }

            var globalTypes = symbolsByName.SelectMany(s => s.Value).ToArray();

            Console.WriteLine(" {0}", sw.Elapsed);
            Console.WriteLine("  Total symbols: {0}", globalTypesPerModule.Sum(gt => gt.Value.Length));
            Console.WriteLine("  Unique symbol names: {0}", symbolsByName.Count);
            Console.WriteLine("  Dedupedlicated symbols: {0}", globalTypes.Length);

            // Initialize GlobalCache with deduplicatedSymbols
            GlobalCache.Update(deduplicatedSymbols);

            // Collecting types
            Console.Write("Collecting types...");

            var factory = new UserTypeFactory(config.Transformations);
            List<UserType> userTypes = new List<UserType>();

            foreach (var module in modules.Keys)
                userTypes.Add(factory.AddSymbol(module.GlobalScope, new XmlType() { Name = "ModuleGlobals" }, module.Name, modules[module].Namespace, generationOptions));

            ConcurrentBag<Symbol> simpleSymbols = new ConcurrentBag<Symbol>();
            Dictionary<Tuple<string, string>, List<Symbol>> templateSymbols = new Dictionary<Tuple<string, string>, List<Symbol>>();

            foreach (Symbol symbol in globalTypes)
            {
                string symbolName = symbol.Name;

                //  TODO add configurable filter
                //
                if (symbolName.StartsWith("$") || symbolName.StartsWith("__vc_attributes") || symbolName.Contains("`anonymous-namespace'") || symbolName.Contains("`anonymous namespace'") || symbolName.Contains("::$") || symbolName.Contains("`"))
                {
                    continue;
                }

                // Do not handle template referenced arguments 
                if (symbolName.Contains("&"))
                {
                    // TODO: Convert this to function pointer
                    continue;
                }

                // TODO: C# doesn't support lengthy names
                if (symbolName.Length > 160)
                {
                    continue;
                }

                // TODO: For now remove all unnamed-type symbols
                string scopedClassName = NameHelper.GetSymbolScopedClassName(symbolName);

                if (scopedClassName == "<>" || symbolName.Contains("::<"))
                {
                    continue;
                }

                // Parent Class is Template, Nested is Physical
                // Check if dealing template type.
                if (NameHelper.ContainsTemplateType(symbolName) && NameHelper.IsTemplateType(scopedClassName))
                {
                    List<string> namespaces = NameHelper.GetFullSymbolNamespaces(symbolName);
                    string className = namespaces.Last();
                    List<string> templateSpecializationArgs = NameHelper.GetTemplateSpecializationArguments(className);
                    var symbolId = Tuple.Create(symbolNamespaces[symbol], NameHelper.GetLookupNameForSymbol(symbol));

                    lock (templateSymbols)
                    {
                        if (templateSymbols.ContainsKey(symbolId) == false)
                            templateSymbols[symbolId] = new List<Symbol>() { symbol };
                        else
                            templateSymbols[symbolId].Add(symbol);
                    }

                    //
                    // TO DO
                    // Do not add physical types for template specialization (not now)
                    // do if types contains static fields
                    // nested in templates
                    continue;
                }

                simpleSymbols.Add(symbol);
            }

            Console.WriteLine(" {0}", sw.Elapsed);

            // Populate Templates
            Console.Write("Populating templates...");
            foreach (List<Symbol> symbols in templateSymbols.Values)
            {
                Symbol symbol = symbols.First();
                string symbolName = NameHelper.GetLookupNameForSymbol(symbol);

                XmlType type = new XmlType()
                {
                    Name = symbolName
                };

                userTypes.AddRange(factory.AddSymbols(symbols, type, symbol.Module.Name, symbolNamespaces[symbol], generationOptions));
            }

            Console.WriteLine(" {0}", sw.Elapsed);

            // Specialized class
            Console.Write("Populating specialized classes...");
            foreach (Symbol symbol in simpleSymbols)
            {
                string symbolName = symbol.Name;
                XmlType type = new XmlType()
                {
                    Name = symbolName
                };

                userTypes.Add(factory.AddSymbol(symbol, type, symbol.Module.Name, symbolNamespaces[symbol], generationOptions));
            }

            Console.WriteLine(" {0}", sw.Elapsed);

            // To solve template dependencies. Update specialization arguments once all the templates has been populated.
            Console.Write("Updating template arguments...");
            foreach (TemplateUserType templateUserType in userTypes.OfType<TemplateUserType>())
            {
                foreach (TemplateUserType specializedTemplateUserType in templateUserType.specializedTypes)
                    if (!specializedTemplateUserType.UpdateArguments(factory))
                    {
#if DEBUG
                        Console.WriteLine("Template user type cannot be updated: {0}", specializedTemplateUserType.Symbol.Name);
#endif
                    }
            }

            Console.WriteLine(" {0}", sw.Elapsed);

            // Post processing user types (filling DeclaredInType)
            Console.Write("Post processing user types...");
            var namespaceTypes = factory.ProcessTypes(userTypes, symbolNamespaces).ToArray();
            userTypes.AddRange(namespaceTypes);

            Console.WriteLine(" {0}", sw.Elapsed);

            // Code generation and saving it to disk
            Console.Write("Saving code to disk...");

            if (!config.SingleFileExport)
            {
                // Generate Code
                Parallel.ForEach(userTypes,
                    (symbolEntry) =>
                    {
                        Tuple<string, string> result = GenerateUseTypeCode(symbolEntry, factory, outputDirectory, error, generationOptions, generatedFiles);
                        string text = result.Item1;
                        string filename = result.Item2;

                        if (config.GenerateAssemblyWithRoslyn && !string.IsNullOrEmpty(config.GeneratedAssemblyName) && !string.IsNullOrEmpty(text))
                            lock (syntaxTrees)
                            {
                                syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, path: filename, encoding: System.Text.UTF8Encoding.Default));
                            }
                    });
            }
            else
            {
                string filename = string.Format(@"{0}\everything.exported.cs", outputDirectory);
                HashSet<string> usings = new HashSet<string>();
                foreach (var symbolEntry in userTypes)
                    foreach (var u in symbolEntry.Usings)
                        usings.Add(u);

                generatedFiles.TryAdd(filename.ToLowerInvariant(), filename);
                using (StringWriter stringOutput = new StringWriter())
                using (TextWriter masterOutput = new StreamWriter(filename, false /* append */, System.Text.Encoding.ASCII, 8192))
                {
                    foreach (var u in usings.OrderBy(s => s))
                    {
                        masterOutput.WriteLine("using {0};\n", u);
                        if (config.GenerateAssemblyWithRoslyn && !string.IsNullOrEmpty(config.GeneratedAssemblyName))
                            stringOutput.WriteLine("using {0};\n", u);
                    }
                    masterOutput.WriteLine();
                    if (config.GenerateAssemblyWithRoslyn)
                        stringOutput.WriteLine();

                    Parallel.ForEach(userTypes,
                        (symbolEntry) =>
                        {
                            using (StringWriter output = new StringWriter())
                            {
                                GenerateUseTypeCodeInSingleFile(output, symbolEntry, factory, error, generationOptions);
                                lock (masterOutput)
                                {
                                    string text = output.ToString();
                                    masterOutput.WriteLine(text);
                                    if (config.GenerateAssemblyWithRoslyn && !string.IsNullOrEmpty(config.GeneratedAssemblyName))
                                        stringOutput.WriteLine(text);
                                }
                            }
                        });

                    if (config.GenerateAssemblyWithRoslyn && !string.IsNullOrEmpty(config.GeneratedAssemblyName))
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(stringOutput.ToString(), path: filename, encoding: System.Text.UTF8Encoding.Default));
                }
            }

            Console.WriteLine(" {0}", sw.Elapsed);

            // Compiling the code
            string binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (config.GenerateAssemblyWithRoslyn && !string.IsNullOrEmpty(config.GeneratedAssemblyName))
            {
                MetadataReference[] references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(binFolder, "CsScriptManaged.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(binFolder, "CsScripts.CommonUserTypes.dll")),
                };

                CSharpCompilation compilation = CSharpCompilation.Create(
                    config.GeneratedAssemblyName,
                    syntaxTrees: syntaxTrees,
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                Console.WriteLine("Syntax trees: {0}", syntaxTrees.Count);

                string dllFilename = Path.Combine(outputDirectory, config.GeneratedAssemblyName);
                string pdbFilename = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(dllFilename) + ".pdb");

                using (var dllStream = new FileStream(dllFilename, FileMode.Create))
                using (var pdbStream = new FileStream(pdbFilename, FileMode.Create))
                {
                    var result = compilation.Emit(dllStream, !config.DisablePdbGeneration ? pdbStream : null);

                    if (!result.Success)
                    {
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);

                        foreach (var diagnostic in failures)
                        {
                            Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        }
                    }
                    else
                    {
                        Console.WriteLine("DLL size: {0}", dllStream.Position);
                        Console.WriteLine("PDB size: {0}", pdbStream.Position);
                    }
                }

                Console.WriteLine("Compiling: {0}", sw.Elapsed);
            }

            // Check whether we should generate assembly
            if (!config.GenerateAssemblyWithRoslyn && !string.IsNullOrEmpty(config.GeneratedAssemblyName))
            {
                var codeProvider = new CSharpCodeProvider();
                var compilerParameters = new CompilerParameters()
                {
                    IncludeDebugInformation = !config.DisablePdbGeneration,
                    OutputAssembly = outputDirectory + config.GeneratedAssemblyName,
                };

                compilerParameters.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location).ToArray());
                //compilerParameters.ReferencedAssemblies.AddRange(referencedAssemblies);

                const string MicrosoftCSharpDll = "Microsoft.CSharp.dll";

                if (!compilerParameters.ReferencedAssemblies.Cast<string>().Where(a => a.Contains(MicrosoftCSharpDll)).Any())
                {
                    compilerParameters.ReferencedAssemblies.Add(MicrosoftCSharpDll);
                }

                compilerParameters.ReferencedAssemblies.Add(Path.Combine(binFolder, "CsScriptManaged.dll"));
                compilerParameters.ReferencedAssemblies.Add(Path.Combine(binFolder, "CsScripts.CommonUserTypes.dll"));

                var compileResult = codeProvider.CompileAssemblyFromFile(compilerParameters, generatedFiles.Values.ToArray());

                if (compileResult.Errors.Count > 0)
                {
                    Console.Error.WriteLine("Compile errors:");
                    foreach (CompilerError err in compileResult.Errors.Cast<CompilerError>().Take(100))
                        Console.Error.WriteLine(err);
                }

                Console.WriteLine("Compiling: {0}", sw.Elapsed);
            }

            // Generating props file
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

            Console.WriteLine("Total time: {0}", sw.Elapsed);
        }

        private static ConcurrentDictionary<string, string> createdDirectories = new ConcurrentDictionary<string, string>();

        private static Tuple<string, string> GenerateUseTypeCode(UserType userType, UserTypeFactory factory, string outputDirectory, TextWriter errorOutput, UserTypeGenerationFlags generationOptions, ConcurrentDictionary<string, string> generatedFiles)
        {
            Symbol symbol = userType.Symbol;

            if (symbol.Tag == SymTagEnum.SymTagBaseType)
            {
                // ignore Base (Primitive) types.
                return Tuple.Create("", "");
            }

            if (userType.DeclaredInType != null)
            {
                return Tuple.Create("", "");
            }

            string classOutputDirectory = outputDirectory;

            classOutputDirectory = Path.Combine(classOutputDirectory, userType.ModuleName);

            if (!string.IsNullOrEmpty(userType.Namespace))
                classOutputDirectory = Path.Combine(classOutputDirectory, UserType.NormalizeSymbolName(UserType.NormalizeSymbolName(userType.Namespace).Replace(".", "\\").Replace(":", ".")));


            string ss;
            if (!createdDirectories.TryGetValue(classOutputDirectory, out ss))
            {
                Directory.CreateDirectory(classOutputDirectory);
                createdDirectories.TryAdd(classOutputDirectory, classOutputDirectory);
            }

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
            using (StringWriter stringOutput = new StringWriter())
            {
                userType.WriteCode(new IndentedWriter(stringOutput), errorOutput, factory, generationOptions);
                string text = stringOutput.ToString();
                output.WriteLine(text);
                return Tuple.Create(text, filename);
            }
        }

        private static bool GenerateUseTypeCodeInSingleFile(TextWriter output, UserType userType, UserTypeFactory factory, TextWriter errorOutput, UserTypeGenerationFlags generationOptions)
        {
            Symbol symbol = userType.Symbol;

            if (symbol != null && symbol.Tag == SymTagEnum.SymTagBaseType)
            {
                // ignore Base (Primitive) types.
                return false;
            }

            if (userType.DeclaredInType != null)
            {
                return false;
            }

            userType.WriteCode(new IndentedWriter(output), errorOutput, factory, generationOptions);
            return true;
        }
    }
}
