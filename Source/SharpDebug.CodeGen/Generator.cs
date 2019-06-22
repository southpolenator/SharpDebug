﻿using SharpDebug.CodeGen.UserTypes;
using SharpDebug.Engine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpDebug.CodeGen
{
    using SharpDebug.CodeGen.CodeWriters;
    using SymbolProviders;
    using UserType = SharpDebug.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Starting point for generating user types from PDBs.
    /// </summary>
    public class Generator
    {
        /// <summary>
        /// Collection of assemblies that should be used when generating assembly.
        /// </summary>
        private static readonly string[] dependentAssemblies = new string[]
        {
            "SharpDebug.Engine.dll",
            "SharpDebug.CommonUserTypes.dll",
        };

        /// <summary>
        /// The stopwatch used for measuring performance
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch;

        /// <summary>
        /// The CodeGen configuration
        /// </summary>
        private XmlConfig xmlConfig;

        /// <summary>
        /// The XML modules
        /// </summary>
        private XmlModule[] xmlModules;

        /// <summary>
        /// The XML types
        /// </summary>
        private XmlType[] typeNames;

        /// <summary>
        /// The XML included files
        /// </summary>
        private XmlIncludedFile[] includedFiles;

        /// <summary>
        /// The XML referenced assemblies
        /// </summary>
        private XmlReferencedAssembly[] referencedAssemblies;

        /// <summary>
        /// The generation options
        /// </summary>
        private UserTypeGenerationFlags generationOptions;

        /// <summary>
        /// The user type factory
        /// </summary>
        private UserTypeFactory userTypeFactory;

        /// <summary>
        /// The user types
        /// </summary>
        private List<UserType> userTypes;

        /// <summary>
        /// The logger
        /// </summary>
        private TextWriter logger;

        /// <summary>
        /// The error logger
        /// </summary>
        private TextWriter errorLogger;

        /// <summary>
        /// The module provider
        /// </summary>
        private IModuleProvider moduleProvider;

        /// <summary>
        /// The code writer used to output generated code
        /// </summary>
        private ICodeWriter codeWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        public Generator()
            : this(new DiaModuleProvider())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        /// <param name="moduleProvider">The module provider.</param>
        public Generator(IModuleProvider moduleProvider)
        {
            this.moduleProvider = moduleProvider;
        }

        /// <summary>
        /// Generates the script code from the specified CodeGen configuration.
        /// </summary>
        /// <param name="config">The CodeGen configuration.</param>
        /// <returns>Script code</returns>
        public string GenerateScriptCode(XmlConfig config)
        {
            using (StringWriter errorOutput = new StringWriter())
            {
                TextWriter consoleOut = Console.Out;

                try
                {
                    Console.SetOut(TextWriter.Null);
                    config.MultiFileExport = false;
                    Initialize(config, TextWriter.Null, errorOutput);

                    // Generate user types
                    GenerateUserTypes();

                    // Throw exception if there was an error
                    StringBuilder sb = errorOutput.GetStringBuilder();

                    if (sb.Length > 0)
                    {
                        throw new Exception(sb.ToString());
                    }

                    return GenerateSingleFile();
                }
                finally
                {
                    Console.SetOut(consoleOut);
                }
            }
        }

        /// <summary>
        /// Generates the assembly from the specified CodeGen configuration.
        /// </summary>
        /// <param name="xmlConfig">The CodeGen configuration.</param>
        /// <param name="assemblyPath">The output assembly path.</param>
        public void GenerateAssembly(XmlConfig xmlConfig, string assemblyPath)
        {
            using (StringWriter errorOutput = new StringWriter())
            {
                TextWriter consoleOut = Console.Out;

                try
                {
                    Console.SetOut(TextWriter.Null);
                    xmlConfig.MultiFileExport = false;
                    Initialize(xmlConfig, TextWriter.Null, errorOutput);

                    // Generate user types
                    GenerateUserTypes();

                    // Throw exception if there was an error
                    StringBuilder sb = errorOutput.GetStringBuilder();

                    if (sb.Length > 0)
                    {
                        throw new Exception(sb.ToString());
                    }

                    if (!xmlConfig.GenerateAssemblyWithILWriter)
                    {
                        string code = GenerateSingleFile();
                        var syntaxTrees = new List<SyntaxTree>();

                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: "AutoGeneratedFile.cs", encoding: UTF8Encoding.Default));

                        using (FileStream dllStream = new FileStream(assemblyPath, FileMode.CreateNew))
                        {
                            errorLogger = errorOutput;
                            if (!GenerateRoslynAssembly(syntaxTrees, dllStream, null))
                                throw new Exception(errorOutput.GetStringBuilder().ToString());
                        }
                    }
                    else
                        codeWriter.GenerateBinary(userTypes, xmlConfig.GeneratedAssemblyName, !xmlConfig.DisablePdbGeneration, dependentAssemblies.Select(da => ResolveAssemblyPath(da)));
                }
                finally
                {
                    Console.SetOut(consoleOut);
                }
            }
        }

        /// <summary>
        /// Generates user types using the specified XML configuration.
        /// </summary>
        /// <param name="xmlConfig">The XML configuration.</param>
        /// <param name="logger">The logger text writer. If set to null, Console.Out will be used.</param>
        /// <param name="errorLogger">The error logger text writer. If set to null, Console.Error will be used.</param>
        public void Generate(XmlConfig xmlConfig, TextWriter logger = null, TextWriter errorLogger = null)
        {
            ConcurrentDictionary<string, string> generatedFiles = new ConcurrentDictionary<string, string>();
            var syntaxTrees = new ConcurrentBag<SyntaxTree>();
            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";

            // Check logger and error logger
            if (errorLogger == null)
            {
                errorLogger = Console.Error;
            }

            if (logger == null)
            {
                logger = Console.Out;
            }

            Initialize(xmlConfig, logger, errorLogger);

            // Create output directory
            Directory.CreateDirectory(outputDirectory);

            // Generate user types
            GenerateUserTypes();

            // Use IL code writer for assembly generation.
            if (xmlConfig.GenerateAssemblyWithILWriter && !string.IsNullOrEmpty(xmlConfig.GeneratedAssemblyName))
            {
                logger.Write("IL emitting time...");
                codeWriter.GenerateBinary(userTypes, xmlConfig.GeneratedAssemblyName, !xmlConfig.DisablePdbGeneration, dependentAssemblies.Select(da => ResolveAssemblyPath(da)));
                logger.WriteLine(" {0}", stopwatch.Elapsed);
            }
            else
            {
                // Code generation and saving it to disk
                logger.Write("Saving code to disk...");

                if (!generationOptions.HasFlag(UserTypeGenerationFlags.SingleFileExport))
                {
                    // Generate Code
                    Parallel.ForEach(userTypes,
                        (symbolEntry) =>
                        {
                            Tuple<string, string> result = GenerateCode(codeWriter, symbolEntry, userTypeFactory, outputDirectory, errorLogger, generationOptions, generatedFiles);
                            string text = result.Item1;
                            string filename = result.Item2;

                            if (!string.IsNullOrEmpty(xmlConfig.GeneratedAssemblyName) && !string.IsNullOrEmpty(text))
                                lock (syntaxTrees)
                                {
                                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, path: filename, encoding: System.Text.UTF8Encoding.Default));
                                }
                        });
                }
                else
                {
                    string filename = string.Format(@"{0}\everything.exported.cs", outputDirectory);
                    string code = GenerateSingleFile();

                    generatedFiles.TryAdd(filename.ToLowerInvariant(), filename);
                    if (!generationOptions.HasFlag(UserTypeGenerationFlags.DontSaveGeneratedCodeFiles))
                    {
                        File.WriteAllText(filename, code);
                    }

                    if (!string.IsNullOrEmpty(xmlConfig.GeneratedAssemblyName))
                    {
                        logger.WriteLine(" {0}", stopwatch.Elapsed);
                        logger.Write("Parsing generated code with Roslyn...");
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: filename, encoding: UTF8Encoding.Default));
                    }
                }

                logger.WriteLine(" {0}", stopwatch.Elapsed);

                // Compiling the code
                if (!string.IsNullOrEmpty(xmlConfig.GeneratedAssemblyName))
                {
                    string dllFilename = Path.Combine(outputDirectory, xmlConfig.GeneratedAssemblyName);
                    string pdbFilename = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(dllFilename) + ".pdb");

                    using (var dllStream = new FileStream(dllFilename, FileMode.Create))
                    using (var pdbStream = new FileStream(pdbFilename, FileMode.Create))
                    {
                        if (GenerateRoslynAssembly(syntaxTrees, dllStream, pdbStream))
                        {
                            logger.WriteLine("DLL size: {0}", dllStream.Position);
                            logger.WriteLine("PDB size: {0}", pdbStream.Position);
                        }
                    }

                    logger.WriteLine("Compiling: {0}", stopwatch.Elapsed);
                }

                // Generating props file
                if (!string.IsNullOrEmpty(xmlConfig.GeneratedPropsFileName))
                {
                    using (TextWriter output = new StreamWriter(outputDirectory + xmlConfig.GeneratedPropsFileName, false /* append */, System.Text.Encoding.UTF8, 16 * 1024 * 1024))
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

            logger.WriteLine("Total time: {0}", stopwatch.Elapsed);
        }

        /// <summary>
        /// Initializes members from the specified CodeGen configuration.
        /// </summary>
        /// <param name="xmlConfig">The CodeGen configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="errorLogger">The error logger.</param>
        private void Initialize(XmlConfig xmlConfig, TextWriter logger, TextWriter errorLogger)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            this.logger = logger;
            this.errorLogger = errorLogger;
            this.xmlConfig = xmlConfig;
            xmlModules = xmlConfig.Modules;
            typeNames = xmlConfig.Types;
            includedFiles = xmlConfig.IncludedFiles;
            referencedAssemblies = xmlConfig.ReferencedAssemblies;
            generationOptions = xmlConfig.GetGenerationFlags();
            int nameLimit = xmlConfig.GenerateAssemblyWithILWriter ? 10000 : 1000;
            if (xmlConfig.GenerateAssemblyWithILWriter && !string.IsNullOrEmpty(xmlConfig.GeneratedAssemblyName))
                codeWriter = new ManagedILCodeWriter(Path.GetFileNameWithoutExtension(xmlConfig.GeneratedAssemblyName), generationOptions, nameLimit);
            else
                codeWriter = new CSharpCodeWriter(generationOptions, nameLimit);
            userTypeFactory = new UserTypeFactory(xmlConfig.Transformations, codeWriter.Naming);
            userTypes = new List<UserType>();
        }

        /// <summary>
        /// Generates the user types.
        /// </summary>
        private void GenerateUserTypes()
        {
            // Verify that included files exist
            if (!string.IsNullOrEmpty(xmlConfig.GeneratedAssemblyName))
            {
                foreach (var file in includedFiles)
                {
                    if (!File.Exists(file.Path))
                    {
                        throw new FileNotFoundException("Included file not found", file.Path);
                    }
                }
            }

            // Loading modules
            Module[] modulesArray = new Module[xmlModules.Length];

            logger.Write("Loading modules...");
            Parallel.For(0, xmlModules.Length, (i) =>
            {
                Module module = moduleProvider.Open(xmlModules[i]);

                modulesArray[i] = module;
            });

            Dictionary<Module, XmlModule> modules = new Dictionary<Module, XmlModule>();
            for (int i = 0; i < modulesArray.Length; i++)
                modules.Add(modulesArray[i], xmlModules[i]);

            logger.WriteLine(" {0}", stopwatch.Elapsed);

            // Enumerating symbols
            Symbol[][] globalTypesPerModule = new Symbol[xmlModules.Length][];

            logger.Write("Enumerating symbols...");
            Parallel.For(0, xmlModules.Length, (i) =>
            {
                XmlModule xmlModule = xmlModules[i];
                Module module = modulesArray[i];
                string moduleName = xmlModule.Name;
                string nameSpace = xmlModule.Namespace;
                HashSet<Symbol> symbols = new HashSet<Symbol>();

                foreach (var type in typeNames)
                {
                    Symbol[] foundSymbols = module.FindGlobalTypeWildcard(type.NameWildcard);

                    if (foundSymbols.Length == 0)
                        errorLogger.WriteLine("Symbol not found: {0}", type.Name);
                    else
                        foreach (Symbol symbol in foundSymbols)
                            symbols.Add(symbol);

                    if (type.ExportDependentTypes)
                        foreach (Symbol symbol in foundSymbols)
                            symbol.ExtractDependentSymbols(symbols, xmlConfig.Transformations);
                }

                if (symbols.Count == 0)
                    foreach (Symbol symbol in module.GetAllTypes())
                        symbols.Add(symbol);

                globalTypesPerModule[i] = symbols.Where(t => t.Tag == CodeTypeTag.Class || t.Tag == CodeTypeTag.Structure || t.Tag == CodeTypeTag.Union || t.Tag == CodeTypeTag.Enum).ToArray();

                // Cache global scope
                if (generationOptions.HasFlag(UserTypeGenerationFlags.InitializeSymbolCaches))
                {
                    var globalScope = module.GlobalScope;

                    globalScope.InitializeCache();
                }
            });

            List<Symbol> allSymbols = globalTypesPerModule.SelectMany(ss => ss).ToList();

            logger.WriteLine(" {0}", stopwatch.Elapsed);

            // Initialize symbol fields and base classes
            if (generationOptions.HasFlag(UserTypeGenerationFlags.InitializeSymbolCaches))
            {
                logger.Write("Initializing symbol values...");
                Parallel.ForEach(Partitioner.Create(allSymbols), symbol => symbol.InitializeCache());
                logger.WriteLine(" {0}", stopwatch.Elapsed);
            }

            logger.Write("Deduplicating symbols...");

            // Group duplicated symbols
            Dictionary<string, List<Symbol>> symbolsByName = new Dictionary<string, List<Symbol>>();
            Dictionary<Symbol, List<Symbol>> duplicatedSymbols = new Dictionary<Symbol, List<Symbol>>();

            foreach (var symbol in allSymbols)
            {
                List<Symbol> symbols;

                if (!symbolsByName.TryGetValue(symbol.Name, out symbols))
                    symbolsByName.Add(symbol.Name, symbols = new List<Symbol>());

                bool found = false;

                for (int i = 0; i < symbols.Count; i++)
                {
                    Symbol s = symbols[i];

                    if (s.Size != 0 && symbol.Size != 0 && s.Size != symbol.Size)
                    {
#if DEBUG
                        logger.WriteLine("{0}!{1} ({2}) {3}!{4} ({5})", s.Module.Name, s.Name, s.Size, symbol.Module.Name, symbol.Name, symbol.Size);
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

                for (int i = 0, n = symbols.Count; i < n; i++)
                {
                    Symbol s = symbols[i];
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
                if (symbols.Count != 1 || modules.Count == 1)
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
                        symbolNamespaces.Add(s, xmlConfig.CommonTypesNamespace);
                }
            }

            var globalTypes = symbolsByName.SelectMany(s => s.Value).ToArray();

            logger.WriteLine(" {0}", stopwatch.Elapsed);
            logger.WriteLine("  Total symbols: {0}", globalTypesPerModule.Sum(gt => gt.Length));
            logger.WriteLine("  Unique symbol names: {0}", symbolsByName.Count);
            logger.WriteLine("  Dedupedlicated symbols: {0}", globalTypes.Length);

            // Initialize GlobalCache with deduplicatedSymbols
            GlobalCache.Update(deduplicatedSymbols);

            // Collecting types
            logger.Write("Collecting types...");

            foreach (var module in modules.Keys)
                userTypes.Add(userTypeFactory.AddSymbol(module.GlobalScope, null, modules[module].Namespace, generationOptions));

            ConcurrentBag<Symbol> simpleSymbols = new ConcurrentBag<Symbol>();
            Dictionary<Tuple<string, string>, List<Symbol>> templateSymbols = new Dictionary<Tuple<string, string>, List<Symbol>>();

            Parallel.ForEach(Partitioner.Create(globalTypes), (symbol) =>
            {
                string symbolName = symbol.Name;

                // TODO: Add configurable filter
                //
                if (symbolName.StartsWith("$") || symbolName.StartsWith("__vc_attributes") || symbolName.Contains("`anonymous-namespace'") || symbolName.Contains("`anonymous namespace'") || symbolName.Contains("::$") || symbolName.Contains("`"))
                {
                    return;
                }

                // Do not handle template referenced arguments
                if (symbolName.Contains("&"))
                {
                    // TODO: Convert this to function pointer
                    return;
                }

                // TODO: For now remove all unnamed-type symbols
                string scopedClassName = symbol.Namespaces.Last();

                if (scopedClassName.StartsWith("<") || symbolName.Contains("::<"))
                {
                    return;
                }

                // Check if symbol contains template type.
                if ((symbol.Tag == CodeTypeTag.Class || symbol.Tag == CodeTypeTag.Structure || symbol.Tag == CodeTypeTag.Union) && SymbolNameHelper.ContainsTemplateType(symbolName))
                {
                    List<string> namespaces = symbol.Namespaces;
                    string className = namespaces.Last();
                    var symbolId = Tuple.Create(symbolNamespaces[symbol], SymbolNameHelper.CreateLookupNameForSymbol(symbol));

                    lock (templateSymbols)
                    {
                        if (!templateSymbols.ContainsKey(symbolId))
                            templateSymbols[symbolId] = new List<Symbol>() { symbol };
                        else
                            templateSymbols[symbolId].Add(symbol);
                    }

                    // TODO:
                    // Do not add physical types for template specialization (not now)
                    // do if types contains static fields
                    // nested in templates
                }
                else
                    simpleSymbols.Add(symbol);
            });

            logger.WriteLine(" {0}", stopwatch.Elapsed);

            // Populate Templates
            logger.Write("Populating templates...");
            foreach (List<Symbol> symbols in templateSymbols.Values)
            {
                Symbol symbol = symbols.First();
                userTypes.AddRange(userTypeFactory.AddTemplateSymbols(symbols, symbolNamespaces[symbol], generationOptions));
            }

            logger.WriteLine(" {0}", stopwatch.Elapsed);

            // Specialized class
            logger.Write("Populating specialized classes...");
            foreach (Symbol symbol in simpleSymbols)
            {
                userTypes.Add(userTypeFactory.AddSymbol(symbol, null, symbolNamespaces[symbol], generationOptions));
            }

            logger.WriteLine(" {0}", stopwatch.Elapsed);

            // To solve template dependencies. Update specialization arguments once all the templates has been populated.
            logger.Write("Updating template arguments...");
            foreach (TemplateUserType templateUserType in userTypes.OfType<TemplateUserType>())
            {
                foreach (SpecializedTemplateUserType specializedTemplateUserType in templateUserType.Specializations)
                    if (!specializedTemplateUserType.UpdateTemplateArguments(userTypeFactory))
                    {
#if DEBUG
                        logger.WriteLine("Template user type cannot be updated: {0}", specializedTemplateUserType.Symbol.Name);
#endif
                    }
            }

            logger.WriteLine(" {0}", stopwatch.Elapsed);

            // Post processing user types (filling DeclaredInType)
            logger.Write("Post processing user types...");
            var namespaceTypes = userTypeFactory.ProcessTypes(userTypes, symbolNamespaces, xmlConfig.CommonTypesNamespace ?? modules.First().Key.Namespace).ToArray();
            userTypes.AddRange(namespaceTypes);

            logger.WriteLine(" {0}", stopwatch.Elapsed);
        }

        /// <summary>
        /// Generates the single file code.
        /// </summary>
        /// <returns>Generated C# code</returns>
        private string GenerateSingleFile()
        {
            StringBuilder stringOutput = new StringBuilder();
            System.Threading.ThreadLocal<StringBuilder> stringWriters = new System.Threading.ThreadLocal<StringBuilder>(() => new StringBuilder());
            string[] texts = new string[userTypes.Count];

            Parallel.For(0, userTypes.Count, (i) =>
            {
                var symbolEntry = userTypes[i];
                var output = stringWriters.Value;

                output.Clear();
                GenerateCodeInSingleFile(codeWriter, output, symbolEntry, userTypeFactory, errorLogger, generationOptions);
                string text = output.ToString();

                if (!string.IsNullOrEmpty(text))
                    texts[i] = text;
            });
            foreach (string text in texts)
                stringOutput.AppendLine(text);
            return stringOutput.ToString();
        }

        /// <summary>
        /// Generates the assembly with Roslyn.
        /// </summary>
        /// <param name="syntaxTreesInput">The syntax trees input.</param>
        /// <param name="dllStream">The DLL stream.</param>
        /// <param name="pdbStream">The PDB stream.</param>
        /// <returns><c>true</c> if compilation succeeded.</returns>
        private bool GenerateRoslynAssembly(IEnumerable<SyntaxTree> syntaxTreesInput, Stream dllStream, Stream pdbStream)
        {
            List<SyntaxTree> syntaxTrees = syntaxTreesInput.ToList();
            List<MetadataReference> references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Dynamic.DynamicObject).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.IO.FileAttributes).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(SharpUtilities.MemoryBuffer).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location),
                };

            foreach (string dependentAssembly in dependentAssemblies)
            {
                if (!xmlConfig.ReferencedAssemblies.Any(a => a.Path.EndsWith(dependentAssembly)))
                {
                    references.Add(MetadataReference.CreateFromFile(ResolveAssemblyPath(dependentAssembly)));
                }
            }

            references.AddRange(xmlConfig.ReferencedAssemblies.Select(r => MetadataReference.CreateFromFile(ResolveAssemblyPath(r.Path))));

            foreach (var includedFile in includedFiles)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(includedFile.Path), path: includedFile.Path, encoding: System.Text.UTF8Encoding.Default));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(xmlConfig.GeneratedAssemblyName),
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, platform: Platform.AnyCpu));

            logger.WriteLine("Syntax trees: {0}", syntaxTrees.Count);

            var result = compilation.Emit(dllStream, pdbStream);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                errorLogger.WriteLine("Compile errors (top 1000):");
                foreach (var diagnostic in failures.Take(1000))
                {
                    errorLogger.WriteLine(diagnostic);
                }
            }

            return result.Success;
        }

        /// <summary>
        /// Generates the code for user type and creates a file for it.
        /// </summary>
        /// <param name="codeWriter">The code writer used to output generated code.</param>
        /// <param name="userType">The user type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="outputDirectory">The output directory where code file will be stored.</param>
        /// <param name="errorOutput">The error output.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <param name="generatedFiles">The list of already generated files.</param>
        /// <returns>Tuple of generated code and filename</returns>
        private static Tuple<string, string> GenerateCode(ICodeWriter codeWriter, UserType userType, UserTypeFactory factory, string outputDirectory, TextWriter errorOutput, UserTypeGenerationFlags generationFlags, ConcurrentDictionary<string, string> generatedFiles)
        {
            Symbol symbol = userType.Symbol;

            if (symbol != null && symbol.Tag == CodeTypeTag.BuiltinType)
            {
                // Ignore built-in types.
                return Tuple.Create("", "");
            }

            bool allParentsAreNamespaces = true;

            for (UserType parentType = userType.DeclaredInType; parentType != null && allParentsAreNamespaces; parentType = parentType.DeclaredInType)
            {
                allParentsAreNamespaces = parentType is NamespaceUserType;
            }

            if (userType is NamespaceUserType || !allParentsAreNamespaces)
            {
                return Tuple.Create("", "");
            }

            string classOutputDirectory = outputDirectory;
            string nameSpace = (userType.DeclaredInType as NamespaceUserType)?.FullTypeName ?? userType.Namespace;

            if (!string.IsNullOrEmpty(nameSpace))
            {
                classOutputDirectory = Path.Combine(classOutputDirectory, nameSpace.Replace(".", "\\").Replace(":", "."));
            }
            if (!generationFlags.HasFlag(UserTypeGenerationFlags.DontSaveGeneratedCodeFiles))
            {
                Directory.CreateDirectory(classOutputDirectory);
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

                filename = string.Format(@"{0}\{1}{2}_{3}.exported.cs", classOutputDirectory, userType.ConstructorName, isEnum ? "_enum" : "", index++);
            }

            StringBuilder stringOutput = new StringBuilder();
            codeWriter.WriteUserType(userType, stringOutput);
            string text = stringOutput.ToString();
            if (!generationFlags.HasFlag(UserTypeGenerationFlags.DontSaveGeneratedCodeFiles))
            {
                File.WriteAllText(filename, text);
            }
            return Tuple.Create(text, filename);
        }

        /// <summary>
        /// Generates the code for user type in single file.
        /// </summary>
        /// <param name="codeWriter">The code writer used to output generated code.</param>
        /// <param name="output">The output text writer.</param>
        /// <param name="userType">The user type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="errorOutput">The error output.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <returns><c>true</c> if code was generated for the type; otherwise <c>false</c></returns>
        private static bool GenerateCodeInSingleFile(ICodeWriter codeWriter, StringBuilder output, UserType userType, UserTypeFactory factory, TextWriter errorOutput, UserTypeGenerationFlags generationFlags)
        {
            Symbol symbol = userType.Symbol;

            if (symbol != null && symbol.Tag == CodeTypeTag.BuiltinType)
            {
                // Ignore built-in types.
                return false;
            }

            if (userType.DeclaredInType != null)
            {
                return false;
            }

            codeWriter.WriteUserType(userType, output);
            return true;
        }

        /// <summary>
        /// Resolves the specified assembly path if it is not rooted.
        /// </summary>
        /// <param name="path">Original assembly path.</param>
        /// <returns>Rooted assembly path if found.</returns>
        private static string ResolveAssemblyPath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                // Check assembly location folder
                Assembly assembly = Assembly.GetExecutingAssembly();
                string binFolder = Path.GetDirectoryName(assembly.Location);
                string rootedPath = Path.Combine(binFolder, path);

                if (File.Exists(rootedPath))
                {
                    return rootedPath;
                }

#if NET461
                // Check loaded assemblies
                foreach (var assemblyPath in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location))
                {
                    if (assemblyPath.EndsWith(path))
                    {
                        return assemblyPath;
                    }
                }
#endif

                // Check assembly code base folder
                Uri codeBaseUrl = new Uri(assembly.CodeBase);
                string codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);

                binFolder = Path.GetDirectoryName(codeBasePath);
                rootedPath = Path.Combine(binFolder, path);
                if (File.Exists(rootedPath))
                {
                    return rootedPath;
                }
            }
            return path;
        }
    }

    /// <summary>
    /// Simple object pool leveraging <see cref="ConcurrentBag{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of object that will be stored in the pool.</typeparam>
    internal class ObjectPool<T>
    {
        /// <summary>
        /// The concurrent bag of objects
        /// </summary>
        private ConcurrentBag<T> objects;

        /// <summary>
        /// The object generator function
        /// </summary>
        private Func<T> objectGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="objectGenerator">The object generator function.</param>
        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
                throw new ArgumentNullException(nameof(objectGenerator));
            objects = new ConcurrentBag<T>();
            this.objectGenerator = objectGenerator;
        }

        /// <summary>
        /// Gets the object from the pool.
        /// </summary>
        public T GetObject()
        {
            T item;

            if (objects.TryTake(out item))
                return item;
            return objectGenerator();
        }

        /// <summary>
        /// Puts the object back to the pool.
        /// </summary>
        /// <param name="item">The object.</param>
        public void PutObject(T item)
        {
            objects.Add(item);
        }
    }
}
