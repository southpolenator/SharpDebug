using CommandLine;
using Dia2Lib;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GenerateUserTypesFromPdb
{
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

        [Option("generated-assembly-name", Default = "", HelpText = "Name of the assembly that will be generated next to sources in output folder", Required = false, SetName = "cmdSettings")]
        public string GeneratedAssemblyName { get; set; }

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

            string moduleName = Path.GetFileNameWithoutExtension(pdbPath).ToLower();
            Dictionary<string, UserType> symbols = new Dictionary<string, UserType>();
            IDiaDataSource dia;
            IDiaSession session;

            OpenPdb(pdbPath, out dia, out session);
            foreach (var type in typeNames)
            {
                IDiaSymbol symbol = session.globalScope.GetChild(type.Name, SymTagEnum.SymTagUDT);

                if (symbol == null)
                {
                    error.WriteLine("Symbol not found: {0}", type.Name);
                }
                else
                {
                    symbols.Add(type.Name, new UserType(symbol, type, moduleName));
                }
            }

            foreach (var symbolEntry in symbols)
            {
                var userType = symbolEntry.Value;

                if (userType.Symbol.name.Contains("::"))
                {
                    string[] names = userType.Symbol.name.Split(new string[] { "::" }, StringSplitOptions.None);

                    string parentTypeName = string.Join("::", names.Take(names.Length - 1));
                    if (symbols.ContainsKey(parentTypeName))
                    {
                        userType.SetDeclaredInType(symbols[parentTypeName]);
                    }
                    else
                    {
                        throw new Exception("Unsupported namespace of class " + userType.Symbol.name);
                    }
                }
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";
            Directory.CreateDirectory(outputDirectory);
            List<string> generatedFiles = new List<string>();

            string[] allUDTs = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).Select(s => s.name).Distinct().OrderBy(s => s).ToArray();

            File.WriteAllLines(outputDirectory + "symbols.txt", allUDTs);

            foreach (var userType in symbols.Values.ToArray())
            {
                userType.UpdateUserTypes(symbols);
            }

            foreach (var symbolEntry in symbols)
            {
                var userType = symbolEntry.Value;
                var symbol = userType.Symbol;

                Console.WriteLine(symbolEntry.Key);
                if (userType.DeclaredInType != null)
                {
                    continue;
                }

                string filename = string.Format("{0}{1}.exported.cs", outputDirectory, symbol.name);

                using (TextWriter output = new StreamWriter(filename))
                {
                    userType.WriteCode(new IndentedWriter(output), error, symbols, config.Transformations, generationOptions);
                    generatedFiles.Add(filename);
                }
            }

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
}
