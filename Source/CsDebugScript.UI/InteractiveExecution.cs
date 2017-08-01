using CsDebugScript.Engine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CsDebugScript
{
    /// <summary>
    /// Internal exception used for stopping interactive scripting
    /// </summary>
    internal class ExitRequestedException : Exception
    {
    }

    internal class InteractiveExecution
    {
        /// <summary>
        /// The default prompt
        /// </summary>
        internal const string DefaultPrompt = "C#> ";

        /// <summary>
        /// Roslyn script state
        /// </summary>
        private ScriptState<object> scriptState;

        /// <summary>
        /// Interactive script base - Roslyn globals object
        /// </summary>
        internal InteractiveScriptBase scriptBase = new InteractiveScriptBase();

        /// <summary>
        /// The imported code (functions and classes defined in the scripts)
        /// </summary>
        private string importedCode = string.Empty;

        /// <summary>
        /// The list of imports (using commands)
        /// </summary>
        private HashSet<string> usings = new HashSet<string>(ScriptCompiler.DefaultUsings);

        /// <summary>
        /// The list of assembly references
        /// </summary>
        private HashSet<string> references = new HashSet<string>(ScriptCompiler.DefaultAssemblyReferences);

        /// <summary>
        /// The metadata resolver
        /// </summary>
        private ScriptExecution.MetadataResolver metadataResolver;

        /// <summary>
        /// The source resolver
        /// </summary>
        private ScriptExecution.SourceResolver sourceResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExecution"/> class.
        /// </summary>
        public InteractiveExecution()
        {
            var scriptOptions = ScriptOptions.Default.WithImports(ScriptCompiler.DefaultUsings).AddReferences(ScriptCompiler.DefaultAssemblyReferences);

            var originalSourceResolver = scriptOptions.SourceResolver;
            var originalMetadataResolver = scriptOptions.MetadataResolver;

            metadataResolver = new ScriptExecution.MetadataResolver(originalMetadataResolver);
            sourceResolver = new ScriptExecution.SourceResolver(originalSourceResolver);
            scriptOptions = scriptOptions.WithMetadataResolver(metadataResolver);
            scriptOptions = scriptOptions.WithSourceResolver(sourceResolver);

            scriptState = CSharpScript.RunAsync("", scriptOptions, scriptBase).Result;
            scriptBase.ObjectWriter = new DefaultObjectWriter();
            scriptBase._InternalObjectWriter_ = new ConsoleObjectWriter();
        }

        /// <summary>
        /// Runs interactive scripting mode.
        /// </summary>
        public void Run()
        {
            string prompt = DefaultPrompt;

            while (true)
            {
                // Read command
                string command = ReadCommand(prompt);

                // Check if we should execute C# command
                try
                {
                    Interpret(command, prompt);
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException is ExitRequestedException)
                        break;
                    throw;
                }
                catch (ExitRequestedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Interprets C# code.
        /// </summary>
        /// <param name="code">The C# code.</param>
        /// <param name="prompt">The prompt.</param>
        public void Interpret(string code, string prompt = "")
        {
            try
            {
                UnsafeInterpret(code);
            }
            catch (CompilationErrorException ex)
            {
                Console.Error.WriteLine("Compile errors:");
                foreach (var error in ex.Diagnostics)
                {
                    Console.Error.WriteLine($"  {error}");
                }
            }
        }

        private static readonly CSharpParseOptions s_parseOptions =
            new CSharpParseOptions(languageVersion: LanguageVersion.CSharp6, kind: SourceCodeKind.Script);

        internal static bool IsCompleteSubmission(string text)
        {
            return SyntaxFactory.IsCompleteSubmission(SyntaxFactory.ParseSyntaxTree(text, options: s_parseOptions));
        }

        /// <summary>
        /// Interprets C# code, but without error handling.
        /// </summary>
        /// <param name="code">The C# code.</param>
        internal void UnsafeInterpret(string code)
        {
            if (!code.StartsWith("#dbg "))
            {
                Execute(code);
            }
            else
            {
                Debugger.Execute(code.Substring(5));
            }
        }

        /// <summary>
        /// Reads the command from the user.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        private static string ReadCommand(string prompt)
        {
            // Write prompt
            prompt = prompt.Replace("%", "%%");
            Console.Write(prompt);

            // Read string
            return Context.Debugger.ReadInput();
        }

        /// <summary>
        /// Counts the number of occurrences in the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="find">The find.</param>
        private static int Count(string str, string find)
        {
            int index = str.IndexOf(find);
            int count = 0;

            while (index >= 0)
            {
                count++;
                index = str.IndexOf(find, index + find.Length);
            }

            return count;
        }

        /// <summary>
        /// Executes the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        private void Execute(string code)
        {
            try
            {
                InteractiveScriptBase.Current = scriptBase;
                scriptBase._ScriptState_ = scriptState;
                scriptBase._CodeGenCode_ = scriptBase._CodeGenCode_ ?? new List<ImportUserTypeCode>();
                scriptBase._CodeResolver_ = sourceResolver;
                scriptBase._CodeGenAssemblies_ = scriptBase._CodeGenAssemblies_ ?? new List<ImportUserTypeAssembly>();
                scriptBase._AssemblyResolver_ = metadataResolver;
                scriptState = scriptState.ContinueWithAsync(code).Result;
                importedCode = ExtractImportedCode(scriptState.Script, importedCode);
                UpdateUsings(scriptState.Script, usings);
                UpdateReferences(scriptState.Script, references);
                scriptBase.Dump(scriptState.ReturnValue);

                if (scriptBase._InteractiveScriptBaseType_ != null && scriptBase._InteractiveScriptBaseType_ != scriptBase.GetType())
                {
                    var oldScriptBase = scriptBase;

                    scriptBase = (InteractiveScriptBase)Activator.CreateInstance(scriptBase._InteractiveScriptBaseType_);
                    scriptBase.ObjectWriter = oldScriptBase.ObjectWriter;
                    scriptBase._InternalObjectWriter_ = oldScriptBase._InternalObjectWriter_;

                    // TODO: Changing globals, but we need to store previous variables
                    scriptState = CSharpScript.RunAsync("", scriptState.Script.Options, scriptBase).Result;
                }

                if (scriptBase._CodeGenCode_.Count > 0)
                {
                    foreach (ImportUserTypeCode codeGenCode in scriptBase._CodeGenCode_)
                    {
                        scriptState = scriptState.ContinueWithAsync(codeGenCode.Code).Result;
                        sourceResolver.AddCode(codeGenCode);
                    }

                    importedCode = ExtractImportedCode(scriptState.Script, importedCode);
                    UpdateUsings(scriptState.Script, usings);
                    UpdateReferences(scriptState.Script, references);
                    scriptBase._CodeGenCode_.Clear();
                }

                if (scriptBase._CodeGenAssemblies_.Count > 0)
                {
                    foreach (ImportUserTypeAssembly assembly in scriptBase._CodeGenAssemblies_)
                    {
                        metadataResolver.AddAssembly(assembly);
                    }

                    List<MetadataReference> originalReferences = scriptState.Script.Options.MetadataReferences.ToList();
                    ScriptOptions options = scriptState.Script.Options
                        .WithReferences(originalReferences)
                        .AddReferences(scriptBase._CodeGenAssemblies_.Select(a => a.AssemblyPath));

                    scriptState = scriptState.Script.ContinueWith("", options).RunFromAsync(scriptState).Result;
                    importedCode = ExtractImportedCode(scriptState.Script, importedCode);
                    UpdateUsings(scriptState.Script, usings);
                    UpdateReferences(scriptState.Script, references);
                    scriptBase._CodeGenAssemblies_.Clear();
                }
            }
            finally
            {
                InteractiveScriptBase.Current = null;
            }
        }

        /// <summary>
        /// Extracts the imported code (functions and classes) from the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="previousImportedCode">The previously imported code.</param>
        private static string ExtractImportedCode(Script script, string previousImportedCode)
        {
            StringBuilder newCode = new StringBuilder(previousImportedCode);
            Compilation compilation = script.GetCompilation();

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SyntaxNode root = tree.GetRoot();

                if (root is CompilationUnitSyntax)
                {
                    foreach (SyntaxNode node in root.ChildNodes())
                    {
                        if (node is MethodDeclarationSyntax || node is ClassDeclarationSyntax || node is EnumDeclarationSyntax
                            || node is InterfaceDeclarationSyntax || node is StructDeclarationSyntax)
                        {
                            newCode.AppendLine(node.ToFullString());
                        }
                    }
                }
            }

            return newCode.ToString();
        }

        /// <summary>
        /// Updates the references from the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="references">The references.</param>
        private static void UpdateReferences(Script script, HashSet<string> references)
        {
            foreach (var reference in script.Options.MetadataReferences)
            {
                var ur = reference as UnresolvedMetadataReference;

                if (ur != null)
                {
                    references.Add(ur.Reference);
                }
                else
                {
                    references.Add(reference.Display);
                }
            }
        }

        /// <summary>
        /// The using command extraction regular expression.
        /// </summary>
        private static Regex usingExtractionRegex = new Regex(@"using ([^;]*);", RegexOptions.Compiled);

        /// <summary>
        /// Updates the list of using commands from the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="usings">The usings.</param>
        private static void UpdateUsings(Script script, HashSet<string> usings)
        {
            foreach (var import in script.Options.Imports)
            {
                usings.Add(import);
            }

            Compilation compilation = script.GetCompilation();

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                if (string.IsNullOrEmpty(tree.FilePath))
                {
                    SyntaxNode root = tree.GetRoot();

                    if (root is CompilationUnitSyntax)
                    {
                        foreach (SyntaxNode node in root.ChildNodes())
                        {
                            var usingDirective = node as UsingDirectiveSyntax;

                            if (usingDirective != null)
                            {
                                Match match = usingExtractionRegex.Match(usingDirective.ToString());

                                usings.Add(match.Groups[1].Value);
                            }
                        }
                    }
                }
            }
        }

        internal IEnumerable<string> GetScriptHelperCode(out string scriptStart, out string scriptEnd)
        {
            const string code = "<This is my unique code string>";
            string importedCode = FixImportedCode(this.importedCode);
            string generatedCode = GenerateCode(code, usings, importedCode);
            int codeStart = generatedCode.IndexOf(code), codeEnd = codeStart + code.Length;

            scriptStart = generatedCode.Substring(0, codeStart);
            scriptEnd = generatedCode.Substring(codeEnd);
            return references;
        }

        internal static string GetCodeName(Type type)
        {
            string typeString = type.FullName;

            if (typeString.Contains(".<") || typeString.Contains("+<"))
            {
                // TODO: Probably not the best one, but good enough for now
                return GetCodeName(type.GetInterfaces()[0]);
            }

            if (type.IsGenericType)
            {
                return string.Format("{0}<{1}>", typeString.Split('`')[0], string.Join(", ", type.GetGenericArguments().Select(x => GetCodeName(x))));
            }
            else
            {
                return typeString.Replace("+", ".");
            }
        }

        private string FixImportedCode(string importedCode)
        {
            StringBuilder newImportedCode = new StringBuilder(importedCode);

            // Get variables and add them as properties
            IEnumerable<string> variableNames = scriptState.Variables.Select(v => v.Name).Distinct();
            foreach (var variableName in variableNames)
            {
                var variable = scriptState.GetVariable(variableName);
                string typeString = GetCodeName(variable.Type);

                newImportedCode.Append(typeString);
                newImportedCode.Append(" ");
                newImportedCode.AppendLine(variableName);
                newImportedCode.AppendLine("{ get; set; }");
            }

            return newImportedCode.ToString();
        }

        private string GenerateCode(string code, IEnumerable<string> usings, string importedCode)
        {
            string generatedCode = code;

            return ScriptCompiler.GenerateCode(usings, importedCode, generatedCode, scriptBase.GetType().FullName);
        }
    }
}
