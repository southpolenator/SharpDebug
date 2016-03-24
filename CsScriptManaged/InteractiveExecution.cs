using CsScripts;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsScriptManaged
{
    internal class InteractiveExecution : ScriptCompiler
    {
        /// <summary>
        /// The default prompt
        /// </summary>
        private const string DefaultPrompt = "C#> ";

        /// <summary>
        /// The clear imports statement
        /// </summary>
        private const string ClearImportsStatement = "ClearImports;";

        /// <summary>
        /// The interactive script name. It is being used as file name during interactive commands compile time.
        /// </summary>
        private const string InteractiveScriptName = "_interactive_script_.cs";

        /// <summary>
        /// The interactive script variables name. This must match variable name in InteractiveScriptBase class.
        /// </summary>
        private const string InteractiveScriptVariables = "_Interactive_Script_Variables_";

        /// <summary>
        /// The dynamic variables used in interactive script.
        /// </summary>
        private ExpandoObject dynamicVariables = new ExpandoObject();

        /// <summary>
        /// The loaded scripts
        /// </summary>
        private List<string> loadedScripts = new List<string>();

        /// <summary>
        /// The imported code from the loaded scripts
        /// </summary>
        private string importedCode = "";

        /// <summary>
        /// The usings
        /// </summary>
        private List<string> usings = new List<string>(new string[] { "System", "System.Linq", "CsScripts" });

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
                string trimmedCommand = command;

                // Check if we should exit main loop
                if (trimmedCommand == "q" || trimmedCommand == "Q")
                    break;

                // Check if we should execute C# command
                try
                {
                    if (trimmedCommand.EndsWith(";"))
                    {
                        Interpret(trimmedCommand, prompt);
                    }
                    else
                    {
                        Debugger.Execute(command);
                    }
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
        public void Interpret(string code, string prompt = "")
        {
            try
            {
                if (code == ClearImportsStatement)
                {
                    importedCode = "";
                    loadedScripts = new List<string>();
                }
                else
                {
                    Execute(code);
                }
            }
            catch (CompileException ex)
            {
                CompilerError[] errors = ex.Errors;

                if (!string.IsNullOrEmpty(prompt) && (errors[0].FileName.EndsWith(InteractiveScriptName) || errors.Count(e => !e.FileName.EndsWith(InteractiveScriptName)) == 0))
                {
                    CompilerError e = errors[0];

                    Console.Error.WriteLine("{0}^ {1}", new string(' ', prompt.Length + e.Column - 1), e.ErrorText);
                }
                else
                {
                    Console.Error.WriteLine("Compile errors:");
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error);
                    }
                }
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
            // Extract imports and usings
            HashSet<string> newLoadedScripts = new HashSet<string>(loadedScripts);
            HashSet<string> newUsings = new HashSet<string>(usings);
            HashSet<string> imports = new HashSet<string>();
            HashSet<string> referencedAssemblies = new HashSet<string>();
            StringBuilder newImportedCode = new StringBuilder(importedCode);

            code = ImportCode(code, newUsings, imports);
            while (imports.Count > 0)
            {
                HashSet<string> newImports = new HashSet<string>();

                foreach (string import in imports)
                {
                    if (!newLoadedScripts.Contains(import))
                    {
                        if (Path.GetExtension(import).ToLower() == ".dll")
                        {
                            referencedAssemblies.Add(import);
                        }
                        else
                        {
                            string file = ImportFile(import, usings, newImports);

                            newImportedCode.AppendLine(file);
                            newLoadedScripts.Add(import);
                        }
                    }
                }

                imports = newImports;
            }

            // Compile code
            var compileResult = CompileByReplacingVariables(ref code, newUsings, newImportedCode.ToString(), referencedAssemblies.ToArray());

            // Report compile error
            List<CompilerError> errors = new List<CompilerError>();
            string[] codeLines = null;
            string varName = InteractiveScriptVariables + ".";

            foreach (CompilerError error in compileResult.Errors)
            {
                if (!error.IsWarning)
                {
                    if (error.FileName.EndsWith(InteractiveScriptName))
                    {
                        if (codeLines == null)
                        {
                            codeLines = code.Replace("\r\n", "\n").Split("\n".ToCharArray());
                        }

                        error.Column -= Count(codeLines[error.Line - 1], varName) * varName.Length;
                    }

                    errors.Add(error);
                }
            }

            if (errors.Count > 0)
            {
                throw new CompileException(errors.ToArray());
            }

            // Execute compiled code
            var myClass = compileResult.CompiledAssembly.GetType(AutoGeneratedNamespace + "." + AutoGeneratedClassName);
            var method = myClass.GetMethod(AutoGeneratedScriptFunctionName);
            dynamic obj = Activator.CreateInstance(myClass);

            obj._Interactive_Script_Variables_ = dynamicVariables;
            method.Invoke(obj, new object[] { new string[] { } });

            // Save imports and usings
            importedCode = newImportedCode.ToString();
            loadedScripts = newLoadedScripts.ToList();
            usings = newUsings.ToList();
        }

        /// <summary>
        /// Compiles the code, but replaces any undeclared variable with dynamic one in InteractiveScriptBase.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="usings">The usings.</param>
        /// <param name="importedCode">The imported code.</param>
        /// <param name="referencedAssemblies">The referenced assemblies.</param>
        private CompilerResults CompileByReplacingVariables(ref string code, IEnumerable<string> usings, string importedCode, params string[] referencedAssemblies)
        {
            while (true)
            {
                string generatedCode = "#line 1 \"" + InteractiveScriptName + "\"\n" + code + "\n#line default\n";
                generatedCode = GenerateCode(usings, importedCode, generatedCode, "CsScriptManaged.InteractiveScriptBase");

                var compileResult = Compile(generatedCode, referencedAssemblies);

                bool fixedError = false;

                foreach (CompilerError error in compileResult.Errors)
                {
                    // Try to fix undeclared variable errors
                    if (error.FileName.EndsWith(InteractiveScriptName) && error.ErrorNumber == "CS0103")
                    {
                        StringBuilder sb = new StringBuilder();

                        using (StringReader reader = new StringReader(code))
                        using (StringWriter writer = new StringWriter(sb))
                        {
                            for (int lineNumber = 1; ; lineNumber++)
                            {
                                string line = reader.ReadLine();

                                if (line == null)
                                    break;

                                if (lineNumber == error.Line)
                                {
                                    line = line.Substring(0, error.Column - 1) + InteractiveScriptVariables + "." + line.Substring(error.Column - 1);
                                }

                                writer.WriteLine(line);
                            }
                        }

                        // Save the code and remove \r\n from the end
                        code = sb.ToString().Substring(0, sb.Length - 2);
                        fixedError = true;
                        break;
                    }
                    else if (error.FileName.EndsWith(InteractiveScriptName) && error.ErrorNumber == "CS1002")
                    {
                        bool incorrect = false;

                        using (StringReader reader = new StringReader(code))
                        {
                            for (int lineNumber = 1; !incorrect; lineNumber++)
                            {
                                string line = reader.ReadLine();

                                if (line == null)
                                    break;

                                if (lineNumber == error.Line)
                                {
                                    if (line.Substring(error.Column - 1).Trim().Length != 0)
                                    {
                                        incorrect = true;
                                    }
                                    else if (reader.ReadToEnd().Trim().Length != 0)
                                    {
                                        incorrect = true;
                                    }
                                }
                            }
                        }

                        if (!incorrect)
                        {
                            code += ";";
                            fixedError = true;
                            break;
                        }
                    }
                }

                if (!fixedError)
                {
                    return compileResult;
                }
            }
        }
    }
}
