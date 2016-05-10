using CsDebugScript.Engine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// Initializes a new instance of the <see cref="InteractiveExecution"/> class.
        /// </summary>
        public InteractiveExecution()
        {
            var scriptOptions = ScriptOptions.Default.WithImports("System", "System.Linq", "CsDebugScript").WithReferences(ScriptCompiler.DefaultAssemblyReferences);

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
                    if (!command.StartsWith("#dbg "))
                    {
                        Interpret(command, prompt);
                    }
                    else
                    {
                        Debugger.Execute(command.Substring(5));
                    }
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
                Execute(code);
            }
            catch (CompilationErrorException ex)
            {
                Console.Error.WriteLine("Compile errors:");
                foreach (var error in ex.Diagnostics)
                {
                    Console.Error.WriteLine(error);
                }
            }
        }

        /// <summary>
        /// Interprets C# code, but without error handling.
        /// </summary>
        /// <param name="code">The C# code.</param>
        internal void UnsafeInterpret(string code)
        {
            Execute(code);
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
                scriptState = scriptState.ContinueWithAsync(code).Result;
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
            }
            finally
            {
                InteractiveScriptBase.Current = null;
            }
        }

        /// <summary>
        /// Gets the imports that are used in the current script.
        /// </summary>
        private IEnumerable<string> GetImports()
        {
            IEnumerable<string> imports = scriptState.Script.Options.Imports;

            for (var a = scriptState.Script.Previous; a != null; a = a.Previous)
                imports = imports.Union(a.Options.Imports);
            return imports.Distinct();
        }

        /// <summary>
        /// Gets the references used in the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        private IEnumerable<string> GetReferences(Script script)
        {
            foreach (var reference in script.Options.MetadataReferences)
            {
                var ur = reference as UnresolvedMetadataReference;

                if (ur != null)
                    yield return ur.Reference;
                else
                    yield return reference.Display;
            }
        }

        /// <summary>
        /// Gets the references that are used in the current script.
        /// </summary>
        private IEnumerable<string> GetReferences()
        {
            IEnumerable<string> references = GetReferences(scriptState.Script);

            for (var a = scriptState.Script.Previous; a != null; a = a.Previous)
                references = references.Union(GetReferences(a));
            return references.Distinct();
        }

        internal IEnumerable<string> GetScriptHelperCode(out string scriptStart, out string scriptEnd)
        {
            IEnumerable<string> imports = GetImports();
            const string code = "<This is my unique code string>";
            string importedCode = FixImportedCode("");
            string generatedCode = GenerateCode(code, imports, importedCode);
            int codeStart = generatedCode.IndexOf(code), codeEnd = codeStart + code.Length;

            scriptStart = generatedCode.Substring(0, codeStart);
            scriptEnd = generatedCode.Substring(codeEnd);
            return GetReferences();
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
            StringBuilder newImportedCode = new StringBuilder();

            newImportedCode.AppendLine(importedCode);
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
