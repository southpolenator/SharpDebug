using CsScriptManaged.UI.CodeWindow;
using CsScriptManaged.Utility;
using DbgEngManaged;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsScriptManaged.UI
{
    internal class InteractiveCodeEditor : CsTextEditor
    {
        private InteractiveExecution interactiveExecution;
        private Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

        public InteractiveCodeEditor()
        {
            interactiveExecution = Context.InteractiveExecution;
            UpdateScriptCode();
        }

        protected override void OnExecuteCSharpScript()
        {
            string code = Document.Text;
            var oldOut = Console.Out;
            var oldError = Console.Error;

            try
            {
                string result = "";

                using (StringWriter writer = new StringWriter())
                {
                    Console.SetOut(writer);
                    Console.SetError(writer);

                    DebugOutput captureFlags = DebugOutput.Normal | DebugOutput.Error | DebugOutput.Warning | DebugOutput.Verbose
                        | DebugOutput.Prompt | DebugOutput.PromptRegisters | DebugOutput.ExtensionWarning | DebugOutput.Debuggee
                        | DebugOutput.DebuggeePrompt | DebugOutput.Symbols | DebugOutput.Status;
                    var callbacks = DebuggerOutputToTextWriter.Create(Console.Out, captureFlags);

                    using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
                    {
                        interactiveExecution.UnsafeInterpret(code);
                        writer.Flush();
                        result = writer.GetStringBuilder().ToString();
                    }
                }

                UpdateScriptCode();
                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result);
                Document.Text = "";
            }
            catch (CompileException ex)
            {
                CompileError[] errors = ex.Errors;

                if (errors[0].FileName.EndsWith(InteractiveExecution.InteractiveScriptName) || errors.Count(e => !e.FileName.EndsWith(InteractiveExecution.InteractiveScriptName)) == 0)
                {
                    CompileError e = errors[0];

                    MessageBox.Show(e.FullMessage);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("Compile errors:");
                    foreach (var error in errors)
                    {
                        sb.AppendLine(error.FullMessage);
                    }

                    MessageBox.Show(sb.ToString());
                }
            }
            catch (ExitRequestedException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is ExitRequestedException)
                    throw ex.InnerException;
                MessageBox.Show(ex.InnerException.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Console.SetError(oldError);
                Console.SetOut(oldOut);
            }
        }

        protected override void OnExecuteWinDbgCommand()
        {
            string command = Document.Text;

            try
            {
                string result = CsScripts.Debugger.ExecuteAndCapture(command);

                if (!string.IsNullOrEmpty(result))
                    MessageBox.Show(result);
                Document.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void UpdateScriptCode()
        {
            string scriptStart, scriptEnd;
            string[] loadedReferences = interactiveExecution.GetScriptHelperCode(out scriptStart, out scriptEnd).ToArray();

            if (loadedReferences.Length != loadedAssemblies.Count)
            {
                var newAssemblies = new List<IUnresolvedAssembly>();

                foreach (var assemblyPath in loadedReferences)
                    if (!loadedAssemblies.ContainsKey(assemblyPath))
                    {
                        try
                        {
                            var loader = new CecilLoader();
                            loader.DocumentationProvider = GetXmlDocumentation(assemblyPath);
                            newAssemblies.Add(loader.LoadAssemblyFile(assemblyPath));
                        }
                        catch (Exception)
                        {
                        }
                    }

                if (newAssemblies.Count > 0)
                {
                    projectContent = projectContent.AddAssemblyReferences(newAssemblies);
                }
            }

            ScriptStart = scriptStart;
            ScriptEnd = scriptEnd;
        }

        private static XmlDocumentationProvider GetXmlDocumentation(string dllPath)
        {
            if (!string.IsNullOrEmpty(dllPath))
            {
                var documentationFile = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".xml");

                if (File.Exists(documentationFile))
                {
                    return new XmlDocumentationProvider(documentationFile);
                }
            }

            return null;
        }
    }
}
